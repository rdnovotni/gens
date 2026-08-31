using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Travel;

/// <summary>§5's Recall: "an early, deliberate end to a trip, resolving the return leg immediately
/// rather than waiting out the original commitment." Not free — "the traveler forfeits whatever
/// Encounter they hadn't yet completed" (this command always forces <see
/// cref="TravelTrip.EncounterCompleted"/> false, since a trip worth recalling by definition hasn't had
/// its business finished the ordinary way). Usable while <see cref="TravelTripStatus.Traveling"/>
/// (abandon the trip before even arriving) or <see cref="TravelTripStatus.Arrived"/> (cut a stay
/// short); a trip already heading home has nothing left to recall.</summary>
public sealed record RecallTravelCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<TravelTrip> TripId) : ICommand;

/// <summary>Emitted whenever a <see cref="RecallTravelCommand"/> is accepted.</summary>
public sealed record TravelRecalledEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<TravelTrip> TripId,
    RuntimeId<Character> TravelerCharacterId,
    string? CausationId) : IDomainEvent
{
    public string Type => "travel.recalled";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { TravelerCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="RecallTravelCommand"/> (ADR 0006).</summary>
public static class RecallTravelCommands
{
    public static readonly ValidationErrorCode TripNotFound = new("travel.recall.tripNotFound");
    public static readonly ValidationErrorCode NotRecallable = new("travel.recall.notRecallable");

    public static readonly CommandPipeline<WorldState, RecallTravelCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RecallTravelCommand command)
    {
        if (!state.TravelTrips.TryGet(command.TripId, out var trip))
            return TripNotFound;
        if (trip.Status is not (TravelTripStatus.Traveling or TravelTripStatus.Arrived))
            return NotRecallable;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, RecallTravelCommand command)
    {
        state.TravelTrips.TryGet(command.TripId, out var trip);
        state.TravelTrips.Remove(command.TripId);
        state.TravelTrips.Add(
            command.TripId,
            trip with { MonthsElapsed = 0, Status = TravelTripStatus.Recalled, EncounterCompleted = false });

        // A trip recalled before ever arriving never set its party's CurrentTravelLocation to the
        // destination (TravelProgressSystem only does that on Arrival) — nothing to unwind there. One
        // recalled from Arrived keeps CurrentTravelLocation at the destination until the return leg
        // actually finishes, matching TravelProgressSystem's own Completed-transition clearing it.

        return new IDomainEvent[]
        {
            new TravelRecalledEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.TripId, trip.Party.TravelerId,
                command.CommandId.ToTaggedString()),
        };
    }
}
