using Gens.Simulation.Health;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Health;

public sealed class CareCapacityCalculatorTests
{
    [TestCase(0)]
    [TestCase(-5)]
    public void NoPhysicianMeansZeroCapacity(int skill)
    {
        Assert.That(CareCapacityCalculator.MonthlyCareCapacity(skill), Is.EqualTo(0));
    }

    [Test]
    public void CapacityScalesWithSkillAndClampsToFive()
    {
        Assert.That(CareCapacityCalculator.MonthlyCareCapacity(1), Is.EqualTo(1));
        Assert.That(CareCapacityCalculator.MonthlyCareCapacity(25), Is.EqualTo(2));
        Assert.That(CareCapacityCalculator.MonthlyCareCapacity(100), Is.EqualTo(5));
        Assert.That(CareCapacityCalculator.MonthlyCareCapacity(1000), Is.EqualTo(5));
    }
}
