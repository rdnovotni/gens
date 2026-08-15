using Gens.Simulation.Characters;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class LaborOutputCalculatorTests
{
    private static readonly RegimenSettings Neutral = RegimenCatalog.Default;

    [Test]
    public void OutputScalesWithEffectiveSkillAtFullHealthAndLowFatigue()
    {
        var low = LaborOutputCalculator.ComputeOutput(20, 100, 0, Neutral);
        var high = LaborOutputCalculator.ComputeOutput(80, 100, 0, Neutral);

        Assert.That(high, Is.GreaterThan(low));
    }

    [Test]
    public void HeavyFatigueCapsOutputRegardlessOfSkill()
    {
        var lowFatigue = LaborOutputCalculator.ComputeOutput(90, 100, 20, Neutral);
        var heavyFatigue = LaborOutputCalculator.ComputeOutput(90, 100, 100, Neutral);

        Assert.That(heavyFatigue, Is.LessThan(lowFatigue));
    }

    [Test]
    public void LowHealthReducesOutputProportionally()
    {
        var fullHealth = LaborOutputCalculator.ComputeOutput(80, 100, 0, Neutral);
        var halfHealth = LaborOutputCalculator.ComputeOutput(80, 50, 0, Neutral);

        Assert.That(halfHealth, Is.EqualTo(fullHealth / 2));
    }

    [Test]
    public void HarshDisciplineRaisesTheOutputCeilingComparedToLenient()
    {
        var lenient = new RegimenSettings(DietTier.Adequate, AccommodationTier.Basic, FreedomsTier.Restricted, DisciplineTier.Lenient);
        var harsh = new RegimenSettings(DietTier.Adequate, AccommodationTier.Basic, FreedomsTier.Restricted, DisciplineTier.Harsh);

        var lenientOutput = LaborOutputCalculator.ComputeOutput(50, 100, 0, lenient);
        var harshOutput = LaborOutputCalculator.ComputeOutput(50, 100, 0, harsh);

        Assert.That(harshOutput, Is.GreaterThan(lenientOutput));
    }

    [Test]
    public void OutputNeverLeavesTheZeroToOneHundredRange()
    {
        foreach (var skill in new[] { 0, 50, 100 })
        {
            foreach (var health in new[] { 0, 50, 100 })
            {
                foreach (var fatigue in new[] { 0, 50, 100 })
                {
                    var output = LaborOutputCalculator.ComputeOutput(skill, health, fatigue, Neutral);
                    Assert.That(output, Is.InRange(0, 100));
                }
            }
        }
    }

    [Test]
    public void MonthlyFatigueAccrualNeverExceedsOneHundred()
    {
        Assert.That(LaborOutputCalculator.ApplyMonthlyFatigueAccrual(95), Is.EqualTo(100));
    }

    [Test]
    public void MonthlyHealthTrendNeverGoesBelowZero()
    {
        var harsh = new RegimenSettings(DietTier.Meager, AccommodationTier.Bare, FreedomsTier.Confined, DisciplineTier.Harsh);
        Assert.That(LaborOutputCalculator.ApplyMonthlyHealthTrend(0, harsh), Is.EqualTo(0));
    }

    [Test]
    public void MonthlyLoyaltyTrendNeverExceedsOneHundred()
    {
        var generous = new RegimenSettings(DietTier.Generous, AccommodationTier.Comfortable, FreedomsTier.FreeMovement, DisciplineTier.Lenient);
        Assert.That(LaborOutputCalculator.ApplyMonthlyLoyaltyTrend(100, generous), Is.EqualTo(100));
    }
}
