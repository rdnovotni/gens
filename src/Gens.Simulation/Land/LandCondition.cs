namespace Gens.Simulation.Land;

/// <summary>A deterministic 0–100 measure of a parcel's usable condition.</summary>
public readonly record struct LandCondition
{
    public LandCondition(int value)
    {
        if (value is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Land condition must be between 0 and 100.");
        Value = value;
    }

    public int Value { get; }

    public static LandCondition Pristine => new(100);
}
