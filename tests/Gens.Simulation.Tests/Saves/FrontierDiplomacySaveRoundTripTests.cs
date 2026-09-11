using Gens.Simulation.Actors;
using Gens.Simulation.Cultures;
using Gens.Simulation.Diplomacy;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

public sealed class FrontierDiplomacySaveRoundTripTests
{
    private static readonly GameDate StartDate = new(0);

    [Test]
    public void ForeignPeopleStandingAndTreatiesRoundTripThroughTheDtoAndDeterministicHashStaysStable()
    {
        var state = new WorldState(StartDate);
        var householdId = state.HouseholdIds.Issue();
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var actor = ForeignPeopleCreationService.CreateAncientSeed(
            state, KnownWorldCultures.BuildCatalog(), "Suebi", KnownWorldCultures.Germanic,
            LivingWorldActorStandingTrend.Established, new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest),
            regionId, settlementId, StartDate);

        PerPeopleStandingMutator.Apply(state, new PerPeopleStandingKey(householdId, actor.ActorId), 40, StartDate);

        var treatyId = state.FrontierTreatyIds.Issue();
        state.FrontierTreaties.Add(treatyId, FrontierTreaty.Create(
            treatyId, householdId, actor.ActorId, FrontierTreatyType.Tribute, TributeDirection.PeoplePayHousehold,
            Money.FromDenarii(50), StartDate, new GameDate(24)));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.ForeignPeopleDetails.Count, Is.EqualTo(state.ForeignPeopleDetails.Count));
            Assert.That(restored.PerPeopleStandings.Count, Is.EqualTo(state.PerPeopleStandings.Count));
            Assert.That(restored.FrontierTreaties.Count, Is.EqualTo(state.FrontierTreaties.Count));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }
}
