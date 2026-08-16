namespace Gens.Simulation.Markets;

/// <summary>Which side of a <see cref="MarketOrder"/> a line represents (Phase 8 item 3's "orders").</summary>
public enum MarketOrderSide
{
    /// <summary>Goods available for sale — sourced from a Holding's <see cref="Goods.Stockpile"/>
    /// (Phase 8 item 4's "production supply").</summary>
    Supply,

    /// <summary>Goods wanted — sourced from a <see cref="Characters.PopGroup"/>'s needs demand (Phase
    /// 8 item 4's "population/household needs").</summary>
    Demand,
}
