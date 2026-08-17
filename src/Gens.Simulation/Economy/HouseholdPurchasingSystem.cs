using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Markets;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Economy;

/// <summary>
/// The monthly household purchasing/sales tick (Phase 8 item 5: "household purchasing, sales,
/// inventory reservation"). <see cref="MarketClearingSystem"/> already computes each settlement's
/// cleared price and aggregate supply/demand for <see
/// cref="NeedsConsumptionCalculator.ConsumptionGood"/> (the one good with a modeled per-capita need —
/// see that system's own doc comment on why every other good clears with zero demand for now); this
/// system is what actually moves the goods and the money it priced, at the household granularity that
/// system's own doc comment named as explicitly out of Phase 8 items 3-4's scope.
///
/// **Mechanism.** For each settlement, every Holding whose owner resolves to a Household (<see
/// cref="HouseholdEconomyCalculator.TryGetOwningHousehold"/>) gets a monthly need (<see
/// cref="HouseholdEconomyCalculator.ComputeHouseholdNeed"/>). A holding short of its need is a buyer;
/// one holding above its need (available stock minus need) is a seller — the same holding is never
/// both, by construction. Buyers and sellers are matched deterministically in ascending Holding-ID
/// order (ADR 0004), a documented simplification of real order-book matching (no price-time priority,
/// no partial-order carry-over month to month): each pairing moves stock via <see
/// cref="Stockpile.Reserve"/> → <see cref="Stockpile.ConsumeReserved"/> on the seller's stockpile (so
/// the goods are protected between being claimed and actually settling, never "teleported"), <see
/// cref="Stockpile.Add"/> on the buyer's, and one balanced <see cref="LedgerService.Post"/> transaction
/// per pairing at this month's already-cleared <see cref="SettlementMarket.Price"/>. A settlement with
/// no cleared price yet for the good (no prior <see cref="MarketClearingSystem"/> tick) is skipped —
/// there is no price to trade at.
/// </summary>
public sealed class HouseholdPurchasingSystem : IMonthlySystem<WorldState>
{
    public string Id => "economy.householdPurchasing";
    public TickPhase Phase => TickPhase.MarketsLedger;
    public IReadOnlyCollection<string> Reads { get; } =
        new[] { "settlements", "holdings", "stockpiles", "marketPrices" };
    public IReadOnlyCollection<string> Writes { get; } =
        new[] { "stockpiles", "ledgerAccounts", "ledgerTransactions", "ledgerTransactionIds", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "markets.clearing" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var goodId = NeedsConsumptionCalculator.ConsumptionGood;

        foreach (var settlementEntry in state.Settlements.InAscendingOrder())
        {
            var settlementId = settlementEntry.Key;
            if (!state.MarketPrices.TryGet(new MarketGoodKey(settlementId, goodId), out var market))
                continue; // No cleared price yet for this settlement/good — nothing to trade at.

            var buyers = new List<(RuntimeId<Holding> HoldingId, RuntimeId<Household> HouseholdId, long Remaining)>();
            var sellers = new List<(RuntimeId<Holding> HoldingId, RuntimeId<Household> HouseholdId, long Remaining)>();

            foreach (var holdingEntry in state.Holdings.InAscendingOrder())
            {
                var holding = holdingEntry.Value;
                if (holding.SettlementId != settlementId)
                    continue;
                if (!HouseholdEconomyCalculator.TryGetOwningHousehold(holding, out var householdId))
                    continue;

                var need = HouseholdEconomyCalculator.ComputeHouseholdNeed(holding.ResidentCapacity);
                var hasStockpile = state.Stockpiles.TryGet(holdingEntry.Key, out var stockpile);
                var available = hasStockpile ? stockpile!.AvailableQuantityOf(goodId) : 0;

                // A buyer with no provisioned Stockpile cannot receive goods yet — skipped rather than
                // auto-provisioning one here, matching WorldState.Stockpiles' own "buildings cannot
                // produce or consume until one exists" sparse convention.
                if (available < need && hasStockpile)
                    buyers.Add((holdingEntry.Key, householdId, need - available));
                else if (available > need)
                    sellers.Add((holdingEntry.Key, householdId, available - need));
            }

            var sellerIndex = 0;
            for (var buyerIndex = 0; buyerIndex < buyers.Count && sellerIndex < sellers.Count; buyerIndex++)
            {
                var (buyerHoldingId, buyerHouseholdId, buyerRemaining) = buyers[buyerIndex];

                while (buyerRemaining > 0 && sellerIndex < sellers.Count)
                {
                    var (sellerHoldingId, sellerHouseholdId, sellerRemaining) = sellers[sellerIndex];
                    var quantity = Math.Min(buyerRemaining, sellerRemaining);

                    if (quantity > 0)
                    {
                        TransferGoods(state, sellerHoldingId, buyerHoldingId, goodId, quantity, context.Date);

                        var amount = HouseholdEconomyCalculator.MultiplyByQuantity(market.Price, quantity);
                        var ledgerEvent = LedgerService.Post(
                            state,
                            context.Date,
                            LedgerTransactionCategory.Sales,
                            new[]
                            {
                                new LedgerPosting(LedgerAccountKey.ForHousehold(buyerHouseholdId), -amount),
                                new LedgerPosting(LedgerAccountKey.ForHousehold(sellerHouseholdId), amount),
                            },
                            reference: $"purchase:{sellerHoldingId.ToTaggedString()}->{buyerHoldingId.ToTaggedString()}");
                        events.Add(ledgerEvent);
                    }

                    buyerRemaining -= quantity;
                    sellerRemaining -= quantity;
                    sellers[sellerIndex] = (sellerHoldingId, sellerHouseholdId, sellerRemaining);
                    if (sellerRemaining == 0)
                        sellerIndex++;
                }
            }
        }

        return events;
    }

    /// <summary>Moves <paramref name="quantity"/> of <paramref name="goodId"/> from the seller's
    /// stockpile to the buyer's via Reserve → ConsumeReserved → Add, preserving each moved lot's
    /// quality/condition/age/provenance (Phase 8 item 5's "inventory reservation" requirement).</summary>
    private static void TransferGoods(
        WorldState state,
        RuntimeId<Holding> sellerHoldingId,
        RuntimeId<Holding> buyerHoldingId,
        DefinitionId<Good> goodId,
        long quantity,
        Time.GameDate date)
    {
        state.Stockpiles.TryGet(sellerHoldingId, out var sellerStockpile);
        state.Stockpiles.TryGet(buyerHoldingId, out var buyerStockpile);

        var reservationId = $"purchase-{date.TotalMonths}-{sellerHoldingId.ToTaggedString()}-{buyerHoldingId.ToTaggedString()}";
        sellerStockpile!.Reserve(reservationId, goodId, quantity);
        var movements = sellerStockpile.ConsumeReserved(reservationId);

        foreach (var movement in movements)
        {
            if (buyerStockpile.RemainingCapacity < movement.Quantity)
                continue; // Buyer has no room — the goods were already removed from the seller; a
                          // documented simplification (no capacity pre-check before matching).
            buyerStockpile.Add(movement.Good, movement.Quantity, movement.Quality, movement.Condition, movement.Provenance, movement.AgeInTicks);
        }
    }
}
