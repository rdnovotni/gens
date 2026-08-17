using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;

namespace Gens.Simulation.Markets;

/// <summary>
/// One settlement's cleared monthly market state for one good (Phase 8 item 3) — the persisted
/// counterpart to <c>gens-resources-goods-design.md</c> §16's <c>SettlementMarket</c> data model,
/// specialized to one good per record rather than that sketch's per-good dictionaries, matching how
/// this codebase already keys <see cref="Characters.PopGroup"/> per (settlement, group) rather than
/// nesting a dictionary inside one settlement-wide record. <see cref="PreviousPrice"/> is carried
/// alongside <see cref="Price"/> specifically so <see cref="MarketBoundedPriceChangeInvariantCheck"/>
/// can verify the bounded-price-change rule tick over tick from a single snapshot, without needing to
/// diff two saves.
/// </summary>
public sealed record SettlementMarket
{
    public SettlementMarket(
        RuntimeId<Settlement> settlementId,
        DefinitionId<Good> goodId,
        Money price,
        Money previousPrice,
        long supply,
        long demand,
        long clearedQuantity,
        long unsatisfiedDemand)
    {
        if (price.RawValue < 0)
            throw new ArgumentOutOfRangeException(nameof(price), price, "A market price cannot be negative.");
        if (previousPrice.RawValue < 0)
            throw new ArgumentOutOfRangeException(nameof(previousPrice), previousPrice, "A market price cannot be negative.");
        if (supply < 0)
            throw new ArgumentOutOfRangeException(nameof(supply), supply, "Supply cannot be negative.");
        if (demand < 0)
            throw new ArgumentOutOfRangeException(nameof(demand), demand, "Demand cannot be negative.");
        if (clearedQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(clearedQuantity), clearedQuantity, "Cleared quantity cannot be negative.");
        if (clearedQuantity > supply || clearedQuantity > demand)
            throw new ArgumentOutOfRangeException(
                nameof(clearedQuantity), clearedQuantity, "Cleared quantity cannot exceed either supply or demand.");
        if (unsatisfiedDemand < 0)
            throw new ArgumentOutOfRangeException(nameof(unsatisfiedDemand), unsatisfiedDemand, "Unsatisfied demand cannot be negative.");
        if (unsatisfiedDemand != demand - clearedQuantity)
            throw new ArgumentOutOfRangeException(
                nameof(unsatisfiedDemand), unsatisfiedDemand, "Unsatisfied demand must equal demand minus cleared quantity.");

        SettlementId = settlementId;
        GoodId = goodId;
        Price = price;
        PreviousPrice = previousPrice;
        Supply = supply;
        Demand = demand;
        ClearedQuantity = clearedQuantity;
        UnsatisfiedDemand = unsatisfiedDemand;
    }

    public RuntimeId<Settlement> SettlementId { get; }
    public DefinitionId<Good> GoodId { get; }

    /// <summary>This month's cleared price, already bounded per <see
    /// cref="MarketClearingCalculator.ComputeNextPrice"/>.</summary>
    public Money Price { get; }

    /// <summary>Last month's cleared price (or the content-authored base price, on a good/settlement's
    /// first clearing) — the reference point the bounded-price-change rule measures this month's move
    /// against.</summary>
    public Money PreviousPrice { get; }

    /// <summary>Total available stock this month, summed across every Holding's Stockpile in this
    /// settlement (Phase 8 item 4).</summary>
    public long Supply { get; }

    /// <summary>Total demand this month, summed across every PopGroup in this settlement (Phase 8
    /// item 4).</summary>
    public long Demand { get; }

    /// <summary>How much of <see cref="Demand"/> was actually matched against <see cref="Supply"/>
    /// this month — <c>min(Supply, Demand)</c>.</summary>
    public long ClearedQuantity { get; }

    /// <summary>Demand that went unmet this month — <c>Demand - ClearedQuantity</c> (Phase 8 item 3's
    /// "unsatisfied demand").</summary>
    public long UnsatisfiedDemand { get; }
}
