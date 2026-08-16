using Gens.Simulation.Characters;
using Gens.Simulation.Numerics;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class GrowthMortalityCalculatorTests
{
    [Test]
    public void HighContentmentAndNoExposureYieldsPositiveGrowth()
    {
        var rate = GrowthMortalityCalculator.MonthlyNetRate(Fixed64.One, Fixed64.Zero);

        Assert.That(rate, Is.GreaterThan(Fixed64.Zero));
    }

    [Test]
    public void LowContentmentAndHighExposureYieldsNegativeRate()
    {
        var rate = GrowthMortalityCalculator.MonthlyNetRate(Fixed64.Zero, Fixed64.One);

        Assert.That(rate, Is.LessThan(Fixed64.Zero));
    }

    [Test]
    public void RateNeverExceedsItsClampedBounds()
    {
        var maxed = GrowthMortalityCalculator.MonthlyNetRate(Fixed64.FromInt(100), Fixed64.Zero);
        var minned = GrowthMortalityCalculator.MonthlyNetRate(Fixed64.Zero, Fixed64.FromInt(100));

        Assert.That(maxed, Is.LessThanOrEqualTo(Fixed64.FromRaw(15_000)));
        Assert.That(minned, Is.GreaterThanOrEqualTo(Fixed64.FromRaw(-20_000)));
    }
}
