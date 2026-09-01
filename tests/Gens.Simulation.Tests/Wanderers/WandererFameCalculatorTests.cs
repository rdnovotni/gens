using Gens.Simulation.Wanderers;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Wanderers;

public sealed class WandererFameCalculatorTests
{
    [Test]
    public void FameIsClampedToTheSameZeroToHundredRangeTheUniversalCharacterFieldUses()
    {
        Assert.Multiple(() =>
        {
            Assert.That(WandererFameCalculator.ApplyDelta(98, 10), Is.EqualTo(100));
            Assert.That(WandererFameCalculator.ApplyDelta(3, -10), Is.EqualTo(0));
            Assert.That(WandererFameCalculator.ApplyDelta(50, 7), Is.EqualTo(57));
        });
    }

    [Test]
    public void ObscurityCostsNothingInsideTheGracePeriodAndBitesAfterIt()
    {
        Assert.Multiple(() =>
        {
            Assert.That(WandererFameCalculator.MonthlyObscurityDecay(0), Is.Zero);
            Assert.That(
                WandererFameCalculator.MonthlyObscurityDecay(WandererFameCalculator.ObscurityGracePeriodMonths - 1),
                Is.Zero);
            Assert.That(
                WandererFameCalculator.MonthlyObscurityDecay(WandererFameCalculator.ObscurityGracePeriodMonths),
                Is.EqualTo(WandererFameCalculator.ObscurityDecayPerMonth));
            Assert.That(
                WandererFameCalculator.MonthlyObscurityDecay(WandererFameCalculator.ObscurityGracePeriodMonths + 40),
                Is.EqualTo(WandererFameCalculator.ObscurityDecayPerMonth));
        });
    }

    [Test]
    public void ANegativeObscurityCounterIsRejected()
    {
        Assert.That(
            () => WandererFameCalculator.MonthlyObscurityDecay(-1),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void TheTrendMirrorsTheRivalHouseRisingEstablishedDecliningShape()
    {
        Assert.Multiple(() =>
        {
            Assert.That(WandererFameCalculator.Trend(40, 45), Is.EqualTo(WandererFameTrend.Rising));
            Assert.That(WandererFameCalculator.Trend(40, 40), Is.EqualTo(WandererFameTrend.Established));
            Assert.That(WandererFameCalculator.Trend(40, 39), Is.EqualTo(WandererFameTrend.Declining));
        });
    }

    [Test]
    public void AFameCappedAtOneHundredReadsAsEstablishedRatherThanRising()
    {
        // The clamp, not the intent, decides the trend — a Wanderer already at the ceiling has stopped
        // rising however successful the engagement was.
        var newFame = WandererFameCalculator.ApplyDelta(100, 5);

        Assert.That(WandererFameCalculator.Trend(100, newFame), Is.EqualTo(WandererFameTrend.Established));
    }

    [Test]
    public void OnlyASufficientlyFamousWandererIsAnObjectOfCompetition()
    {
        Assert.Multiple(() =>
        {
            Assert.That(
                WandererFameCalculator.IsCompetitionVisible(WandererFameCalculator.CompetitionVisibilityThreshold - 1),
                Is.False);
            Assert.That(
                WandererFameCalculator.IsCompetitionVisible(WandererFameCalculator.CompetitionVisibilityThreshold),
                Is.True);
        });
    }

    [Test]
    public void TheStartingFameBandStraddlesTheCompetitionThreshold()
    {
        Assert.Multiple(() =>
        {
            Assert.That(WandererFameCalculator.MinimumStartingFame, Is.LessThan(WandererFameCalculator.CompetitionVisibilityThreshold));
            Assert.That(WandererFameCalculator.MaximumStartingFame, Is.GreaterThanOrEqualTo(WandererFameCalculator.CompetitionVisibilityThreshold));
            Assert.That(WandererFameCalculator.MinimumStartingFame, Is.GreaterThanOrEqualTo(0));
            Assert.That(WandererFameCalculator.MaximumStartingFame, Is.LessThanOrEqualTo(100));
        });
    }

    [Test]
    public void AHighFameWandererDeliversAMoreValuableHostBenefitThanAnObscureOne()
    {
        Assert.Multiple(() =>
        {
            Assert.That(WandererFameCalculator.ScaleByFame(10, 0), Is.EqualTo(5));
            Assert.That(WandererFameCalculator.ScaleByFame(10, 50), Is.EqualTo(10), "Fame 50 reproduces the base amount exactly.");
            Assert.That(WandererFameCalculator.ScaleByFame(10, 100), Is.EqualTo(15));
            Assert.That(WandererFameCalculator.ScaleByFame(10, 0), Is.LessThan(WandererFameCalculator.ScaleByFame(10, 100)));
        });
    }

    [Test]
    public void AScaledBenefitNeverDropsBelowOne()
    {
        Assert.That(WandererFameCalculator.ScaleByFame(1, 0), Is.EqualTo(1));
    }

    [Test]
    public void ScaleByFameRejectsANonPositiveBaseAmountOrAnOutOfRangeFame()
    {
        Assert.Multiple(() =>
        {
            Assert.That(() => WandererFameCalculator.ScaleByFame(0, 50), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => WandererFameCalculator.ScaleByFame(10, -1), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => WandererFameCalculator.ScaleByFame(10, 101), Throws.TypeOf<ArgumentOutOfRangeException>());
        });
    }
}
