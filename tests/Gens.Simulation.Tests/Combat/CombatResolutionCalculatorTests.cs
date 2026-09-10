using System.Linq;
using Gens.Simulation.Combat;
using Gens.Simulation.Numerics;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Combat;

public sealed class CombatResolutionCalculatorTests
{
    private static readonly CombatSituation Neutral = new(CombatTerrain.Open, Ambush: false, Fortified: false);

    [Test]
    public void EffectiveStrengthFavorsCavalryOnOpenGroundOverHillsAndForest()
    {
        var group = new CombatantGroup(CombatantType.Cavalry, Manpower: 50, EquipmentTier: 2, Readiness: 80, Morale: 80);
        var open = CombatResolutionCalculator.EffectiveStrength(new CombatSide(new[] { group }, null, Neutral));
        var hills = CombatResolutionCalculator.EffectiveStrength(new CombatSide(new[] { group }, null, Neutral with { Terrain = CombatTerrain.Hills }));

        Assert.That(open, Is.GreaterThan(hills));
    }

    [Test]
    public void AbsentCommanderIsAFlatUnfavorableDefaultNotANeutralOne()
    {
        var group = new CombatantGroup(CombatantType.Legionary, Manpower: 50, EquipmentTier: 2, Readiness: 80, Morale: 80);
        var withoutCommander = CombatResolutionCalculator.EffectiveStrength(new CombatSide(new[] { group }, null, Neutral));
        var withGoodCommander = CombatResolutionCalculator.EffectiveStrength(
            new CombatSide(new[] { group }, new CombatantCommander(Fixed64.FromInt(1)), Neutral));

        Assert.That(withoutCommander, Is.LessThan(withGoodCommander));
    }

    [Test]
    public void FortifiedAndAmbushSituationsBoostASidesOwnStrength()
    {
        var group = new CombatantGroup(CombatantType.Militia, Manpower: 30, EquipmentTier: 1, Readiness: 50, Morale: 50);
        var baseline = CombatResolutionCalculator.EffectiveStrength(new CombatSide(new[] { group }, null, Neutral));
        var fortified = CombatResolutionCalculator.EffectiveStrength(new CombatSide(new[] { group }, null, Neutral with { Fortified = true }));
        var ambushed = CombatResolutionCalculator.EffectiveStrength(new CombatSide(new[] { group }, null, Neutral with { Ambush = true }));

        Assert.Multiple(() =>
        {
            Assert.That(fortified, Is.GreaterThan(baseline));
            Assert.That(ambushed, Is.GreaterThan(baseline));
        });
    }

    [Test]
    public void OutcomeTierRisesWithAttackerStrengthAtAFixedVarianceRoll()
    {
        var weak = CombatResolutionCalculator.ResolveOutcomeTier(Fixed64.FromInt(10), Fixed64.FromInt(100), varianceRoll: 500);
        var even = CombatResolutionCalculator.ResolveOutcomeTier(Fixed64.FromInt(100), Fixed64.FromInt(100), varianceRoll: 500);
        var strong = CombatResolutionCalculator.ResolveOutcomeTier(Fixed64.FromInt(100), Fixed64.FromInt(10), varianceRoll: 500);

        Assert.Multiple(() =>
        {
            Assert.That(weak, Is.EqualTo(CombatOutcome.CatastrophicDefeat));
            Assert.That(even, Is.EqualTo(CombatOutcome.RepulsedStalemate));
            Assert.That(strong, Is.EqualTo(CombatOutcome.DecisiveVictory));
        });
    }

    [Test]
    public void SameInputsAndVarianceRollAlwaysProduceTheSameTier()
    {
        var first = CombatResolutionCalculator.ResolveOutcomeTier(Fixed64.FromInt(60), Fixed64.FromInt(40), varianceRoll: 217);
        var second = CombatResolutionCalculator.ResolveOutcomeTier(Fixed64.FromInt(60), Fixed64.FromInt(40), varianceRoll: 217);

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public void MirrorIsAnInvolutionExceptForTheSymmetricStalemateTier()
    {
        Assert.Multiple(() =>
        {
            Assert.That(CombatResolutionCalculator.Mirror(CombatOutcome.DecisiveVictory), Is.EqualTo(CombatOutcome.CatastrophicDefeat));
            Assert.That(CombatResolutionCalculator.Mirror(CombatOutcome.CatastrophicDefeat), Is.EqualTo(CombatOutcome.DecisiveVictory));
            Assert.That(CombatResolutionCalculator.Mirror(CombatOutcome.CostlyVictory), Is.EqualTo(CombatOutcome.Defeat));
            Assert.That(CombatResolutionCalculator.Mirror(CombatOutcome.Defeat), Is.EqualTo(CombatOutcome.CostlyVictory));
            Assert.That(CombatResolutionCalculator.Mirror(CombatOutcome.RepulsedStalemate), Is.EqualTo(CombatOutcome.RepulsedStalemate));
        });
    }

    [Test]
    public void LossesNeverExceedTheGroupsOwnManpowerReadinessOrMorale()
    {
        var groups = new[]
        {
            new CombatantGroup(CombatantType.Legionary, Manpower: 40, EquipmentTier: 2, Readiness: 20, Morale: 10),
            new CombatantGroup(CombatantType.Cavalry, Manpower: 15, EquipmentTier: 3, Readiness: 90, Morale: 95),
        };

        var losses = CombatResolutionCalculator.ResolveLosses(groups, CombatOutcome.CatastrophicDefeat);

        Assert.Multiple(() =>
        {
            Assert.That(losses, Has.Count.EqualTo(2));
            for (var index = 0; index < groups.Length; index++)
            {
                Assert.That(losses[index].GroupIndex, Is.EqualTo(index));
                Assert.That(losses[index].Casualties, Is.LessThanOrEqualTo(groups[index].Manpower));
                Assert.That(losses[index].ReadinessLoss, Is.LessThanOrEqualTo(groups[index].Readiness));
                Assert.That(losses[index].MoraleLoss, Is.LessThanOrEqualTo(groups[index].Morale));
            }
        });
    }

    [Test]
    public void DecisiveVictoryCostsFarLessThanCatastrophicDefeat()
    {
        var groups = new[] { new CombatantGroup(CombatantType.Legionary, Manpower: 100, EquipmentTier: 2, Readiness: 100, Morale: 100) };

        var decisive = CombatResolutionCalculator.ResolveLosses(groups, CombatOutcome.DecisiveVictory).Single();
        var catastrophic = CombatResolutionCalculator.ResolveLosses(groups, CombatOutcome.CatastrophicDefeat).Single();

        Assert.That(decisive.Casualties, Is.LessThan(catastrophic.Casualties));
    }
}
