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
}
