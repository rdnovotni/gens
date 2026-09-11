using Gens.Simulation.Actors;
using Gens.Simulation.Cultures;
using Gens.Simulation.Diplomacy;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Queries;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Queries;

public sealed class FrontierDiplomacyQueryTests
{
    private static readonly GameDate StartDate = new(0);

    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Actor> PeopleActorId) SetupWithAPeople()
    {
        var state = new WorldState(StartDate);
        var householdId = state.HouseholdIds.Issue();
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var actor = ForeignPeopleCreationService.CreateAncientSeed(
            state, KnownWorldCultures.BuildCatalog(), "Suebi", KnownWorldCultures.Germanic,
            LivingWorldActorStandingTrend.Established, new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest),
            regionId, settlementId, StartDate);
        return (state, householdId, actor.ActorId);
    }

    [Test]
    public void ReturnsNoEntriesForAHouseholdWithNoContact()
    {
        var (state, householdId, _) = SetupWithAPeople();

        var projection = new FrontierDiplomacyQuery().Execute(state, householdId.ToTaggedString());

        Assert.That(projection.ContactedPeoples, Is.Empty);
    }

    [Test]
    public void SurfacesAPeopleOnceTheHouseholdHasContactedThem()
    {
        var (state, householdId, actorId) = SetupWithAPeople();
        PerPeopleStandingMutator.Apply(state, new PerPeopleStandingKey(householdId, actorId), 10, StartDate);

        var projection = new FrontierDiplomacyQuery().Execute(state, householdId.ToTaggedString());

        Assert.That(projection.ContactedPeoples, Has.Count.EqualTo(1));
        Assert.That(projection.ContactedPeoples[0].ForeignPeopleActorId, Is.EqualTo(actorId));
    }

    [Test]
    public void NeverSurfacesAnotherHouseholdsOwnStandingOrTreaties()
    {
        var (state, householdId, actorId) = SetupWithAPeople();
        var otherHouseholdId = state.HouseholdIds.Issue();
        PerPeopleStandingMutator.Apply(state, new PerPeopleStandingKey(otherHouseholdId, actorId), 10, StartDate);

        var projection = new FrontierDiplomacyQuery().Execute(state, householdId.ToTaggedString());

        Assert.That(projection.ContactedPeoples, Is.Empty);
    }
}
