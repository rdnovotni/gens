using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Markets;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Tests.Economy;

/// <summary>Shared scenario builders for the Phase 8 items 5-8 economy system tests, mirroring <see
/// cref="Gens.Simulation.Tests.Buildings.EstateChainFixtures"/>'s "authored fixtures, no content
/// catalog loader" convention.</summary>
internal static class EconomyTestFixtures
{
    public static readonly DefinitionId<Good> GrainId = new("grain");

    public static (WorldState State, RuntimeId<Settlement> SettlementId) NewSettlement()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        return (state, settlementId);
    }

    /// <summary>Adds a Holding owned by a fresh household, with a Stockpile of the given capacity.</summary>
    public static (RuntimeId<Holding> HoldingId, RuntimeId<Household> HouseholdId) AddHouseholdHolding(
        WorldState state, RuntimeId<Settlement> settlementId, int residentCapacity = 4, long stockpileCapacity = 1_000)
    {
        var householdId = state.HouseholdIds.Issue();
        var holdingId = state.HoldingIds.Issue();
        state.Holdings.Add(holdingId, Holding.Create(
            holdingId, settlementId, ownerId: householdId.ToTaggedString(), residentCapacity: residentCapacity));
        state.Stockpiles.Add(holdingId, new Stockpile(stockpileCapacity));
        return (holdingId, householdId);
    }

    public static void SetGrainPrice(WorldState state, RuntimeId<Settlement> settlementId, Money price, long supply = 0, long demand = 0)
    {
        var key = new MarketGoodKey(settlementId, GrainId);
        var market = new SettlementMarket(settlementId, GrainId, price, price, supply, demand, Math.Min(supply, demand), Math.Max(0, demand - supply));
        state.MarketPrices.Remove(key);
        state.MarketPrices.Add(key, market);
    }

    public static GoodDefinition Grain() => new(GrainId, Perishability.NonPerishable);
}
