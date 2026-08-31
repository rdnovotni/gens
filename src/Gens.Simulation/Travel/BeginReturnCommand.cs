using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Travel;

/// <summary>Deliberately leaves the destination and starts the return leg, once a <see
/// cref="TravelTrip"/> is <see cref="TravelTripStatus.Arrived"/> (§7's Encounter is done, or the
/// traveler chooses not to engage it further). Distinct from <see cref="RecallTravelCommand"/>'s §5
/// Recall: this is an ordinary, planned departure, not a crisis cutting the trip short.</summary>
public sealed record BeginReturnCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<TravelTrip> TripId,
    bool EncounterCompleted) : ICommand;

/// <summary>Emitted whenever a <see cref="BeginReturnCommand"/> is accepted.</summary>
public sealed record TravelReturnBegunEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<TravelTrip> TripId,
    RuntimeId<Character> TravelerCharacterId,
    bool EncounterCompleted,
    string? CausationId) : IDomainEvent
{
    public string Type => "travel.returnBegun";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { TravelerCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="BeginReturnCommand"/> (ADR 0006).</summary>
public static class BeginReturnCommands
{
    public static readonly ValidationErrorCode TripNotFound = new("travel.beginReturn.tripNotFound");
    public static readonly ValidationErrorCode NotArrived = new("travel.beginReturn.notArrived");

    public static readonly CommandPipeline<WorldState, BeginReturnCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, BeginReturnCommand command)
    {
        if (!state.TravelTrips.TryGet(command.TripId, out var trip))
            return TripNotFound;
        if (trip.Status != TravelTripStatus.Arrived)
            return NotArrived;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, BeginReturnCommand command)
    {
        state.TravelTrips.TryGet(command.TripId, out var trip);
        state.TravelTrips.Remove(command.TripId);
        state.TravelTrips.Add(
            command.TripId,
            trip with { MonthsElapsed = 0, Status = TravelTripStatus.Returning, EncounterCompleted = command.EncounterCompleted });

        return new IDomainEvent[]
        {
            new TravelReturnBegunEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.TripId, trip.Party.TravelerId,
                command.EncounterCompleted, command.CommandId.ToTaggedString()),
        };
    }
}
