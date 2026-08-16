using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;

namespace Gens.Simulation.Markets;

/// <summary>Pure market-clearing and price-formation math (Phase 8 item 3), factored out of <see
/// cref="MarketClearingSystem"/> the same way <see cref="Characters.NeedsConsumptionCalculator"/> sits
/// beside <see cref="Characters.NeedsConsumptionSystem"/>.</summary>
public static class MarketClearingCalculator
{
    /// <summary>Clears total supply against total demand for one good at one settlement: the matched
    /// quantity is <c>min(supply, demand)</c>, and whatever demand remains is "unsatisfied demand"
    /// (Phase 8 item 3). No partial-order preference is modeled — this phase clears by aggregate
    /// quantity only, per <see cref="MarketOrder"/>'s own doc comment on why per-order matching is
    /// deferred to Phase 8 item 5.</summary>
    public static (long ClearedQuantity, long UnsatisfiedDemand) ComputeClearing(long supply, long demand)
    {
        if (supply < 0)
            throw new ArgumentOutOfRangeException(nameof(supply), supply, "Supply cannot be negative.");
        if (demand < 0)
            throw new ArgumentOutOfRangeException(nameof(demand), demand, "Demand cannot be negative.");

        var cleared = Math.Min(supply, demand);
        var unsatisfied = demand - cleared;
        return (cleared, unsatisfied);
    }

    /// <summary>
    /// Price formation with a bounded per-tick move (Phase 8 item 3): the "desired" price is last
    /// month's price scaled by the demand/supply scarcity ratio (more demand than supply pushes the
    /// price up toward that ratio; more supply than demand pushes it down), then clamped so it never
    /// moves more than <see cref="MarketPriceBoundConfig.MaxPriceMoveFraction"/> in either direction
    /// from <paramref name="previousPrice"/>, and never falls below <see
    /// cref="MarketPriceBoundConfig.MinimumPrice"/>. With no supply and no demand at all, the price
    /// simply holds; with demand but zero supply the desired price is treated as infinite scarcity and
    /// clamped straight to the tick's maximum allowed rise.
    /// </summary>
    public static Money ComputeNextPrice(Money previousPrice, long supply, long demand, MarketPriceBoundConfig config)
    {
        if (previousPrice.RawValue <= 0)
            throw new ArgumentOutOfRangeException(nameof(previousPrice), previousPrice, "The previous price must be positive.");
        if (supply < 0)
            throw new ArgumentOutOfRangeException(nameof(supply), supply, "Supply cannot be negative.");
        if (demand < 0)
            throw new ArgumentOutOfRangeException(nameof(demand), demand, "Demand cannot be negative.");
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        var maxUp = previousPrice.Scale(Fixed64.One + config.MaxPriceMoveFraction);
        var maxDown = previousPrice.Scale(Fixed64.One - config.MaxPriceMoveFraction);

        Money target;
        if (supply == 0 && demand == 0)
            target = previousPrice;
        else if (supply == 0)
            target = maxUp;
        else
        {
            var scarcityRatio = Fixed64.Divide(ToFixed64(demand), ToFixed64(supply));
            target = previousPrice.Scale(scarcityRatio);
        }

        var clamped = target;
        if (clamped > maxUp)
            clamped = maxUp;
        if (clamped < maxDown)
            clamped = maxDown;
        if (clamped < config.MinimumPrice)
            clamped = config.MinimumPrice;

        return clamped;
    }

    private static Fixed64 ToFixed64(long whole) => Fixed64.FromRaw(checked(whole * Fixed64.ScaleFactor));
}
