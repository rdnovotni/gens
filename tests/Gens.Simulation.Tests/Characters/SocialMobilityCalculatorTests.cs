using Gens.Simulation.Characters;
using Gens.Simulation.Numerics;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class SocialMobilityCalculatorTests
{
    [Test]
    public void EmploymentPullCountIsZeroBelowTheThreshold()
    {
        // Dest ratio only 10% above source's own — below the 20% pull threshold.
        var count = SocialMobilityCalculator.EmploymentPullCount(1_000, Fixed64.One, Fixed64.FromRaw(1_100_000));

        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void EmploymentPullCountIsPositiveAboveTheThreshold()
    {
        var count = SocialMobilityCalculator.EmploymentPullCount(1_000, Fixed64.One, Fixed64.FromInt(2));

        Assert.That(count, Is.GreaterThan(0));
    }

    [Test]
    public void WealthCeilingCountRequiresEliteDiscretionaryAndOpenCurialesCapacity()
    {
        var wrongBand = SocialMobilityCalculator.WealthCeilingCount(1_000, WealthBand.ModestSurplus, Fixed64.FromInt(2));
        var noRoom = SocialMobilityCalculator.WealthCeilingCount(1_000, WealthBand.EliteDiscretionary, Fixed64.FromRaw(500_000));
        var eligible = SocialMobilityCalculator.WealthCeilingCount(1_000, WealthBand.EliteDiscretionary, Fixed64.FromInt(2));

        Assert.That(wrongBand, Is.EqualTo(0));
        Assert.That(noRoom, Is.EqualTo(0));
        Assert.That(eligible, Is.GreaterThan(0));
    }

    [Test]
    public void CurialesAttritionAndVeteranDriftAreSmallButPositiveForALargeGroup()
    {
        Assert.That(SocialMobilityCalculator.CurialesAttritionCount(10_000), Is.GreaterThan(0));
        Assert.That(SocialMobilityCalculator.VeteranDriftCount(10_000), Is.GreaterThan(0));
        Assert.That(SocialMobilityCalculator.ManumissionCount(10_000), Is.GreaterThan(0));
    }

    [Test]
    public void RatesFloorToZeroForATinyGroup()
    {
        Assert.That(SocialMobilityCalculator.CurialesAttritionCount(1), Is.EqualTo(0));
        Assert.That(SocialMobilityCalculator.VeteranDriftCount(1), Is.EqualTo(0));
    }
}
