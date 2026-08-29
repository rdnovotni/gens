using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Religion;

/// <summary>Raises a new <see cref="OmenEvent"/> against a household that has already chosen a Patron
/// Deity (Phase 12 item 3; §4.1) — see <see cref="OmenEvent"/>'s own doc comment for why this is a
/// caller-driven primitive rather than a self-triggering periodic generator.</summary>
public sealed record RaiseOmenCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    int Severity) : ICommand;

/// <summary>Emitted whenever a <see cref="RaiseOmenCommand"/> is accepted. Public: an Omen is a shared,
/// observable portent (a flight of birds, a storm), not a private household fact.</summary>
public sealed record OmenRaisedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<OmenEvent> OmenId,
    RuntimeId<Household> HouseholdId,
    PatronDeity ThemedDeity,
    int Severity,
    string? CausationId) : IDomainEvent
{
    public string Type => "religion.omenRaised";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="RaiseOmenCommand"/> (ADR 0006).</summary>
public static class RaiseOmenCommands
{
    public static readonly ValidationErrorCode NoPatronDeityYet = new("religion.raiseOmen.noPatronDeityYet");
    public static readonly ValidationErrorCode SeverityOutOfRange = new("religion.raiseOmen.severityOutOfRange");

    public static readonly CommandPipeline<WorldState, RaiseOmenCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RaiseOmenCommand command)
    {
        if (!HouseholdReligionResolver.HasChosenPatron(state, command.HouseholdId))
            return NoPatronDeityYet;
        if (command.Severity is < ReligionCatalog.MinOmenSeverity or > ReligionCatalog.MaxOmenSeverity)
            return SeverityOutOfRange;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, RaiseOmenCommand command)
    {
        state.HouseholdReligions.TryGet(command.HouseholdId, out var religion);
        var omenId = state.OmenEventIds.Issue();

        state.OmenEvents.Add(
            omenId,
            new OmenEvent(omenId, command.HouseholdId, command.SubmittedDate, religion!.PatronDeity, command.Severity));

        return new IDomainEvent[]
        {
            new OmenRaisedEvent(
                state.EventIds.Issue(), command.SubmittedDate, omenId, command.HouseholdId, religion.PatronDeity, command.Severity,
                command.CommandId.ToTaggedString()),
        };
    }
}
