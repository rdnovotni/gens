using Gens.Simulation.Health;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Health;

public sealed class EndemicExposureCalculatorTests
{
    [Test]
    public void RomanFeverProbabilityRisesWithMarshFractionAndIsZeroWithNoMarsh()
    {
        Assert.That(EndemicExposureCalculator.RomanFeverMonthlyProbability(0.0), Is.EqualTo(0.0));
        Assert.That(
            EndemicExposureCalculator.RomanFeverMonthlyProbability(1.0),
            Is.GreaterThan(EndemicExposureCalculator.RomanFeverMonthlyProbability(0.5)));
    }

    [Test]
    public void TheFluxProbabilityIsAFixedPositiveBaseline()
    {
        Assert.That(EndemicExposureCalculator.TheFluxMonthlyProbability(), Is.GreaterThan(0.0));
    }

    [Test]
    public void ConsumptionProbabilityRisesWithCrowding()
    {
        Assert.That(EndemicExposureCalculator.ConsumptionMonthlyProbability(0.0), Is.EqualTo(0.0));
        Assert.That(
            EndemicExposureCalculator.ConsumptionMonthlyProbability(3.0),
            Is.GreaterThan(EndemicExposureCalculator.ConsumptionMonthlyProbability(1.0)));
    }

    [Test]
    public void LeprosyProbabilityIsAFlatRareBaseline()
    {
        Assert.That(EndemicExposureCalculator.LeprosyMonthlyProbability(), Is.GreaterThan(0.0));
        Assert.That(EndemicExposureCalculator.LeprosyMonthlyProbability(), Is.LessThan(EndemicExposureCalculator.TheFluxMonthlyProbability()));
    }

    [Test]
    public void GoutProbabilityIsHigherForALavishSettlement()
    {
        Assert.That(EndemicExposureCalculator.GoutMonthlyProbability(true), Is.GreaterThan(EndemicExposureCalculator.GoutMonthlyProbability(false)));
    }

    [Test]
    public void OphthalmiaProbabilityIsAFlatPositiveBaseline()
    {
        Assert.That(EndemicExposureCalculator.OphthalmiaMonthlyProbability(), Is.GreaterThan(0.0));
    }

    [Test]
    public void SaturnismProbabilityTakesTheLargerOfItsTwoUnrelatedDrivers()
    {
        var wealthOnly = EndemicExposureCalculator.SaturnismMonthlyProbability(settlementIsLavish: true, hillsFraction: 0.0);
        var miningOnly = EndemicExposureCalculator.SaturnismMonthlyProbability(settlementIsLavish: false, hillsFraction: 1.0);
        var neither = EndemicExposureCalculator.SaturnismMonthlyProbability(settlementIsLavish: false, hillsFraction: 0.0);
        var both = EndemicExposureCalculator.SaturnismMonthlyProbability(settlementIsLavish: true, hillsFraction: 1.0);

        Assert.That(wealthOnly, Is.GreaterThan(neither));
        Assert.That(miningOnly, Is.GreaterThan(neither));
        Assert.That(neither, Is.EqualTo(0.0));
        // The two drivers are an OR, not a sum: having both never exceeds the larger of the two alone.
        Assert.That(both, Is.EqualTo(Math.Max(wealthOnly, miningOnly)));
    }

    [Test]
    public void EveryProbabilityStaysWithinZeroToOne()
    {
        Assert.That(EndemicExposureCalculator.RomanFeverMonthlyProbability(1.0), Is.InRange(0.0, 1.0));
        Assert.That(EndemicExposureCalculator.ConsumptionMonthlyProbability(1000.0), Is.InRange(0.0, 1.0));
        Assert.That(EndemicExposureCalculator.SaturnismMonthlyProbability(true, 1.0), Is.InRange(0.0, 1.0));
    }
}
