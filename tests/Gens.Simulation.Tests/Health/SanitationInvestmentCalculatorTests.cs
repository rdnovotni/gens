using Gens.Simulation.Health;
using Gens.Simulation.Ledger;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Health;

public sealed class SanitationInvestmentCalculatorTests
{
    [Test]
    public void ExposureMultiplierStrictlyDecreasesAsTierRises()
    {
        var minimal = SanitationInvestmentCalculator.ExposureMultiplier(SanitationInvestmentTier.Minimal);
        var standard = SanitationInvestmentCalculator.ExposureMultiplier(SanitationInvestmentTier.Standard);
        var comprehensive = SanitationInvestmentCalculator.ExposureMultiplier(SanitationInvestmentTier.Comprehensive);

        Assert.That(minimal, Is.EqualTo(1.0));
        Assert.That(standard, Is.LessThan(minimal));
        Assert.That(comprehensive, Is.LessThan(standard));
    }

    [Test]
    public void MonthlyTreasuryCostStrictlyIncreasesAsTierRisesAndMinimalIsFree()
    {
        var minimal = SanitationInvestmentCalculator.MonthlyTreasuryCost(SanitationInvestmentTier.Minimal);
        var standard = SanitationInvestmentCalculator.MonthlyTreasuryCost(SanitationInvestmentTier.Standard);
        var comprehensive = SanitationInvestmentCalculator.MonthlyTreasuryCost(SanitationInvestmentTier.Comprehensive);

        Assert.That(minimal, Is.EqualTo(Money.Zero));
        Assert.That(standard, Is.GreaterThan(minimal));
        Assert.That(comprehensive, Is.GreaterThan(standard));
    }
}
