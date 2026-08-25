using System.Linq;
using Gens.Simulation.Actors;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Actors;

/// <summary>Phase 10 item 3 ("<c>LivingWorldActor</c> framework") data-model coverage, mirroring
/// <see cref="Land.Region"/>'s equivalent construction-validation test shape.</summary>
public sealed class LivingWorldActorTests
{
    private static LivingWorldActor CreateAncientGens(WorldState state, string name = "Gens Valeria") =>
        LivingWorldActor.Create(
            state.ActorIds.Issue(),
            LivingWorldActorType.Gens,
            name,
            LivingWorldActorTier.Background,
            LivingWorldActorStandingTrend.Established,
            LivingWorldActorOrigin.Ancient,
            parentActorId: null,
            new LivingWorldActorIdentity(EconomicIdentityTag.Agrarian, FactionTag.Traditionalist),
            dignitas: 10,
            new LivingWorldActorNetWorth(HouseholdWealthBand.Comfortable, Figure: null),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest),
            state.RegionIds.Issue(),
            state.SettlementIds.Issue());

    [Test]
    public void CreateBuildsAnAncientBackgroundActorWithNoParentAndNoHead()
    {
        var state = new WorldState(new GameDate(0));
        var actor = CreateAncientGens(state);

        Assert.Multiple(() =>
        {
            Assert.That(actor.ParentActorId, Is.Null);
            Assert.That(actor.HeadCharacterId, Is.Null);
            Assert.That(actor.Tier, Is.EqualTo(LivingWorldActorTier.Background));
            Assert.That(actor.OriginStory, Is.EqualTo(LivingWorldActorOrigin.Ancient));
        });
    }

    [Test]
    public void CreateRejectsAnEmptyName()
    {
        var state = new WorldState(new GameDate(0));

        Assert.Throws<ArgumentException>(() => LivingWorldActor.Create(
            state.ActorIds.Issue(),
            LivingWorldActorType.Gens,
            " ",
            LivingWorldActorTier.Background,
            LivingWorldActorStandingTrend.Established,
            LivingWorldActorOrigin.Ancient,
            parentActorId: null,
            LivingWorldActorIdentity.None,
            dignitas: 0,
            new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Negligible),
            state.RegionIds.Issue(),
            state.SettlementIds.Issue()));
    }

    [Test]
    public void CreateRequiresAParentActorIdForACadetBranch()
    {
        var state = new WorldState(new GameDate(0));

        Assert.Throws<ArgumentException>(() => LivingWorldActor.Create(
            state.ActorIds.Issue(),
            LivingWorldActorType.Gens,
            "Gens Valeria Minor",
            LivingWorldActorTier.Background,
            LivingWorldActorStandingTrend.Rising,
            LivingWorldActorOrigin.CadetBranch,
            parentActorId: null,
            LivingWorldActorIdentity.None,
            dignitas: 0,
            new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Negligible),
            state.RegionIds.Issue(),
            state.SettlementIds.Issue()));
    }

    [Test]
    public void CreateRejectsAParentActorIdOnANonCadetBranchOrigin()
    {
        var state = new WorldState(new GameDate(0));
        var parent = CreateAncientGens(state, "Gens Valeria");

        Assert.Throws<ArgumentException>(() => LivingWorldActor.Create(
            state.ActorIds.Issue(),
            LivingWorldActorType.Gens,
            "Gens Cornelia",
            LivingWorldActorTier.Background,
            LivingWorldActorStandingTrend.Rising,
            LivingWorldActorOrigin.NovusHomo,
            parentActorId: parent.ActorId,
            LivingWorldActorIdentity.None,
            dignitas: 0,
            new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Negligible),
            state.RegionIds.Issue(),
            state.SettlementIds.Issue()));
    }

    [Test]
    public void CreateAcceptsAValidCadetBranch()
    {
        var state = new WorldState(new GameDate(0));
        var parent = CreateAncientGens(state, "Gens Valeria");

        var cadet = LivingWorldActor.Create(
            state.ActorIds.Issue(),
            LivingWorldActorType.Gens,
            "Gens Valeria Minor",
            LivingWorldActorTier.Background,
            LivingWorldActorStandingTrend.Rising,
            LivingWorldActorOrigin.CadetBranch,
            parentActorId: parent.ActorId,
            LivingWorldActorIdentity.None,
            dignitas: 0,
            new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Negligible),
            state.RegionIds.Issue(),
            state.SettlementIds.Issue());

        Assert.That(cadet.ParentActorId, Is.EqualTo(parent.ActorId));
    }

    [Test]
    public void ActorsRegistryKeepsAscendingIdOrderRegardlessOfInsertionOrder()
    {
        var state = new WorldState(new GameDate(0));
        var first = CreateAncientGens(state, "Gens Aemilia");
        var second = CreateAncientGens(state, "Gens Cornelia");

        state.Actors.Add(second.ActorId, second);
        state.Actors.Add(first.ActorId, first);

        var orderedNames = state.Actors.InAscendingOrder().Select(e => e.Value.Name).ToArray();
        Assert.That(orderedNames, Is.EqualTo(new[] { "Gens Aemilia", "Gens Cornelia" }));
    }
}
