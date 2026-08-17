using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Economy;

/// <summary>Where a <see cref="DebtRecord"/> came from (§6.5's "several doors, one mechanism") — a
/// subset of <c>gens-economy-finance-design.md</c> §12's <c>DebtRecord.origin</c> vocabulary; this
/// implementation only ever opens a debt through <see cref="Loan"/> (via <see
/// cref="DebtService.IssueLoan"/>) for now, but the other origins are named so a future rent/tax-arrears
/// trigger can reuse this record shape without a save-breaking enum change.</summary>
public enum DebtOrigin
{
    Loan,
    RentArrears,
    TaxArrears,
    ContractAdvance,
}

/// <summary>A <see cref="DebtRecord"/>'s position on §6.3's default ladder.</summary>
public enum DebtStatus
{
    Current,
    Overdue,
    Defaulted,

    /// <summary>§7.1's fenus nauticum: "loss forgives the debt... never feeds §6.4's default ladder."
    /// Not reachable by any system in this implementation (no shipping/voyage mechanic exists yet to
    /// trigger it) — named so <see cref="DebtRecord.IsFenusNauticum"/> has a real terminal state to
    /// reach once one does.</summary>
    Forgiven,
}

/// <summary>How a <see cref="DebtStatus.Defaulted"/> debt was resolved (§6.3 rungs 2-4, collapsed —
/// see <see cref="DebtSystem"/>'s own doc comment for the Legal &amp; Court and debt-bondage rungs this
/// implementation deliberately skips).</summary>
public enum DebtResolution
{
    None,
    AssetSeizure,
}

/// <summary>
/// A standing loan (Phase 8 item 6; <c>gens-economy-finance-design.md</c> §6.1's "principal plus an
/// accruing interest rate, tracked on the Ledger as a standing <c>DebtRecord</c>"). Interest-only and
/// non-amortizing — the monthly obligation is always <c>Principal * InterestRate</c>, and <see
/// cref="Principal"/> itself only ever changes on an <see cref="DebtResolution.AssetSeizure"/>
/// resolution — a deliberate simplification of §6's fuller amortization-free model appropriate for a
/// first debt slice. The counterparty is always the debtor's own settlement Treasury (the Argentaria
/// operator §6.1 names has no building/character record of its own yet); <see
/// cref="LenderIsPlayer"/> always reads <c>false</c> in this implementation — §6.2's "player as lender"
/// posture needs a non-Treasury counterparty this codebase doesn't model yet, and is out of Phase 8's
/// scope per this document's own scoping instructions.
/// </summary>
public sealed record DebtRecord
{
    public DebtRecord(
        RuntimeId<DebtRecord> id,
        RuntimeId<Settlement> settlementId,
        RuntimeId<Household> debtorHouseholdId,
        Money principal,
        Fixed64 interestRate,
        DebtOrigin origin,
        bool isFenusNauticum,
        int monthsOverdue,
        DebtStatus status,
        DebtResolution resolution)
    {
        if (principal.RawValue < 0)
            throw new ArgumentOutOfRangeException(nameof(principal), principal, "Principal cannot be negative.");
        if (interestRate < Fixed64.Zero)
            throw new ArgumentOutOfRangeException(nameof(interestRate), interestRate, "Interest rate cannot be negative.");
        if (monthsOverdue < 0)
            throw new ArgumentOutOfRangeException(nameof(monthsOverdue), monthsOverdue, "Months overdue cannot be negative.");
        if (status == DebtStatus.Defaulted && resolution == DebtResolution.None && monthsOverdue < HouseholdEconomyCalculator.DebtDefaultThresholdMonths)
            throw new ArgumentOutOfRangeException(
                nameof(monthsOverdue), monthsOverdue, "A defaulted debt must be overdue at least the default threshold.");

        Id = id;
        SettlementId = settlementId;
        DebtorHouseholdId = debtorHouseholdId;
        Principal = principal;
        InterestRate = interestRate;
        Origin = origin;
        IsFenusNauticum = isFenusNauticum;
        MonthsOverdue = monthsOverdue;
        Status = status;
        Resolution = resolution;
    }

    public RuntimeId<DebtRecord> Id { get; }
    public RuntimeId<Settlement> SettlementId { get; }
    public RuntimeId<Household> DebtorHouseholdId { get; }
    public Money Principal { get; init; }
    public Fixed64 InterestRate { get; init; }
    public DebtOrigin Origin { get; }
    public bool IsFenusNauticum { get; }
    public int MonthsOverdue { get; init; }
    public DebtStatus Status { get; init; }
    public DebtResolution Resolution { get; init; }

    /// <summary>§6.2's "player as lender" posture is out of scope (see this record's own doc comment);
    /// always <c>false</c> for every debt this implementation opens.</summary>
    public static bool LenderIsPlayer => false;
}

/// <summary>Opens new <see cref="DebtRecord"/>s (§6.1's "Borrowing"). Not a monthly system — matching
/// how Phase 9's not-yet-built actions/commands layer would trigger a new loan (a Reserve-Threshold
/// auto-borrow, a disaster-recovery loan, a player action), this is a plain service method a future
/// command or test harness calls directly, in the same spirit as <see
/// cref="Markets.MarketClearingCalculator"/> being pure math a system calls into.</summary>
public static class DebtService
{
    public static DebtRecord IssueLoan(
        WorldState state,
        GameDate date,
        RuntimeId<Settlement> settlementId,
        RuntimeId<Household> debtorHouseholdId,
        Money principal,
        Fixed64? interestRate = null,
        DebtOrigin origin = DebtOrigin.Loan,
        bool isFenusNauticum = false)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (principal.RawValue <= 0)
            throw new ArgumentOutOfRangeException(nameof(principal), principal, "A loan's principal must be positive.");

        var id = state.DebtRecordIds.Issue();
        var record = new DebtRecord(
            id,
            settlementId,
            debtorHouseholdId,
            principal,
            interestRate ?? HouseholdEconomyCalculator.InitialDebtInterestRate,
            origin,
            isFenusNauticum,
            monthsOverdue: 0,
            status: DebtStatus.Current,
            resolution: DebtResolution.None);
        state.DebtRecords.Add(id, record);

        // The loan's proceeds actually move: the settlement Treasury (standing in for the Argentaria,
        // per this record's own doc comment) pays the principal to the debtor's household.
        LedgerService.Post(
            state, date, LedgerTransactionCategory.Debt,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(debtorHouseholdId), principal),
                new LedgerPosting(LedgerAccountKey.ForSettlementTreasury(settlementId), -principal),
            },
            reference: $"loanIssued:{id.ToTaggedString()}");

        return record;
    }
}
