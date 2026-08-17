namespace Gens.Simulation.Ledger;

/// <summary>
/// One line of a <see cref="LedgerTransaction"/>: a signed <see cref="Money"/> movement against one
/// <see cref="LedgerAccountKey"/>. Positive credits (increases) the account's balance; negative debits
/// (decreases) it — <see cref="LedgerService.Post"/> requires every transaction's postings to sum to
/// <see cref="Money.Zero"/>, which is this codebase's "double-entry-style" balance rule (Phase 8 item
/// 1): money leaving one account is always accounted for by money entering another, the <see
/// cref="LedgerAccountKey.Mint"/> system account included when a transaction is a genuine external
/// injection or removal rather than a transfer between two campaign-owned accounts.
/// </summary>
public readonly record struct LedgerPosting(LedgerAccountKey Account, Money Amount);
