using Gens.Simulation.Regions;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Regions;

public sealed class DatedRuleTests
{
    [Test]
    public void EffectiveAsOfReturnsBaseValueWhenNoOverrideCovers()
    {
        var rule = new DatedRule<ReputationDualityMode>(ReputationDualityMode.Full);

        Assert.That(rule.EffectiveAsOf(new GameDate(0)), Is.EqualTo(ReputationDualityMode.Full));
    }

    [Test]
    public void EffectiveAsOfResolvesTaperingAcrossTheConquestArcBoundary()
    {
        var boundary = new GameDate(120);
        var rule = new DatedRule<ReputationDualityMode>(
            ReputationDualityMode.Full,
            new[] { new DatedOverride<ReputationDualityMode>(ReputationDualityMode.Tapering, effectiveFrom: boundary) });

        Assert.Multiple(() =>
        {
            Assert.That(rule.EffectiveAsOf(new GameDate(119)), Is.EqualTo(ReputationDualityMode.Full));
            Assert.That(rule.EffectiveAsOf(boundary), Is.EqualTo(ReputationDualityMode.Tapering));
            Assert.That(rule.EffectiveAsOf(new GameDate(121)), Is.EqualTo(ReputationDualityMode.Tapering));
        });
    }

    [Test]
    public void EffectiveAsOfRespectsAClosedWindow()
    {
        var rule = new DatedRule<int>(
            baseValue: 0,
            overrides: new[] { new DatedOverride<int>(1, effectiveFrom: new GameDate(10), effectiveUntil: new GameDate(20)) });

        Assert.Multiple(() =>
        {
            Assert.That(rule.EffectiveAsOf(new GameDate(9)), Is.EqualTo(0));
            Assert.That(rule.EffectiveAsOf(new GameDate(10)), Is.EqualTo(1));
            Assert.That(rule.EffectiveAsOf(new GameDate(19)), Is.EqualTo(1));
            Assert.That(rule.EffectiveAsOf(new GameDate(20)), Is.EqualTo(0));
        });
    }

    [Test]
    public void ConstructorRejectsAnOverrideWhoseFromDoesNotPrecedeUntil()
    {
        Assert.Throws<ArgumentException>(() =>
            new DatedOverride<int>(1, effectiveFrom: new GameDate(10), effectiveUntil: new GameDate(10)));
    }

    [Test]
    public void ConstructorRejectsOverlappingOverrideWindows()
    {
        var overrides = new[]
        {
            new DatedOverride<int>(1, effectiveFrom: new GameDate(0), effectiveUntil: new GameDate(20)),
            new DatedOverride<int>(2, effectiveFrom: new GameDate(10)),
        };

        Assert.Throws<ArgumentException>(() => new DatedRule<int>(0, overrides));
    }
}
