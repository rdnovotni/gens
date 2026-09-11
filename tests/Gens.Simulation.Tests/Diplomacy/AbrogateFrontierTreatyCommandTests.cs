using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Cultures;
using Gens.Simulation.Diplomacy;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Diplomacy;

public sealed class AbrogateFrontierTreatyCommandTests
{
    private static readonly GameDate StartDate = new(0);

    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Character> HeadId, RuntimeId<FrontierTreaty> TreatyId)
        SetupWithAnActiveTreaty()
    {
        var state = new WorldState(StartDate);
        var householdId = state.HouseholdIds.Issue();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, household: householdId));

        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var actor = ForeignPeopleCreationService.CreateAncientSeed(
            state, KnownWorldCultures.BuildCatalog(), "Suebi", KnownWorldCultures.Germanic,
            LivingWorldActorStandingTrend.Established, new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest),
            regionId, settlementId, StartDate);

        var treatyId = state.FrontierTreatyIds.Issue();
        state.FrontierTreaties.Add(treatyId, FrontierTreaty.Create(
            treatyId, householdId, actor.ActorId, FrontierTreatyType.NonAggression, TributeDirection.None, Money.Zero,
            StartDate, new GameDate(60)));

        return (state, householdId, headId, treatyId);
    }

    [Test]
    public void AbrogatesAnActiveTreatyAndAppliesAGoodwillPenalty()
    {
        var (state, householdId, headId, treatyId) = SetupWithAnActiveTreaty();
        state.FrontierTreaties.TryGet(treatyId, out var treaty);

        var result = AbrogateFrontierTreatyCommands.Pipeline.Execute(state, new AbrogateFrontierTreatyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, treatyId, headId));

        state.FrontierTreaties.TryGet(treatyId, out var updated);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(updated!.Status, Is.EqualTo(FrontierTreatyStatus.Abrogated));
            Assert.That(updated.EndedDate, Is.EqualTo(StartDate));
            Assert.That(PerPeopleStandingResolver.GetEffective(state, householdId, treaty!.ForeignPeopleActorId).Goodwill, Is.LessThan(0));
        });
    }

    [Test]
    public void RejectsATreatyThatDoesNotExist()
    {
        var (state, _, headId, _) = SetupWithAnActiveTreaty();

        var result = AbrogateFrontierTreatyCommands.Pipeline.Execute(state, new AbrogateFrontierTreatyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, state.FrontierTreatyIds.Issue(), headId));

        Assert.That(result.Error, Is.EqualTo(AbrogateFrontierTreatyCommands.TreatyNotFound));
    }

    [Test]
    public void RejectsATreatyThatIsAlreadyTerminal()
    {
        var (state, _, headId, treatyId) = SetupWithAnActiveTreaty();
        AbrogateFrontierTreatyCommands.Pipeline.Execute(state, new AbrogateFrontierTreatyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, treatyId, headId));

        var result = AbrogateFrontierTreatyCommands.Pipeline.Execute(state, new AbrogateFrontierTreatyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, treatyId, headId));

        Assert.That(result.Error, Is.EqualTo(AbrogateFrontierTreatyCommands.TreatyNotActive));
    }

    [Test]
    public void RejectsADeciderNotOfTheOwningHousehold()
    {
        var (state, _, _, treatyId) = SetupWithAnActiveTreaty();
        var otherHouseholdId = state.HouseholdIds.Issue();
        var outsiderId = state.CharacterIds.Issue();
        state.Characters.Add(outsiderId, CharacterTestFixtures.Minimal(outsiderId, nomen: "Outsider", household: otherHouseholdId));

        var result = AbrogateFrontierTreatyCommands.Pipeline.Execute(state, new AbrogateFrontierTreatyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, treatyId, outsiderId));

        Assert.That(result.Error, Is.EqualTo(AbrogateFrontierTreatyCommands.DeciderNotOfHousehold));
    }
}
