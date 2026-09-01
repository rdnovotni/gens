using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Hazards;

/// <summary>Linear-scan read helpers over the Hazards partitions, the same shape
/// <c>Health.HealthQueries</c> already established for its own namespace. §3's "Exposure is a standing,
/// emergent reading" is realized entirely by <see cref="CurrentExposure"/>: a caller (a future UI, a
/// future Omen-skew wire-up per §6.1) can read the exact same live number <see
/// cref="NaturalDisasterSystem"/> rolled against, at any point in the month, with no separate forecast
/// state to keep in sync — this item's own whole realization of §3's "forecast/knowledge" scope
/// item.</summary>
public static class HazardQueries
{
    /// <summary>One settlement's live, standing Exposure score for one of the eight ordinary hazards
    /// (<see cref="HazardType.VolcanicEruption"/> excepted — see <see cref="DormantVolcano"/>), computed
    /// fresh from this month's real terrain/building/seasonal inputs rather than read from any stored
    /// snapshot, per §3's own "an emergent number... not a slider" framing.</summary>
    public static int CurrentExposure(WorldState state, RuntimeId<Settlement> settlementId, HazardType hazardType, GameDate date) =>
        HazardExposureProfile.Compute(state, settlementId, date).ExposureFor(hazardType);

    /// <summary>Every <see cref="DisasterEvent"/> recorded for one settlement, in ascending-<see
    /// cref="RuntimeId{T}"/> (chronological, since IDs are issued in order) order.</summary>
    public static IEnumerable<DisasterEvent> EventsFor(WorldState state, RuntimeId<Settlement> settlementId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.DisasterEvents.InAscendingOrder())
        {
            if (entry.Value.SettlementId == settlementId)
                yield return entry.Value;
        }
    }

    /// <summary>The <see cref="DormantVolcano"/> designated on one Plot, if any — a plot with no entry
    /// carries no Dormant Volcano, the ordinary "absence reads as the default" convention every other
    /// keyed-by-owner-ID partition in this codebase already uses.</summary>
    public static DormantVolcano? DormantVolcanoOn(WorldState state, RuntimeId<Plot> plotId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        return state.DormantVolcanoes.TryGet(plotId, out var volcano) ? volcano : null;
    }
}
