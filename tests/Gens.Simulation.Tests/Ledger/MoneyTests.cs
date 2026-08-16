using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Ledger;

public sealed class MoneyTests
{
    [Test]
    public void FromDenariiScalesToMinorUnits()
    {
        var money = Money.FromDenarii(5);
        Assert.That(money.RawValue, Is.EqualTo(500));
    }

    [Test]
    public void AdditionAndSubtractionAreExact()
    {
        var a = Money.FromDenarii(10);
        var b = Money.FromMinorUnits(25);
        Assert.That((a + b).RawValue, Is.EqualTo(1025));
        Assert.That((a - b).RawValue, Is.EqualTo(975));
    }

    [Test]
    public void CanRepresentANegativeBalance()
    {
        var negative = Money.Zero - Money.FromDenarii(50);
        Assert.That(negative.IsNegative, Is.True);
        Assert.That(negative.RawValue, Is.EqualTo(-5000));
    }

    [Test]
    public void AdditionSaturatesRatherThanOverflowing()
    {
        var sum = Money.MaxValue + Money.FromDenarii(1);
        Assert.That(sum, Is.EqualTo(Money.MaxValue));
    }

    [Test]
    public void SubtractionSaturatesAtMinValue()
    {
        var result = Money.MinValue - Money.FromDenarii(1);
        Assert.That(result, Is.EqualTo(Money.MinValue));
    }

    [Test]
    public void ScaleAppliesAFixed64Factor()
    {
        var price = Money.FromDenarii(10);
        var scaled = price.Scale(Fixed64.FromRaw(1_500_000)); // x1.5
        Assert.That(scaled, Is.EqualTo(Money.FromDenarii(15)));
    }

    [Test]
    public void ScaleRoundsHalfToEven()
    {
        // 1 minor unit * 0.5 = 0.5, rounds to even (0).
        var half = Money.FromMinorUnits(1).Scale(Fixed64.FromRaw(500_000));
        Assert.That(half.RawValue, Is.EqualTo(0));

        // 3 minor units * 0.5 = 1.5, rounds to even (2).
        var oneAndHalf = Money.FromMinorUnits(3).Scale(Fixed64.FromRaw(500_000));
        Assert.That(oneAndHalf.RawValue, Is.EqualTo(2));
    }

    [Test]
    public void ToDisplayStringFormatsTwoDecimalPlaces()
    {
        Assert.That(Money.FromMinorUnits(1250).ToDisplayString(), Is.EqualTo("12.50"));
        Assert.That(Money.FromMinorUnits(5).ToDisplayString(), Is.EqualTo("0.05"));
    }

    [Test]
    public void ComparisonOperatorsOrderByRawValue()
    {
        Assert.That(Money.FromDenarii(5) < Money.FromDenarii(10), Is.True);
        Assert.That(Money.FromDenarii(10) > Money.FromDenarii(5), Is.True);
        Assert.That(Money.FromDenarii(5) <= Money.FromDenarii(5), Is.True);
    }

    [Test]
    public void AbsReturnsAPositiveMagnitude()
    {
        Assert.That((Money.Zero - Money.FromDenarii(7)).Abs(), Is.EqualTo(Money.FromDenarii(7)));
        Assert.That(Money.FromDenarii(7).Abs(), Is.EqualTo(Money.FromDenarii(7)));
    }
}
