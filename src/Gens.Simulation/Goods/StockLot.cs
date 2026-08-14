namespace Gens.Simulation.Goods;

/// <summary>An immutable view of one homogeneous batch in a <see cref="Stockpile"/>.</summary>
public sealed record StockLot(
    GoodDefinition Good,
    long Quantity,
    GoodQuality? Quality,
    StockCondition? Condition,
    int AgeInTicks,
    StockProvenance? Provenance);

/// <summary>A removed batch, preserving its quality, condition, age, and origin.</summary>
public sealed record StockMovement(
    GoodDefinition Good,
    long Quantity,
    GoodQuality? Quality,
    StockCondition? Condition,
    int AgeInTicks,
    StockProvenance? Provenance);
