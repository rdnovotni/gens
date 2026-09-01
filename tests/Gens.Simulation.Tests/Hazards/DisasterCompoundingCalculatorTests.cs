using Gens.Simulation.Hazards;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Hazards;

public sealed class DisasterCompoundingCalculatorTests
{
    [Test]
    public void DrySeasonSpansJuneThroughAugust()
    {
        Assert.That(DisasterCompoundingCalculator.IsDrySeasonMonth(DateFor(6)), Is.True);
        Assert.That(DisasterCompoundingCalculator.IsDrySeasonMonth(DateFor(7)), Is.True);
        Assert.That(DisasterCompoundingCalculator.IsDrySeasonMonth(DateFor(8)), Is.True);
        Assert.That(DisasterCompoundingCalculator.IsDrySeasonMonth(DateFor(5)), Is.False);
        Assert.That(DisasterCompoundingCalculator.IsDrySeasonMonth(DateFor(9)), Is.False);
    }

    [Test]
    public void StormSeasonSpansOctoberThroughDecember()
    {
        Assert.That(DisasterCompoundingCalculator.IsStormSeasonMonth(DateFor(10)), Is.True);
        Assert.That(DisasterCompoundingCalculator.IsStormSeasonMonth(DateFor(11)), Is.True);
        Assert.That(DisasterCompoundingCalculator.IsStormSeasonMonth(DateFor(12)), Is.True);
        Assert.That(DisasterCompoundingCalculator.IsStormSeasonMonth(DateFor(9)), Is.False);
        Assert.That(DisasterCompoundingCalculator.IsStormSeasonMonth(DateFor(1)), Is.False);
    }

    [Test]
    public void DrySeasonFireBonusIsOnlyAppliedDuringTheDrySeason()
    {
        Assert.That(DisasterCompoundingCalculator.DrySeasonFireExposureBonus(true), Is.GreaterThan(0));
        Assert.That(DisasterCompoundingCalculator.DrySeasonFireExposureBonus(false), Is.EqualTo(0));
    }

    [Test]
    public void StormSeasonBonusIsOnlyAppliedDuringStormSeason()
    {
        Assert.That(DisasterCompoundingCalculator.StormSeasonExposureBonus(true), Is.GreaterThan(0));
        Assert.That(DisasterCompoundingCalculator.StormSeasonExposureBonus(false), Is.EqualTo(0));
    }

    [Test]
    public void StormToFloodChainProbabilityIsZeroBelowSevereAndRisesAtCatastrophic()
    {
        Assert.That(DisasterCompoundingCalculator.StormToFloodChainProbability(DisasterSeverity.Minor), Is.EqualTo(0.0));
        Assert.That(DisasterCompoundingCalculator.StormToFloodChainProbability(DisasterSeverity.Moderate), Is.EqualTo(0.0));
        Assert.That(DisasterCompoundingCalculator.StormToFloodChainProbability(DisasterSeverity.Severe), Is.GreaterThan(0.0));
        Assert.That(
            DisasterCompoundingCalculator.StormToFloodChainProbability(DisasterSeverity.Catastrophic),
            Is.GreaterThan(DisasterCompoundingCalculator.StormToFloodChainProbability(DisasterSeverity.Severe)));
    }

    [Test]
    public void ChainedFloodSeverityIsNeverBelowModerate()
    {
        Assert.That(DisasterCompoundingCalculator.ChainedFloodSeverity(DisasterSeverity.Severe), Is.EqualTo(DisasterSeverity.Moderate));
        Assert.That(DisasterCompoundingCalculator.ChainedFloodSeverity(DisasterSeverity.Catastrophic), Is.EqualTo(DisasterSeverity.Severe));
    }

    private static GameDate DateFor(int monthOfYear)
    {
        // TotalMonths = 0 is January of the epoch year (GameDate's own doc comment), so month N of that
        // year is TotalMonths = N - 1.
        return new GameDate(monthOfYear - 1);
    }
}
