using Gens.Simulation.Actors;
using Gens.Simulation.Economy;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Actors;

/// <summary>Phase 10 item 3 ("<c>LivingWorldActor</c> framework and background/noteworthy fidelity
/// tiers") promotion/demotion coverage.</summary>
public sealed class LivingWorldActorTieringServiceTests
{
    private static (WorldState State, LivingWorldActor Actor) CreateBackgroundActor(int atMonth = 0)
    {
        var state = new WorldState(new GameDate(atMonth));
        var actorId = state.ActorIds.Issue();
        var actor = LivingWorldActor.Create(
            actorId,
            LivingWorldActorType.Gens,
            "Gens Valeria",
            LivingWorldActorTier.Background,
            LivingWorldActorStandingTrend.Established,
            LivingWorldActorOrigin.Ancient,
            parentActorId: null,
            LivingWorldActorIdentity.None,
            dignitas: 0,
            new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest),
            state.RegionIds.Issue(),
            state.SettlementIds.Issue());
        state.Actors.Add(actorId, actor);
        return (state, actor);
    }

    [Test]
    public void RecordContactAndPromotePromotesABackgroundActorAndStampsTheContactDate()
    {
        var (state, actor) = CreateBackgroundActor();
        var contactDate = new GameDate(10);

        var promoted = LivingWorldActorTieringService.RecordContactAndPromote(state, actor.ActorId, contactDate);

        Assert.Multiple(() =>
        {
            Assert.That(promoted.Tier, Is.EqualTo(LivingWorldActorTier.Noteworthy));
            Assert.That(promoted.LastContactDate, Is.EqualTo(contactDate));
            Assert.That(state.Actors.TryGet(actor.ActorId, out var stored), Is.True);
            Assert.That(stored, Is.EqualTo(promoted));
        });
    }

    [Test]
    public void RecordContactRefreshesTheDateWithoutChangingTier()
    {
        var (state, actor) = CreateBackgroundActor();

        var touched = LivingWorldActorTieringService.RecordContact(state, actor.ActorId, new GameDate(3));

        Assert.Multiple(() =>
        {
            Assert.That(touched.Tier, Is.EqualTo(LivingWorldActorTier.Background));
            Assert.That(touched.LastContactDate, Is.EqualTo(new GameDate(3)));
        });
    }

    [Test]
    public void DemoteIfQuietIsANoOpForABackgroundActor()
    {
        var (state, actor) = CreateBackgroundActor();

        var result = LivingWorldActorTieringService.DemoteIfQuiet(state, actor.ActorId, new GameDate(1000));

        Assert.That(result.Tier, Is.EqualTo(LivingWorldActorTier.Background));
    }

    [Test]
    public void DemoteIfQuietLeavesARecentlyContactedNoteworthyActorAlone()
    {
        var (state, actor) = CreateBackgroundActor();
        LivingWorldActorTieringService.RecordContactAndPromote(state, actor.ActorId, new GameDate(0));

        var stillWithinWindow = new GameDate(LivingWorldActorTieringCatalog.DemotionQuietPeriodMonths - 1);
        var result = LivingWorldActorTieringService.DemoteIfQuiet(state, actor.ActorId, stillWithinWindow);

        Assert.That(result.Tier, Is.EqualTo(LivingWorldActorTier.Noteworthy));
    }

    [Test]
    public void DemoteIfQuietFreezesANoteworthyActorBackToBackgroundOnceTheQuietWindowElapses()
    {
        var (state, actor) = CreateBackgroundActor();
        var promoted = LivingWorldActorTieringService.RecordContactAndPromote(state, actor.ActorId, new GameDate(0));

        var justPastWindow = new GameDate(LivingWorldActorTieringCatalog.DemotionQuietPeriodMonths);
        var result = LivingWorldActorTieringService.DemoteIfQuiet(state, actor.ActorId, justPastWindow);

        Assert.Multiple(() =>
        {
            Assert.That(result.Tier, Is.EqualTo(LivingWorldActorTier.Background));
            // Freezing is a tier flip only — every other field, including HeadCharacterId, is left
            // exactly as it stood (§2.4's "last-known state freezes").
            Assert.That(result with { Tier = LivingWorldActorTier.Noteworthy }, Is.EqualTo(promoted));
        });
    }

    [Test]
    public void DemoteIfQuietTreatsANeverContactedNoteworthyActorAsQuiet()
    {
        var (state, actor) = CreateBackgroundActor();
        state.Actors.Remove(actor.ActorId);
        var neverContacted = actor with { Tier = LivingWorldActorTier.Noteworthy };
        state.Actors.Add(actor.ActorId, neverContacted);

        var result = LivingWorldActorTieringService.DemoteIfQuiet(state, actor.ActorId, new GameDate(0));

        Assert.That(result.Tier, Is.EqualTo(LivingWorldActorTier.Background));
    }
}
