using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;

namespace Gens.Simulation.PurchasingPower;

/// <summary>
/// §3's/§9's <c>AggregateDemandReading</c> data model — a settlement's own total demand as a direct,
/// derived reading of how many residents sit in each of §2's own three <see cref="WealthBand"/> tiers,
/// weighted toward the top (<see cref="PurchasingPowerCatalog"/>). Settlement-scoped, not District-
/// scoped: <see cref="Characters.PopGroupKey"/> carries no District attribution at all (confirmed by
/// direct search of that key's own two-field shape — the identical gap <see
/// cref="PublicWorks.PublicWorksCatalog.SewerContentmentBonus"/>'s own doc comment already names), so
/// this item reads the same settlement-wide reading everywhere a District needs one, matching that
/// precedent exactly rather than fabricating a per-District population split this codebase has never
/// modeled. Sourced entirely from <see cref="PopGroup"/>'s own already-tracked <see cref="PopGroup.Size"/>
/// and <see cref="PopGroup.WealthBand"/> — the real population this document's own §2 asks for, with no
/// Notable Households contribution folded in: no <c>Notable Households</c> domain exists anywhere in
/// this codebase (confirmed by direct search — only its own design doc does, the same gap Phase 15 item
/// 4's own <see cref="NotableBusinesses.NotableBusiness"/> doc comments already name), and the closest
/// real analog this codebase tracks — <see cref="Actors.LivingWorldActor.NetWorth"/>'s <see
/// cref="Economy.HouseholdWealthBand"/> for a handful of named Rival Gens households — counts households,
/// not the per-person population figures this reading is built from, and mixing the two units would need
/// an unspecified per-household population-equivalent allocation rule this item does not invent. A real,
/// disclosed scope cut, left open exactly like §10's own "whether Modest Surplus should itself be split
/// further" question.
/// </summary>
public sealed record AggregateDemandReading(
    RuntimeId<Settlement> SettlementId,
    int SubsistencePopulation,
    int ModestSurplusPopulation,
    int EliteDiscretionaryPopulation,
    Fixed64 SubsistenceWeight,
    Fixed64 ModestSurplusWeight,
    Fixed64 EliteDiscretionaryWeight,
    Fixed64 TotalDemandIndex);

public static class AggregateDemandResolver
{
    public static bool TryGetCurrent(WorldState state, RuntimeId<Settlement> settlementId, out AggregateDemandReading reading) =>
        state.AggregateDemandReadings.TryGet(settlementId, out reading!);
}

/// <summary>
/// §3's own monthly demand-aggregation tick, matching <see
/// cref="BusinessCompetition.MarketSaturationSystem"/>'s established static <c>Tick(state)</c>
/// convention (no central <see cref="Time.IMonthlySystem{TState}"/> pipeline registry exists
/// anywhere in this codebase for any Phase 15 system to register into — the same gap every prior Phase
/// 15 item's own progress note already names). For every settlement carrying at least one non-empty
/// <see cref="PopGroup"/>: sums population and per-capita-weighted demand by <see cref="WealthBand"/>
/// (<see cref="PurchasingPowerCalculator.ComputeAggregateDemand"/>) and records the result, replacing
/// any prior reading. A settlement with no PopGroups at all is simply never given a reading rather than
/// a zero-population placeholder — <see cref="AggregateDemandResolver.TryGetCurrent"/>'s absence is
/// itself the honest "no data yet" signal every downstream reader (District Property Value, Market
/// Capacity, Business Viability) already treats as neutral.
/// </summary>
public static class AggregatePurchasingPowerSystem
{
    public static void Tick(WorldState state)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var bySettlement = new Dictionary<RuntimeId<Settlement>, (int Subsistence, int Modest, int Elite)>();

        foreach (var entry in state.PopGroups.InAscendingOrder())
        {
            if (entry.Value.Size <= 0)
                continue;

            var current = bySettlement.TryGetValue(entry.Key.SettlementId, out var existing) ? existing : (0, 0, 0);
            current = entry.Value.WealthBand switch
            {
                WealthBand.Subsistence => (current.Item1 + entry.Value.Size, current.Item2, current.Item3),
                WealthBand.ModestSurplus => (current.Item1, current.Item2 + entry.Value.Size, current.Item3),
                WealthBand.EliteDiscretionary => (current.Item1, current.Item2, current.Item3 + entry.Value.Size),
                _ => current,
            };
            bySettlement[entry.Key.SettlementId] = current;
        }

        foreach (var settlementId in bySettlement.Keys.OrderBy(id => id.Value).ToArray())
        {
            var (subsistence, modest, elite) = bySettlement[settlementId];
            var reading = PurchasingPowerCalculator.ComputeAggregateDemand(settlementId, subsistence, modest, elite);

            if (state.AggregateDemandReadings.TryGet(settlementId, out _))
                state.AggregateDemandReadings.Remove(settlementId);
            state.AggregateDemandReadings.Add(settlementId, reading);
        }
    }
}
