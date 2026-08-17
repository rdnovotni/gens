using Gens.Simulation.Commands;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Markets;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Economy;

/// <summary>
/// The monthly debt-servicing tick (Phase 8 item 6; <c>gens-economy-finance-design.md</c> §6.3's
/// default ladder, "when the household owes"). For every non-terminal <see cref="DebtRecord"/>:
///
/// 1. Compute this month's interest-only payment (<c>Principal * InterestRate</c>) and attempt to post
///    it, Household → settlement Treasury.
/// 2. If the household's current <see cref="LedgerAccountKind.Household"/> balance can cover it, the
///    payment posts, <see cref="DebtRecord.MonthsOverdue"/> resets to 0, and status returns to <see
///    cref="DebtStatus.Current"/>.
/// 3. Otherwise the payment is skipped (§6.3 rung 1, "interest escalation"): <see
///    cref="DebtRecord.MonthsOverdue"/> increments, the rate is scaled up by <see
///    cref="HouseholdEconomyCalculator.DebtInterestEscalationFactor"/>, and status becomes <see
///    cref="DebtStatus.Overdue"/>.
/// 4. Once <see cref="DebtRecord.MonthsOverdue"/> reaches <see
///    cref="HouseholdEconomyCalculator.DebtDefaultThresholdMonths"/>, status becomes <see
///    cref="DebtStatus.Defaulted"/> and — once, the first month it defaults — this system seizes and
///    liquidates a fraction of the debtor's own Holding stock (§6.3 rung 3, "asset seizure") to pay
///    down principal, then stops: <see cref="DebtResolution.AssetSeizure"/> is the ladder's terminal
///    rung here.
///
/// **Deliberately not implemented: §6.3 rungs 2 and 4.** Rung 2 ("Legal exposure... a real dispute with
/// a ruling") needs Legal &amp; Court, which does not exist — this system goes straight from sustained
/// default to seizure rather than gating it behind an unbuilt court case. Rung 4 ("Debt bondage for
/// free household members") needs both Legal &amp; Court's ruling and Legal Status modeling this
/// codebase's <see cref="Characters.Character"/> already carries but this system never reads or
/// mutates — bonding a household member is a person-level consequence this ledger-and-stockpile-only
/// system has no business triggering unilaterally. Both are explicit Phase 8 scope boundaries, not
/// oversights, per this document's own scoping instructions to skip anything gated on an unbuilt
/// system.
/// </summary>
public sealed class DebtSystem : IMonthlySystem<WorldState>
{
    public string Id => "economy.debt";
    public TickPhase Phase => TickPhase.MarketsLedger;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "ledgerAccounts", "holdings", "stockpiles", "marketPrices" };
    public IReadOnlyCollection<string> Writes { get; } =
        new[] { "ledgerAccounts", "ledgerTransactions", "ledgerTransactionIds", "stockpiles", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } =
        new[] { "economy.householdPurchasing", "economy.wages", "economy.rentAndTax" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // DebtRecords is keyed by RuntimeId<DebtRecord> (creation order, ADR 0004) — already
        // deterministic without a further sort.
        foreach (var entry in state.DebtRecords.InAscendingOrder().ToArray())
        {
            var debt = entry.Value;
            if (debt.Status is DebtStatus.Forgiven)
                continue;
            if (debt.Status == DebtStatus.Defaulted && debt.Resolution != DebtResolution.None)
                continue; // Ladder already resolved — no further monthly action.

            var household = LedgerAccountKey.ForHousehold(debt.DebtorHouseholdId);
            var treasury = LedgerAccountKey.ForSettlementTreasury(debt.SettlementId);
            var payment = debt.Principal.Scale(debt.InterestRate);

            var currentBalance = state.LedgerAccounts.TryGet(household, out var account) ? account!.Balance : Money.Zero;

            if (payment == Money.Zero || currentBalance >= payment)
            {
                if (payment != Money.Zero)
                {
                    var paymentEvent = LedgerService.Post(
                        state, context.Date, LedgerTransactionCategory.Debt,
                        new[] { new LedgerPosting(household, -payment), new LedgerPosting(treasury, payment) },
                        reference: $"debtPayment:{debt.Id.ToTaggedString()}");
                    events.Add(paymentEvent);
                }

                debt = debt with { MonthsOverdue = 0, Status = DebtStatus.Current };
            }
            else
            {
                var escalatedRate = Fixed64.Multiply(debt.InterestRate, HouseholdEconomyCalculator.DebtInterestEscalationFactor);
                var monthsOverdue = debt.MonthsOverdue + 1;
                var status = monthsOverdue >= HouseholdEconomyCalculator.DebtDefaultThresholdMonths
                    ? DebtStatus.Defaulted
                    : DebtStatus.Overdue;
                debt = debt with { MonthsOverdue = monthsOverdue, InterestRate = escalatedRate, Status = status };

                if (status == DebtStatus.Defaulted)
                {
                    var (resolvedDebt, seizureEvents) = SeizeAssets(state, debt, context.Date);
                    debt = resolvedDebt;
                    events.AddRange(seizureEvents);
                }
            }

            state.DebtRecords.Remove(entry.Key);
            state.DebtRecords.Add(entry.Key, debt);
        }

        return events;
    }

    /// <summary>§6.3 rung 3: liquidates <see
    /// cref="HouseholdEconomyCalculator.DebtAssetSeizureFraction"/> of the debtor's Holding stock (the
    /// first good found with a known settlement market price, in ascending <see cref="Good"/>-ID order
    /// for determinism) toward the debt, crediting the settlement Treasury and reducing <see
    /// cref="DebtRecord.Principal"/> by the seized value. Marks <see
    /// cref="DebtResolution.AssetSeizure"/> whether or not the seizure fully covers the remaining
    /// principal — a debtor with no seizable assets simply keeps an unresolved, uncollectable debt,
    /// consistent with debt bondage (the design doc's own next rung) being out of this phase's scope.</summary>
    private static (DebtRecord Debt, IReadOnlyList<IDomainEvent> Events) SeizeAssets(WorldState state, DebtRecord debt, GameDate date)
    {
        var events = new List<IDomainEvent>();

        RuntimeId<Holding>? holdingId = null;
        foreach (var holdingEntry in state.Holdings.InAscendingOrder())
        {
            if (!HouseholdEconomyCalculator.TryGetOwningHousehold(holdingEntry.Value, out var owner) || owner != debt.DebtorHouseholdId)
                continue;
            holdingId = holdingEntry.Key;
            break;
        }

        if (holdingId is null || !state.Stockpiles.TryGet(holdingId.Value, out var stockpile))
            return (debt with { Resolution = DebtResolution.AssetSeizure }, events);

        DefinitionId<Good>? seizedGood = null;
        long seizedQuantity = 0;
        Money seizedValue = Money.Zero;

        foreach (var lot in stockpile.Lots.Select(l => l.Good.Id).Distinct().OrderBy(id => id.Value, StringComparer.Ordinal))
        {
            if (!state.MarketPrices.TryGet(new MarketGoodKey(debt.SettlementId, lot), out var market))
                continue;

            var available = stockpile.AvailableQuantityOf(lot);
            var quantity = HouseholdEconomyCalculator.ApplyFraction(available, HouseholdEconomyCalculator.DebtAssetSeizureFraction);
            if (quantity <= 0)
                continue;

            stockpile.ConsumeAvailable(lot, quantity);
            seizedGood = lot;
            seizedQuantity = quantity;
            seizedValue = HouseholdEconomyCalculator.MultiplyByQuantity(market.Price, quantity);
            break;
        }

        var appliedValue = seizedValue >= debt.Principal ? debt.Principal : seizedValue;
        var resolvedPrincipal = seizedValue >= debt.Principal ? Money.Zero : debt.Principal - seizedValue;
        var resolvedDebt = debt with { Principal = resolvedPrincipal, Resolution = DebtResolution.AssetSeizure };

        // Seized goods repay the debt directly (a barter-style settlement, not a cash sale): the
        // Stockpile.ConsumeAvailable above already moved the physical goods out of the debtor's
        // Holding, and DebtRecord.Principal above already reflects the reduction. No LedgerService.Post
        // is needed for the value itself — goods are not a Ledger-tracked asset class in this codebase
        // (only Money accounts are), so there is no Money account on either side of this exchange for a
        // balanced transaction to move between; forcing one would either double-count the seizure as a
        // cash sale that never happened, or net to a no-op pair of postings that cancel out. Still
        // emitted as its own event, though, so the seizure remains visible to the Monthly Report.
        if (seizedGood is not null && appliedValue != Money.Zero)
        {
            events.Add(new DebtAssetSeizedEvent(
                state.EventIds.Issue(), date, debt.Id, debt.DebtorHouseholdId, seizedGood.Value, seizedQuantity, appliedValue));
        }

        return (resolvedDebt, events);
    }
}

/// <summary>Emitted once, the month a <see cref="DebtRecord"/> first reaches <see
/// cref="DebtResolution.AssetSeizure"/> and seizable stock was actually found — see <see
/// cref="DebtSystem.SeizeAssets"/>'s own doc comment for why no <see cref="Ledger.LedgerTransactionPostedEvent"/>
/// accompanies it.</summary>
public sealed record DebtAssetSeizedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<DebtRecord> DebtId,
    RuntimeId<Household> HouseholdId,
    DefinitionId<Good> GoodId,
    long Quantity,
    Money Value) : IDomainEvent
{
    public string Type => "economy.debtAssetSeized";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), DebtId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
