using Gens.Simulation.Numerics;
using Gens.Simulation.State;

namespace Gens.Simulation.Markets;

/// <summary>
/// No cleared <see cref="SettlementMarket"/> may ever carry a negative price, supply, demand, cleared
/// quantity, or unsatisfied demand — Phase 8's "market clearing never produces negative stock or
/// negative prices" invariant. Defense in depth: <see cref="SettlementMarket"/>'s own constructor
/// already rejects a negative field, matching <see
/// cref="Ledger.LedgerTransactionBalanceInvariantCheck"/>'s identical "guard what a well-behaved
/// writer already guarantees" reasoning.
/// </summary>
public sealed class MarketNoNegativeStockOrPriceInvariantCheck : IInvariantCheck
{
    public string Id => "markets.noNegativeStockOrPrice";
    public bool IsFatal => true;

    public IEnumerable<InvariantViolation> Check(WorldState state)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.MarketPrices.InAscendingOrder())
        {
            var market = entry.Value;
            var subjects = new[] { market.SettlementId.ToTaggedString(), market.GoodId.Value };

            if (market.Price.RawValue < 0)
                yield return new InvariantViolation(Id, $"Market price for '{market.GoodId.Value}' is negative.", subjects);
            if (market.Supply < 0)
                yield return new InvariantViolation(Id, $"Market supply for '{market.GoodId.Value}' is negative.", subjects);
            if (market.Demand < 0)
                yield return new InvariantViolation(Id, $"Market demand for '{market.GoodId.Value}' is negative.", subjects);
            if (market.ClearedQuantity < 0)
                yield return new InvariantViolation(Id, $"Market cleared quantity for '{market.GoodId.Value}' is negative.", subjects);
            if (market.UnsatisfiedDemand < 0)
                yield return new InvariantViolation(Id, $"Market unsatisfied demand for '{market.GoodId.Value}' is negative.", subjects);
        }
    }
}

/// <summary>
/// Every cleared <see cref="SettlementMarket.Price"/> must sit within <see
/// cref="MarketPriceBoundConfig.MaxPriceMoveFraction"/> of <see cref="SettlementMarket.PreviousPrice"/>
/// (or exactly at <see cref="MarketPriceBoundConfig.MinimumPrice"/>, the one case where the bound is
/// deliberately overridden by the floor) — Phase 8's "bounded price change is actually respected tick
/// over tick" invariant. Takes the same <see cref="MarketPriceBoundConfig"/> the producing <see
/// cref="MarketClearingSystem"/> was constructed with, since the bound itself is a tunable constant,
/// not campaign state.
/// </summary>
public sealed class MarketBoundedPriceChangeInvariantCheck : IInvariantCheck
{
    private readonly MarketPriceBoundConfig _config;

    public MarketBoundedPriceChangeInvariantCheck(MarketPriceBoundConfig config) =>
        _config = config ?? throw new ArgumentNullException(nameof(config));

    public string Id => "markets.boundedPriceChange";
    public bool IsFatal => true;

    public IEnumerable<InvariantViolation> Check(WorldState state)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.MarketPrices.InAscendingOrder())
        {
            var market = entry.Value;
            if (market.PreviousPrice.RawValue <= 0)
                continue; // No prior clearing to bound against.

            var maxUp = market.PreviousPrice.Scale(Fixed64.One + _config.MaxPriceMoveFraction);
            var maxDown = market.PreviousPrice.Scale(Fixed64.One - _config.MaxPriceMoveFraction);
            var withinBound = market.Price <= maxUp && market.Price >= maxDown;
            var atFloor = market.Price == _config.MinimumPrice;

            if (!withinBound && !atFloor)
                yield return new InvariantViolation(
                    Id,
                    $"Market price for '{market.GoodId.Value}' at settlement '{market.SettlementId.ToTaggedString()}' " +
                    $"moved from {market.PreviousPrice.ToDisplayString()} to {market.Price.ToDisplayString()}, " +
                    $"outside the +/-{_config.MaxPriceMoveFraction.ToDisplayString()} bound.",
                    new[] { market.SettlementId.ToTaggedString(), market.GoodId.Value });
        }
    }
}
