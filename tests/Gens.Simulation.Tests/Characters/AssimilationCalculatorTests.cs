using Gens.Simulation.Characters;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class AssimilationCalculatorTests
{
    [Test]
    public void ApplyShiftsEachTierTowardCitizenshipByAFlooredMonthlyRate()
    {
        // 0.005 * 1,000 = 5 exactly at every tier.
        var distribution = new LegalStatusDistribution(0, 1_000, 1_000, 1_000);

        var result = AssimilationCalculator.Apply(distribution);

        Assert.That(result.Citizen, Is.EqualTo(5));
        Assert.That(result.LatinRights, Is.EqualTo(1_000 - 5 + 5)); // Loses 5 to Citizen, gains 5 from Freedman.
        Assert.That(result.Freedman, Is.EqualTo(1_000 - 5 + 5)); // Loses 5 to LatinRights, gains 5 from Peregrine.
        Assert.That(result.Peregrine, Is.EqualTo(1_000 - 5));
        Assert.That(result.Total, Is.EqualTo(distribution.Total));
    }

    [Test]
    public void ApplyDoesNotDoubleHopATierInOneMonth()
    {
        // A tiny Freedman-only population: if Freedman→LatinRights and LatinRights→Citizen both fired
        // against the *same* freshly-arrived unit in one call, Citizen would go positive. It must stay
        // untouched this month.
        var distribution = new LegalStatusDistribution(0, 0, 0, 1_000);

        var result = AssimilationCalculator.Apply(distribution);

        Assert.That(result.Citizen, Is.EqualTo(0));
        Assert.That(result.LatinRights, Is.EqualTo(5));
        Assert.That(result.Freedman, Is.EqualTo(995));
    }

    [Test]
    public void ApplyIsANoOpOnAnEmptyDistribution()
    {
        var result = AssimilationCalculator.Apply(LegalStatusDistribution.Empty);

        Assert.That(result, Is.EqualTo(LegalStatusDistribution.Empty));
    }

    [Test]
    public void ApplyFloorsFractionalRemaindersToZeroForASmallTier()
    {
        var distribution = new LegalStatusDistribution(0, 0, 0, 10); // 0.005 * 10 = 0.05, floors to 0.

        var result = AssimilationCalculator.Apply(distribution);

        Assert.That(result, Is.EqualTo(distribution));
    }
}
