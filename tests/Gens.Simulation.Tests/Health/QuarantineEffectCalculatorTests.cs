using Gens.Simulation.Health;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Health;

public sealed class QuarantineEffectCalculatorTests
{
    [Test]
    public void PersonalSpreadMultiplierIsReducedWhenQuarantinedAndFullOtherwise()
    {
        Assert.That(QuarantineEffectCalculator.PersonalSpreadMultiplier(quarantined: false), Is.EqualTo(1.0));
        Assert.That(QuarantineEffectCalculator.PersonalSpreadMultiplier(quarantined: true), Is.LessThan(1.0));
        Assert.That(QuarantineEffectCalculator.PersonalSpreadMultiplier(quarantined: true), Is.GreaterThan(0.0));
    }

    [Test]
    public void SettlementSpreadMultiplierIsFullWhenNoQuarantineIsActive()
    {
        Assert.That(QuarantineEffectCalculator.SettlementSpreadMultiplier(settlementQuarantineActive: false, imperialScale: false), Is.EqualTo(1.0));
        Assert.That(QuarantineEffectCalculator.SettlementSpreadMultiplier(settlementQuarantineActive: false, imperialScale: true), Is.EqualTo(1.0));
    }

    [Test]
    public void ImperialScaleQuarantineIsMeaningfullyLessEffectiveThanLocalQuarantine()
    {
        var local = QuarantineEffectCalculator.SettlementSpreadMultiplier(settlementQuarantineActive: true, imperialScale: false);
        var imperial = QuarantineEffectCalculator.SettlementSpreadMultiplier(settlementQuarantineActive: true, imperialScale: true);

        Assert.That(local, Is.LessThan(1.0));
        Assert.That(imperial, Is.LessThan(1.0));
        Assert.That(imperial, Is.GreaterThan(local));
    }
}
