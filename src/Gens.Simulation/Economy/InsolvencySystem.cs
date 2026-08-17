using Gens.Simulation.Commands;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Markets;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Villas;

namespace Gens.Simulation.Economy;

/// <summary>
/// The monthly Net Worth / Insolvency tick (Phase 8 item 6; <c>gens-economy-finance-design.md</c> §8's
/// Net Worth and §9's Insolvency ladder). Considers exactly one Holding per household — the first
/// found in ascending Holding-ID order whose owner resolves to that household — as that household's
/// full estate for valuation purposes; a household that owns more than one Holding only has the first
/// one counted, a documented simplification (this codebase has no "household's full property list"
/// index yet to sum across).
///
/// **Net Worth** (§8): Treasury balance + stored-goods value (that one Holding's Stockpile, valued at
/// each good's current cleared <see cref="SettlementMarket.Price"/> where known, 0 otherwise) minus
/// outstanding <see cref="DebtRecord.Principal"/>. Livestock and land/building value are not modeled —
/// see <see cref="NetWorth"/>'s own doc comment.
///
/// **Insolvency** (§9): a household accumulates <see cref="InsolvencyState.MonthsBelowThreshold"/>
/// while Net Worth stays below <see cref="HouseholdEconomyCalculator.InsolvencyNetWorthFloor"/>
/// (resetting to 0 the moment it isn't), and <see cref="InsolvencyStage"/> escalates on fixed month
/// thresholds. At <see cref="InsolvencyStage.Insolvent"/> or worse, this system forcibly liquidates a
/// fraction of that Holding's stock into the settlement Treasury each month, at a below-market rate
/// (§9 rung 1). At <see cref="InsolvencyStage.Ruined"/> specifically, if that Holding carries a <see
/// cref="Holding.Villa"/> above <see cref="VillaStage.Rustica"/>, its stage is forced back one step
/// (§9 rung 3; <see cref="Villa.DemoteStage"/>).
///
/// **Deliberately not implemented: §9 rungs 2, 4, and 5.** Rung 2 ("Forced asset sale" of a plot or
/// building) needs a demolition/repurposing mechanic this codebase's Estate &amp; Settlement layer
/// hasn't built yet. Rung 4 ("Loss of qualifying office or census standing") is explicitly gated on
/// Politics &amp; Patronage, which does not exist. Rung 5 (a Dynasty Chronicle entry) needs the
/// Dynasty Chronicle system, also unbuilt. All three are named directly in this document's own scoping
/// instructions as systems to skip rather than half-build.
/// </summary>
public sealed class InsolvencySystem : IMonthlySystem<WorldState>
{
    public string Id => "economy.insolvency";
    public TickPhase Phase => TickPhase.MarketsLedger;
    public IReadOnlyCollection<string> Reads { get; } =
        new[] { "ledgerAccounts", "holdings", "stockpiles", "marketPrices" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "ledgerAccounts", "stockpiles", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } =
        new[] { "economy.householdPurchasing", "economy.wages", "economy.rentAndTax", "economy.debt" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var processed = new HashSet<RuntimeId<Household>>();

        foreach (var holdingEntry in state.Holdings.InAscendingOrder())
        {
            var holding = holdingEntry.Value;
            if (!HouseholdEconomyCalculator.TryGetOwningHousehold(holding, out var householdId))
                continue;
            if (!processed.Add(householdId))
                continue;

            var treasuryBalance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account)
                ? account!.Balance
                : Money.Zero;

            var storedGoodsValue = Money.Zero;
            if (state.Stockpiles.TryGet(holdingEntry.Key, out var stockpile))
            {
                foreach (var goodId in stockpile.Lots.Select(l => l.Good.Id).Distinct())
                {
                    if (!state.MarketPrices.TryGet(new MarketGoodKey(holding.SettlementId, goodId), out var market))
                        continue;
                    storedGoodsValue += HouseholdEconomyCalculator.MultiplyByQuantity(market.Price, stockpile.QuantityOf(goodId));
                }
            }

            var outstandingDebt = Money.Zero;
            foreach (var debtEntry in state.DebtRecords.InAscendingOrder())
            {
                var debt = debtEntry.Value;
                if (debt.DebtorHouseholdId == householdId && debt.Status != DebtStatus.Forgiven)
                    outstandingDebt += debt.Principal;
            }

            var total = treasuryBalance + storedGoodsValue - outstandingDebt;
            var netWorth = new NetWorth(householdId, context.Date, treasuryBalance, storedGoodsValue, outstandingDebt, total);
            state.NetWorthAssessments.Remove(householdId);
            state.NetWorthAssessments.Add(householdId, netWorth);
            events.Add(new NetWorthAssessedEvent(state.EventIds.Issue(), context.Date, householdId, total, netWorth.Band));

            var priorMonthsBelow = state.InsolvencyStates.TryGet(householdId, out var priorState) ? priorState!.MonthsBelowThreshold : 0;
            var priorConsequences = priorState?.ConsequencesApplied ?? Array.Empty<InsolvencyConsequence>();

            var monthsBelow = total < HouseholdEconomyCalculator.InsolvencyNetWorthFloor ? priorMonthsBelow + 1 : 0;
            var stage = StageFor(monthsBelow);
            var consequences = new List<InsolvencyConsequence>(priorConsequences);

            if (stage is InsolvencyStage.Insolvent or InsolvencyStage.Ruined && stockpile is not null)
            {
                var liquidated = TryForcedLiquidation(state, holding, stockpile, context.Date);
                if (liquidated && !consequences.Contains(InsolvencyConsequence.ForcedLiquidation))
                    consequences.Add(InsolvencyConsequence.ForcedLiquidation);
            }

            if (stage == InsolvencyStage.Ruined && holding.Villa is { Stage: > VillaStage.Rustica } villa)
            {
                villa.DemoteStage();
                if (!consequences.Contains(InsolvencyConsequence.VillaStageDemotion))
                    consequences.Add(InsolvencyConsequence.VillaStageDemotion);
            }

            var insolvencyState = new InsolvencyState(householdId, monthsBelow, stage, consequences);
            state.InsolvencyStates.Remove(householdId);
            state.InsolvencyStates.Add(householdId, insolvencyState);

            if (stage != InsolvencyStage.Solvent)
                events.Add(new InsolvencyStageChangedEvent(state.EventIds.Issue(), context.Date, householdId, stage, monthsBelow));
        }

