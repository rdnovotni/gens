using Gens.Simulation.Buildings;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;

namespace Gens.Simulation.Characters;

/// <summary>Pure background-job-capacity math (<c>gens-settlement-demographics-design.md</c> §4.1),
/// factored out of <see cref="JobCapacitySystem"/> so <see cref="EmploymentMatchingSystem"/> (Phase 7
/// item 3) can read the exact same per-settlement, per-<see cref="PopGroupType"/> slot counts without
/// re-deriving them from <see cref="PopGroup.EmploymentRatio"/>'s rounded ratio.</summary>
public static class BackgroundJobCapacityCalculator
{
    /// <summary>Curiales capacity as a fraction of total settlement population, in basis points (parts
    /// per 1,000), rising with <see cref="SettlementStage"/> — a modest, growing pool of upward-mobility
    /// slots as a settlement matures, standing in for "Dignitas/stage" until Dignitas exists.</summary>
    private static readonly Dictionary<SettlementStage, int> CurialesBasisPoints =
        new Dictionary<SettlementStage, int>
        {
            [SettlementStage.Villa] = 20,
            [SettlementStage.Vicus] = 30,
            [SettlementStage.Town] = 50,
            [SettlementStage.City] = 80,
        };

    public static Dictionary<RuntimeId<Settlement>, Dictionary<BuildingSector, int>> SumBuildingCapacityBySettlement(WorldState state)
    {
        var result = new Dictionary<RuntimeId<Settlement>, Dictionary<BuildingSector, int>>();

        foreach (var entry in state.Buildings.InAscendingOrder())
        {
            var building = entry.Value;
            if (building.Condition == BuildingCondition.Ruined)
                continue;
            if (building.Definition.Sector == BuildingSector.None || building.Definition.BackgroundJobCapacity == 0)
                continue;
            if (!state.Plots.TryGet(building.PlotId, out var plot))
                continue;

            if (!result.TryGetValue(plot.SettlementId, out var bySector))
            {
                bySector = new Dictionary<BuildingSector, int>();
                result[plot.SettlementId] = bySector;
            }

            AddCapacity(bySector, building.Definition.Sector, building.Definition.BackgroundJobCapacity);
        }

        return result;
    }

    /// <summary>The full per-<see cref="PopGroupType"/> capacity table for one settlement (§4.1):
    /// every sector-driven group gets an entry, even at zero, plus Curiales on top.</summary>
    public static Dictionary<PopGroupType, int> ComputeCapacityByGroup(
        IReadOnlyDictionary<BuildingSector, int>? sectorCapacity, int totalPopulation, SettlementStage stage)
    {
        var capacityByGroup = new Dictionary<PopGroupType, int>
        {
            [PopGroupType.Coloni] = 0,
            [PopGroupType.Operarii] = 0,
            [PopGroupType.Opifices] = 0,
            [PopGroupType.Negotiatores] = 0,
            [PopGroupType.Aeditui] = 0,
        };

        if (sectorCapacity is not null)
        {
            foreach (var (sector, slots) in sectorCapacity)
            {
                if (PrimaryGroupFor(sector) is { } primaryGroup)
                    AddCapacity(capacityByGroup, primaryGroup, slots);
                if (sector == BuildingSector.Commerce)
                    AddCapacity(capacityByGroup, PopGroupType.Operarii, slots);
            }
        }

        capacityByGroup[PopGroupType.Curiales] = CurialesCapacity(totalPopulation, stage);
        return capacityByGroup;
    }

    private static void AddCapacity<TKey>(Dictionary<TKey, int> capacity, TKey key, int slots) where TKey : notnull
    {
        capacity.TryGetValue(key, out var existing);
        capacity[key] = existing + slots;
    }

    private static PopGroupType? PrimaryGroupFor(BuildingSector sector) => sector switch
    {
        BuildingSector.Agriculture => PopGroupType.Coloni,
        BuildingSector.Industry => PopGroupType.Opifices,
        BuildingSector.Commerce => PopGroupType.Negotiatores,
        BuildingSector.Religion => PopGroupType.Aeditui,
        _ => null,
    };

    private static int CurialesCapacity(int totalPopulation, SettlementStage stage) =>
        (int)((long)totalPopulation * CurialesBasisPoints[stage] / 1000);
}
