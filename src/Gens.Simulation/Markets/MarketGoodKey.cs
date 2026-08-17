using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;

namespace Gens.Simulation.Markets;

/// <summary>The <see cref="State.WorldState.MarketPrices"/> key: one traded good, at one settlement
/// (Phase 8 item 3's "settlement markets"), matching <c>gens-resources-goods-design.md</c> §16's
/// <c>SettlementMarket</c> being keyed by settlement.</summary>
public readonly record struct MarketGoodKey(RuntimeId<Settlement> SettlementId, DefinitionId<Good> GoodId)
    : IComparable<MarketGoodKey>
{
    public int CompareTo(MarketGoodKey other)
    {
        var settlementCompare = SettlementId.CompareTo(other.SettlementId);
        return settlementCompare != 0 ? settlementCompare : string.CompareOrdinal(GoodId.Value, other.GoodId.Value);
    }

    public static bool operator <(MarketGoodKey left, MarketGoodKey right) => left.CompareTo(right) < 0;
    public static bool operator >(MarketGoodKey left, MarketGoodKey right) => left.CompareTo(right) > 0;
    public static bool operator <=(MarketGoodKey left, MarketGoodKey right) => left.CompareTo(right) <= 0;
    public static bool operator >=(MarketGoodKey left, MarketGoodKey right) => left.CompareTo(right) >= 0;
}
