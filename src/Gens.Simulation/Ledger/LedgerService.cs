using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Ledger;

/// <summary>
/// The single write path for posting a double-entry-style ledger transaction (Phase 8 item 1) —
/// every future wages/rents/taxes/debt system (Phase 8 item 6, out of this phase's scope) posts
/// through this one method rather than mutating <see cref="WorldState.LedgerAccounts"/> directly,
/// matching ADR 0006's "even AI/steward/migration writers go through the same handler" discipline
/// applied to the ledger specifically.
/// </summary>
public static class LedgerService
{
    /// <summary>
    /// Validates that <paramref name="postings"/> is a balanced double-entry-style set (at least two
    /// lines, summing to <see cref="Money.Zero"/>), applies each line to its <see
    /// cref="LedgerAccount"/> (creating the account at a zero balance on first use — accounts are
    /// sparse, matching <see cref="Buildings.EstateLookup"/>'s stockpile-provisioning convention),
    /// appends the resulting <see cref="LedgerTransaction"/> to the audit log, and returns the
    /// "always emitted" posting event. Throws <see cref="ArgumentException"/> for an unbalanced or
    /// under-specified posting set — the concrete mechanism behind "a transaction with unequal debits
    /// and credits is rejected."
    /// </summary>
    public static LedgerTransactionPostedEvent Post(
        WorldState state,
        GameDate date,
        LedgerTransactionCategory category,
        IReadOnlyList<LedgerPosting> postings,
        string? reference = null)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (postings is null)
            throw new ArgumentNullException(nameof(postings));
        if (postings.Count < 2)
            throw new ArgumentException(
                "A ledger transaction needs at least two postings (one debit, one credit).", nameof(postings));

        var sum = Money.Zero;
        foreach (var posting in postings)
            sum += posting.Amount;
        if (sum != Money.Zero)
            throw new ArgumentException(
                $"Ledger transaction postings must sum to zero (debits must equal credits); got {sum.ToDisplayString()}.",
                nameof(postings));

        foreach (var posting in postings)
        {
            var account = state.LedgerAccounts.TryGet(posting.Account, out var existing)
                ? existing
                : new LedgerAccount(posting.Account, Money.Zero);
            state.LedgerAccounts.Remove(posting.Account);
            state.LedgerAccounts.Add(posting.Account, account with { Balance = account.Balance + posting.Amount });
        }

        var transactionId = state.LedgerTransactionIds.Issue();
        var transaction = new LedgerTransaction(transactionId, date, category, postings, reference);
        state.LedgerTransactions.Add(transactionId, transaction);

        return new LedgerTransactionPostedEvent(state.EventIds.Issue(), date, transactionId, category, postings, reference);
    }
}

/// <summary>Emitted every time <see cref="LedgerService.Post"/> successfully posts a transaction —
/// ledger-ready groundwork for the Monthly Report (Phase 9), matching <see
/// cref="Buildings.ProductionResolvedEvent"/>'s identical "receipt of a state change" convention.</summary>
public sealed record LedgerTransactionPostedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<LedgerTransaction> TransactionId,
    LedgerTransactionCategory Category,
    IReadOnlyList<LedgerPosting> Postings,
    string? Reference) : IDomainEvent
{
    public string Type => "ledger.transactionPosted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => Postings
        .Select(posting => $"{posting.Account.Kind}:{posting.Account.OwnerId}")
        .Distinct(StringComparer.Ordinal)
        .ToArray();
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
