using Gens.Simulation.Actors;
using Gens.Simulation.Economy;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Actors;

/// <summary>Phase 10 item 5 coverage for the Ancestral Grudge decay sweep.</summary>
public sealed class AncestralGrudgeDecaySystemTests
{
    private static (WorldState State, LivingWorldActor A, LivingWorldActor B) TwoActors()
    {
        var state = new WorldState(new GameDate(0));
        var netWorth = new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null);
        var military = new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest);
        var a = RivalHouseCreationService.CreateAncientSeed(
            state, "Aemilia", LivingWorldActorStandingTrend.Established, LivingWorldActorIdentity.None,
            0, netWorth, military, state.RegionIds.Issue(), state.SettlementIds.Issue());
        var b = RivalHouseCreationService.CreateAncientSeed(
            state, "Cornelia", LivingWorldActorStandingTrend.Established, LivingWorldActorIdentity.None,
            0, netWorth, military, state.RegionIds.Issue(), state.SettlementIds.Issue());
        return (state, a, b);
    }

    [Test]
    public void LeavesAnActiveGrudgeAlone()
    {
        var (state, a, b) = TwoActors();
        var key = HouseStandingKey.Between(a.ActorId, b.ActorId);
        var grudge = new AncestralGrudge("engagement_placeholder", new GameDate(0));
        state.HouseStandings.Add(key, new HouseStanding(HouseStandingLevel.Feuding, grudge));

        var events = new AncestralGrudgeDecaySystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        Assert.Multiple(() =>
        {
            Assert.That(events, Is.Empty);
            Assert.That(state.HouseStandings.TryGet(key, out var stored), Is.True);
            Assert.That(stored!.Grudge, Is.EqualTo(grudge));
        });
    }

    [Test]
    public void ClearsAGrudgeOnceItHasFullyDecayedWithoutChangingTheStandingLevel()
    {
        var (state, a, b) = TwoActors();
        var key = HouseStandingKey.Between(a.ActorId, b.ActorId);
        var grudge = new AncestralGrudge("engagement_placeholder", new GameDate(0));
        state.HouseStandings.Add(key, new HouseStanding(HouseStandingLevel.Feuding, grudge));

        var farFuture = new GameDate(AncestralGrudgeCatalog.DecayMonths);
        var events = new AncestralGrudgeDecaySystem().Tick(state, new MonthlyTickContext(farFuture, new RandomStreamSet()));

        Assert.Multiple(() =>
        {
            Assert.That(events, Has.Count.EqualTo(1));
            Assert.That(events[0], Is.InstanceOf<AncestralGrudgeDecayedEvent>());
            Assert.That(state.HouseStandings.TryGet(key, out var stored), Is.True);
            Assert.That(stored!.Grudge, Is.Null);
            Assert.That(stored.Standing, Is.EqualTo(HouseStandingLevel.Feuding));
        });
    }

    [Test]
    public void SkipsAPairWithNoGrudgeAtAll()
    {
        var (state, a, b) = TwoActors();
        var key = HouseStandingKey.Between(a.ActorId, b.ActorId);
        state.HouseStandings.Add(key, new HouseStanding(HouseStandingLevel.Rivalrous));

        var events = new AncestralGrudgeDecaySystem().Tick(
            state, new MonthlyTickContext(new GameDate(AncestralGrudgeCatalog.DecayMonths + 100), new RandomStreamSet()));

        Assert.That(events, Is.Empty);
    }
}
