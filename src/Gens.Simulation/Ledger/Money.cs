using System.Globalization;
using System.Numerics;
using Gens.Simulation.Numerics;

namespace Gens.Simulation.Ledger;

/// <summary>
/// A deterministic, integer-backed currency amount in Denarii minor units (ADR 0002, Phase 8 item 1:
/// "double-entry-style household and actor ledgers using integer minor units"). One Denarius equals
/// <see cref="ScaleFactor"/> minor units. Modeled directly on <see cref="Fixed64"/>'s pattern —
/// checked/saturating arithmetic, no <c>double</c>/<c>float</c> anywhere — but kept as its own type
/// rather than reusing <see cref="Fixed64"/> for money: money's natural precision (whole minor units)
/// and a rate's parts-per-million precision differ by orders of magnitude, and ADR 0002 explicitly
/// rejects forcing one scale to serve both (see that ADR's "Alternatives Considered"). A balance may
/// be negative — the Treasury "can run negative" (<c>gens-economy-finance-design.md</c> §2) — so no
/// non-negative invariant is enforced by this type itself; callers that need a floor (e.g. a market
/// price) validate that themselves.
/// </summary>
public readonly record struct Money : IComparable<Money>
{
    /// <summary>Minor units per Denarius (Phase 8 item 1's "integer minor units").</summary>
    public const long ScaleFactor = 100;

    private Money(long rawValue) => RawValue = rawValue;

    /// <summary>The underlying amount, in minor units.</summary>
    public long RawValue { get; }

    public static readonly Money Zero = new(0);
    public static readonly Money MaxValue = new(long.MaxValue);
    public static readonly Money MinValue = new(long.MinValue);

    public static Money FromMinorUnits(long minorUnits) => new(minorUnits);

    public static Money FromDenarii(long denarii) => new(checked(denarii * ScaleFactor));

    public static Money operator +(Money a, Money b) => new(SaturatingAdd(a.RawValue, b.RawValue));

    public static Money operator -(Money a, Money b) => new(SaturatingAdd(a.RawValue, SaturatingNegate(b.RawValue)));

    public static Money operator -(Money value) => new(SaturatingNegate(value.RawValue));

    public static bool operator <(Money a, Money b) => a.RawValue < b.RawValue;
    public static bool operator >(Money a, Money b) => a.RawValue > b.RawValue;
    public static bool operator <=(Money a, Money b) => a.RawValue <= b.RawValue;
    public static bool operator >=(Money a, Money b) => a.RawValue >= b.RawValue;

    public int CompareTo(Money other) => RawValue.CompareTo(other.RawValue);

    public bool IsNegative => RawValue < 0;

    public Money Abs() => RawValue < 0 ? new Money(SaturatingNegate(RawValue)) : this;

    /// <summary>Scales this amount by a <see cref="Fixed64"/> factor (e.g. a market price-move
    /// multiplier), rounding half-to-even and saturating on overflow — the money-side counterpart to
    /// <see cref="Fixed64.Multiply"/>, needed because <see cref="Markets.MarketClearingCalculator"/>
    /// (Phase 8 item 3) computes bounded price moves as a <see cref="Fixed64"/> ratio applied to a
    /// <see cref="Money"/> price.</summary>
    public Money Scale(Fixed64 factor)
    {
        var product = (BigInteger)RawValue * factor.RawValue;
        var scaled = DivideRoundHalfToEven(product, Fixed64.ScaleFactor);
        return new Money(Saturate(scaled));
    }

    /// <summary>The sole sanctioned path to a human-readable amount (e.g. <c>"12.50"</c>). Presentation only.</summary>
    public string ToDisplayString()
    {
        var whole = RawValue / ScaleFactor;
        var fraction = Math.Abs(RawValue % ScaleFactor);
        return $"{whole}.{fraction.ToString("D2", CultureInfo.InvariantCulture)}";
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
