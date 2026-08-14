namespace Gens.Simulation.Goods;

/// <summary>A deterministic 0-100 condition measure for goods that physically degrade.</summary>
public readonly record struct StockCondition
{
    public StockCondition(int value)
    {
        if (value is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Stock condition must be between 0 and 100.");
        Value = value;
    }

    public int Value { get; }
    public static StockCondition Pristine => new(100);
}
