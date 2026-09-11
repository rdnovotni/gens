using Gens.Simulation.Actors;
using Gens.Simulation.Diplomacy;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Diplomacy;

public sealed class PerPeopleStandingTests
{
    [Test]
    public void ResolverDefaultsToNeutralWithNoTrackedEntry()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var actorId = state.ActorIds.Issue();

        var standing = PerPeopleStandingResolver.GetEffective(state, householdId, actorId);

        Assert.That(standing.Standing, Is.EqualTo(HouseStandingLevel.Neutral));
    }

    [Test]
    public void KeyOrderingIsHouseholdThenActor()
    {
        var state = new WorldState(new GameDate(0));
        var householdA = state.HouseholdIds.Issue();
        var householdB = state.HouseholdIds.Issue();
        var actorA = state.ActorIds.Issue();
        var actorB = state.ActorIds.Issue();

        Assert.Multiple(() =>
        {
            Assert.That(new PerPeopleStandingKey(householdA, actorB).CompareTo(new PerPeopleStandingKey(householdB, actorA)), Is.LessThan(0));
            Assert.That(new PerPeopleStandingKey(householdA, actorA).CompareTo(new PerPeopleStandingKey(householdA, actorB)), Is.LessThan(0));
            Assert.That(new PerPeopleStandingKey(householdA, actorA).CompareTo(new PerPeopleStandingKey(householdA, actorA)), Is.EqualTo(0));
        });
    }

    [Test]
    public void ApplyCrossesOneTierStepOnceGoodwillReachesTheThreshold()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var actorId = state.ActorIds.Issue();
        var key = new PerPeopleStandingKey(householdId, actorId);

        var updated = PerPeopleStandingMutator.Apply(state, key, FrontierDiplomacyCatalog.GoodwillPerTierStep, new GameDate(1));

        Assert.Multiple(() =>
        {
            Assert.That(updated.Standing, Is.EqualTo(HouseStandingLevel.Allied));
            Assert.That(updated.Goodwill, Is.EqualTo(0));
        });
    }

    [Test]
    public void ApplyClampsAtAlliedAndDoesNotAccumulateExcessGoodwill()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var actorId = state.ActorIds.Issue();
        var key = new PerPeopleStandingKey(householdId, actorId);

        var updated = PerPeopleStandingMutator.Apply(state, key, FrontierDiplomacyCatalog.GoodwillPerTierStep * 5, new GameDate(1));

        Assert.Multiple(() =>
        {
            Assert.That(updated.Standing, Is.EqualTo(HouseStandingLevel.Allied));
            Assert.That(updated.Goodwill, Is.EqualTo(0));
        });
    }

    [Test]
    public void ApplyClampsAtFeudingInTheOppositeDirection()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var actorId = state.ActorIds.Issue();
        var key = new PerPeopleStandingKey(householdId, actorId);

        var updated = PerPeopleStandingMutator.Apply(state, key, -FrontierDiplomacyCatalog.GoodwillPerTierStep * 5, new GameDate(1));

        Assert.Multiple(() =>
        {
            Assert.That(updated.Standing, Is.EqualTo(HouseStandingLevel.Feuding));
            Assert.That(updated.Goodwill, Is.EqualTo(0));
        });
    }

    [Test]
    public void ApplyCarriesARemainderBelowTheThreshold()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var actorId = state.ActorIds.Issue();
        var key = new PerPeopleStandingKey(householdId, actorId);

        var updated = PerPeopleStandingMutator.Apply(state, key, FrontierDiplomacyCatalog.GoodwillPerTierStep - 1, new GameDate(1));

        Assert.Multiple(() =>
        {
            Assert.That(updated.Standing, Is.EqualTo(HouseStandingLevel.Neutral));
            Assert.That(updated.Goodwill, Is.EqualTo(FrontierDiplomacyCatalog.GoodwillPerTierStep - 1));
        });
    }
}
