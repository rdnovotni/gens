using Gens.Simulation.Health;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Health;

public sealed class HealthConditionProgressionCalculatorTests
{
    [TestCase(0)]
    [TestCase(50)]
    [TestCase(100)]
    public void AnAcuteCaseDrainsFasterThanAChronicOneAtTheSameSeverity(int severity)
    {
        var acute = HealthConditionProgressionCalculator.MonthlyHealthDrain(HealthConditionCategory.Acute, severity, treated: false);
        var chronic = HealthConditionProgressionCalculator.MonthlyHealthDrain(HealthConditionCategory.Chronic, severity, treated: false);

        Assert.That(acute, Is.GreaterThan(chronic));
    }

    [Test]
    public void TreatmentReducesMonthlyDrain()
    {
        var untreated = HealthConditionProgressionCalculator.MonthlyHealthDrain(HealthConditionCategory.Acute, 100, treated: false);
        var treated = HealthConditionProgressionCalculator.MonthlyHealthDrain(HealthConditionCategory.Acute, 100, treated: true);

        Assert.That(treated, Is.LessThan(untreated));
    }

    [Test]
    public void DrainIsNeverLessThanOne()
    {
        var drain = HealthConditionProgressionCalculator.MonthlyHealthDrain(HealthConditionCategory.Chronic, 0, treated: true);

        Assert.That(drain, Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public void AnAcuteCaseRecoversFasterThanAChronicOne()
    {
        var acute = HealthConditionProgressionCalculator.MonthlyRecoveryProbability(HealthConditionCategory.Acute, hasCure: true, treated: false);
        var chronic = HealthConditionProgressionCalculator.MonthlyRecoveryProbability(HealthConditionCategory.Chronic, hasCure: true, treated: false);

        Assert.That(acute, Is.GreaterThan(chronic));
    }

    [Test]
    public void ACureableConditionRecoversFasterThanAnIncurableOne()
    {
        var curable = HealthConditionProgressionCalculator.MonthlyRecoveryProbability(HealthConditionCategory.Chronic, hasCure: true, treated: false);
        var incurable = HealthConditionProgressionCalculator.MonthlyRecoveryProbability(HealthConditionCategory.Chronic, hasCure: false, treated: false);

        Assert.That(curable, Is.GreaterThan(incurable));
    }

    [Test]
    public void TreatmentImprovesRecoveryOdds()
    {
        var untreated = HealthConditionProgressionCalculator.MonthlyRecoveryProbability(HealthConditionCategory.Chronic, hasCure: true, treated: false);
        var treated = HealthConditionProgressionCalculator.MonthlyRecoveryProbability(HealthConditionCategory.Chronic, hasCure: true, treated: true);

        Assert.That(treated, Is.GreaterThan(untreated));
    }

    [Test]
    public void LowerHealthRaisesFatalityRisk()
    {
        var fullHealth = HealthConditionProgressionCalculator.MonthlyFatalityProbability(HealthConditionCategory.Acute, 100, 100, treated: false);
        var lowHealth = HealthConditionProgressionCalculator.MonthlyFatalityProbability(HealthConditionCategory.Acute, 100, 0, treated: false);

        Assert.That(lowHealth, Is.GreaterThan(fullHealth));
    }

    [Test]
    public void HigherSeverityRaisesFatalityRisk()
    {
        var lowSeverity = HealthConditionProgressionCalculator.MonthlyFatalityProbability(HealthConditionCategory.Acute, 10, 50, treated: false);
        var highSeverity = HealthConditionProgressionCalculator.MonthlyFatalityProbability(HealthConditionCategory.Acute, 100, 50, treated: false);

        Assert.That(highSeverity, Is.GreaterThan(lowSeverity));
    }

    [Test]
    public void TreatmentRoughlyHalvesFatalityRisk()
    {
        var untreated = HealthConditionProgressionCalculator.MonthlyFatalityProbability(HealthConditionCategory.Acute, 100, 0, treated: false);
        var treated = HealthConditionProgressionCalculator.MonthlyFatalityProbability(HealthConditionCategory.Acute, 100, 0, treated: true);

        Assert.That(treated, Is.EqualTo(untreated / 2.0).Within(1e-9));
    }

    [Test]
    public void TreatedSeverityNeverDrifts()
    {
        Assert.That(HealthConditionProgressionCalculator.MonthlySeverityDrift(HealthConditionCategory.Acute, treated: true), Is.Zero);
        Assert.That(HealthConditionProgressionCalculator.MonthlySeverityDrift(HealthConditionCategory.Chronic, treated: true), Is.Zero);
    }

    [Test]
    public void UntreatedAcuteSeverityDriftsFasterThanChronic()
    {
        var acute = HealthConditionProgressionCalculator.MonthlySeverityDrift(HealthConditionCategory.Acute, treated: false);
        var chronic = HealthConditionProgressionCalculator.MonthlySeverityDrift(HealthConditionCategory.Chronic, treated: false);

        Assert.That(acute, Is.GreaterThan(chronic));
        Assert.That(chronic, Is.GreaterThan(0));
    }
}
