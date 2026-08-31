using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Religion;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Edicts;

/// <summary>Emitted whenever an <see cref="IssueManumissionEdictCommand"/> is accepted — §5.1's
/// Declaration, "a real, immediate Dynasty Chronicle entry." Public: an Edict is, by definition, a
/// formal public proclamation, matching <see cref="Legal.LawsuitFiledEvent"/>'s own "a formal,
/// on-the-record civic act" reasoning.</summary>
public sealed record ManumissionEdictIssuedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<EdictRecord> EdictId,
    RuntimeId<Household> IssuingHouseholdId,
    int CharactersFreed,
    string? CausationId) : IDomainEvent
{
    public string Type => "edicts.manumissionEdictIssued";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { IssuingHouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// §5.5's Manumission Edict — "a sweeping, single-stroke mass-freeing of enslaved workers." Effect: every
/// living Enslaved Character belonging to <see cref="HouseholdId"/> is manumitted, one additive call per
/// Character into Labor &amp; Slavery's own already-shipped, already-tested <see cref="ManumitCommand"/>
/// (Vindicta) rather than a second, parallel legal-status mutation — matching Phase 12 item 8's own
/// "additive extension to an already-tested command" precedent, applied here to a loop of calls rather
/// than new parameters. A large real Dignitas gain follows (§5.5: "a very large Favor/Dignitas gain"),
/// plus a Favor gain when the household has a chosen Patron Deity (<see cref="HouseholdReligion"/>, Phase
/// 12 item 3) — households with no Patron Deity simply skip that half, matching that item's own "no
/// meaningful Favor to default to zero" precedent rather than forcing a deity choice here. Reception
/// (§5.5: "sharp Traditionalist and fellow-slaveholder backlash") is a real <see
/// cref="Scandal.ScandalRecord"/> via <see cref="EdictIssuance.RecordReception"/> — Faction-dependent
/// severity is exactly what that engine's own Traditionalist/Popularist reading already computes.
/// </summary>
public sealed record IssueManumissionEdictCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> PatronusCharacterId) : ICommand;

/// <summary>The validate/mutate pipeline for <see cref="IssueManumissionEdictCommand"/> (ADR 0006).</summary>
public static class IssueManumissionEdictCommands
{
    public static readonly ValidationErrorCode InsufficientInfluence = EdictIssuance.InsufficientInfluence;
    public static readonly ValidationErrorCode PatronusNotFound = new("edicts.issueManumission.patronusNotFound");
    public static readonly ValidationErrorCode PatronusDeceased = new("edicts.issueManumission.patronusDeceased");
    public static readonly ValidationErrorCode NoEnslavedCharacters = new("edicts.issueManumission.noEnslavedCharacters");

    public static readonly CommandPipeline<WorldState, IssueManumissionEdictCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, IssueManumissionEdictCommand command)
    {
        if (!state.Characters.TryGet(command.PatronusCharacterId, out var patronus))
            return PatronusNotFound;
        if (!patronus!.IsAlive)
            return PatronusDeceased;
        if (!EnslavedHouseholdMembers(state, command.HouseholdId).Any())
            return NoEnslavedCharacters;

        return EdictIssuance.ValidateAffordability(state, command.HouseholdId, EdictCatalog.ManumissionEdictInfluenceCost);
    }

    private static IDomainEvent[] Mutate(WorldState state, IssueManumissionEdictCommand command)
    {
        var events = EdictIssuance.ChargeCosts(
            state, command.CommandId, command.ActorId, command.SubmittedDate, command.HouseholdId,
            EdictCatalog.ManumissionEdictInfluenceCost, EdictCatalog.ManumissionEdictDignitasCost,
            "Edict issued: Manumission Edict");

        var freed = EnslavedHouseholdMembers(state, command.HouseholdId).ToArray();
        foreach (var characterId in freed)
        {
            events.AddRange(ManumitCommands.Pipeline.Execute(
                state, new ManumitCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    characterId, command.PatronusCharacterId, ManumissionType.Vindicta)).Events);
        }

        events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                command.HouseholdId, EdictCatalog.ManumissionEdictDignitasGain, "Manumission Edict: freed workers")).Events);

        if (state.HouseholdReligions.TryGet(command.HouseholdId, out _))
        {
            events.AddRange(AdjustFavorCommands.Pipeline.Execute(
                state, new AdjustFavorCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    command.HouseholdId, EdictCatalog.ManumissionEdictFavorGain, "Manumission Edict")).Events);
        }

        var (scandalId, receptionEvents) = EdictIssuance.RecordReception(
            state, command.CommandId, command.ActorId, command.SubmittedDate, command.HouseholdId,
            EdictCatalog.ManumissionEdictReceptionSeverity);
        events.AddRange(receptionEvents);

        var edictId = state.EdictRecordIds.Issue();
        state.EdictRecords.Add(edictId, new EdictRecord(
            edictId, command.HouseholdId, EdictType.ManumissionEdict, command.SubmittedDate,
            EdictCatalog.ManumissionEdictInfluenceCost, EdictCatalog.ManumissionEdictDignitasCost, scandalId));

        events.Add(new ManumissionEdictIssuedEvent(
            state.EventIds.Issue(), command.SubmittedDate, edictId, command.HouseholdId, freed.Length, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }

    private static IEnumerable<RuntimeId<Character>> EnslavedHouseholdMembers(WorldState state, RuntimeId<Household> householdId) =>
        state.Characters.InAscendingOrder()
            .Where(entry => entry.Value.Household == householdId && entry.Value.LegalStatus == LegalStatus.Enslaved && entry.Value.IsAlive)
            .Select(entry => entry.Key);
}
