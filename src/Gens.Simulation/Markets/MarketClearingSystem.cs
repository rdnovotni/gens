using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Goods;
using Gens.Simulation.Health;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Markets;

/// <summary>
/// The monthly settlement-market clearing tick (Phase 8 items 3-4): for every Settlement and every
/// traded good, sums this month's <see cref="MarketOrder"/> supply (every Holding's available <see
/// cref="Stockpile"/> quantity in that settlement) against demand (every <see cref="PopGroup"/>'s
/// needs-consumption demand for that good, per <see cref="NeedsConsumptionCalculator"/>), clears them
/// via <see cref="MarketClearingCalculator.ComputeClearing"/>, forms this month's bounded price via
/// <see cref="MarketClearingCalculator.ComputeNextPrice"/>, and records the result in <see
/// cref="WorldState.MarketPrices"/>. Reads <see cref="WorldState.Stockpiles"/> and <see
/// cref="WorldState.PopGroups"/> directly rather than re-deriving from <see
/// cref="Buildings.ProductionResolvedEvent"/>/<see cref="NeedsConsumptionComputedEvent"/> — direct
/// <see cref="WorldState"/> reads are this codebase's established norm for a system's primary inputs
/// (matching how <see cref="NeedsConsumptionSystem"/> itself reads <see cref="WorldState.PopGroups"/>
/// rather than another system's event); those two events remain each their own producing system's
/// receipt of what happened, not another system's read path. This system does not itself move goods
/// between Stockpiles or post any <see cref="LedgerService"/> transaction — matching a stockpile or
/// a household's own ledger is exactly Phase 8 item 5's "household purchasing... and inventory
/// reservation," explicitly out of this phase's scope; this system only measures and prices.
///
/// <para>Phase 14 item 5's own closed gap: total supply is scaled by <see
/// cref="QuarantineEffectCalculator.CommerceSupplyMultiplier"/> before it ever reaches <see
/// cref="MarketClearingCalculator.ComputeClearing"/> when the settlement is currently under an active
/// Settlement-Wide Quarantine (<see cref="HealthQueries.IsSettlementUnderQuarantine"/>) — <c>gens-
/// disease-public-health-design.md</c> §4.2's own "at a real Contentment and Commerce cost," the
/// Commerce half; <see cref="Health.EpidemicContagionSystem"/> carries the Contentment half.</para>
/// </summary>
public sealed class MarketClearingSystem : IMonthlySystem<WorldState>
{
    private readonly Dictionary<DefinitionId<Good>, Money> _basePrices;
    private readonly IReadOnlyList<DefinitionId<Good>> _tradedGoods;
    private readonly MarketPriceBoundConfig _boundConfig;

    /// <param name="basePrices">Every good this settlement market clears, and the seed price used the
    /// first time a (settlement, good) pair clears (no prior <see cref="SettlementMarket"/> to read a
    /// previous price from yet) — content-authored "base price by tier"
    /// (<c>gens-resources-goods-design.md</c> §11), supplied by the caller the same way <see
    /// cref="Buildings.ProductionSystem"/> takes its good catalog rather than this project inventing a
    /// service locator.</param>
    /// <param name="boundConfig">The bounded-price-change constants (Phase 8 item 3).</param>
    public MarketClearingSystem(IEnumerable<KeyValuePair<DefinitionId<Good>, Money>> basePrices, MarketPriceBoundConfig boundConfig)
    {
        if (basePrices is null)
            throw new ArgumentNullException(nameof(basePrices));
        _boundConfig = boundConfig ?? throw new ArgumentNullException(nameof(boundConfig));

        var materialized = basePrices.ToDictionary(pair => pair.Key, pair => pair.Value);
        foreach (var price in materialized.Values)
        {
            if (price.RawValue <= 0)
                throw new ArgumentOutOfRangeException(nameof(basePrices), price, "Every base price must be positive.");
        }

        _basePrices = materialized;
        // Deterministic (ADR 0004) rather than dictionary enumeration order.
        _tradedGoods = materialized.Keys.OrderBy(static id => id.Value, StringComparer.Ordinal).ToArray();
    }

