using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Actors;

/// <summary>Phase 10 item 4's remaining "retirement/extinction" coverage.</summary>
public sealed class LivingWorldActorExtinctionSystemTests
{
    private static RandomStreamSet Streams(ulong seed)
    {
        var streams = new RandomStreamSet();
        streams.Add("actors.extinction", seed, 1);
        return streams;
    }

    private static LivingWorldActor AddActor(
        WorldState state,
        LivingWorldActorTier tier,
        LivingWorldActorStandingTrend trend,
        RuntimeId<Character>? headCharacterId = null)
    {
        var actor = LivingWorldActor.Create(
            state.ActorIds.Issue(), LivingWorldActorType.Gens, "Valeria", tier, trend,
            LivingWorldActorOrigin.Ancient, parentActorId: null, LivingWorldActorIdentity.None,
            dignitas: 0, new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest),
            state.RegionIds.Issue(), state.SettlementIds.Issue(),
            headCharacterId: headCharacterId);
        state.Actors.Add(actor.ActorId, actor);
        return actor;
    }

    [Test]
    public void DeclaresTheRelationshipsActorsPhaseAndReadWriteSet()
    {
        var system = new LivingWorldActorExtinctionSystem();

        Assert.Multiple(() =>
        {
            Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
            Assert.That(system.Reads, Is.EquivalentTo(new[] { "actors", "characters", "houseStandings" }));
            Assert.That(system.Writes, Is.EquivalentTo(new[] { "actors", "houseStandings", "eventIds" }));
        });
    }

    [Test]
    public void ExtinguishesANoteworthyActorWhoseDeadHeadHasNoLivingDescendant()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        var head = CharacterTestFixtures.Minimal(headId, deathRecord: new DeathRecord(new GameDate(0), DeathCause.OldAge, 70));
        state.Characters.Add(headId, head);
        var actor = AddActor(state, LivingWorldActorTier.Noteworthy, LivingWorldActorStandingTrend.Established, headId);

        var events = new LivingWorldActorExtinctionSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        Assert.Multiple(() =>
        {
            Assert.That(events, Has.Count.EqualTo(1));
            Assert.That(events[0], Is.InstanceOf<LivingWorldActorExtinguishedEvent>());
            Assert.That(state.Actors.TryGet(actor.ActorId, out _), Is.False);
        });
    }

    [Test]
    public void LeavesANoteworthyActorAloneWhenTheDeadHeadHasALivingChild()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        var head = CharacterTestFixtures.Minimal(headId, deathRecord: new DeathRecord(new GameDate(0), DeathCause.OldAge, 70));
        state.Characters.Add(headId, head);
        var childId = state.CharacterIds.Issue();
        state.Characters.Add(childId, CharacterTestFixtures.Minimal(childId, fatherId: headId));
        var actor = AddActor(state, LivingWorldActorTier.Noteworthy, LivingWorldActorStandingTrend.Established, headId);

        var events = new LivingWorldActorExtinctionSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        Assert.Multiple(() =>
        {
            Assert.That(events, Is.Empty);
            Assert.That(state.Actors.TryGet(actor.ActorId, out _), Is.True);
        });
    }

    [Test]
    public void LeavesANoteworthyActorAloneWhileItsHeadIsStillAlive()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId));
        var actor = AddActor(state, LivingWorldActorTier.Noteworthy, LivingWorldActorStandingTrend.Established, headId);

        var events = new LivingWorldActorExtinctionSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        Assert.Multiple(() =>
        {
            Assert.That(events, Is.Empty);
            Assert.That(state.Actors.TryGet(actor.ActorId, out _), Is.True);
        });
    }

    [Test]
    public void ExtinguishingAnActorRemovesItsHouseStandingEntries()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, deathRecord: new DeathRecord(new GameDate(0), DeathCause.OldAge, 70)));
        var actor = AddActor(state, LivingWorldActorTier.Noteworthy, LivingWorldActorStandingTrend.Established, headId);
        var other = AddActor(state, LivingWorldActorTier.Noteworthy, LivingWorldActorStandingTrend.Established);
        var key = HouseStandingKey.Between(actor.ActorId, other.ActorId);
        state.HouseStandings.Add(key, new HouseStanding(HouseStandingLevel.Rivalrous));

        new LivingWorldActorExtinctionSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        Assert.That(state.HouseStandings.TryGet(key, out _), Is.False);
    }

    [Test]
    public void NeverExtinguishesABackgroundActorThatIsNotDeclining()
    {
        var state = new WorldState(new GameDate(0));
        var actor = AddActor(state, LivingWorldActorTier.Background, LivingWorldActorStandingTrend.Rising);

        var system = new LivingWorldActorExtinctionSystem();
        for (var month = 0; month < 500; month++)
            system.Tick(state, new MonthlyTickContext(new GameDate(month), Streams((ulong)month)));

        Assert.That(state.Actors.TryGet(actor.ActorId, out _), Is.True);
    }

    [Test]
    public void EventuallyExtinguishesADecliningBackgroundActorGivenEnoughTicks()
    {
        var state = new WorldState(new GameDate(0));
        var actor = AddActor(state, LivingWorldActorTier.Background, LivingWorldActorStandingTrend.Declining);

        var system = new LivingWorldActorExtinctionSystem();
        var streams = Streams(7);
        var stillPresent = true;
        for (var month = 0; month < 2000 && stillPresent; month++)
        {
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));
            stillPresent = state.Actors.TryGet(actor.ActorId, out _);
        }

        Assert.That(stillPresent, Is.False);
    }

    [Test]
    public void ProcessingMoreBackgroundActorsThanTheBudgetCompletesWithoutError()
    {
        var state = new WorldState(new GameDate(0));
        for (var i = 0; i < LivingWorldActorDriftCatalog.MaxBackgroundActorsProcessedPerTick + 10; i++)
            AddActor(state, LivingWorldActorTier.Background, LivingWorldActorStandingTrend.Declining);

        var system = new LivingWorldActorExtinctionSystem();
        Assert.DoesNotThrow(() => system.Tick(state, new MonthlyTickContext(new GameDate(0), Streams(3))));
    }

    [Test]
    public void ATickWithNoActorsReturnsNoEventsAndDoesNotThrow()
    {
        var state = new WorldState(new GameDate(0));
        var events = new LivingWorldActorExtinctionSystem().Tick(state, new MonthlyTickContext(new GameDate(0), Streams(1)));

        Assert.That(events, Is.Empty);
    }
}
