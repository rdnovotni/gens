using Gens.Simulation.Economy;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Economy;

public sealed class HouseholdPurchasingSystemTests
{
    [Test]
    public void ABuyerShortOfNeedPurchasesFromASellerWithSurplusAtTheClearedPrice()
    {
        var (state, settlementId) = EconomyTestFixtures.NewSettlement();
        var (sellerHoldingId, sellerHouseholdId) = EconomyTestFixtures.AddHouseholdHolding(state, settlementId, residentCapacity: 1);
        var (buyerHoldingId, buyerHouseholdId) = EconomyTestFixtures.AddHouseholdHolding(state, settlementId, residentCapacity: 4);

        state.Stockpiles.TryGet(sellerHoldingId, out var sellerStock);
        sellerStock!.Add(EconomyTestFixtures.Grain(), 50);
        EconomyTestFixtures.SetGrainPrice(state, settlementId, Money.FromDenarii(2));

        var system = new HouseholdPurchasingSystem();
        var events = system.Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        Assert.That(events, Is.Not.Empty);
        state.Stockpiles.TryGet(buyerHoldingId, out var buyerStock);
        // DietTier.Adequate need for residentCapacity 4 is 2*4 = 8 (NeedsConsumptionCalculator).
        Assert.That(buyerStock!.QuantityOf(EconomyTestFixtures.GrainId), Is.EqualTo(8));

        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(buyerHouseholdId), out var buyerAccount);
        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(sellerHouseholdId), out var sellerAccount);
        Assert.That(buyerAccount!.Balance, Is.EqualTo(Money.FromDenarii(-16)));
        Assert.That(sellerAccount!.Balance, Is.EqualTo(Money.FromDenarii(16)));
    }

    [Test]
    public void NoTradeHappensWithoutAClearedMarketPrice()
    {
        var (state, settlementId) = EconomyTestFixtures.NewSettlement();
        var (sellerHoldingId, _) = EconomyTestFixtures.AddHouseholdHolding(state, settlementId, residentCapacity: 1);
        var (buyerHoldingId, _) = EconomyTestFixtures.AddHouseholdHolding(state, settlementId, residentCapacity: 4);
        state.Stockpiles.TryGet(sellerHoldingId, out var sellerStock);
        sellerStock!.Add(EconomyTestFixtures.Grain(), 50);

        var events = new HouseholdPurchasingSystem().Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        Assert.That(events, Is.Empty);
        state.Stockpiles.TryGet(buyerHoldingId, out var buyerStock);
        Assert.That(buyerStock!.Quantity, Is.EqualTo(0));
    }

    [Test]
    public void ASellerNeverSellsBelowItsOwnNeed()
    {
        var (state, settlementId) = EconomyTestFixtures.NewSettlement();
        var (sellerHoldingId, _) = EconomyTestFixtures.AddHouseholdHolding(state, settlementId, residentCapacity: 1);
        var (buyerHoldingId, _) = EconomyTestFixtures.AddHouseholdHolding(state, settlementId, residentCapacity: 4);
        state.Stockpiles.TryGet(sellerHoldingId, out var sellerStock);
        // Need for residentCapacity 1 is 2; stocking exactly 2 leaves zero surplus.
        sellerStock!.Add(EconomyTestFixtures.Grain(), 2);
        EconomyTestFixtures.SetGrainPrice(state, settlementId, Money.FromDenarii(2));

        new HouseholdPurchasingSystem().Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        state.Stockpiles.TryGet(buyerHoldingId, out var buyerStock);
        Assert.That(buyerStock!.Quantity, Is.EqualTo(0));
        Assert.That(sellerStock.Quantity, Is.EqualTo(2));
    }
}
