using Gens.Simulation.Economy;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;
using Gens.Simulation.Random;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Economy;

public sealed class StandingContractSystemTests
{
    [Test]
    public void AProvincialSupplyContractSellsFromTheHoldingIntoTheTreasuryAtAPremiumPrice()
    {
        var (state, settlementId) = EconomyTestFixtures.NewSettlement();
        var (holdingId, householdId) = EconomyTestFixtures.AddHouseholdHolding(state, settlementId);
        state.Stockpiles.TryGet(holdingId, out var stockpile);
        stockpile!.Add(EconomyTestFixtures.Grain(), 100);
        EconomyTestFixtures.SetGrainPrice(state, settlementId, Money.FromDenarii(2));

        StandingContractService.OpenProvincialSupplyContract(
            state, settlementId, householdId, holdingId, EconomyTestFixtures.GrainId,
            quantityPerMonth: 10, priceOverMarketFraction: Fixed64.FromRaw(200_000)); // +20%

        var events = new StandingContractSystem().Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        Assert.That(events, Has.Count.EqualTo(1));
        Assert.That(stockpile.Quantity, Is.EqualTo(90));

        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var household);
        // 10 units * (2.00 * 1.2) = 24.00 denarii.
        Assert.That(household!.Balance, Is.EqualTo(Money.FromDenarii(24)));
    }

    [Test]
    public void ATradeRouteInvestmentMovesMoneyOnceAtCommitmentAndTakesNoMonthlyAction()
    {
        var (state, settlementId) = EconomyTestFixtures.NewSettlement();
        var householdId = state.HouseholdIds.Issue();

        var contract = StandingContractService.CommitToTradeRoute(
            state, state.Date, settlementId, householdId, "Ostia-Alexandria Grain Route", Money.FromDenarii(200));

        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var household);
        Assert.That(household!.Balance, Is.EqualTo(Money.FromDenarii(-200)));

        var events = new StandingContractSystem().Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));
        Assert.That(events, Is.Empty);

        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var unchanged);
        Assert.That(unchanged!.Balance, Is.EqualTo(Money.FromDenarii(-200)));
        Assert.That(contract.Kind, Is.EqualTo(StandingContractKind.TradeRouteInvestment));
    }
}
