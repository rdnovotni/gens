using Gens.Simulation.Hazards;
using Gens.Simulation.Numerics;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Hazards;

public sealed class DisasterDamageCalculatorTests
{
    [Test]
    public void BuildingConditionStepsLostRisesWithSeverityAndCatastrophicAlwaysRuinsAPristineBuilding()
    {
        Assert.That(DisasterDamageCalculator.BuildingConditionStepsLost(DisasterSeverity.Minor), Is.EqualTo(1));
        Assert.That(
            DisasterDamageCalculator.BuildingConditionStepsLost(DisasterSeverity.Catastrophic),
            Is.GreaterThanOrEqualTo(DisasterDamageCalculator.BuildingConditionStepsLost(DisasterSeverity.Severe)));
        // Pristine = 4; a Catastrophic hit must remove at least 4 steps to reach Ruined = 0.
        Assert.That(DisasterDamageCalculator.BuildingConditionStepsLost(DisasterSeverity.Catastrophic), Is.GreaterThanOrEqualTo(4));
    }

    [Test]
    public void FrostStepsLostMatchesOrdinaryStepsBelowSevereButJumpsToTheHarshestFigureAtSevereAndAbove()
    {
        Assert.That(
            DisasterDamageCalculator.FrostBuildingConditionStepsLost(DisasterSeverity.Minor),
            Is.EqualTo(DisasterDamageCalculator.BuildingConditionStepsLost(DisasterSeverity.Minor)));
        Assert.That(
            DisasterDamageCalculator.FrostBuildingConditionStepsLost(DisasterSeverity.Moderate),
            Is.EqualTo(DisasterDamageCalculator.BuildingConditionStepsLost(DisasterSeverity.Moderate)));
        Assert.That(
            DisasterDamageCalculator.FrostBuildingConditionStepsLost(DisasterSeverity.Severe),
            Is.EqualTo(DisasterDamageCalculator.BuildingConditionStepsLost(DisasterSeverity.Catastrophic)));
        Assert.That(
            DisasterDamageCalculator.FrostBuildingConditionStepsLost(DisasterSeverity.Catastrophic),
            Is.EqualTo(DisasterDamageCalculator.BuildingConditionStepsLost(DisasterSeverity.Catastrophic)));
    }

    [Test]
    public void CatastrophicPopulationLossFractionIsZeroBelowCatastrophic()
    {
        Assert.That(DisasterDamageCalculator.CatastrophicPopulationLossFraction(DisasterSeverity.Minor), Is.EqualTo(0.0));
        Assert.That(DisasterDamageCalculator.CatastrophicPopulationLossFraction(DisasterSeverity.Moderate), Is.EqualTo(0.0));
        Assert.That(DisasterDamageCalculator.CatastrophicPopulationLossFraction(DisasterSeverity.Severe), Is.EqualTo(0.0));
        Assert.That(DisasterDamageCalculator.CatastrophicPopulationLossFraction(DisasterSeverity.Catastrophic), Is.GreaterThan(0.0));
    }

    [Test]
    public void BuildingHitProbabilityRisesWithSeverityAndStaysWithinZeroToOne()
    {
        var minor = DisasterDamageCalculator.BuildingHitProbability(DisasterSeverity.Minor);
        var catastrophic = DisasterDamageCalculator.BuildingHitProbability(DisasterSeverity.Catastrophic);
        Assert.That(catastrophic, Is.GreaterThan(minor));
        Assert.That(catastrophic, Is.InRange(0.0, 1.0));
    }

    [Test]
    public void ContentmentImpactIsNegativeAndGrowsHarsherWithSeverity()
    {
        var minor = DisasterDamageCalculator.ContentmentImpact(DisasterSeverity.Minor);
        var catastrophic = DisasterDamageCalculator.ContentmentImpact(DisasterSeverity.Catastrophic);
        Assert.That(minor, Is.LessThan(Fixed64.Zero));
        Assert.That(catastrophic, Is.LessThan(minor));
    }
}
