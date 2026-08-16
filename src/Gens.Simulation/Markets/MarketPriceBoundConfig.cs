using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;

namespace Gens.Simulation.Markets;

/// <summary>
/// The tunable constants Phase 8 item 3's "bounded price changes" rule needs (
/// <c>gens-resources-goods-design.md</c> §17 and <c>gens-economy-finance-design.md</c> §13 both leave
/// every rate/threshold "unsized," per this project's numbers-later convention). Passed into <see
/// cref="MarketClearingSystem"/>'s constructor rather than hardcoded, matching how <see
/// cref="Buildings.ProductionSystem"/> takes its good catalog rather than a service locator. These
/// specific values (15% max monthly move, a 1-denarius-minor-unit floor) are this implementation's own
/// invented, documented default, not a design-corpus number — a human reviewer should treat them as a
/// starting point for future balancing, not an authoritative figure.
/// </summary>
public sealed record MarketPriceBoundConfig
{
    /// <summary>A conservative, easily-tunable default: no more than a 15% price move in either
    /// direction per month, with a one-minor-unit floor so a price can never reach (or cross) zero.</summary>
    public static readonly MarketPriceBoundConfig Default = new(
        maxPriceMoveFraction: Fixed64.FromRaw(150_000), // 0.15
        minimumPrice: Money.FromMinorUnits(1));

    public MarketPriceBoundConfig(Fixed64 maxPriceMoveFraction, Money minimumPrice)
    {
        if (maxPriceMoveFraction <= Fixed64.Zero || maxPriceMoveFraction >= Fixed64.One)
            throw new ArgumentOutOfRangeException(
                nameof(maxPriceMoveFraction), maxPriceMoveFraction, "The max price move fraction must be strictly between 0 and 1.");
        if (minimumPrice.RawValue <= 0)
            throw new ArgumentOutOfRangeException(nameof(minimumPrice), minimumPrice, "The minimum price must be positive.");

        MaxPriceMoveFraction = maxPriceMoveFraction;
        MinimumPrice = minimumPrice;
    }

    /// <summary>The largest fraction the cleared price may move, up or down, from last month's cleared
    /// price in a single tick (e.g. <c>0.15</c> for +/-15%).</summary>
    public Fixed64 MaxPriceMoveFraction { get; }

    /// <summary>The lowest a cleared price is ever allowed to fall to — the concrete "never reaches
    /// zero" floor Phase 8's "market clearing never produces negative stock or negative prices"
    /// invariant depends on.</summary>
    public Money MinimumPrice { get; }
}
