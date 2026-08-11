using System.Globalization;
using System.Numerics;

namespace Gens.Simulation.Numerics;

/// <summary>
/// A deterministic, integer-backed fixed-point number scaled by one million (parts-per-million),
/// used for every rate, ratio, or multiplier the simulation needs (ADR 0002). Arithmetic never
/// touches <see cref="double"/> or <see cref="float"/>; overflow saturates rather than wrapping or
/// throwing, since a rate clamped at its representable extreme is a safer failure mode than a
/// crashed tick.
/// </summary>
public readonly record struct Fixed64 : IComparable<Fixed64>
{
    public const long ScaleFactor = 1_000_000;

    private Fixed64(long rawValue) => RawValue = rawValue;

    /// <summary>The underlying value, scaled by <see cref="ScaleFactor"/>.</summary>
    public long RawValue { get; }

    public static readonly Fixed64 Zero = new(0);
    public static readonly Fixed64 One = new(ScaleFactor);
    public static readonly Fixed64 MaxValue = new(long.MaxValue);
    public static readonly Fixed64 MinValue = new(long.MinValue);

    public static Fixed64 FromRaw(long rawValue) => new(rawValue);

    public static Fixed64 FromInt(int whole) => new(checked((long)whole * ScaleFactor));

    public static Fixed64 operator +(Fixed64 a, Fixed64 b) => new(SaturatingAdd(a.RawValue, b.RawValue));

    public static Fixed64 operator -(Fixed64 a, Fixed64 b) => new(SaturatingAdd(a.RawValue, SaturatingNegate(b.RawValue)));

    public static Fixed64 operator -(Fixed64 value) => new(SaturatingNegate(value.RawValue));

    public static Fixed64 Multiply(Fixed64 a, Fixed64 b)
    {
        var product = (BigInteger)a.RawValue * b.RawValue;
        var scaled = DivideRoundHalfToEven(product, ScaleFactor);
        return new Fixed64(Saturate(scaled));
    }

    public static Fixed64 Divide(Fixed64 a, Fixed64 b)
    {
        if (b.RawValue == 0)
            throw new DivideByZeroException("Cannot divide by a zero Fixed64 value.");

        var numerator = (BigInteger)a.RawValue * ScaleFactor;
        var scaled = DivideRoundHalfToEven(numerator, b.RawValue);
        return new Fixed64(Saturate(scaled));
    }

    public static bool operator <(Fixed64 a, Fixed64 b) => a.RawValue < b.RawValue;
    public static bool operator >(Fixed64 a, Fixed64 b) => a.RawValue > b.RawValue;
    public static bool operator <=(Fixed64 a, Fixed64 b) => a.RawValue <= b.RawValue;
    public static bool operator >=(Fixed64 a, Fixed64 b) => a.RawValue >= b.RawValue;

    public int CompareTo(Fixed64 other) => RawValue.CompareTo(other.RawValue);

    /// <summary>The sole sanctioned path to a human-readable fractional value. Presentation only.</summary>
    public string ToDisplayString()
    {
        var whole = RawValue / ScaleFactor;
        var fraction = Math.Abs(RawValue % ScaleFactor);
        return $"{whole}.{fraction.ToString("D6", CultureInfo.InvariantCulture)}";
    }

    private static long SaturatingAdd(long a, long b)
    {
        try
        {
            return checked(a + b);
        }
        catch (OverflowException)
        {
            return a > 0 ? long.MaxValue : long.MinValue;
        }
    }

    private static long SaturatingNegate(long value) => value == long.MinValue ? long.MaxValue : -value;

    private static long Saturate(BigInteger value)
    {
        if (value > long.MaxValue)
            return long.MaxValue;
        if (value < long.MinValue)
            return long.MinValue;
        return (long)value;
    }

    private static BigInteger DivideRoundHalfToEven(BigInteger numerator, BigInteger denominator)
    {
        var quotient = BigInteger.DivRem(numerator, denominator, out var remainder);
        if (remainder == 0)
            return quotient;

        var twiceRemainder = BigInteger.Abs(remainder) * 2;
        var absDenominator = BigInteger.Abs(denominator);
        var sameSign = (numerator < 0) == (denominator < 0);

        if (twiceRemainder > absDenominator || (twiceRemainder == absDenominator && !quotient.IsEven))
            quotient += sameSign ? 1 : -1;

        return quotient;
    }
}