        return events;
    }

    private static InsolvencyStage StageFor(int monthsBelow) => monthsBelow switch
    {
        >= HouseholdEconomyCalculator.InsolvencyRuinedMonths => InsolvencyStage.Ruined,
        >= HouseholdEconomyCalculator.InsolvencyInsolventMonths => InsolvencyStage.Insolvent,
        >= HouseholdEconomyCalculator.InsolvencyAtRiskMonths => InsolvencyStage.AtRisk,
        _ => InsolvencyStage.Solvent,
    };

    /// <summary>§9 rung 1: sells <see cref="HouseholdEconomyCalculator.ForcedLiquidationFraction"/> of
    /// the first stocked good with a known market price, at <see
    /// cref="HouseholdEconomyCalculator.ForcedLiquidationPriceFactor"/> of that price, crediting the
    /// household and debiting the settlement Treasury (the buyer of last resort, matching <see
    /// cref="DebtSystem"/>'s Treasury-as-Argentaria stand-in). Returns whether anything was actually
    /// liquidated (an insolvent household with nothing left to sell applies no consequence this
    /// month).</summary>
    private static bool TryForcedLiquidation(WorldState state, Holding holding, Stockpile stockpile, GameDate date)
    {
        foreach (var goodId in stockpile.Lots.Select(l => l.Good.Id).Distinct().OrderBy(id => id.Value, StringComparer.Ordinal))
        {
            if (!state.MarketPrices.TryGet(new MarketGoodKey(holding.SettlementId, goodId), out var market))
                continue;

            var available = stockpile.AvailableQuantityOf(goodId);
            var quantity = HouseholdEconomyCalculator.ApplyFraction(available, HouseholdEconomyCalculator.ForcedLiquidationFraction);
            if (quantity <= 0)
                continue;

            stockpile.ConsumeAvailable(goodId, quantity);
            var proceeds = HouseholdEconomyCalculator.MultiplyByQuantity(
                market.Price.Scale(HouseholdEconomyCalculator.ForcedLiquidationPriceFactor), quantity);
            if (proceeds == Money.Zero)
                return true;

            if (!HouseholdEconomyCalculator.TryGetOwningHousehold(holding, out var householdId))
                return true;

            LedgerService.Post(
                state, date, LedgerTransactionCategory.Sales,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), proceeds),
                    new LedgerPosting(LedgerAccountKey.ForSettlementTreasury(holding.SettlementId), -proceeds),
                },
                reference: $"forcedLiquidation:{goodId.Value}:{quantity}");
            return true;
        }

        return false;
    }
}

/// <summary>Emitted for every household assessed this month — ledger-ready groundwork for the Monthly
/// Report (Phase 9), matching <see cref="Markets.MarketClearedEvent"/>'s identical "always emitted"
/// convention.</summary>
public sealed record NetWorthAssessedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    Money Total,
    HouseholdWealthBand Band) : IDomainEvent
{
    public string Type => "economy.netWorthAssessed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>Emitted only for a household currently at <see cref="InsolvencyStage.AtRisk"/> or
/// worse.</summary>
public sealed record InsolvencyStageChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    InsolvencyStage Stage,
    int MonthsBelowThreshold) : IDomainEvent
{
    public string Type => "economy.insolvencyStageChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
