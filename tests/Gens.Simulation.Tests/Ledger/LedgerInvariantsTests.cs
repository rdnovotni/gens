using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Ledger;

public sealed class LedgerInvariantsTests
{
    [Test]
    public void TransactionBalanceCheckPassesForEveryTransactionPostedThroughLedgerService()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var household = LedgerAccountKey.ForHousehold(householdId);

        LedgerService.Post(
            state, new GameDate(0), LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(household, Money.FromDenarii(20)),
                new LedgerPosting(LedgerAccountKey.Mint, Money.FromDenarii(-20)),
            });

        var violations = new LedgerTransactionBalanceInvariantCheck().Check(state).ToArray();
        Assert.That(violations, Is.Empty);
    }

    [Test]
    public void TransactionBalanceCheckFlagsATransactionWhoseStoredPostingsDoNotSumToZero()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var household = LedgerAccountKey.ForHousehold(householdId);

        // Directly append a malformed transaction, bypassing LedgerService.Post, to exercise the
        // invariant's defense-in-depth role.
        var transactionId = state.LedgerTransactionIds.Issue();
        state.LedgerTransactions.Add(transactionId, new LedgerTransaction(
            transactionId,
            new GameDate(0),
            LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(household, Money.FromDenarii(20)),
                new LedgerPosting(LedgerAccountKey.Mint, Money.FromDenarii(-19)),
            },
            null));

        var violations = new LedgerTransactionBalanceInvariantCheck().Check(state).ToArray();
        Assert.That(violations, Has.Length.EqualTo(1));
    }

    [Test]
    public void AccountBalanceConsistencyCheckPassesAfterOrdinaryPosting()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var household = LedgerAccountKey.ForHousehold(householdId);

        LedgerService.Post(
            state, new GameDate(0), LedgerTransactionCategory.Wages,
            new[]
            {
                new LedgerPosting(household, Money.FromDenarii(8)),
                new LedgerPosting(LedgerAccountKey.Mint, Money.FromDenarii(-8)),
            });

        var violations = new LedgerAccountBalanceConsistencyInvariantCheck().Check(state).ToArray();
        Assert.That(violations, Is.Empty);
    }

    [Test]
    public void AccountBalanceConsistencyCheckFlagsAnAccountThatDriftedFromTheTransactionLog()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var household = LedgerAccountKey.ForHousehold(householdId);

        LedgerService.Post(
            state, new GameDate(0), LedgerTransactionCategory.Wages,
            new[]
            {
                new LedgerPosting(household, Money.FromDenarii(8)),
                new LedgerPosting(LedgerAccountKey.Mint, Money.FromDenarii(-8)),
            });

        // Corrupt the stored balance directly, bypassing LedgerService.Post.
        state.LedgerAccounts.TryGet(household, out var account);
        state.LedgerAccounts.Remove(household);
        state.LedgerAccounts.Add(household, account! with { Balance = Money.FromDenarii(999) });

        var violations = new LedgerAccountBalanceConsistencyInvariantCheck().Check(state).ToArray();
        Assert.That(violations, Has.Length.EqualTo(1));
    }

    [Test]
    public void TotalMoneyIsConservedAcrossManyPostedTransactions()
    {
        var state = new WorldState(new GameDate(0));
        var householdA = LedgerAccountKey.ForHousehold(state.HouseholdIds.Issue());
        var householdB = LedgerAccountKey.ForHousehold(state.HouseholdIds.Issue());

        // Seed both accounts from the system mint, then shuffle money between them.
        LedgerService.Post(
            state, new GameDate(0), LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(householdA, Money.FromDenarii(100)),
                new LedgerPosting(LedgerAccountKey.Mint, Money.FromDenarii(-100)),
            });
        LedgerService.Post(
            state, new GameDate(1), LedgerTransactionCategory.Transfers,
            new[]
            {
                new LedgerPosting(householdA, Money.FromDenarii(-30)),
                new LedgerPosting(householdB, Money.FromDenarii(30)),
            });
        LedgerService.Post(
            state, new GameDate(2), LedgerTransactionCategory.Transfers,
            new[]
            {
                new LedgerPosting(householdB, Money.FromDenarii(-10)),
                new LedgerPosting(householdA, Money.FromDenarii(10)),
            });

        var total = Money.Zero;
        foreach (var entry in state.LedgerAccounts.InAscendingOrder())
            total += entry.Value.Balance;

        // Every account nets to zero against the mint's own balance, so the grand total across every
        // account (mint included) is exactly zero — no value was created or destroyed outside the
        // mint's own explicit injection.
        Assert.That(total, Is.EqualTo(Money.Zero));
        Assert.That(new LedgerAccountBalanceConsistencyInvariantCheck().Check(state).ToArray(), Is.Empty);
    }
}
