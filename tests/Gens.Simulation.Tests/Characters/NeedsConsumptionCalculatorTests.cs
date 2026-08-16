using Gens.Simulation.Characters;
using Gens.Simulation.Numerics;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class NeedsConsumptionCalculatorTests
{
    [TestCase(DietTier.Meager, 10, 10L)]
    [TestCase(DietTier.Adequate, 10, 20L)]
    [TestCase(DietTier.Generous, 10, 30L)]
    public void ComputeDemandScalesLinearlyWithSizeByTier(DietTier tier, int size, long expected)
    {
        Assert.That(NeedsConsumptionCalculator.ComputeDemand(tier, size), Is.EqualTo(expected));
    }

    [Test]
    public void ComputeDemandIsZeroForAnEmptyGroup()
    {
        Assert.That(NeedsConsumptionCalculator.ComputeDemand(DietTier.Generous, 0), Is.EqualTo(0L));
    }

    [Test]
    public void SatisfactionRisesWithDietTier()
    {
        var meager = NeedsConsumptionCalculator.SatisfactionFor(DietTier.Meager);
        var adequate = NeedsConsumptionCalculator.SatisfactionFor(DietTier.Adequate);
        var generous = NeedsConsumptionCalculator.SatisfactionFor(DietTier.Generous);

        Assert.That(meager, Is.LessThan(adequate));
        Assert.That(adequate, Is.LessThan(generous));
        Assert.That(generous, Is.EqualTo(Fixed64.One));
    }
}
