using Gens.Simulation.Characters;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class FlightRiskCalculatorTests
{
    [Test]
    public void LowLoyaltyRaisesRiskComparedToHighLoyalty()
    {
        var neutral = RegimenCatalog.Default;
        var lowLoyaltyRisk = FlightRiskCalculator.ComputeRiskScore(10, neutral);
        var highLoyaltyRisk = FlightRiskCalculator.ComputeRiskScore(90, neutral);

        Assert.That(lowLoyaltyRisk, Is.GreaterThan(highLoyaltyRisk));
    }

    [Test]
    public void HarshConfinedRegimenRaisesRiskComparedToLenientFreeMovement()
    {
        // Both raise risk through different channels (motive vs. opportunity, §7) — confirming
        // neither a purely lenient nor a purely harsh regimen is risk-free.
        var lenientFree = new RegimenSettings(DietTier.Adequate, AccommodationTier.Basic, FreedomsTier.FreeMovement, DisciplineTier.Lenient);
        var harshConfined = new RegimenSettings(DietTier.Adequate, AccommodationTier.Basic, FreedomsTier.Confined, DisciplineTier.Harsh);
        var neutral = RegimenCatalog.Default;

        var neutralRisk = FlightRiskCalculator.ComputeRiskScore(50, neutral);
        var lenientFreeRisk = FlightRiskCalculator.ComputeRiskScore(50, lenientFree);
        var harshConfinedRisk = FlightRiskCalculator.ComputeRiskScore(50, harshConfined);

        Assert.That(lenientFreeRisk, Is.GreaterThan(neutralRisk));
        Assert.That(harshConfinedRisk, Is.GreaterThan(neutralRisk));
    }

    [Test]
    public void RiskScoreNeverLeavesItsDocumentedRange()
    {
        var neutral = RegimenCatalog.Default;
        foreach (var loyalty in new[] { 0, 50, 100 })
        {
            var risk = FlightRiskCalculator.ComputeRiskScore(loyalty, neutral);
            Assert.That(risk, Is.InRange(0, 200));
        }
    }

    [Test]
    public void MonthlyProbabilityThresholdScalesMonotonicallyWithRisk()
    {
        var low = FlightRiskCalculator.MonthlyProbabilityThreshold(10);
        var high = FlightRiskCalculator.MonthlyProbabilityThreshold(150);

        Assert.That(high, Is.GreaterThan(low));
        Assert.That(high, Is.LessThanOrEqualTo(FlightRiskCalculator.RollPrecision));
    }

    [Test]
    public void MonthlyProbabilityThresholdAtZeroRiskIsZero()
    {
        Assert.That(FlightRiskCalculator.MonthlyProbabilityThreshold(0), Is.EqualTo(0u));
    }
}
