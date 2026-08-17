using Gens.Simulation.Economy;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Economy;

public sealed class HouseholdStatementSystemTests
{
    [Test]
    public void AStatementSummarizesThisMonthsIncomeAndExpensesForEveryActiveHousehold()
    {
        var (state, settlementId) = EconomyTestFixtures.NewSettlement();
        var householdId = state.HouseholdIds.Issue();
        var household = LedgerAccountKey.ForHousehold(householdId);
        var treasury = LedgerAccountKey.ForSettlementTreasury(settlementId);

        LedgerService.Post(state, state.Date, LedgerTransactionCategory.Sales,
            new[] { new LedgerPosting(household, Money.FromDenarii(20)), new LedgerPosting(treasury, Money.FromDenarii(-20)) });
        LedgerService.Post(state, state.Date, LedgerTransactionCategory.Taxes,
            new[] { new LedgerPosting(household, Money.FromDenarii(-5)), new LedgerPosting(treasury, Money.FromDenarii(5)) });

        var events = new HouseholdStatementSystem().Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        Assert.That(events, Has.Count.EqualTo(1));
        state.HouseholdStatements.TryGet(householdId, out var statement);
        Assert.Multiple(() =>
        {
            Assert.That(statement!.Income, Is.EqualTo(Money.FromDenarii(20)));
            Assert.That(statement.Expenses, Is.EqualTo(Money.FromDenarii(5)));
            Assert.That(statement.Net, Is.EqualTo(Money.FromDenarii(15)));
        });
    }

    [Test]
    public void OnlyThisMonthsPostingsAreCounted()
    {
        var (state, settlementId) = EconomyTestFixtures.NewSettlement();
        var householdId = state.HouseholdIds.Issue();
        var household = LedgerAccountKey.ForHousehold(householdId);
        var treasury = LedgerAccountKey.ForSettlementTreasury(settlementId);

        LedgerService.Post(state, state.Date, LedgerTransactionCategory.Sales,
            new[] { new LedgerPosting(household, Money.FromDenarii(20)), new LedgerPosting(treasury, Money.FromDenarii(-20)) });
        state.AdvanceMonth();

        var events = new HouseholdStatementSystem().Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        Assert.That(events, Is.Empty);
        Assert.That(state.HouseholdStatements.Count, Is.EqualTo(0));
    }
}
