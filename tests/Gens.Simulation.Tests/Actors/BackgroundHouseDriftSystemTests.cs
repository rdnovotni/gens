using Gens.Simulation.Actors;
using Gens.Simulation.Economy;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Actors;

/// <summary>Phase 10 item 3/7 ("background/noteworthy fidelity tiers"... "simulation budgets")
/// coverage for the Background-tier abstract tick.</summary>
public sealed class BackgroundHouseDriftSystemTests
{
    private static RandomStreamSet Streams(ulong seed)
    {
        var streams = new RandomStreamSet();
        streams.Add("actors.backgroundHouseDrift", seed, 1);
        return streams;
    }

    private static LivingWorldActor AddActor(
        WorldState state,
        LivingWorldActorTier tier,
        LivingWorldActorStandingTrend trend,
        HouseholdWealthBand band)
    {
        var actor = LivingWorldActor.Create(
            state.ActorIds.Issue(), LivingWorldActorType.Gens, "Valeria", tier, trend,
            LivingWorldActorOrigin.Ancient, parentActorId: null, LivingWorldActorIdentity.None,
            dignitas: 0, new LivingWorldActorNetWorth(band, Figure: null),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest),
            state.RegionIds.Issue(), state.SettlementIds.Issue());
        state.Actors.Add(actor.ActorId, actor);
        return actor;
    }

    [Test]
    public void DeclaresTheRelationshipsActorsPhaseAndActorsReadWriteSet()
    {
        var system = new BackgroundHouseDriftSystem();

        Assert.Multiple(() =>
        {
            Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
            Assert.That(system.Reads, Is.EquivalentTo(new[] { "actors" }));
            Assert.That(system.Writes, Is.EquivalentTo(new[] { "actors" }));
            Assert.That(system.Prerequisites, Is.Empty);
        });
    }

    [Test]
    public void NeverTouchesANoteworthyActor()
    {
        var state = new WorldState(new GameDate(0));
        var actor = AddActor(state, LivingWorldActorTier.Noteworthy, LivingWorldActorStandingTrend.Rising, HouseholdWealthBand.Modest);

        var system = new BackgroundHouseDriftSystem();
        for (var month = 0; month < 200; month++)
            system.Tick(state, new MonthlyTickContext(new GameDate(month), Streams((ulong)month)));

        Assert.That(state.Actors.TryGet(actor.ActorId, out var stored), Is.True);
        Assert.That(stored, Is.EqualTo(actor));
    }

    [Test]
    public void NetWorthBandNeverLeavesItsDefinedRangeAcrossManyTicks()
    {
        var state = new WorldState(new GameDate(0));
        var rising = AddActor(state, LivingWorldActorTier.Background, LivingWorldActorStandingTrend.Rising, HouseholdWealthBand.Wealthy);
        var declining = AddActor(state, LivingWorldActorTier.Background, LivingWorldActorStandingTrend.Declining, HouseholdWealthBand.Ruined);

        var system = new BackgroundHouseDriftSystem();
        var streams = Streams(99);
        for (var month = 0; month < 500; month++)
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));

        Assert.Multiple(() =>
        {
            Assert.That(state.Actors.TryGet(rising.ActorId, out var risingStored), Is.True);
            Assert.That(Enum.IsDefined(risingStored!.NetWorth.Band), Is.True);
            Assert.That(state.Actors.TryGet(declining.ActorId, out var decliningStored), Is.True);
            Assert.That(Enum.IsDefined(decliningStored!.NetWorth.Band), Is.True);
        });
    }

    [Test]
    public void SameSeedProducesTheSameDriftEveryTime()
    {
        var stateA = new WorldState(new GameDate(0));
        var actorA = AddActor(stateA, LivingWorldActorTier.Background, LivingWorldActorStandingTrend.Rising, HouseholdWealthBand.Modest);
        new BackgroundHouseDriftSystem().Tick(stateA, new MonthlyTickContext(new GameDate(0), Streams(42)));
        stateA.Actors.TryGet(actorA.ActorId, out var resultA);

        var stateB = new WorldState(new GameDate(0));
        var actorB = AddActor(stateB, LivingWorldActorTier.Background, LivingWorldActorStandingTrend.Rising, HouseholdWealthBand.Modest);
        new BackgroundHouseDriftSystem().Tick(stateB, new MonthlyTickContext(new GameDate(0), Streams(42)));
        stateB.Actors.TryGet(actorB.ActorId, out var resultB);

        Assert.Multiple(() =>
        {
            Assert.That(resultB!.StandingTrend, Is.EqualTo(resultA!.StandingTrend));
            Assert.That(resultB.NetWorth, Is.EqualTo(resultA.NetWorth));
        });
    }

    [Test]
    public void ATickWithNoBackgroundActorsReturnsNoEventsAndDoesNotThrow()
    {
        var state = new WorldState(new GameDate(0));
        var system = new BackgroundHouseDriftSystem();

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(0), Streams(1)));

        Assert.That(events, Is.Empty);
    }

    [Test]
    public void ProcessingMoreActorsThanTheBudgetCompletesWithoutError()
    {
        var state = new WorldState(new GameDate(0));
        for (var i = 0; i < LivingWorldActorDriftCatalog.MaxBackgroundActorsProcessedPerTick + 10; i++)
            AddActor(state, LivingWorldActorTier.Background, LivingWorldActorStandingTrend.Rising, HouseholdWealthBand.Modest);

        var system = new BackgroundHouseDriftSystem();
        Assert.DoesNotThrow(() => system.Tick(state, new MonthlyTickContext(new GameDate(0), Streams(3))));
    }
}
