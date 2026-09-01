using Gens.Simulation.Health;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Health;

public sealed class EpidemicSpreadCalculatorTests
{
    [Test]
    public void IgnitionProbabilityScalesWithSanitationMultiplierAndIsPositiveAtBaseline()
    {
        Assert.That(EpidemicSpreadCalculator.MonthlyIgnitionProbability(1.0), Is.GreaterThan(0.0));
        Assert.That(
            EpidemicSpreadCalculator.MonthlyIgnitionProbability(0.5),
            Is.LessThan(EpidemicSpreadCalculator.MonthlyIgnitionProbability(1.0)));
    }

    [Test]
    public void HouseholdContactSpreadIsZeroWithNoInfectedMembers()
    {
        Assert.That(EpidemicSpreadCalculator.HouseholdContactSpreadProbability(0, 1.0, 1.0, 1.0), Is.EqualTo(0.0));
    }

    [Test]
    public void HouseholdContactSpreadRisesWithMoreInfectedMembers()
    {
        var oneSource = EpidemicSpreadCalculator.HouseholdContactSpreadProbability(1, 1.0, 1.0, 1.0);
        var threeSources = EpidemicSpreadCalculator.HouseholdContactSpreadProbability(3, 1.0, 1.0, 1.0);
        Assert.That(threeSources, Is.GreaterThan(oneSource));
    }

    [Test]
    public void HouseholdContactSpreadIsReducedBySanitationAndQuarantineMultipliers()
    {
        var baseline = EpidemicSpreadCalculator.HouseholdContactSpreadProbability(2, 1.0, 1.0, 1.0);
        var sanitized = EpidemicSpreadCalculator.HouseholdContactSpreadProbability(2, 0.5, 1.0, 1.0);
        var quarantinedSource = EpidemicSpreadCalculator.HouseholdContactSpreadProbability(2, 1.0, 0.2, 1.0);
        var quarantinedSettlement = EpidemicSpreadCalculator.HouseholdContactSpreadProbability(2, 1.0, 1.0, 0.35);

        Assert.That(sanitized, Is.LessThan(baseline));
        Assert.That(quarantinedSource, Is.LessThan(baseline));
        Assert.That(quarantinedSettlement, Is.LessThan(baseline));
    }

    [Test]
    public void WaterborneSpreadIsZeroWithNoActiveCasesAndRisesWithMore()
    {
        Assert.That(EpidemicSpreadCalculator.WaterborneSpreadProbability(0, 1.0, 1.0), Is.EqualTo(0.0));
        var few = EpidemicSpreadCalculator.WaterborneSpreadProbability(1, 1.0, 1.0);
        var many = EpidemicSpreadCalculator.WaterborneSpreadProbability(10, 1.0, 1.0);
        Assert.That(many, Is.GreaterThan(few));
    }

    [Test]
    public void WaterborneSpreadIsReducedBySanitationAndSettlementQuarantine()
    {
        var baseline = EpidemicSpreadCalculator.WaterborneSpreadProbability(5, 1.0, 1.0);
        var sanitized = EpidemicSpreadCalculator.WaterborneSpreadProbability(5, 0.5, 1.0);
        var quarantined = EpidemicSpreadCalculator.WaterborneSpreadProbability(5, 1.0, 0.35);

        Assert.That(sanitized, Is.LessThan(baseline));
        Assert.That(quarantined, Is.LessThan(baseline));
    }

    [Test]
    public void EveryProbabilityStaysWithinZeroToOne()
    {
        Assert.That(EpidemicSpreadCalculator.HouseholdContactSpreadProbability(50, 1.0, 1.0, 1.0), Is.InRange(0.0, 1.0));
        Assert.That(EpidemicSpreadCalculator.WaterborneSpreadProbability(1000, 1.0, 1.0), Is.InRange(0.0, 1.0));
    }
}
