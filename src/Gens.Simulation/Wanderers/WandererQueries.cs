using Gens.Simulation.Identity;
using Gens.Simulation.Regions;
using Gens.Simulation.State;

namespace Gens.Simulation.Wanderers;

/// <summary>Linear-scan read helpers over the Wanderers partitions, the same shape
/// <c>Health.HealthQueries</c> and <c>Hazards.HazardQueries</c> already established for their own
/// namespaces. Every one is a scan rather than an index: the actively-tracked Wanderer roster is small
/// by construction (§8's whole point), the same reasoning <c>Health.HealthQueries</c> used for a
/// Character's standing conditions.</summary>
public static class WandererQueries
{
    /// <summary>Every <see cref="Wanderer"/> still moving on their own Itinerary, in ascending-<see
    /// cref="RuntimeId{T}"/> (instantiation) order.</summary>
    public static IEnumerable<Wanderer> ActivelyTracked(WorldState state)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.Wanderers.InAscendingOrder())
        {
            if (entry.Value.IsActivelyTracked)
                yield return entry.Value;
        }
    }

    /// <summary>Every actively-tracked <see cref="Wanderer"/> currently standing at <paramref
    /// name="locationId"/> — §5's direct-Travel-encounter read, answerable the moment a caller knows
    /// which Gazetteer entry a destination is (see <see cref="InstantiateWandererCommands"/> for what
    /// is and is not wired into Travel's own Arrival-Encounter framework this pass).</summary>
    public static IEnumerable<Wanderer> At(WorldState state, DefinitionId<GazetteerLocationDefinition> locationId)
    {
        foreach (var wanderer in ActivelyTracked(state))
        {
            if (wanderer.CurrentLocationId.Equals(locationId))
                yield return wanderer;
        }
    }

    /// <summary>Whether <paramref name="householdId"/> can still engage <paramref name="wandererId"/> —
    /// false once the Wanderer has been Recruited, or once §7's race has been won by somebody
    /// else.</summary>
    public static bool IsAvailableTo(WorldState state, RuntimeId<Wanderer> wandererId, RuntimeId<Household> householdId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        if (!state.Wanderers.TryGet(wandererId, out var wanderer))
            return false;
        if (!wanderer!.IsActivelyTracked || wanderer.Status != WandererStatus.Wandering)
            return false;

        return wanderer.CommittedHouseholdId is not { } committed || committed == householdId;
    }

    /// <summary>Every recorded <see cref="WandererEngagement"/> for one Wanderer, in ascending-<see
    /// cref="RuntimeId{T}"/> (chronological, since IDs are issued in order) order — the same read shape
    /// <c>Hazards.HazardQueries.EventsFor</c> already established.</summary>
    public static IEnumerable<WandererEngagement> EngagementsFor(WorldState state, RuntimeId<Wanderer> wandererId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.WandererEngagements.InAscendingOrder())
        {
            if (entry.Value.WandererId == wandererId)
                yield return entry.Value;
        }
    }

    /// <summary>Every recorded <see cref="WandererEngagement"/> one household has made.</summary>
    public static IEnumerable<WandererEngagement> EngagementsBy(WorldState state, RuntimeId<Household> householdId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.WandererEngagements.InAscendingOrder())
        {
            if (entry.Value.HouseholdId == householdId)
                yield return entry.Value;
        }
    }
}
