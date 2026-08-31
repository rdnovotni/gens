using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Travel;

/// <summary>Emitted when a <see cref="TravelTrip"/>'s outbound leg finishes (§7's Arrival) — the
/// destination's own Encounter menu becomes available from here, though building that menu is a later
/// item's job (§7 names it per-destination-type; this item only tracks the trip's own state machine).</summary>
public sealed record TravelArrivedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<TravelTrip> TripId,
    RuntimeId<Character> TravelerCharacterId,
    LocationKind DestinationKind,
    string? CausationId) : IDomainEvent
{
    public string Type => "travel.arrived";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { TravelerCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>Emitted when a <see cref="TravelTrip"/>'s return leg finishes and its party is home again.</summary>
public sealed record TravelCompletedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<TravelTrip> TripId,
    RuntimeId<Character> TravelerCharacterId,
    bool EncounterCompleted,
    string? CausationId) : IDomainEvent
{
    public string Type => "travel.completed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { TravelerCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// Advances every non-terminal <see cref="TravelTrip"/> by one month (§3's committed block of time; §5's
/// "multiple trips resolve fully concurrently" — every trip in the registry advances independently in
/// the same tick, regardless of how many others are in flight). <see
/// cref="TravelTripStatus.Traveling"/> counts up toward Arrival; <see
/// cref="TravelTripStatus.Returning"/> and <see cref="TravelTripStatus.Recalled"/> both count up toward
/// <see cref="TravelTripStatus.Completed"/> against the same <see cref="TravelTrip.TravelTimeMonths"/>
/// (this item's own invented "return leg costs the same as the outbound leg" default — §11 leaves
/// return-trip timing an explicit open question). <see cref="TravelTripStatus.Arrived"/> is a resting
/// state this system never advances on its own — see <see cref="BeginReturnCommand"/>.
/// </summary>
public sealed class TravelProgressSystem : IMonthlySystem<WorldState>
{
    public string Id => "travel.progress";
    public TickPhase Phase => TickPhase.Lifecycle;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "travelTrips", "characters" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "travelTrips", "characters", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var tripUpdates = new List<(RuntimeId<TravelTrip> Id, TravelTrip Trip)>();
        var locationUpdates = new List<(RuntimeId<Character> MemberId, TravelLocation? Location)>();

        foreach (var entry in state.TravelTrips.InAscendingOrder())
        {
            var trip = entry.Value;
            switch (trip.Status)
            {
                case TravelTripStatus.Traveling:
                    AdvanceOutboundLeg(state, context, entry.Key, trip, tripUpdates, locationUpdates, events);
                    break;
                case TravelTripStatus.Returning:
                case TravelTripStatus.Recalled:
                    AdvanceReturnLeg(state, context, entry.Key, trip, tripUpdates, locationUpdates, events);
                    break;
                case TravelTripStatus.Arrived:
                case TravelTripStatus.Completed:
                default:
                    break;
            }
        }

        foreach (var (id, trip) in tripUpdates)
        {
            state.TravelTrips.Remove(id);
            state.TravelTrips.Add(id, trip);
        }

        foreach (var (memberId, location) in locationUpdates)
        {
            if (!state.Characters.TryGet(memberId, out var member))
                continue;
            state.Characters.Remove(memberId);
            state.Characters.Add(memberId, member with { CurrentTravelLocation = location });
        }

        return events;
    }

    private static void AdvanceOutboundLeg(
        WorldState state, MonthlyTickContext context, RuntimeId<TravelTrip> tripId, TravelTrip trip,
        List<(RuntimeId<TravelTrip> Id, TravelTrip Trip)> tripUpdates,
        List<(RuntimeId<Character> MemberId, TravelLocation? Location)> locationUpdates,
        List<IDomainEvent> events)
    {
        var elapsed = trip.MonthsElapsed + 1;
        if (elapsed < trip.TravelTimeMonths)
        {
            tripUpdates.Add((tripId, trip with { MonthsElapsed = elapsed }));
            return;
        }

        tripUpdates.Add((tripId, trip with { MonthsElapsed = 0, Status = TravelTripStatus.Arrived }));
        foreach (var memberId in trip.Party.AllMembers)
            locationUpdates.Add((memberId, trip.Destination));
        events.Add(new TravelArrivedEvent(
            state.EventIds.Issue(), context.Date, tripId, trip.Party.TravelerId,
            trip.Destination.Kind, CausationId: null));
    }

    private static void AdvanceReturnLeg(
        WorldState state, MonthlyTickContext context, RuntimeId<TravelTrip> tripId, TravelTrip trip,
        List<(RuntimeId<TravelTrip> Id, TravelTrip Trip)> tripUpdates,
        List<(RuntimeId<Character> MemberId, TravelLocation? Location)> locationUpdates,
        List<IDomainEvent> events)
    {
        var elapsed = trip.MonthsElapsed + 1;
        if (elapsed < trip.TravelTimeMonths)
        {
            tripUpdates.Add((tripId, trip with { MonthsElapsed = elapsed }));
            return;
        }

        tripUpdates.Add((tripId, trip with { MonthsElapsed = 0, Status = TravelTripStatus.Completed }));
        foreach (var memberId in trip.Party.AllMembers)
            locationUpdates.Add((memberId, null));
        events.Add(new TravelCompletedEvent(
            state.EventIds.Issue(), context.Date, tripId, trip.Party.TravelerId,
            trip.EncounterCompleted, CausationId: null));
    }
}
