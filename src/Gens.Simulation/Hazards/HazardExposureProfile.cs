using Gens.Simulation.Buildings;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Hazards;

/// <summary>One settlement's real, already-computable terrain/building inputs for this month's <see
/// cref="HazardExposureCalculator"/> reads, assembled once per settlement per tick rather than
/// re-scanned per hazard — the same shape <c>Health.EndemicIllnessSystem</c>'s own private
/// <c>SettlementHealthProfile</c> established, made <c>public</c> here (rather than a private nested
/// type) specifically so <see cref="HazardQueries.CurrentExposure"/> can share the exact same
/// computation <see cref="NaturalDisasterSystem"/> rolls against — §3's own "Exposure is a standing,
/// emergent reading" is only really true if a caller outside the tick itself can read the same number
/// the tick used, which is this record's whole reason for being public.</summary>
public readonly record struct HazardExposureProfile(
    double BuildingDensity,
    double RiverAdjacentFraction,
    double ForestCoverFraction,
    double CoastalFraction,
    double HillsFraction,
    bool DrySeasonMonth,
    bool StormSeasonMonth,
    double IrrigatedFraction = 0.0)
{
    public static HazardExposureProfile Compute(WorldState state, RuntimeId<Settlement> settlementId, GameDate date)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var plots = state.Plots.InAscendingOrder()
            .Where(entry => entry.Value.SettlementId == settlementId)
            .Select(entry => entry.Value)
            .ToArray();

        var totalPlots = plots.Length;
        var totalCapacity = plots.Sum(p => (double)p.Capacity);

        var riverAdjacentFraction = totalPlots == 0 ? 0.0 :
            plots.Count(p => p.Terrain == TerrainType.River || p.Features.HasFlag(TerrainFeature.RiverAdjacent)) / (double)totalPlots;
        var forestCoverFraction = totalPlots == 0 ? 0.0 : plots.Count(p => p.Terrain == TerrainType.Forest) / (double)totalPlots;
        var coastalFraction = totalPlots == 0 ? 0.0 :
            plots.Count(p => p.Terrain == TerrainType.Coast || p.Features.HasFlag(TerrainFeature.Coastline)) / (double)totalPlots;
        var hillsFraction = totalPlots == 0 ? 0.0 : plots.Count(p => p.Terrain == TerrainType.Hills) / (double)totalPlots;

        var plotIds = plots.Select(p => p.Id).ToHashSet();
        var buildingCount = state.Buildings.InAscendingOrder().Count(entry => plotIds.Contains(entry.Value.PlotId));
        var buildingDensity = totalCapacity <= 0 ? 0.0 : buildingCount / totalCapacity;

        // Phase 15 item 7's own real, live §3/§3.1 extension: the settlement's own share of Plots
        // carrying a Private Infrastructure Irrigation Canal or Well/Cistern, fed straight into
        // DroughtFamineExposure below (see that calculator's own doc comment). A pre-item-7 save (or
        // any settlement with no such structures) reads exactly 0.0, preserving every prior reading.
        var irrigatedPlotIds = state.IrrigationCanals.InAscendingOrder().Select(entry => entry.Key)
            .Concat(state.WellOrCisterns.InAscendingOrder().Select(entry => entry.Key))
            .ToHashSet();
        var irrigatedFraction = totalPlots == 0 ? 0.0 : plots.Count(p => irrigatedPlotIds.Contains(p.Id)) / (double)totalPlots;

        return new HazardExposureProfile(
            buildingDensity, riverAdjacentFraction, forestCoverFraction, coastalFraction, hillsFraction,
            DisasterCompoundingCalculator.IsDrySeasonMonth(date), DisasterCompoundingCalculator.IsStormSeasonMonth(date),
            irrigatedFraction);
    }

    /// <summary>This settlement's live Exposure score for one standing hazard (<see
    /// cref="HazardType.VolcanicEruption"/> excepted — it carries no standing Exposure score at all, per
    /// <see cref="DormantVolcano"/>'s own doc comment). This method, together with <see
    /// cref="HazardQueries.CurrentExposure"/>, is this item's entire realization of §3's "Forecast/
    /// knowledge" scope item: Exposure is already a real, continuously-legible number any caller can
    /// read at any time, satisfying §3's own "an emergent number... not a slider" framing directly — no
    /// separate Omen/forecast UI concept is built on top of it (Religion's own Omen skew toward a
    /// household's highest-Exposure hazard, §6.1, is named there as unchanged-in-substance cross-system
    /// wiring, which is Phase 14 item 5's own "goods, buildings, populations... events" integration
    /// wave, not this item's).</summary>
    public int ExposureFor(HazardType hazardType) => hazardType switch
    {
        HazardType.Fire => HazardExposureCalculator.FireExposure(
            BuildingDensity, DisasterCompoundingCalculator.DrySeasonFireExposureBonus(DrySeasonMonth)),
        HazardType.Flood => Math.Clamp(
            HazardExposureCalculator.FloodExposure(RiverAdjacentFraction, ForestCoverFraction) +
            DisasterCompoundingCalculator.StormSeasonExposureBonus(StormSeasonMonth), 0, 100),
        HazardType.Earthquake => HazardExposureCalculator.EarthquakeExposure(),
        HazardType.DroughtFamine => HazardExposureCalculator.DroughtFamineExposure(DrySeasonMonth, IrrigatedFraction),
        HazardType.Storm => Math.Clamp(
            HazardExposureCalculator.StormExposure(CoastalFraction) +
            DisasterCompoundingCalculator.StormSeasonExposureBonus(StormSeasonMonth), 0, 100),
        HazardType.Landslide => HazardExposureCalculator.LandslideExposure(HillsFraction, ForestCoverFraction),
        HazardType.BlightInfestation => HazardExposureCalculator.BlightInfestationExposure(),
        HazardType.Frost => HazardExposureCalculator.FrostExposure(),
        HazardType.VolcanicEruption => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(hazardType), hazardType, "Unhandled hazard type."),
    };
}
