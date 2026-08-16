using Gens.Simulation.Characters;
using Gens.Simulation.Numerics;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class ContentmentCalculatorTests
{
    [Test]
    public void FullySatisfiedInputsProduceFullContentment()
    {
        var contentment = ContentmentCalculator.ComputeContentment(Fixed64.One, Fixed64.One, Fixed64.One);

        Assert.That(contentment, Is.EqualTo(Fixed64.One));
    }

    [Test]
    public void EmploymentRatioAboveOneIsCappedRatherThanBoostingContentment()
    {
        var uncapped = ContentmentCalculator.ComputeContentment(Fixed64.FromInt(3), Fixed64.One, Fixed64.One);

        Assert.That(uncapped, Is.EqualTo(Fixed64.One));
    }

    [Test]
    public void HousingSatisfactionAboveOneIsCappedRatherThanBoostingContentment()
    {
        var uncapped = ContentmentCalculator.ComputeContentment(Fixed64.One, Fixed64.FromInt(3), Fixed64.One);

        Assert.That(uncapped, Is.EqualTo(Fixed64.One));
    }

    [Test]
    public void LowInputsProduceLowContentment()
    {
        var contentment = ContentmentCalculator.ComputeContentment(Fixed64.Zero, Fixed64.Zero, Fixed64.Zero);

        Assert.That(contentment, Is.EqualTo(Fixed64.Zero));
    }

    [Test]
    public void HealthExposureIsZeroWhenHousingIsAdequate()
    {
        Assert.That(ContentmentCalculator.ComputeHealthExposure(Fixed64.One), Is.EqualTo(Fixed64.Zero));
        Assert.That(ContentmentCalculator.ComputeHealthExposure(Fixed64.FromInt(2)), Is.EqualTo(Fixed64.Zero));
    }

    [Test]
    public void HealthExposureRisesAsHousingSatisfactionFallsBelowOne()
    {
        var halfSatisfied = Fixed64.FromRaw(500_000);

        var exposure = ContentmentCalculator.ComputeHealthExposure(halfSatisfied);

        Assert.That(exposure, Is.EqualTo(Fixed64.FromRaw(500_000)));
    }
}
