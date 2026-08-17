namespace Gens.Simulation.Ledger;

/// <summary>
/// What a <see cref="LedgerAccount"/> represents (Phase 8 items 1-2's "household and actor ledgers"
/// plus the accounts the roadmap's transaction-type list implies — a treasury needs somewhere to
/// post to). Fixed and code-defined, matching every other small closed vocabulary in this codebase
/// (e.g. <see cref="Characters.WealthBand"/>).
/// </summary>
public enum LedgerAccountKind
{
    /// <summary>A household's own ledger (Phase 8 item 1) — the concrete stand-in for
    /// <c>gens-economy-finance-design.md</c> §2's per-settlement Treasury until Settlement itself
    /// carries an owning household.</summary>
    Household,

    /// <summary>An individual Actor's own ledger (Phase 8 item 1) — a Companion or staff member's
    /// personal funds, distinct from the household they serve (e.g. a wage they've been paid but not
    /// yet folded into the household's own account).</summary>
    Actor,

    /// <summary>A settlement-level Treasury (<c>gens-economy-finance-design.md</c> §2), for
    /// settlement-scoped income/expense categories (tribute, tax revenue) that do not belong to any
    /// single household or actor.</summary>
    SettlementTreasury,

    /// <summary>An external/system account — the explicit, named conservation boundary Phase 8's
    /// "total money is conserved... no value created/destroyed outside a defined mint/external
    /// injection point" requirement calls for, rather than an untracked leak. Used for campaign
    /// bootstrap seeding and any future minting (<c>gens-economy-finance-design.md</c> §3.5).</summary>
    System,
}
