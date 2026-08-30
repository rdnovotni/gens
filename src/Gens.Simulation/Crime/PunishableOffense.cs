using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Legal;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Crime;

/// <summary>§3's source vocabulary. Only <see cref="LegalConviction"/> and <see cref="Fabricated"/>
/// are ever actually minted by this item: <see cref="Legal.LegalCaseRuling"/> mints <see
/// cref="LegalConviction"/> directly on a <see cref="Legal.LegalCaseVerdict.Convicted"/> verdict (a
/// real, immediately reachable source this item wires on sight), and <see cref="Fabricated"/> is a
/// generic, source-agnostic flag any future caller can set directly through <see
/// cref="RecordPunishableOffenseCommand"/> — this item does not build the Scheme type §9 names to
/// originate one (<c>Interactions.SchemeType</c> currently has exactly one real value, <c>Coercive</c>
/// (Phase 10 item 6); its own doc comment already earmarks room for "every future consumer... adds its
/// own scheme type here," but wiring §9's own Discovered/retroactive-Unjust consequence loop is a real,
/// separate item of work this pass does not open). <see cref="DiscoveredScheme"/>, <see
/// cref="DiscoveredAffair"/>, <see cref="MilitaryCapture"/>, and <see cref="PiracyCapture"/> are kept in
/// the enum for schema completeness (every real design-doc source is represented) but are never rolled
/// by any caller in this codebase: a Scheme's own discovery-and-escalation outcome (<see
/// cref="Interactions.SchemeStatus.DiscoveredAndEscalated"/>) carries no "was this an adultery-shaped
/// scheme" distinction to source <see cref="DiscoveredAffair"/> from, Romance &amp; Sexuality &amp;
/// Lineage's own affair-discovery mechanic (§11 of that document) doesn't exist in this codebase at
/// all, and neither does Military &amp; Combat nor Piracy &amp; Banditry (both Phase 16) — matching
/// <see cref="Legal.LegalCase.CaseType"/>'s own "every real category represented, only some
/// reachable" precedent.</summary>
public enum PunishableOffenseSource
{
    LegalConviction,
    DiscoveredScheme,
    DiscoveredAffair,
    MilitaryCapture,
    PiracyCapture,
    Fabricated,
}

/// <summary>§7's tiered sentencing catalog needs to know how serious an offense actually was before a
/// <see cref="SentenceType"/> can be chosen against it. Derived automatically when an offense is minted
/// from a <see cref="Legal.LegalCase"/> — <see cref="Legal.LegalCaseType.Criminal"/>/<see
/// cref="Legal.LegalCaseType.Political"/> (that document's own two "capital-shaped" types) map to <see
/// cref="Capital"/>, everything else to <see cref="Serious"/> — and left to the caller's own judgment
/// for a <see cref="PunishableOffenseSource.Fabricated"/> entry, since no real source event exists yet
/// to derive it from automatically.</summary>
public enum OffenseSeverity
{
    Minor,
    Serious,
    Capital,
}

/// <summary>
/// One formally tracked mark on a Character's own record (Phase 12 item 5; §3: "what opens the door" to
/// a Justified reading of an otherwise-naked exercise of power). Kept forever once recorded, matching
/// <see cref="Legal.LegalCase"/>'s identical "kept for the campaign's lifetime" convention — a
/// character's full offense history is exactly the kind of record a future sentencing/ransom/Chronicle
/// query needs the whole log for, not just "is there currently one active entry."
///
/// <b>Character-scoped, not household-scoped</b> — the one deliberate point of contrast with <see
/// cref="Legal.LegalCase"/>'s own household-level party model (that item's own scope note): §4's Imprison
/// action targets a specific Character (a dependent, a Client), so the offense that justifies or fails
/// to justify imprisoning them has to be checked against that same Character, not their whole household.
/// When a <see cref="Legal.LegalCase"/> convicts a household, <see cref="Legal.LegalCaseRuling"/> records
/// the offense against the defendant household's own recorded head (<see
/// cref="Succession.HouseholdHeadship.HeadCharacterId"/>) — the same "lands on the household's recorded
/// head" simplification that item's own Patria Potestas Scandal-Marked trait already accepts.
/// </summary>
/// <param name="IsFabricated">§9's generic hook: true for a manufactured offense. Mechanically
/// identical to a real one everywhere this item reads it (§3: "the entire reason a player or an NPC
/// would use it") — nothing in this item's own resolvers branches on this flag at all.</param>
/// <param name="FabricationDiscovered">§9: "a discovered Fabrication... retroactively converts the
/// original action into a provably unjust one." Always false in this item — no Scheme type exists to
/// ever flip it true, matching <see cref="Legal.LegalCase.PresidingCharacterScouted"/>'s own "the flag
/// is the documented hook" precedent for a consequence this pass names but cannot yet trigger.</param>
public sealed record PunishableOffense(
    RuntimeId<PunishableOffense> OffenseId,
    RuntimeId<Character> CharacterId,
    PunishableOffenseSource Source,
    OffenseSeverity Severity,
    GameDate RecordedDate,
    bool IsFabricated = false,
    bool FabricationDiscovered = false,
    RuntimeId<LegalCase>? SourceLegalCaseId = null);

