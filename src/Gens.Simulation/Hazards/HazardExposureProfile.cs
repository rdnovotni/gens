using Gens.Simulation.Buildings;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.PrivateInfrastructure;
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
        // carrying a still-operational Private Infrastructure Irrigation Canal or Well/Cistern, fed
        // straight into DroughtFamineExposure below (see that calculator's own doc comment). A
        // pre-item-7 save (or any settlement with no such structures) reads exactly 0.0, preserving
        // every prior reading.
        //
        // Per direct review finding, this can't simply be a boolean "does this Plot have any
        // irrigation" fraction: DroughtFamineExposure multiplies IrrigatedFraction straight against
        // IrrigationCanalDroughtExposureReduction (the Canal's own, largest reduction), so a flat
        // coverage fraction would credit a Well or Cistern with the Canal's full 40% reduction instead
        // of its own lower 18%/26% figure, and would keep crediting a structure whose condition has
        // lapsed below MinimumOperationalCondition. Each Plot instead contributes its own operational
        // structure's reduction as a fraction of the Canal's own reduction — 1.0 for an operational
        // Canal, a smaller ratio for an operational Well/Cistern, 0.0 for a lapsed or absent one — so
        // the averaged result, once re-multiplied by the Canal's reduction below, reproduces each
        // Plot's real, type-specific, condition-gated contribution.
        var canalMaxReduction = (double)PrivateInfrastructureCatalog.IrrigationCanalDroughtExposureReduction.RawValue;
        var irrigationWeight = 0.0;
        foreach (var plot in plots)
        {
            if (state.IrrigationCanals.TryGet(plot.Id, out var canal) &&
                InfrastructureConditionResolver.IsOperational(state, canal!.ConditionKey))
            {
                irrigationWeight += 1.0;
            }
            else if (state.WellOrCisterns.TryGet(plot.Id, out var wellOrCistern) &&
                InfrastructureConditionResolver.IsOperational(state, wellOrCistern!.ConditionKey))
            {
                var reduction = wellOrCistern.Type == WellOrCisternType.Well
                    ? PrivateInfrastructureCatalog.WellDroughtExposureReduction
                    : PrivateInfrastructureCatalog.CisternDroughtExposureReduction;
                irrigationWeight += reduction.RawValue / canalMaxReduction;
            }
        }
        var irrigatedFraction = totalPlots == 0 ? 0.0 : irrigationWeight / totalPlots;

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
