using Gens.Simulation.Characters;
using Gens.Simulation.Numerics;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class MigrationCalculatorTests
{
    [Test]
    public void EmigrationRateIsZeroAboveTheThreshold()
    {
        Assert.That(MigrationCalculator.EmigrationRate(Fixed64.FromRaw(400_000)), Is.EqualTo(Fixed64.Zero));
    }

    [Test]
    public void EmigrationRateRisesAsContentmentFallsFurtherBelowTheThreshold()
    {
        var mild = MigrationCalculator.EmigrationRate(Fixed64.FromRaw(300_000));
        var severe = MigrationCalculator.EmigrationRate(Fixed64.Zero);

        Assert.That(severe, Is.GreaterThan(mild));
        Assert.That(mild, Is.GreaterThan(Fixed64.Zero));
    }

    [Test]
    public void ImmigrationRateIsZeroBelowTheContentmentThreshold()
    {
        var rate = MigrationCalculator.ImmigrationRate(Fixed64.FromRaw(500_000), Fixed64.FromInt(2), Fixed64.FromInt(2));

        Assert.That(rate, Is.EqualTo(Fixed64.Zero));
    }

    [Test]
    public void ImmigrationRateIsZeroWithoutOpenEmploymentOrHousing()
    {
        var noJobs = MigrationCalculator.ImmigrationRate(Fixed64.One, Fixed64.FromRaw(500_000), Fixed64.One);
        var noHousing = MigrationCalculator.ImmigrationRate(Fixed64.One, Fixed64.One, Fixed64.FromRaw(500_000));

        Assert.That(noJobs, Is.EqualTo(Fixed64.Zero));
        Assert.That(noHousing, Is.EqualTo(Fixed64.Zero));
    }

    [Test]
    public void ImmigrationRateIsPositiveWhenContentAndRoomy()
    {
        var rate = MigrationCalculator.ImmigrationRate(Fixed64.One, Fixed64.One, Fixed64.One);

        Assert.That(rate, Is.GreaterThan(Fixed64.Zero));
    }
}