/// <summary>Read-side helpers over <see cref="WorldState.PunishableOffenses"/>, matching <see
/// cref="Legal.LegalCaseResolver"/>'s identical "a small, hand-curated collection doesn't need a
/// maintained secondary index yet" linear-scan convention.</summary>
public static class PunishableOffenseResolver
{
    /// <summary>§3: "a character with a real, standing Punishable Offense" — true the moment any
    /// offense is on <paramref name="characterId"/>'s own record, undiscovered-as-fabricated. (No
    /// caller in this codebase ever sets <see cref="PunishableOffense.FabricationDiscovered"/> true —
    /// see that field's own doc comment — so this check is, for now, simply "does at least one offense
    /// exist," kept separate from that field so a future Fabrication-discovery pass has somewhere real
    /// to plug in without changing this method's own signature.)</summary>
    public static bool HasActiveOffense(WorldState state, RuntimeId<Character> characterId)
    {
        foreach (var entry in state.PunishableOffenses.InAscendingOrder())
            if (entry.Value.CharacterId == characterId && !entry.Value.FabricationDiscovered)
                return true;

        return false;
    }

    /// <summary>The most severe still-active offense on record, if any — §7's own tier-selection input
    /// for a sentence a caller wants to justify against the worst thing actually on file.</summary>
    public static PunishableOffense? MostSevere(WorldState state, RuntimeId<Character> characterId)
    {
        PunishableOffense? best = null;
        foreach (var entry in state.PunishableOffenses.InAscendingOrder())
        {
            if (entry.Value.CharacterId != characterId || entry.Value.FabricationDiscovered)
                continue;
            if (best is null || entry.Value.Severity > best.Severity)
                best = entry.Value;
        }

        return best;
    }
}

/// <summary>Records a new <see cref="PunishableOffense"/> against a Character (Phase 12 item 5; §3).
/// The one command path every real or future source routes through — <see cref="Legal.LegalCaseRuling"/>
/// (wired directly by this item) and any future Scheme/affair/capture caller (not wired — see <see
/// cref="PunishableOffenseSource"/>'s own doc comment) alike — matching <see
/// cref="Reputation.AdjustDignitasCommand"/>'s own "the one command path every future mover routes
/// through" precedent.</summary>
public sealed record RecordPunishableOffenseCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    PunishableOffenseSource Source,
    OffenseSeverity Severity,
    bool IsFabricated = false,
    RuntimeId<LegalCase>? SourceLegalCaseId = null) : ICommand;

/// <summary>Emitted whenever a <see cref="RecordPunishableOffenseCommand"/> is accepted. Public: an
/// offense on record is exactly the kind of fact §4's Justified/Unjust check needs the rest of the
/// world to be able to read, the same reasoning <see cref="Reputation.AdjustDignitasCommand"/>'s own
/// <c>DignitasChangedEvent</c> already established for Dignitas.</summary>
public sealed record PunishableOffenseRecordedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<PunishableOffense> OffenseId,
    RuntimeId<Character> CharacterId,
    PunishableOffenseSource Source,
    OffenseSeverity Severity,
    string? CausationId) : IDomainEvent
{
    public string Type => "crime.punishableOffenseRecorded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="RecordPunishableOffenseCommand"/> (ADR 0006).</summary>
public static class RecordPunishableOffenseCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("crime.recordPunishableOffense.characterNotFound");

    public static readonly CommandPipeline<WorldState, RecordPunishableOffenseCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RecordPunishableOffenseCommand command) =>
        state.Characters.TryGet(command.CharacterId, out _) ? null : CharacterNotFound;

    private static IDomainEvent[] Mutate(WorldState state, RecordPunishableOffenseCommand command)
    {
        var offenseId = state.PunishableOffenseIds.Issue();
        var offense = new PunishableOffense(
            offenseId, command.CharacterId, command.Source, command.Severity, command.SubmittedDate,
            command.IsFabricated, FabricationDiscovered: false, command.SourceLegalCaseId);
        state.PunishableOffenses.Add(offenseId, offense);

        return new IDomainEvent[]
        {
            new PunishableOffenseRecordedEvent(
                state.EventIds.Issue(), command.SubmittedDate, offenseId, command.CharacterId,
                command.Source, command.Severity, command.CommandId.ToTaggedString()),
        };
    }
}
