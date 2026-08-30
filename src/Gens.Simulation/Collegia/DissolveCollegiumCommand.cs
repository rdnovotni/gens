using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Magistracies;
using Gens.Simulation.Reputation;
using Gens.Simulation.Scandal;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Collegia;

/// <summary>
/// §7's dissolution authority: a sitting magistrate formally dissolves an Illicit collegium. §7 also
/// names a provincial governor as a second authority basis — omitted entirely, matching <see
/// cref="Crime.ImprisonAuthorityBasis"/>'s own "omitted rather than included-but-unreachable" precedent,
/// since Reputation Duality and the provincial-governor concept it would need do not exist anywhere in
/// this codebase yet. A genuinely terminal action, matching <see
/// cref="LivingWorldActorExtinctionSystem"/>'s identical "removed outright, not frozen" precedent: both
/// the collegium's own <see cref="CollegiumDetails"/> entry and its underlying <see
/// cref="LivingWorldActor"/> entry are removed from <see cref="WorldState"/>, rather than left as a
/// lingering "dissolved" status a later lookup could still stumble onto.
/// </summary>
public sealed record DissolveCollegiumCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Actor> CollegiumId,
    RuntimeId<Character> InitiatingMagistrateCharacterId) : ICommand;

/// <summary>Emitted whenever a <see cref="DissolveCollegiumCommand"/> is accepted. Public — a formal
/// dissolution is a real, visible act of a magistrate's own standing power, the same reasoning <see
/// cref="LivingWorldActorExtinguishedEvent"/> already gives for a house's own extinction.</summary>
public sealed record CollegiumDissolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Actor> CollegiumId,
    RuntimeId<Character> InitiatingMagistrateCharacterId,
    RuntimeId<Household>? FormerPatronHouseholdId,
    string? CausationId) : IDomainEvent
{
    public string Type => "collegia.dissolved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CollegiumId.ToTaggedString(), InitiatingMagistrateCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="DissolveCollegiumCommand"/> (ADR 0006).</summary>
public static class DissolveCollegiumCommands
{
    public static readonly ValidationErrorCode CollegiumNotFound = new("collegia.dissolve.collegiumNotFound");
    public static readonly ValidationErrorCode NotIllicit = new("collegia.dissolve.notIllicit");
    public static readonly ValidationErrorCode NoRealAuthority = new("collegia.dissolve.noRealAuthority");

    public static readonly CommandPipeline<WorldState, DissolveCollegiumCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, DissolveCollegiumCommand command)
    {
        if (!state.Collegia.TryGet(command.CollegiumId, out var details))
            return CollegiumNotFound;
        if (details!.LegalStatus != CollegiumLegalStatus.Illicit)
            return NotIllicit;
        if (!HoldsActiveOfficeAtCollegiumSettlement(state, command))
            return NoRealAuthority;

        return null;
    }

    /// <summary>Mirrors <see cref="Crime.ImprisonCommands"/>'s identical "a sitting magistrate acting
    /// outside a formal Hearing" authority check, applied to the collegium's own <see
    /// cref="LivingWorldActor.HomeSettlementId"/> rather than a Character's <see
    /// cref="Character.Location"/>.</summary>
    private static bool HoldsActiveOfficeAtCollegiumSettlement(WorldState state, DissolveCollegiumCommand command)
    {
        if (!state.Actors.TryGet(command.CollegiumId, out var collegiumActor))
            return false;

        foreach (var entry in state.MagistracyRecords.InAscendingOrder())
        {
            var record = entry.Value;
            if (MagistracyResolver.IsActive(record) && record.HolderId == command.InitiatingMagistrateCharacterId &&
                record.SettlementId == collegiumActor!.HomeSettlementId)
                return true;
        }

        return false;
    }

    private static IDomainEvent[] Mutate(WorldState state, DissolveCollegiumCommand command)
    {
        state.Collegia.TryGet(command.CollegiumId, out var details);
        var formerPatronHouseholdId = details!.PatronHouseholdId;

        var events = new List<IDomainEvent>();

        if (formerPatronHouseholdId is { } patronHouseholdId)
        {
            events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                state, new AdjustDignitasCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    patronHouseholdId, -CollegiumCatalog.IllicitPatronDignitasPenalty,
                    $"sponsored collegium {command.CollegiumId.ToTaggedString()} dissolved as Illicit")).Events);

            // Phase 12 item 7's real, immediately reachable IllicitCollegiumExposure Scandal source
            // (gens-scandal-design.md §4: "an Illicit Collegium's exposure — a patron's own public
            // association with a dissolved, disgraced collegium"), §7 exactly — this dissolution, not
            // the earlier Unjust-disruption flip to Illicit, is the actual "exposure" moment the design
            // doc names. The Dignitas penalty above already covers §7's own "ordinary case" bundle in
            // full; this adds only the ScandalRecord itself and the Scandal-Marked Trait.
            events.AddRange(RecordScandalCommands.Pipeline.Execute(
                state, new RecordScandalCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    patronHouseholdId, ScandalSourceType.IllicitCollegiumExposure, ScandalSeverity.PublicDisgrace,
                    ApplyOrdinaryDignitasPenalty: false, ApplyTraitGrant: true)).Events);
        }

        state.Collegia.Remove(command.CollegiumId);
        state.Actors.Remove(command.CollegiumId);

        events.Add(new CollegiumDissolvedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.CollegiumId, command.InitiatingMagistrateCharacterId,
            formerPatronHouseholdId, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}
