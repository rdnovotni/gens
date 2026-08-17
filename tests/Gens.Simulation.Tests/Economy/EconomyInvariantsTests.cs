using Gens.Simulation.Economy;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Economy;

public sealed class EconomyInvariantsTests
{
    [Test]
    public void TotalMoneySupplyPassesWhenEveryTransactionIsBalanced()
    {
        var (state, settlementId) = EconomyTestFixtures.NewSettlement();
        var householdId = state.HouseholdIds.Issue();
        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(30)),
                new LedgerPosting(LedgerAccountKey.ForSettlementTreasury(settlementId), Money.FromDenarii(-30)),
            });

        var violations = new EconomyTotalMoneySupplyInvariantCheck().Check(state);
        Assert.That(violations, Is.Empty);
    }

    [Test]
    public void TotalMoneySupplyFailsWhenAnAccountBalanceIsMutatedOutsideTheLedgerService()
    {
        var (state, _) = EconomyTestFixtures.NewSettlement();
        var householdId = state.HouseholdIds.Issue();
        var key = LedgerAccountKey.ForHousehold(householdId);
        state.LedgerAccounts.Add(key, new LedgerAccount(key, Money.FromDenarii(10)));

        var violations = new EconomyTotalMoneySupplyInvariantCheck().Check(state);
        Assert.That(violations, Is.Not.Empty);
    }

    [Test]
    public void StockpileNonNegativePassesForAnOrdinaryStockpile()
    {
        var (state, settlementId) = EconomyTestFixtures.NewSettlement();
        var (holdingId, _) = EconomyTestFixtures.AddHouseholdHolding(state, settlementId);
        state.Stockpiles.TryGet(holdingId, out var stockpile);
        stockpile!.Add(EconomyTestFixtures.Grain(), 5);

        var violations = new EconomyStockpileNonNegativeInvariantCheck().Check(state);
        Assert.That(violations, Is.Empty);
    }

    [Test]
    public void DebtRecordConsistencyFlagsADefaultedRecordBelowTheThresholdMonthsOverdue()
    {
        var (state, settlementId) = EconomyTestFixtures.NewSettlement();
        var householdId = state.HouseholdIds.Issue();
        var debtId = state.DebtRecordIds.Issue();
        var invalid = new DebtRecord(
            debtId, settlementId, householdId, Money.FromDenarii(10), Fixed64.Zero,
            DebtOrigin.Loan, isFenusNauticum: false, monthsOverdue: 0, status: DebtStatus.Current, resolution: DebtResolution.None)
            with
        { Status = DebtStatus.Defaulted, MonthsOverdue = 0 };
        state.DebtRecords.Add(debtId, invalid);

        var violations = new EconomyDebtRecordConsistencyInvariantCheck().Check(state);
        Assert.That(violations, Is.Not.Empty);
    }

    [Test]
    public void NetWorthConsistencyPassesWhenTotalMatchesItsComponents()
    {
        var (state, _) = EconomyTestFixtures.NewSettlement();
        var householdId = state.HouseholdIds.Issue();
        var netWorth = new NetWorth(householdId, new GameDate(0), Money.FromDenarii(10), Money.FromDenarii(5), Money.FromDenarii(3), Money.FromDenarii(12));
        state.NetWorthAssessments.Add(householdId, netWorth);

        var violations = new EconomyNetWorthConsistencyInvariantCheck().Check(state);
        Assert.That(violations, Is.Empty);
    }
}
