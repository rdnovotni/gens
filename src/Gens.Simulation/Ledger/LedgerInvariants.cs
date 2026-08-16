using Gens.Simulation.State;

namespace Gens.Simulation.Ledger;

/// <summary>
/// Every posted <see cref="LedgerTransaction"/>'s postings must sum to <see cref="Money.Zero"/> —
/// defense in depth for Phase 8's "every ledger transaction's debits equal its credits" invariant,
/// since <see cref="LedgerService.Post"/> already rejects an unbalanced posting set at write time.
/// This check exists for the same reason <see
/// cref="Characters.RelationshipReferentialIntegrityCheck"/> re-validates state that a well-behaved
/// writer already guarantees: a future writer that bypasses <see cref="LedgerService"/> should fail
/// loudly here rather than silently corrupting the campaign's money supply.
/// </summary>
public sealed class LedgerTransactionBalanceInvariantCheck : IInvariantCheck
{
    public string Id => "ledger.transactionsBalance";
    public bool IsFatal => true;

    public IEnumerable<InvariantViolation> Check(WorldState state)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.LedgerTransactions.InAscendingOrder())
        {
            var transaction = entry.Value;
            var sum = Money.Zero;
            foreach (var posting in transaction.Postings)
                sum += posting.Amount;

            if (sum != Money.Zero)
                yield return new InvariantViolation(
                    Id,
                    $"Ledger transaction '{transaction.Id.ToTaggedString()}' postings sum to " +
                    $"{sum.ToDisplayString()} instead of zero.",
                    new[] { transaction.Id.ToTaggedString() });
        }
    }
}

/// <summary>
/// Every <see cref="LedgerAccount"/>'s stored balance must equal that account's postings folded from
/// the full <see cref="LedgerTransaction"/> log — Phase 8's "total money is conserved across all
/// accounts" invariant, expressed as "the accounts are exactly what the transaction log says they
/// are" rather than as a separate running total, since <see
/// cref="LedgerTransactionBalanceInvariantCheck"/> already guarantees each individual transaction
/// nets to zero (so the sum of every account's balance is constant transaction-over-transaction by
/// construction; this check instead catches an account whose balance drifted from its own postings,
/// e.g. a future bypass of <see cref="LedgerService.Post"/>).
/// </summary>
public sealed class LedgerAccountBalanceConsistencyInvariantCheck : IInvariantCheck
{
    public string Id => "ledger.accountBalancesMatchTransactionLog";
    public bool IsFatal => true;

    public IEnumerable<InvariantViolation> Check(WorldState state)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        // A plain Dictionary accumulator is safe here even though ADR 0004 forbids iterating one for
        // anything event/hash/RNG/save-facing: Money addition is commutative and this method never
        // iterates the dictionary itself, only looks values up by key while walking the already-
        // ordered LedgerAccounts registry below.
        var recomputed = new Dictionary<LedgerAccountKey, Money>();
        foreach (var entry in state.LedgerTransactions.InAscendingOrder())
        {
            foreach (var posting in entry.Value.Postings)
            {
                recomputed.TryGetValue(posting.Account, out var running);
                recomputed[posting.Account] = running + posting.Amount;
            }
        }

        foreach (var entry in state.LedgerAccounts.InAscendingOrder())
        {
            var expected = recomputed.TryGetValue(entry.Key, out var found) ? found : Money.Zero;
            if (entry.Value.Balance != expected)
                yield return new InvariantViolation(
                    Id,
                    $"Ledger account '{entry.Key.Kind}:{entry.Key.OwnerId}' balance " +
                    $"{entry.Value.Balance.ToDisplayString()} does not match the transaction log's " +
                    $"{expected.ToDisplayString()}.",
                    new[] { entry.Key.OwnerId });
        }
    }
}
