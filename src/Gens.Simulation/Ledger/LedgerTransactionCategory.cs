namespace Gens.Simulation.Ledger;

/// <summary>
/// Every ledger transaction type Phase 8 item 2 names: "accounts and transaction types for treasury,
/// wages, sales, purchases, taxes, upkeep, construction, debt, gifts, contracts, and transfers." This
/// is the roadmap's own shorter list, adapted from — and each value traceable to —
/// <c>gens-economy-finance-design.md</c> §12's finer-grained <c>LedgerEntry.category</c> vocabulary
/// (noted per value below); a future Phase 8 item 5-8 pass may split a category further (e.g. <see
/// cref="Taxes"/> into inbound/outbound) without renumbering the values already shipped, per ADR
/// 0011's additive-only migration policy.
/// </summary>
public enum LedgerTransactionCategory
{
    /// <summary>Direct treasury movements not covered by a more specific category below — seeding,
    /// windfalls, minting/seigniorage (design doc's <c>windfall</c>, <c>seigniorage</c>).</summary>
    Treasury,

    /// <summary>Staff and hired-labor pay (design doc's <c>wages</c>).</summary>
    Wages,

    /// <summary>Goods, rent, and contract-adjacent income the household or settlement receives from
    /// selling something (design doc's <c>goodsSale</c>, <c>rentAgricultural</c>, <c>rentUrban</c>).</summary>
    Sales,

    /// <summary>Goods bought from a market or another party, and general capital acquisition (design
    /// doc's <c>capitalExpenditure</c>).</summary>
    Purchases,

    /// <summary>Inbound or outbound taxation (design doc's <c>taxRevenue</c>, <c>tributum</c>).</summary>
    Taxes,

    /// <summary>Recurring building/holding upkeep (design doc's <c>upkeep</c>).</summary>
    Upkeep,

    /// <summary>Construction-related spend (design doc's <c>capitalExpenditure</c> when the acquisition
    /// is a building; kept as its own category per the roadmap's explicit "construction" transaction
    /// type rather than folded into <see cref="Purchases"/>).</summary>
    Construction,

    /// <summary>Loan principal, interest paid/received, and default resolution (design doc's
    /// <c>loanInterestPaid</c>, <c>loanInterestReceived</c>).</summary>
    Debt,

    /// <summary>Bribes, funded actions, dowries, and other non-reciprocal transfers with a specific
    /// social/political purpose (design doc's <c>bribe</c>, <c>fundedAction</c>).</summary>
    Gifts,

    /// <summary>Standing market contracts (military supply, concessions, provincial supply — design
    /// doc's <c>contractMilitary</c>, <c>contractConcession</c>, <c>contractProvincial</c>). Phase 8
    /// item 7 (out of this phase's scope) is what will actually populate these; the category exists
    /// now so item 7 does not need a save-breaking enum change to use it.</summary>
    Contracts,

    /// <summary>A plain balance transfer between two accounts with no other category attached (e.g.
    /// moving funds from an Actor's personal account into their household's).</summary>
    Transfers,
}
