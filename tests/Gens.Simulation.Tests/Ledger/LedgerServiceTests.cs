using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Ledger;

public sealed class LedgerServiceTests
{
    [Test]
    public void PostAppliesEachPostingToItsAccountAndCreatesMissingAccountsAtZero()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var household = LedgerAccountKey.ForHousehold(householdId);

        LedgerService.Post(
            state,
            new GameDate(0),
            LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(household, Money.FromDenarii(100)),
                new LedgerPosting(LedgerAccountKey.Mint, Money.FromDenarii(-100)),
            });

        Assert.Multiple(() =>
        {
            Assert.That(state.LedgerAccounts.TryGet(household, out var householdAccount), Is.True);
            Assert.That(householdAccount!.Balance, Is.EqualTo(Money.FromDenarii(100)));
            Assert.That(state.LedgerAccounts.TryGet(LedgerAccountKey.Mint, out var mintAccount), Is.True);
            Assert.That(mintAccount!.Balance, Is.EqualTo(Money.FromDenarii(-100)));
        });
    }

    [Test]
    public void PostAppendsATransactionRecordAndEmitsAPostedEvent()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var household = LedgerAccountKey.ForHousehold(householdId);

        var evt = LedgerService.Post(
            state,
            new GameDate(3),
            LedgerTransactionCategory.Wages,
            new[]
            {
                new LedgerPosting(household, Money.FromDenarii(5)),
                new LedgerPosting(LedgerAccountKey.Mint, Money.FromDenarii(-5)),
            },
            reference: "test-wages");

        Assert.Multiple(() =>
        {
            Assert.That(state.LedgerTransactions.TryGet(evt.TransactionId, out var transaction), Is.True);
            Assert.That(transaction!.Category, Is.EqualTo(LedgerTransactionCategory.Wages));
            Assert.That(transaction.Reference, Is.EqualTo("test-wages"));
            Assert.That(evt.Type, Is.EqualTo("ledger.transactionPosted"));
            Assert.That(evt.SubjectIds, Does.Contain($"Household:{householdId.ToTaggedString()}"));
        });
    }

    [Test]
    public void PostAccumulatesRepeatedPostingsToTheSameAccountWithinOneTransaction()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var household = LedgerAccountKey.ForHousehold(householdId);

        LedgerService.Post(
            state,
            new GameDate(0),
            LedgerTransactionCategory.Transfers,
            new[]
            {
                new LedgerPosting(household, Money.FromDenarii(3)),
                new LedgerPosting(household, Money.FromDenarii(2)),
                new LedgerPosting(LedgerAccountKey.Mint, Money.FromDenarii(-5)),
            });

        state.LedgerAccounts.TryGet(household, out var account);
        Assert.That(account!.Balance, Is.EqualTo(Money.FromDenarii(5)));
    }

    [Test]
    public void PostRejectsUnbalancedPostings()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var household = LedgerAccountKey.ForHousehold(householdId);

        Assert.That(
            () => LedgerService.Post(
                state,
                new GameDate(0),
                LedgerTransactionCategory.Treasury,
                new[]
                {
                    new LedgerPosting(household, Money.FromDenarii(100)),
                    new LedgerPosting(LedgerAccountKey.Mint, Money.FromDenarii(-99)),
                }),
            Throws.ArgumentException);

        Assert.That(state.LedgerTransactions.Count, Is.EqualTo(0));
        Assert.That(state.LedgerAccounts.Count, Is.EqualTo(0));
    }

    [Test]
    public void PostRejectsFewerThanTwoPostings()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var household = LedgerAccountKey.ForHousehold(householdId);

        Assert.That(
            () => LedgerService.Post(
                state, new GameDate(0), LedgerTransactionCategory.Treasury,
                new[] { new LedgerPosting(household, Money.Zero) }),
            Throws.ArgumentException);
    }

    [Test]
    public void SequentialTransactionsProduceIdenticalAccountBalancesAcrossRepeatedRuns()
    {
        (WorldState State, LedgerAccountKey Household) RunScenario()
        {
            var state = new WorldState(new GameDate(0));
            var householdId = state.HouseholdIds.Issue();
            var household = LedgerAccountKey.ForHousehold(householdId);

            for (var i = 1; i <= 5; i++)
            {
                LedgerService.Post(
                    state, new GameDate(i), LedgerTransactionCategory.Sales,
                    new[]
                    {
                        new LedgerPosting(household, Money.FromDenarii(i)),
                        new LedgerPosting(LedgerAccountKey.Mint, Money.FromDenarii(-i)),
                    });
            }

            return (state, household);
        }

        var (firstState, firstHousehold) = RunScenario();
        var (secondState, secondHousehold) = RunScenario();

        firstState.LedgerAccounts.TryGet(firstHousehold, out var firstAccount);
        secondState.LedgerAccounts.TryGet(secondHousehold, out var secondAccount);

        Assert.That(firstAccount!.Balance, Is.EqualTo(secondAccount!.Balance));
        Assert.That(firstAccount.Balance, Is.EqualTo(Money.FromDenarii(15)));
    }
}
