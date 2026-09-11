using Gens.Simulation.Actors;
using Gens.Simulation.Cultures;
using Gens.Simulation.Diplomacy;
using Gens.Simulation.Identity;
using Gens.Simulation.Languages;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Diplomacy;

public sealed class ForeignPeopleCreationServiceTests
{
    [Test]
    public void CreateAncientSeedRegistersABackgroundActorAndDetails()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();

        var actor = ForeignPeopleCreationService.CreateAncientSeed(
            state, KnownWorldCultures.BuildCatalog(), "Suebi", KnownWorldCultures.Germanic,
            LivingWorldActorStandingTrend.Established, new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest),
            regionId, settlementId, new GameDate(0));

        Assert.Multiple(() =>
        {
            Assert.That(actor.ActorType, Is.EqualTo(LivingWorldActorType.ForeignPeople));
            Assert.That(actor.Tier, Is.EqualTo(LivingWorldActorTier.Background));
            Assert.That(actor.HeadCharacterId, Is.Null);
            Assert.That(ForeignPeopleQueries.TryGet(state, actor.ActorId, out var details), Is.True);
            Assert.That(details.CultureId, Is.EqualTo(KnownWorldCultures.Germanic));
        });
    }

    [Test]
    public void CreateAncientSeedRejectsANonFrontierCulture()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();

        Assert.Throws<ArgumentException>(() => ForeignPeopleCreationService.CreateAncientSeed(
            state, KnownWorldCultures.BuildCatalog(), "Romans", KnownWorldCultures.Roman,
            LivingWorldActorStandingTrend.Established, new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest),
            regionId, settlementId, new GameDate(0)));
    }

    [Test]
    public void ForeignPeopleQueriesResolveLanguageFromCulture()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var actor = ForeignPeopleCreationService.CreateAncientSeed(
            state, KnownWorldCultures.BuildCatalog(), "Suebi", KnownWorldCultures.Germanic,
            LivingWorldActorStandingTrend.Established, new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest),
            regionId, settlementId, new GameDate(0));

        var language = ForeignPeopleQueries.TryResolveLanguage(state, actor.ActorId, CultureLanguageMap.BuildKnownWorldMap());

        Assert.That(language, Is.EqualTo(KnownWorldLanguages.Germanic));
    }
}
