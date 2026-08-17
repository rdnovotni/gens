using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Time;

namespace Gens.Simulation.Economy;

/// <summary>
/// One household's aggregate wealth figure for one month (Phase 8 item 6; <c>gens-economy-finance-
/// design.md</c> §8's Net Worth: "combining Treasury balance, stored-goods value..., outstanding
/// DebtRecords..., and land/building value"). <see cref="TreasuryBalance"/>, <see
/// cref="StoredGoodsValue"/>, and <see cref="OutstandingDebt"/> are implemented here; livestock value
/// and land/building value are skipped — no livestock headcount or building-valuation figure exists
/// anywhere in this codebase yet (Estate &amp; Settlement's "construction-cost figures, depreciated by
/// neglect" that §8 calls for is future work), a documented simplification rather than the design
/// doc's full component list. <see cref="Band"/> is read, not stored redundantly from elsewhere — see
/// <see cref="HouseholdEconomyCalculator.BandFor"/>.
/// </summary>
public sealed record NetWorth
{
    public NetWorth(
        RuntimeId<Household> householdId,
        GameDate month,
        Money treasuryBalance,
        Money storedGoodsValue,
        Money outstandingDebt,
        Money total)
    {
        if (total != treasuryBalance + storedGoodsValue - outstandingDebt)
            throw new ArgumentException(
                "Total must equal TreasuryBalance + StoredGoodsValue - OutstandingDebt.", nameof(total));

        HouseholdId = householdId;
        Month = month;
        TreasuryBalance = treasuryBalance;
        StoredGoodsValue = storedGoodsValue;
        OutstandingDebt = outstandingDebt;
        Total = total;
    }

    public RuntimeId<Household> HouseholdId { get; }
    public GameDate Month { get; }
    public Money TreasuryBalance { get; }
    public Money StoredGoodsValue { get; }

    /// <summary>Sum of every active <see cref="DebtRecord.Principal"/> where this household is the
    /// debtor — "owed-to minus owed-by," per §8's own phrasing, collapsed to "owed-by only" since
    /// §6.2's player-as-lender posture is out of this phase's scope (<see cref="DebtRecord"/>'s own doc
    /// comment).</summary>
    public Money OutstandingDebt { get; }
    public Money Total { get; }
    public HouseholdWealthBand Band => HouseholdEconomyCalculator.BandFor(Total);
}
