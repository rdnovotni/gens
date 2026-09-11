using Gens.Simulation.Actors;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Cultures;
using Gens.Simulation.Diplomacy;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Languages;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Diplomacy;

public sealed class ProposeFrontierTreatyCommandTests
{
    private static readonly GameDate StartDate = new(0);

    private static RandomStreamSet Streams(ulong seed)
    {
        var streams = new RandomStreamSet();
        streams.Add(ProposeFrontierTreatyCommands.StreamName, seed, 1);
        return streams;
    }

    private static CommandPipeline<WorldState, ProposeFrontierTreatyCommand> Pipeline(RandomStreamSet streams) =>
        ProposeFrontierTreatyCommands.CreatePipeline(CultureLanguageMap.BuildKnownWorldMap(), streams);

    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Character> NegotiatorId, RuntimeId<Actor> PeopleActorId)
        Setup()
    {
        var state = new WorldState(StartDate);
        var householdId = state.HouseholdIds.Issue();
        var negotiatorId = state.CharacterIds.Issue();
        state.Characters.Add(negotiatorId, CharacterTestFixtures.Minimal(negotiatorId, household: householdId));
        AcquireLanguageCommands.BuildPipeline(KnownWorldLanguages.BuildCatalog(), CultureLanguageMap.BuildKnownWorldMap()).Execute(
            state, new AcquireLanguageCommand(
                state.CommandIds.Issue(), "player", StartDate, null, negotiatorId, KnownWorldLanguages.Germanic,
                FluencyTier.Conversational, LanguageAcquisitionMethod.FormalEducation));

        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var actor = ForeignPeopleCreationService.CreateAncientSeed(
            state, KnownWorldCultures.BuildCatalog(), "Suebi", KnownWorldCultures.Germanic,
            LivingWorldActorStandingTrend.Established, new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest),
            regionId, settlementId, StartDate);

        return (state, householdId, negotiatorId, actor.ActorId);
    }

    [Test]
    public void AcceptedProposalEitherConcludesOrRejectsTheTreatyButNeverBothLeavesGoodwillUnchanged()
    {
        var (state, householdId, negotiatorId, actorId) = Setup();
        var beforeGoodwill = PerPeopleStandingResolver.GetEffective(state, householdId, actorId).Goodwill;

        var result = Pipeline(Streams(12345)).Execute(state, new ProposeFrontierTreatyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, negotiatorId, actorId,
            FrontierTreatyType.NonAggression, TributeDirection.None, Money.Zero, 12));

        Assert.That(result.Accepted, Is.True);
        var afterGoodwill = PerPeopleStandingResolver.GetEffective(state, householdId, actorId).Goodwill;
        Assert.That(afterGoodwill, Is.Not.EqualTo(beforeGoodwill));

        if (state.FrontierTreaties.Count > 0)
        {
            Assert.That(result.Events, Has.Some.InstanceOf<FrontierTreatyConcludedEvent>());
            Assert.That(afterGoodwill, Is.GreaterThan(beforeGoodwill));
        }
        else
        {
            Assert.That(result.Events, Has.Some.InstanceOf<FrontierTreatyRejectedEvent>());
            Assert.That(afterGoodwill, Is.LessThan(beforeGoodwill));
        }
    }

    [Test]
    public void RejectsWhenTheLanguageGateDoesNotClear()
    {
        var state = new WorldState(StartDate);
        var householdId = state.HouseholdIds.Issue();
        var negotiatorId = state.CharacterIds.Issue();
        state.Characters.Add(negotiatorId, CharacterTestFixtures.Minimal(negotiatorId, household: householdId));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var actor = ForeignPeopleCreationService.CreateAncientSeed(
            state, KnownWorldCultures.BuildCatalog(), "Suebi", KnownWorldCultures.Germanic,
            LivingWorldActorStandingTrend.Established, new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest),
            regionId, settlementId, StartDate);

        var result = Pipeline(Streams(1)).Execute(state, new ProposeFrontierTreatyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, negotiatorId, actor.ActorId,
            FrontierTreatyType.NonAggression, TributeDirection.None, Money.Zero, 12));

        Assert.That(result.Error, Is.EqualTo(ProposeFrontierTreatyCommands.LanguageGateNotCleared));
    }

    [Test]
    public void RejectsAnUnmappedForeignPeopleLanguage()
    {
        var state = new WorldState(StartDate);
        var householdId = state.HouseholdIds.Issue();
        var negotiatorId = state.CharacterIds.Issue();
        state.Characters.Add(negotiatorId, CharacterTestFixtures.Minimal(negotiatorId, household: householdId));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var actor = ForeignPeopleCreationService.CreateAncientSeed(
            state, KnownWorldCultures.BuildCatalog(), "Blemmyes", KnownWorldCultures.Blemmyes,
            LivingWorldActorStandingTrend.Established, new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest),
            regionId, settlementId, StartDate);

        var result = Pipeline(Streams(1)).Execute(state, new ProposeFrontierTreatyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, negotiatorId, actor.ActorId,
            FrontierTreatyType.NonAggression, TributeDirection.None, Money.Zero, 12));

        Assert.That(result.Error, Is.EqualTo(ProposeFrontierTreatyCommands.UnmappedPeopleLanguage));
    }

    [Test]
    public void RejectsAnUnknownNegotiator()
    {
        var (state, householdId, _, actorId) = Setup();

        var result = Pipeline(Streams(1)).Execute(state, new ProposeFrontierTreatyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, state.CharacterIds.Issue(), actorId,
            FrontierTreatyType.NonAggression, TributeDirection.None, Money.Zero, 12));

        Assert.That(result.Error, Is.EqualTo(ProposeFrontierTreatyCommands.NegotiatorNotFound));
    }

    [Test]
    public void RejectsANegotiatorNotOfTheHousehold()
    {
        var (state, householdId, _, actorId) = Setup();
        var otherHouseholdId = state.HouseholdIds.Issue();
        var outsiderId = state.CharacterIds.Issue();
        state.Characters.Add(outsiderId, CharacterTestFixtures.Minimal(outsiderId, nomen: "Outsider", household: otherHouseholdId));

        var result = Pipeline(Streams(1)).Execute(state, new ProposeFrontierTreatyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, outsiderId, actorId,
            FrontierTreatyType.NonAggression, TributeDirection.None, Money.Zero, 12));

        Assert.That(result.Error, Is.EqualTo(ProposeFrontierTreatyCommands.NegotiatorNotOfHousehold));
    }

    [Test]
    public void RejectsAnUnknownForeignPeople()
    {
        var (state, householdId, negotiatorId, _) = Setup();

        var result = Pipeline(Streams(1)).Execute(state, new ProposeFrontierTreatyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, negotiatorId, state.ActorIds.Issue(),
            FrontierTreatyType.NonAggression, TributeDirection.None, Money.Zero, 12));

        Assert.That(result.Error, Is.EqualTo(ProposeFrontierTreatyCommands.UnknownForeignPeople));
    }

    [Test]
    public void RejectsInvalidTributeTermsOnANonTributeTreaty()
    {
        var (state, householdId, negotiatorId, actorId) = Setup();

        var result = Pipeline(Streams(1)).Execute(state, new ProposeFrontierTreatyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, negotiatorId, actorId,
            FrontierTreatyType.NonAggression, TributeDirection.HouseholdPaysPeople, Money.FromDenarii(10), 12));

        Assert.That(result.Error, Is.EqualTo(ProposeFrontierTreatyCommands.TributeTermsInvalid));
    }

    [Test]
    public void RejectsATributeTreatyWithNoDirection()
    {
        var (state, householdId, negotiatorId, actorId) = Setup();

        var result = Pipeline(Streams(1)).Execute(state, new ProposeFrontierTreatyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, negotiatorId, actorId,
            FrontierTreatyType.Tribute, TributeDirection.None, Money.FromDenarii(10), 12));

        Assert.That(result.Error, Is.EqualTo(ProposeFrontierTreatyCommands.TributeTermsInvalid));
    }

    [Test]
    public void RejectsATermOfZeroOrLess()
    {
        var (state, householdId, negotiatorId, actorId) = Setup();

        var result = Pipeline(Streams(1)).Execute(state, new ProposeFrontierTreatyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, negotiatorId, actorId,
            FrontierTreatyType.NonAggression, TributeDirection.None, Money.Zero, 0));

        Assert.That(result.Error, Is.EqualTo(ProposeFrontierTreatyCommands.TermOutOfRange));
    }

    [Test]
    public void RejectsWhenStandingIsAlreadyFeuding()
    {
        var (state, householdId, negotiatorId, actorId) = Setup();
        PerPeopleStandingMutator.Apply(
            state, new PerPeopleStandingKey(householdId, actorId), -FrontierDiplomacyCatalog.GoodwillPerTierStep * 5, StartDate);

        var result = Pipeline(Streams(1)).Execute(state, new ProposeFrontierTreatyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, negotiatorId, actorId,
            FrontierTreatyType.NonAggression, TributeDirection.None, Money.Zero, 12));

        Assert.That(result.Error, Is.EqualTo(ProposeFrontierTreatyCommands.StandingTooHostile));
    }

    [Test]
    public void RejectsADuplicateActiveTreatyOfTheSameType()
    {
        var (state, householdId, negotiatorId, actorId) = Setup();
        var existingTreatyId = state.FrontierTreatyIds.Issue();
        state.FrontierTreaties.Add(existingTreatyId, FrontierTreaty.Create(
            existingTreatyId, householdId, actorId, FrontierTreatyType.NonAggression, TributeDirection.None, Money.Zero,
            StartDate, new GameDate(60)));

        var result = Pipeline(Streams(1)).Execute(state, new ProposeFrontierTreatyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, negotiatorId, actorId,
            FrontierTreatyType.NonAggression, TributeDirection.None, Money.Zero, 12));

        Assert.That(result.Error, Is.EqualTo(ProposeFrontierTreatyCommands.TreatyTypeAlreadyActive));
    }
}
