using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Ledger;

/// <summary>
/// One posted, immutable double-entry-style transaction (Phase 8 item 1) — the audit-log record <see
/// cref="LedgerService.Post"/> appends to <see cref="State.WorldState.LedgerTransactions"/> once its
/// <see cref="Postings"/> are validated to sum to <see cref="Money.Zero"/>. Never mutated or removed
/// once posted, matching <c>gens-economy-finance-design.md</c> §12's <c>LedgerEntry</c> being an
/// append-only monthly record.
/// </summary>
public sealed record LedgerTransaction(
    RuntimeId<LedgerTransaction> Id,
    GameDate OccurredDate,
    LedgerTransactionCategory Category,
    IReadOnlyList<LedgerPosting> Postings,
    string? Reference);
