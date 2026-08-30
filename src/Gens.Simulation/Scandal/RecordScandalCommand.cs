using Gens.Simulation.Characters;
using Gens.Simulation.Clientela;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Time;

namespace Gens.Simulation.Scandal;

/// <summary>Emitted whenever a <see cref="RecordScandalCommand"/> is accepted. Public — §3 frames a
/// Scandal as, definitionally, a matter that "stops being a private matter and becomes a real,
/// talked-about public fact," and every real source this item wires (<see
/// cref="Crime.CharacterImprisonedEvent"/>, <see cref="Legal.LegalCaseRuledEvent"/>, <see
/// cref="Collegia.CollegiumDissolvedEvent"/>) already carries the same <see
/// cref="Commands.Visibility.Public"/> reasoning at its own source event.</summary>
public sealed record ScandalRecordedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<ScandalRecord> ScandalId,
    RuntimeId<Household> PrimaryHouseholdId,
    ScandalSourceType SourceType,
    ScandalSeverity Severity,
    ScandalScope Scope,
    bool ScandalMarkedTraitApplied,
    string? CausationId) : IDomainEvent
{
    public string Type => "scandal.recorded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { PrimaryHouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// The one command path (rule 2) every real or future Scandal source routes through, mirroring <see
/// cref="AdjustDignitasCommand"/>'s and <see cref="Crime.RecordPunishableOffenseCommand"/>'s own "the
/// one command path every future mover routes through" precedent. Always creates a real <see
/// cref="ScandalRecord"/>; the three bool flags below let a caller opt out of the parts of §7's own
/// "ordinary case" bundle (a Dignitas penalty, a relationship-web scar, the Scandal-Marked Trait) that
/// an already-shipped, already-tested call site has already applied through its own existing mechanism
/// — <see cref="Legal.LegalCaseRuling"/>'s own Patria Potestas Dignitas penalty and trait grant chief
/// among them (see that type's own doc comment) — so this command never double-applies a consequence
/// another already-tested command already produced, while still recording the one real, comparable
/// <see cref="ScandalRecord"/> §3 calls for.
///
/// Real, reachable callers wired by this item: an Unjust <see cref="Crime.ImprisonCommand"/> and an
/// Unjust, execution-resulting <see cref="Crime.ApplySentenceCommand"/> (§4's "an Unjust imprisonment or
/// execution"), <see cref="DiscoverFabricationCommand"/> (§4's "a discovered Fabrication"), <see
/// cref="Legal.LegalCaseRuling"/>'s Patria Potestas ruling (§4's "a politically-weaponized Legal &amp;
/// Court case"), and <see cref="Collegia.DissolveCollegiumCommand"/> (§4's "an Illicit Collegium's
/// exposure").
/// </summary>
public sealed record RecordScandalCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> PrimaryHouseholdId,
    ScandalSourceType SourceType,
    ScandalSeverity Severity,
    bool ApplyOrdinaryDignitasPenalty = true,
    bool ApplyTraitGrant = true,
    RuntimeId<Character>? TraitGrantCharacterId = null,
    RuntimeId<Character>? ScarredAgainstCharacterId = null,
    bool OriginatedViaLibellusFamosus = false) : ICommand;

