using Gens.Simulation.Ledger;
using Gens.Simulation.State;

namespace Gens.Simulation.Economy;

/// <summary>
/// The sum of every <see cref="LedgerAccount.Balance"/>, across every <see cref="LedgerAccountKind"/>
/// this Phase 8 items 5-7 pass introduces new postings for (Household, Actor, SettlementTreasury,
/// System), must always be exactly <see cref="Money.Zero"/> — Phase 8 item 8's "total money
/// conservation across all new account kinds." This is implied by, but distinct in what it actually
/// asserts from, <see cref="LedgerAccountBalanceConsistencyInvariantCheck"/> (which checks each
/// account's balance against its own posting history) and <see
/// cref="LedgerTransactionBalanceInvariantCheck"/> (which checks each transaction sums to zero): a
/// writer could satisfy both of those and still, through some future bypass, leave the aggregate total
/// non-zero if an account's *key* itself were wrong (e.g. two different owners silently sharing one
/// key) — this check is the direct, no-recomputation-needed assertion of the actual conservation
/// property Phase 8's exit gate names, kept as its own explicit check for that reason.
/// </summary>
public sealed class EconomyTotalMoneySupplyInvariantCheck : IInvariantCheck
{
    public string Id => "economy.totalMoneySupplyIsZero";
    public bool IsFatal => true;

    public IEnumerable<InvariantViolation> Check(WorldState state)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var total = Money.Zero;
        foreach (var entry in state.LedgerAccounts.InAscendingOrder())
            total += entry.Value.Balance;

        if (total != Money.Zero)
            yield return new InvariantViolation(
                Id, $"Total balance across every Ledger account is {total.ToDisplayString()} instead of zero.", Array.Empty<string>());
    }
}

/// <summary>No <see cref="Goods.Stockpile"/> may hold a negative quantity of any good — Phase 8 item
/// 8's "no negative stockpile from a forced liquidation" (<see cref="InsolvencySystem"/>) or asset
/// seizure (<see cref="DebtSystem"/>). Defense in depth: <see cref="Goods.Stockpile.Quantity"/> is
/// always the sum of non-negative lot quantities by construction, and every consuming call
/// (<see cref="Goods.Stockpile.ConsumeAvailable"/>/<see cref="Goods.Stockpile.ConsumeReserved"/>)
/// already throws rather than allow an over-draw — this check exists for the same "guard what a
/// well-behaved writer already guarantees" reason as <see
/// cref="Markets.MarketNoNegativeStockOrPriceInvariantCheck"/>.</summary>
public sealed class EconomyStockpileNonNegativeInvariantCheck : IInvariantCheck
{
    public string Id => "economy.stockpileNeverNegative";
    public bool IsFatal => true;

    public IEnumerable<InvariantViolation> Check(WorldState state)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.Stockpiles.InAscendingOrder())
        {
            if (entry.Value.Quantity < 0)
                yield return new InvariantViolation(
                    Id, $"Stockpile for holding '{entry.Key.ToTaggedString()}' has negative quantity.",
                    new[] { entry.Key.ToTaggedString() });
        }
    }
}

/// <summary>Every <see cref="DebtRecord"/>'s own fields must stay internally consistent — Phase 8
/// item 8's "debt/insolvency state consistency": a <see cref="DebtStatus.Defaulted"/> record's
/// escalation history is monotonic (<see cref="DebtRecord.MonthsOverdue"/> at least the default
/// threshold, and once <see cref="DebtRecord.Resolution"/> is set it stays a recognized value),
/// <see cref="DebtRecord.Principal"/> and <see cref="DebtRecord.InterestRate"/> never go
/// negative.</summary>
public sealed class EconomyDebtRecordConsistencyInvariantCheck : IInvariantCheck
{
    public string Id => "economy.debtRecordConsistency";
    public bool IsFatal => true;

    public IEnumerable<InvariantViolation> Check(WorldState state)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.DebtRecords.InAscendingOrder())
        {
            var debt = entry.Value;
            var subject = new[] { debt.Id.ToTaggedString() };

            if (debt.Principal.RawValue < 0)
                yield return new InvariantViolation(Id, $"Debt '{debt.Id.ToTaggedString()}' has negative principal.", subject);
            if (debt.InterestRate.RawValue < 0)
                yield return new InvariantViolation(Id, $"Debt '{debt.Id.ToTaggedString()}' has a negative interest rate.", subject);
            if (debt.MonthsOverdue < 0)
                yield return new InvariantViolation(Id, $"Debt '{debt.Id.ToTaggedString()}' has negative months overdue.", subject);
            if (debt.Status == DebtStatus.Defaulted && debt.MonthsOverdue < HouseholdEconomyCalculator.DebtDefaultThresholdMonths)
                yield return new InvariantViolation(
                    Id, $"Debt '{debt.Id.ToTaggedString()}' is Defaulted with fewer months overdue than the default threshold.", subject);
            if (debt.Status != DebtStatus.Defaulted && debt.Resolution != DebtResolution.None)
                yield return new InvariantViolation(
                    Id, $"Debt '{debt.Id.ToTaggedString()}' has a resolution set without being Defaulted.", subject);
        }
    }
}

/// <summary>Every stored <see cref="NetWorth"/> must equal the sum of its own components — Phase 8
/// item 8's "net worth calculation matches its components." Defense in depth: <see
/// cref="NetWorth"/>'s own constructor already enforces this at construction time, matching <see
/// cref="Markets.SettlementMarket"/>'s identical "guard what a well-behaved writer already
/// guarantees" reasoning — this check only re-verifies a persisted record was never rebuilt some other
/// way.</summary>
public sealed class EconomyNetWorthConsistencyInvariantCheck : IInvariantCheck
{
    public string Id => "economy.netWorthMatchesComponents";
    public bool IsFatal => true;

    public IEnumerable<InvariantViolation> Check(WorldState state)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.NetWorthAssessments.InAscendingOrder())
        {
            var netWorth = entry.Value;
            var expected = netWorth.TreasuryBalance + netWorth.StoredGoodsValue - netWorth.OutstandingDebt;
            if (netWorth.Total != expected)
                yield return new InvariantViolation(
                    Id,
                    $"Net Worth for household '{entry.Key.ToTaggedString()}' total {netWorth.Total.ToDisplayString()} " +
                    $"does not match its components' sum {expected.ToDisplayString()}.",
                    new[] { entry.Key.ToTaggedString() });
        }
    }
}
