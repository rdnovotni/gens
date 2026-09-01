using Gens.Simulation.Hazards;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Hazards;

public sealed class DisasterSeverityCalculatorTests
{
    [Test]
    public void IgnitionProbabilityIsZeroAtZeroExposureAndRisesWithExposure()
    {
        Assert.That(DisasterSeverityCalculator.MonthlyIgnitionProbability(0), Is.EqualTo(0.0));
        Assert.That(
            DisasterSeverityCalculator.MonthlyIgnitionProbability(100),
            Is.GreaterThan(DisasterSeverityCalculator.MonthlyIgnitionProbability(50)));
    }

    [Test]
    public void IgnitionProbabilityNeverReachesCertaintyEvenAtMaximumExposure()
    {
        Assert.That(DisasterSeverityCalculator.MonthlyIgnitionProbability(100), Is.LessThan(1.0));
    }

    [Test]
    public void LowExposureSkewsSeverityTowardMinor()
    {
        var roll = DisasterSeverityCalculator.RollPrecision / 2;
        Assert.That(DisasterSeverityCalculator.RollSeverity(0, roll), Is.EqualTo(DisasterSeverity.Minor));
    }

    [Test]
    public void HighExposureShiftsRealMassTowardCatastrophicAndSevere()
    {
        var topOfRange = (uint)(DisasterSeverityCalculator.RollPrecision * 0.1);
        Assert.That(DisasterSeverityCalculator.RollSeverity(100, topOfRange), Is.EqualTo(DisasterSeverity.Catastrophic));
    }

    [Test]
    public void SeverityRollIsMonotonicAcrossTheFullPrecisionRange()
    {
        var lowExposureCatastrophicShare = CountAtSeverity(0, DisasterSeverity.Catastrophic);
        var highExposureCatastrophicShare = CountAtSeverity(100, DisasterSeverity.Catastrophic);
        Assert.That(highExposureCatastrophicShare, Is.GreaterThan(lowExposureCatastrophicShare));
    }

    private static int CountAtSeverity(int exposure, DisasterSeverity severity)
    {
        var count = 0;
        for (uint roll = 0; roll < DisasterSeverityCalculator.RollPrecision; roll += 10_000)
        {
            if (DisasterSeverityCalculator.RollSeverity(exposure, roll) == severity)
                count++;
        }

        return count;
    }
}
