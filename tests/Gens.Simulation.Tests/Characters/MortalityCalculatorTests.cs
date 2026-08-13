using Gens.Simulation.Characters;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class MortalityCalculatorTests
{
    [Test]
    public void AdultBaselineRiskIsLow()
    {
        var probability = MortalityCalculator.MonthlyProbability(LifecycleStage.Adult, 30, 50);

        Assert.That(probability, Is.LessThan(0.001));
    }

    [Test]
    public void InfantRiskIsHigherThanAdultAtNeutralHealth()
    {
        var infant = MortalityCalculator.MonthlyProbability(LifecycleStage.Infant, 1, 50);
        var adult = MortalityCalculator.MonthlyProbability(LifecycleStage.Adult, 30, 50);

        Assert.That(infant, Is.GreaterThan(adult));
    }

    [Test]
    public void ElderlyRiskRisesWithAge()
    {
        var youngElderly = MortalityCalculator.MonthlyProbability(LifecycleStage.Elderly, 60, 50);
        var oldElderly = MortalityCalculator.MonthlyProbability(LifecycleStage.Elderly, 90, 50);

        Assert.That(oldElderly, Is.GreaterThan(youngElderly));
    }

    [Test]
    public void ElderlyRiskIsHigherThanAdultAtNeutralHealth()
    {
        var elderly = MortalityCalculator.MonthlyProbability(LifecycleStage.Elderly, 70, 50);
        var adult = MortalityCalculator.MonthlyProbability(LifecycleStage.Adult, 40, 50);

        Assert.That(elderly, Is.GreaterThan(adult));
    }

    [Test]
    public void LowHealthIncreasesRiskRelativeToNeutral()
    {
        var lowHealth = MortalityCalculator.MonthlyProbability(LifecycleStage.Adult, 30, 10);
        var neutralHealth = MortalityCalculator.MonthlyProbability(LifecycleStage.Adult, 30, 50);

        Assert.That(lowHealth, Is.GreaterThan(neutralHealth));
    }

    [Test]
    public void HighHealthDecreasesRiskRelativeToNeutral()
    {
        var highHealth = MortalityCalculator.MonthlyProbability(LifecycleStage.Adult, 30, 100);
        var neutralHealth = MortalityCalculator.MonthlyProbability(LifecycleStage.Adult, 30, 50);

        Assert.That(highHealth, Is.LessThan(neutralHealth));
    }

    [TestCase(LifecycleStage.Infant, 0, 0)]
    [TestCase(LifecycleStage.Child, 8, 50)]
    [TestCase(LifecycleStage.Adolescent, 15, 100)]
    [TestCase(LifecycleStage.Adult, 40, 50)]
    [TestCase(LifecycleStage.Elderly, 120, 0)]
    public void ResultIsAlwaysAValidProbability(LifecycleStage stage, int ageInYears, int health)
    {
        var probability = MortalityCalculator.MonthlyProbability(stage, ageInYears, health);

        Assert.That(probability, Is.GreaterThanOrEqualTo(0.0));
        Assert.That(probability, Is.LessThan(1.0));
    }
}
