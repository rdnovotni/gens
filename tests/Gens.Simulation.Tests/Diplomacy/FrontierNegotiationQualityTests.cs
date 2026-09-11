using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Cultures;
using Gens.Simulation.Diplomacy;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Languages;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Diplomacy;

public sealed class FrontierNegotiationQualityTests
{
    private static readonly GameDate StartDate = new(0);

    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Character> NegotiatorId, RuntimeId<Actor> PeopleActorId)
        OneHouseholdAndOneGermanicPeople()
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

        return (state, householdId, negotiatorId, actor.ActorId);
    }

    private static void GrantConversational(WorldState state, RuntimeId<Character> characterId, DefinitionId<LanguageDefinition> languageId) =>
        AcquireLanguageCommands.BuildPipeline(KnownWorldLanguages.BuildCatalog(), CultureLanguageMap.BuildKnownWorldMap()).Execute(
            state, new AcquireLanguageCommand(
                state.CommandIds.Issue(), "player", StartDate, null, characterId, languageId, FluencyTier.Conversational,
                LanguageAcquisitionMethod.FormalEducation));

    [Test]
    public void EvaluatesToNoneWithNoQualifiedSpeaker()
    {
        var (state, householdId, negotiatorId, actorId) = OneHouseholdAndOneGermanicPeople();

        var quality = FrontierNegotiationQualityEvaluator.Evaluate(
            state, negotiatorId, householdId, actorId, CultureLanguageMap.BuildKnownWorldMap());

        Assert.Multiple(() =>
        {
            Assert.That(quality.GateCleared, Is.False);
            Assert.That(quality.Source, Is.EqualTo(FrontierNegotiationQualitySource.None));
        });
    }

    [Test]
    public void EvaluatesToNegotiatorFluencyWhenTheNegotiatorSpeaksTheLanguageButIsNotCulturallyMatched()
    {
        var (state, householdId, negotiatorId, actorId) = OneHouseholdAndOneGermanicPeople();
        GrantConversational(state, negotiatorId, KnownWorldLanguages.Germanic);

        var quality = FrontierNegotiationQualityEvaluator.Evaluate(
            state, negotiatorId, householdId, actorId, CultureLanguageMap.BuildKnownWorldMap());

        Assert.Multiple(() =>
        {
            Assert.That(quality.GateCleared, Is.True);
            Assert.That(quality.Source, Is.EqualTo(FrontierNegotiationQualitySource.NegotiatorFluency));
        });
    }

    [Test]
    public void EvaluatesToCulturalFamiliarityWhenTheNegotiatorsOriginCultureMatches()
    {
        var (state, householdId, _, actorId) = OneHouseholdAndOneGermanicPeople();
        var negotiatorId = state.CharacterIds.Issue();
        state.Characters.Add(negotiatorId, Character.Create(
            id: negotiatorId, praenomen: "Ario", nomen: "Vistus", cognomen: null, sex: Sex.Male, birthDate: new GameDate(-300),
            visualProfile: CharacterTestFixtures.MinimalVisualProfile, status: Simulation.Characters.LegalStatus.Peregrine,
            socialClass: null, culture: KnownWorldCultures.Germanic, location: default, household: householdId,
            attributes: new CoreAttributes(10, 10, 10, 10, 10), skills: new LaborSkills(10, 10, 10, 10, 10),
            condition: new Condition(80, 0, 50, 20, 50), source: CharacterSource.Familia, instantiatedAtMonth: 0));
        GrantConversational(state, negotiatorId, KnownWorldLanguages.Germanic);

        var quality = FrontierNegotiationQualityEvaluator.Evaluate(
            state, negotiatorId, householdId, actorId, CultureLanguageMap.BuildKnownWorldMap());

        Assert.Multiple(() =>
        {
            Assert.That(quality.GateCleared, Is.True);
            Assert.That(quality.Source, Is.EqualTo(FrontierNegotiationQualitySource.CulturalFamiliarity));
        });
    }

    [Test]
    public void CulturalMatchAloneWithoutTheLanguageStillDoesNotClearTheGate()
    {
        var (state, householdId, _, actorId) = OneHouseholdAndOneGermanicPeople();
        var negotiatorId = state.CharacterIds.Issue();
        state.Characters.Add(negotiatorId, Character.Create(
            id: negotiatorId, praenomen: "Ario", nomen: "Vistus", cognomen: null, sex: Sex.Male, birthDate: new GameDate(-300),
            visualProfile: CharacterTestFixtures.MinimalVisualProfile, status: Simulation.Characters.LegalStatus.Peregrine,
            socialClass: null, culture: KnownWorldCultures.Germanic, location: default, household: householdId,
            attributes: new CoreAttributes(10, 10, 10, 10, 10), skills: new LaborSkills(10, 10, 10, 10, 10),
            condition: new Condition(80, 0, 50, 20, 50), source: CharacterSource.Familia, instantiatedAtMonth: 0));

        var quality = FrontierNegotiationQualityEvaluator.Evaluate(
            state, negotiatorId, householdId, actorId, CultureLanguageMap.BuildKnownWorldMap());

        Assert.That(quality.GateCleared, Is.False);
    }

    [Test]
    public void EvaluatesToInterpresPresentThroughAnInformalHouseholdMember()
    {
        var (state, householdId, negotiatorId, actorId) = OneHouseholdAndOneGermanicPeople();
        var informalId = state.CharacterIds.Issue();
        state.Characters.Add(informalId, CharacterTestFixtures.Minimal(informalId, nomen: "Informal", household: householdId));
        GrantConversational(state, informalId, KnownWorldLanguages.Germanic);

        var quality = FrontierNegotiationQualityEvaluator.Evaluate(
            state, negotiatorId, householdId, actorId, CultureLanguageMap.BuildKnownWorldMap());

        Assert.Multiple(() =>
        {
            Assert.That(quality.GateCleared, Is.True);
            Assert.That(quality.Source, Is.EqualTo(FrontierNegotiationQualitySource.InterpresPresent));
            Assert.That(quality.InterpresCharacterId, Is.EqualTo(informalId));
        });
    }

    [Test]
    public void EvaluatesToNoneForAnUnmappedForeignPeopleLanguage()
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

        var quality = FrontierNegotiationQualityEvaluator.Evaluate(
            state, negotiatorId, householdId, actor.ActorId, CultureLanguageMap.BuildKnownWorldMap());

        Assert.That(quality.GateCleared, Is.False);
    }
}
