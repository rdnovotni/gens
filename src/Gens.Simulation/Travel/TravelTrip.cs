using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Travel;

/// <summary>§10's <c>TravelTrip{}</c> shape: one committed block of travel time for a <see
/// cref="TravelParty"/>, from <see cref="Origin"/> to <see cref="Destination"/>. A real <see
/// cref="State.WorldState"/> partition, unlike <see cref="TravelLocation"/>/<see cref="TravelRoute"/> —
/// a trip's own progress is genuine campaign state that changes tick to tick, not a value derivable
/// fresh each time from other entities' own records.</summary>
public sealed record TravelTrip
{
    private TravelTrip()
    {
    }

    public required RuntimeId<TravelTrip> Id { get; init; }
    public required TravelParty Party { get; init; }
    public required TravelLocation Origin { get; init; }
    public required TravelLocation Destination { get; init; }
    public required DistanceTier DistanceTier { get; init; }
    public required RouteRiskLevel RiskExposure { get; init; }
    public required int TravelTimeMonths { get; init; }

    /// <summary>Months elapsed on the current leg (outbound, or return once <see cref="Status"/>
    /// leaves <see cref="TravelTripStatus.Arrived"/>) — reset to 0 whenever <see cref="Status"/>
    /// changes, since each leg counts up against the same <see cref="TravelTimeMonths"/> independently.</summary>
    public int MonthsElapsed { get; init; }

    public required GameDate DepartedDate { get; init; }
    public TravelTripStatus Status { get; init; } = TravelTripStatus.Traveling;

    /// <summary>False if a <see cref="TravelTripStatus.Recalled"/> trip forfeited an unfinished
    /// Encounter (§7); set true only by a deliberate <see cref="BeginReturnCommand"/> once the
    /// traveler is done at the destination.</summary>
    public bool EncounterCompleted { get; init; }

    /// <summary>The only supported way to begin a trip — <paramref name="route"/>'s own <see
    /// cref="TravelRoute.TravelTimeMonths"/> must be positive (guaranteed by every <see
    /// cref="TravelRoute.Resolve"/> outcome; guarded here rather than trusted, matching this
    /// codebase's "constructor validates" convention).</summary>
    public static TravelTrip Begin(RuntimeId<TravelTrip> id, TravelParty party, TravelRoute route, GameDate departedDate)
    {
        if (party is null)
            throw new ArgumentNullException(nameof(party));
        if (route is null)
            throw new ArgumentNullException(nameof(route));
        if (route.TravelTimeMonths <= 0)
            throw new ArgumentException("A route's travel time must be positive.", nameof(route));

        return new TravelTrip
        {
            Id = id,
            Party = party,
            Origin = route.Origin,
            Destination = route.Destination,
            DistanceTier = route.DistanceTier,
            RiskExposure = route.RiskExposure,
            TravelTimeMonths = route.TravelTimeMonths,
            MonthsElapsed = 0,
            DepartedDate = departedDate,
            Status = TravelTripStatus.Traveling,
            EncounterCompleted = false,
        };
    }

    /// <summary>Reconstructs a <see cref="TravelTrip"/> from persisted save data (ADR 0010), carrying
    /// whatever leg progress/status/Encounter state the save actually had — unlike <see cref="Begin"/>,
    /// this does not assume the trip is freshly starting. Mirrors <see
    /// cref="Characters.Character.Create"/>'s own "the mapper's own restore path, not the in-fiction
    /// factory" shape.</summary>
    public static TravelTrip Restore(
        RuntimeId<TravelTrip> id, TravelParty party, TravelLocation origin, TravelLocation destination,
        DistanceTier distanceTier, RouteRiskLevel riskExposure, int travelTimeMonths, int monthsElapsed,
        GameDate departedDate, TravelTripStatus status, bool encounterCompleted) =>
        new()
        {
            Id = id,
            Party = party,
            Origin = origin,
            Destination = destination,
            DistanceTier = distanceTier,
            RiskExposure = riskExposure,
            TravelTimeMonths = travelTimeMonths,
            MonthsElapsed = monthsElapsed,
            DepartedDate = departedDate,
            Status = status,
            EncounterCompleted = encounterCompleted,
        };
}
