using Gens.Simulation.Buildings;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;

namespace Gens.Simulation.Characters;

/// <summary>Pure housing/land-capacity math (<c>gens-settlement-demographics-design.md</c> §7),
/// factored out of <see cref="HousingCapacitySystem"/> the same way <see
/// cref="BackgroundJobCapacityCalculator"/> is factored out of <see cref="JobCapacitySystem"/>.</summary>
public static class HousingCapacityCalculator
{
    /// <summary>How many residents one unclaimed Fertile-Plain plot supports (§7.2). No numeric
    /// figure exists anywhere in the design corpus for this — this implementation's own invented
    /// baseline, in the same spirit as <see cref="EmploymentMatchingSystem"/>'s wage table, chosen so
    /// a handful of unclaimed plots meaningfully move a small Coloni/Veterans population's <see
    /// cref="PopGroup.HousingSatisfaction"/> rather than requiring hundreds of plots to matter.</summary>
    public const int ResidentsPerUnclaimedFertilePlot = 6;

    /// <summary>The five urban groups §7.1 names as sharing Insulae/Domus housing stock.</summary>
    public static readonly IReadOnlyList<PopGroupType> UrbanGroups = new[]
    {
        PopGroupType.Operarii, PopGroupType.Opifices, PopGroupType.Negotiatores,
        PopGroupType.Aeditui, PopGroupType.Curiales,
    };

    /// <summary>The two rural groups §7.2 names as sharing unclaimed-fertile-plot capacity.</summary>
    public static readonly IReadOnlyList<PopGroupType> RuralGroups = new[] { PopGroupType.Coloni, PopGroupType.Veterans };

    public static Dictionary<RuntimeId<Settlement>, int> SumResidentialCapacityBySettlement(WorldState state)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var result = new Dictionary<RuntimeId<Settlement>, int>();
        foreach (var entry in state.Buildings.InAscendingOrder())
        {
            var building = entry.Value;
            if (building.Condition == BuildingCondition.Ruined || building.Definition.ResidentialCapacity == 0)
                continue;
            if (!state.Plots.TryGet(building.PlotId, out var plot))
                continue;

            result.TryGetValue(plot.SettlementId, out var existing);
            result[plot.SettlementId] = existing + building.Definition.ResidentialCapacity;
        }

        return result;
    }

    /// <summary>Unclaimed (unowned, unoccupied), Fertile-Plain plot count for one settlement, times
    /// <see cref="ResidentsPerUnclaimedFertilePlot"/> (§7.2). "Unclaimed" means neither owned (<see
    /// cref="Plot.OwnerId"/> is <see langword="null"/>) nor already occupied by a Holding (<see
    /// cref="Plot.OccupyingHoldingId"/> is <see langword="null"/>) — matching §7.2's "the player's own
    /// Estate &amp; Settlement buildings haven't already occupied" carve-out; a plot the player owns
    /// but hasn't built on yet still competes with this rural population for the same land, per that
    /// section's own "real, felt tension" framing.</summary>
    public static int ComputeRuralCapacity(WorldState state, RuntimeId<Settlement> settlementId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var unclaimedFertilePlots = 0;
        foreach (var entry in state.Plots.InAscendingOrder())
        {
            var plot = entry.Value;
            if (plot.SettlementId != settlementId)
                continue;
            if (plot.Terrain != TerrainType.FertilePlain)
                continue;
            if (plot.OwnerId is not null || plot.OccupyingHoldingId is not null)
                continue;
            unclaimedFertilePlots++;
        }

        return unclaimedFertilePlots * ResidentsPerUnclaimedFertilePlot;
    }

    /// <summary>A shared capacity pool's occupancy ratio (§7.1/§7.2: every group sharing one pool sees
    /// the same <see cref="PopGroup.HousingSatisfaction"/>, since neither Insula-vs-Domus skew nor a
    /// per-group land carve-out exists) — <c>capacity / totalSharingPopulation</c>, or <see
    /// cref="Fixed64.One"/> when nobody shares the pool (matching <see
    /// cref="BackgroundJobCapacityCalculator"/>'s and <see cref="JobCapacitySystem"/>'s identical
    /// "size zero reads as fully satisfied, not starved" convention).</summary>
    public static Fixed64 SharedSatisfaction(int capacity, int totalSharingPopulation) =>
        totalSharingPopulation > 0
            ? Fixed64.Divide(Fixed64.FromInt(capacity), Fixed64.FromInt(totalSharingPopulation))
            : Fixed64.One;
}
