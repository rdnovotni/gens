using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Markets;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Markets;

public sealed class MarketInvariantsTests
{
    private static readonly DefinitionId<Good> GrainId = new("grain");

    private static (WorldState State, RuntimeId<Settlement> SettlementId) BuildState()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        return (state, settlementId);
    }

    [Test]
    public void NoNegativeStockOrPriceCheckPassesForAnOrdinaryClearedMarket()
    {
        var (state, settlementId) = BuildState();
        state.MarketPrices.Add(
            new MarketGoodKey(settlementId, GrainId),
            new SettlementMarket(settlementId, GrainId, Money.FromDenarii(10), Money.FromDenarii(10), 5, 5, 5, 0));

        var violations = new MarketNoNegativeStockOrPriceInvariantCheck().Check(state).ToArray();
        Assert.That(violations, Is.Empty);
    }

    [Test]
    public void BoundedPriceChangeCheckPassesWhenTheMoveStaysWithinBound()
    {
        var (state, settlementId) = BuildState();
        var config = new MarketPriceBoundConfig(Fixed64.FromRaw(150_000), Money.FromMinorUnits(1));
        // 10 -> 11.5 is exactly a +15% move, at the edge of the bound.
        state.MarketPrices.Add(
            new MarketGoodKey(settlementId, GrainId),
            new SettlementMarket(settlementId, GrainId, Money.FromDenarii(11) + Money.FromMinorUnits(50), Money.FromDenarii(10), 5, 8, 5, 3));

        var violations = new MarketBoundedPriceChangeInvariantCheck(config).Check(state).ToArray();
        Assert.That(violations, Is.Empty);
    }

    [Test]
    public void BoundedPriceChangeCheckFlagsAMoveThatExceedsTheConfiguredBound()
    {
        var (state, settlementId) = BuildState();
        var config = new MarketPriceBoundConfig(Fixed64.FromRaw(150_000), Money.FromMinorUnits(1));
        // 10 -> 20 is a +100% move, far outside +/-15%, and not equal to the configured floor either.
        state.MarketPrices.Add(
            new MarketGoodKey(settlementId, GrainId),
            new SettlementMarket(settlementId, GrainId, Money.FromDenarii(20), Money.FromDenarii(10), 1, 100, 1, 99));

        var violations = new MarketBoundedPriceChangeInvariantCheck(config).Check(state).ToArray();
        Assert.That(violations, Has.Length.EqualTo(1));
    }

    [Test]
    public void BoundedPriceChangeCheckAllowsTheFloorPriceEvenWhenItIsOutsideTheOrdinaryBound()
    {
        var (state, settlementId) = BuildState();
        var config = new MarketPriceBoundConfig(Fixed64.FromRaw(150_000), Money.FromDenarii(9));
        // The floor (9) sits below what a +/-15% move from 10 would otherwise allow (8.5 to 11.5) —
        // this is the one deliberate override the check must accept rather than flag.
        state.MarketPrices.Add(
            new MarketGoodKey(settlementId, GrainId),
            new SettlementMarket(settlementId, GrainId, Money.FromDenarii(9), Money.FromDenarii(10), 100, 1, 1, 0));

        var violations = new MarketBoundedPriceChangeInvariantCheck(config).Check(state).ToArray();
        Assert.That(violations, Is.Empty);
    }

    [Test]
    public void SettlementMarketConstructorRejectsAnInternallyInconsistentUnsatisfiedDemand()
    {
        Assert.That(
            () => new SettlementMarket(
                RuntimeId<Settlement>.Parse("settlement_0000000"), GrainId,
                Money.FromDenarii(10), Money.FromDenarii(10), supply: 5, demand: 10, clearedQuantity: 5, unsatisfiedDemand: 999),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }
}