/// <summary>The validate/mutate pipeline for <see cref="RecordScandalCommand"/> (ADR 0006).</summary>
public static class RecordScandalCommands
{
    public static readonly CommandPipeline<WorldState, RecordScandalCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RecordScandalCommand command) => null;

    private static IDomainEvent[] Mutate(WorldState state, RecordScandalCommand command)
    {
        var events = new List<IDomainEvent>();

        // §6: always the "ordinary default once ambient spread runs its course" — see
        // ScandalScope's own doc comment for why HouseholdOnly/Provincial/RomeWide are never assigned.
        const ScandalScope scope = ScandalScope.SettlementWide;

        var headId = ResolveHead(state, command.PrimaryHouseholdId);

        // §7/§10: Faction-dependent reception, read directly off CharacterFactionAlignment.
        var basePenalty = SeverityPenalty(command.Severity);
        var traditionalistReading = basePenalty;
        var popularistReading = basePenalty;
        if (headId is { } headForFaction)
        {
            var faction = CharacterFactionResolver.Current(state, headForFaction);
            if (faction == PoliticalFaction.Traditionalist)
                traditionalistReading += ScandalCatalog.FactionAlignedReadingPenalty;
            else if (faction == PoliticalFaction.Popularist)
                popularistReading += ScandalCatalog.FactionAlignedReadingPenalty;
        }

        // §7: the Scandal-Marked Trait, for "a sufficiently severe or public case" only.
        var traitApplied = false;
        if (command.ApplyTraitGrant && command.Severity != ScandalSeverity.MinorEmbarrassment)
        {
            var grantTargetId = command.TraitGrantCharacterId ?? headId;
            if (grantTargetId is { } targetId)
                traitApplied = ApplyScandalMarkedTrait(state, targetId);
        }

        // §7: the ordinary-case Dignitas penalty, routed through the one shared Dignitas mover.
        if (command.ApplyOrdinaryDignitasPenalty)
        {
            events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                state, new AdjustDignitasCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    command.PrimaryHouseholdId, -basePenalty, $"scandal recorded: {command.SourceType}")).Events);
        }

        // §7: "a relationship-web scar across everyone connected to the matter" — this item's own
        // real, narrower slice: a scar between the household's recorded head and a specifically named
        // other party, when one is actually supplied.
        if (command.ScarredAgainstCharacterId is { } scarTargetId && headId is { } scarSourceId &&
            state.Characters.TryGet(scarSourceId, out var scarSource) && scarSource!.IsAlive &&
            state.Characters.TryGet(scarTargetId, out var scarTarget) && scarTarget!.IsAlive)
        {
            events.AddRange(RecordInteractionCommands.Pipeline.Execute(
                state, new RecordInteractionCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    scarSourceId, scarTargetId, ScandalCatalog.RelationshipScarOpinionDelta,
                    BondTag.None, BondTag.None, RelationshipOrigin.Political)).Events);
        }

        var scandalId = state.ScandalRecordIds.Issue();
        var record = new ScandalRecord(
            scandalId, command.PrimaryHouseholdId, command.SourceType, command.Severity, scope,
            command.SubmittedDate, command.OriginatedViaLibellusFamosus, CurrentFameEffect: null,
            traitApplied, NotaCensoriaIssued: false,
            new FactionDependentReception(traditionalistReading, popularistReading));
        state.ScandalRecords.Add(scandalId, record);

        events.Add(new ScandalRecordedEvent(
            state.EventIds.Issue(), command.SubmittedDate, scandalId, command.PrimaryHouseholdId,
            command.SourceType, command.Severity, scope, traitApplied, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }

    private static int SeverityPenalty(ScandalSeverity severity) => severity switch
    {
        ScandalSeverity.MinorEmbarrassment => ScandalCatalog.MinorEmbarrassmentDignitasPenalty,
        ScandalSeverity.PublicDisgrace => ScandalCatalog.PublicDisgraceDignitasPenalty,
        ScandalSeverity.NotaCensoriaEligible => ScandalCatalog.NotaCensoriaEligibleDignitasPenalty,
        _ => ScandalCatalog.MinorEmbarrassmentDignitasPenalty,
    };

    private static RuntimeId<Character>? ResolveHead(WorldState state, RuntimeId<Household> householdId) =>
        state.HouseholdHeadships.TryGet(householdId, out var headship) ? headship!.HeadCharacterId : null;

    /// <summary>Grants <see cref="ScandalCatalog.ScandalMarkedTraitId"/> directly on <paramref
    /// name="characterId"/>, matching <see cref="Legal.LegalCaseRuling"/>'s own existing trait-grant
    /// plumbing exactly (remove-then-readd <see cref="WorldState.Characters"/> with the trait appended)
    /// rather than inventing a second one. Returns whether the Character carries the trait once this
    /// call returns — true whether just granted or already present, since that is what <see
    /// cref="ScandalRecord.ScandalMarkedTraitApplied"/> actually records.</summary>
    internal static bool ApplyScandalMarkedTrait(WorldState state, RuntimeId<Character> characterId)
    {
        if (!state.Characters.TryGet(characterId, out var character) || character is null || !character.IsAlive)
            return false;
        if (character.Traits.Contains(ScandalCatalog.ScandalMarkedTraitId))
            return true;

        var updatedTraits = character.Traits.Append(ScandalCatalog.ScandalMarkedTraitId).ToArray();
        state.Characters.Remove(characterId);
        state.Characters.Add(characterId, character with { Traits = updatedTraits });
        return true;
    }
}
