namespace Gens.Simulation.Travel;

/// <summary>§10's <c>status</c> field, plus a terminal <see cref="Completed"/> state §10's own sketch
/// doesn't name explicitly but its lifecycle needs: a trip that has fully returned home stays on record
/// (matching this codebase's own "kept forever once recorded" convention for a resolved runtime
/// record) without still reading as <see cref="Traveling"/>/<see cref="Returning"/> and blocking its
/// party's next trip (§5's reservation).</summary>
public enum TravelTripStatus
{
    /// <summary>Outbound leg in progress.</summary>
    Traveling,

    /// <summary>At the destination; the Arrival Encounter (§7) is available.</summary>
    Arrived,

    /// <summary>Return leg in progress, begun deliberately once the Encounter is done (or forfeited).</summary>
    Returning,

    /// <summary>§5's Recall: the trip was cut short, forfeiting whatever Encounter wasn't yet
    /// completed, and is now resolving its return leg immediately.</summary>
    Recalled,

    /// <summary>Home again. Terminal.</summary>
    Completed,
}