    public string Id => "markets.clearing";
    public TickPhase Phase => TickPhase.MarketsLedger;
    public IReadOnlyCollection<string> Reads { get; } =
        new[] { "settlements", "holdings", "stockpiles", "popGroups", "marketPrices", "epidemicOutbreaks" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "marketPrices", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "buildings.production", "characters.needsConsumption" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var settlementEntry in state.Settlements.InAscendingOrder())
        {
            var settlementId = settlementEntry.Key;
            var lines = new List<MarketClearingLine>();
            var commerceMultiplier = QuarantineEffectCalculator.CommerceSupplyMultiplier(
                HealthQueries.IsSettlementUnderQuarantine(state, settlementId));

            foreach (var goodId in _tradedGoods)
            {
                var supplyOrders = CollectSupplyOrders(state, settlementId, goodId);
                var demandOrders = CollectDemandOrders(state, settlementId, goodId);
                var rawSupply = supplyOrders.Sum(static order => order.Quantity);
                var totalSupply = (long)Math.Round(rawSupply * commerceMultiplier, MidpointRounding.AwayFromZero);
                var totalDemand = demandOrders.Sum(static order => order.Quantity);

                var (cleared, unsatisfied) = MarketClearingCalculator.ComputeClearing(totalSupply, totalDemand);

                var key = new MarketGoodKey(settlementId, goodId);
                var previousPrice = state.MarketPrices.TryGet(key, out var existingMarket)
                    ? existingMarket.Price
                    : _basePrices[goodId];
                var newPrice = MarketClearingCalculator.ComputeNextPrice(previousPrice, totalSupply, totalDemand, _boundConfig);

                var market = new SettlementMarket(settlementId, goodId, newPrice, previousPrice, totalSupply, totalDemand, cleared, unsatisfied);
                state.MarketPrices.Remove(key);
                state.MarketPrices.Add(key, market);

                lines.Add(new MarketClearingLine(goodId, newPrice, totalSupply, totalDemand, cleared, unsatisfied));
            }

            events.Add(new MarketClearedEvent(state.EventIds.Issue(), context.Date, settlementId, lines));
        }

        return events;
    }

    /// <summary>Every Holding in <paramref name="settlementId"/> with unreserved stock of <paramref
    /// name="goodId"/> contributes one <see cref="MarketOrder"/> — this phase's supply side (Phase 8
    /// item 4's "production supply," read as the stock production has already accumulated rather than
    /// re-deriving this month's <see cref="Buildings.ProductionResolvedEvent"/> deltas, since the
    /// Stockpile is production's own settled record of what exists).</summary>
    private static List<MarketOrder> CollectSupplyOrders(
        WorldState state, RuntimeId<Settlement> settlementId, DefinitionId<Good> goodId)
    {
        var orders = new List<MarketOrder>();
        foreach (var holdingEntry in state.Holdings.InAscendingOrder())
        {
            if (holdingEntry.Value.SettlementId != settlementId)
                continue;
            if (!state.Stockpiles.TryGet(holdingEntry.Key, out var stockpile))
                continue;

            var available = stockpile.AvailableQuantityOf(goodId);
            if (available > 0)
                orders.Add(new MarketOrder(settlementId, goodId, MarketOrderSide.Supply, holdingEntry.Key.ToTaggedString(), available));
        }

        return orders;
    }

    /// <summary>Every PopGroup in <paramref name="settlementId"/> with needs demand for <paramref
    /// name="goodId"/> contributes one <see cref="MarketOrder"/> — this phase's demand side (Phase 8
    /// item 4's "population/household needs"). Only <see
    /// cref="NeedsConsumptionCalculator.ConsumptionGood"/> currently has any modeled per-good demand
    /// (that calculator's own doc comment: a single invented grain-equivalent basket stands in for a
    /// real per-good needs basket) — every other traded good in this settlement clears with zero
    /// demand until Phase 8 item 5 or a later needs-basket pass expands that basket; this is a
    /// deliberate, documented scope boundary, not an oversight.</summary>
    private static List<MarketOrder> CollectDemandOrders(
        WorldState state, RuntimeId<Settlement> settlementId, DefinitionId<Good> goodId)
    {
        var orders = new List<MarketOrder>();
        if (goodId != NeedsConsumptionCalculator.ConsumptionGood)
            return orders;

        foreach (var popGroupEntry in state.PopGroups.InAscendingOrder())
        {
            if (popGroupEntry.Key.SettlementId != settlementId)
                continue;

            var group = popGroupEntry.Value;
            var demand = NeedsConsumptionCalculator.ComputeDemand(group.NeedsProfile, group.Size);
            if (demand > 0)
                orders.Add(new MarketOrder(settlementId, goodId, MarketOrderSide.Demand, group.GroupType.ToString(), demand));
        }

        return orders;
    }
}

/// <summary>One settlement's cleared line for one good this month, part of <see cref="MarketClearedEvent"/>.</summary>
public sealed record MarketClearingLine(
    DefinitionId<Good> GoodId, Money Price, long Supply, long Demand, long ClearedQuantity, long UnsatisfiedDemand);

/// <summary>Emitted for every Settlement, every month, whether or not any line's clearing changed —
/// ledger-ready groundwork for Phase 8 item 5 and the Monthly Report (Phase 9), matching <see
/// cref="NeedsConsumptionComputedEvent"/>'s identical "always emitted" convention.</summary>
public sealed record MarketClearedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Settlement> SettlementId,
    IReadOnlyList<MarketClearingLine> Lines) : IDomainEvent
{
    public string Type => "markets.cleared";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
