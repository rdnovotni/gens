using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Religion;

/// <summary>
/// The passive Sacred Calendar observance tier (Phase 12 item 3; §5: "the household marks the day
/// without special expense, a small automatic Favor tick and nothing more"). <paramref
/// name="FeastDay"/> is a plain, free-form string rather than a closed enum: §5's own table (Kalends of
/// January, Lupercalia, Parentalia, Cerealia, Vestalia, Neptunalia, Saturnalia, a household's own
/// Founding-Day Rite) is explicitly "a representative, non-exhaustive sample... the full year-round
/// roster is a natural later-pass task" (§11), so this item does not close over a fixed feast-day
/// catalog it would need to invent, matching <see cref="Reputation.AdjustDignitasCommand"/>'s identical
/// <c>Reason</c> convention for an open-ended, not-yet-cataloged vocabulary.
/// </summary>
public sealed record ObserveFeastDayCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    string FeastDay) : ICommand;

/// <summary>Emitted whenever an <see cref="ObserveFeastDayCommand"/> is accepted. Public, matching every
/// other Favor-moving event in this domain.</summary>
public sealed record FeastDayObservedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    string FeastDay,
    string? CausationId) : IDomainEvent
{
    public string Type => "religion.feastDayObserved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="ObserveFeastDayCommand"/> (ADR 0006).</summary>
public static class ObserveFeastDayCommands
{
    public static readonly ValidationErrorCode NoPatronDeityYet = new("religion.observeFeastDay.noPatronDeityYet");
    public static readonly ValidationErrorCode EmptyFeastDay = new("religion.observeFeastDay.emptyFeastDay");

    public static readonly CommandPipeline<WorldState, ObserveFeastDayCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, ObserveFeastDayCommand command)
    {
        if (!HouseholdReligionResolver.HasChosenPatron(state, command.HouseholdId))
            return NoPatronDeityYet;
        if (string.IsNullOrWhiteSpace(command.FeastDay))
            return EmptyFeastDay;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, ObserveFeastDayCommand command)
    {
        HouseholdReligionResolver.ApplyFavorDelta(state, command.HouseholdId, ReligionCatalog.PassiveFeastDayFavorGain);

        return new IDomainEvent[]
        {
            new FeastDayObservedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.FeastDay, command.CommandId.ToTaggedString()),
        };
    }
}
