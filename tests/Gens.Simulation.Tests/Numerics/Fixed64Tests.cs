using Gens.Simulation.Numerics;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Numerics;

public sealed class Fixed64Tests
{
    [Test]
    public void FromIntRoundTripsThroughRawValue()
    {
        var value = Fixed64.FromInt(3);
        Assert.That(value.RawValue, Is.EqualTo(3_000_000));
    }

    [Test]
    public void AdditionAndSubtractionAreExact()
    {
        var half = Fixed64.FromRaw(500_000);
        var sum = half + half;
        Assert.That(sum, Is.EqualTo(Fixed64.One));
        Assert.That(sum - half, Is.EqualTo(half));
    }

    [Test]
    public void MultiplyScalesCorrectly()
    {
        var oneAndHalf = Fixed64.FromRaw(1_500_000);
        var two = Fixed64.FromInt(2);
        Assert.That(Fixed64.Multiply(oneAndHalf, two), Is.EqualTo(Fixed64.FromInt(3)));
    }

    [Test]
    public void DivideScalesCorrectly()
    {
        var three = Fixed64.FromInt(3);
        var two = Fixed64.FromInt(2);
        Assert.That(Fixed64.Divide(three, two), Is.EqualTo(Fixed64.FromRaw(1_500_000)));
    }

    [Test]
    public void DivideByZeroThrows()
    {
        Assert.That(() => Fixed64.Divide(Fixed64.One, Fixed64.Zero), Throws.TypeOf<DivideByZeroException>());
    }

    [Test]
    public void MultiplyRoundsHalfToEven()
    {
        // 0.0000005 rounds to the nearest even representable raw unit.
        var a = Fixed64.FromRaw(5);
        var half = Fixed64.FromRaw(500_000);
        var result = Fixed64.Multiply(a, half);
        Assert.That(result.RawValue, Is.EqualTo(2));
    }

    [Test]
    public void AdditionSaturatesInsteadOfOverflowing()
    {
        var result = Fixed64.MaxValue + Fixed64.One;
        Assert.That(result, Is.EqualTo(Fixed64.MaxValue));
    }

    [Test]
    public void SubtractionSaturatesAtMinValue()
    {
        var result = Fixed64.MinValue - Fixed64.One;
        Assert.That(result, Is.EqualTo(Fixed64.MinValue));
    }

    [Test]
    public void MultiplySaturatesOnOverflow()
    {
        var result = Fixed64.Multiply(Fixed64.MaxValue, Fixed64.FromInt(2));
        Assert.That(result, Is.EqualTo(Fixed64.MaxValue));
    }

    [Test]
    public void ComparisonOperatorsOrderByRawValue()
    {
        var small = Fixed64.FromInt(1);
        var large = Fixed64.FromInt(2);
        Assert.Multiple(() =>
        {
            Assert.That(small < large, Is.True);
            Assert.That(large > small, Is.True);
            Assert.That(small <= small, Is.True);
            Assert.That(large >= large, Is.True);
        });
    }

    [Test]
    public void ToDisplayStringFormatsSixDecimalPlaces()
    {
        Assert.That(Fixed64.FromRaw(1_250_000).ToDisplayString(), Is.EqualTo("1.250000"));
    }
}
