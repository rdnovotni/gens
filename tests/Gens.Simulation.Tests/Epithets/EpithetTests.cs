using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Commands;
using Gens.Simulation.Epithets;
using Gens.Simulation.Identity;
using Gens.Simulation.Queries;
using Gens.Simulation.Random;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Epithets;

/// <summary>Phase 11 item 5 coverage: achievement/succession-victory Agnomen awards, dynastic epithets,
/// and inherited-cognomen adoption (<c>gens-epithets-nicknames-titles-design.md</c>).</summary>
public sealed class EpithetTests
{
    private const string StreamName = "test-generation";

    private static RuntimeId<Household> Establish(WorldState state, RuntimeId<Character> headId, GameDate since)
    {
        var householdId = state.HouseholdIds.Issue();
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, since));
        return householdId;
    }

    private static RuntimeId<ChronicleEntry> SeedEntry(
        WorldState state, RuntimeId<Household>? householdId, ChronicleCategory category, ChronicleTier tier,
        RuntimeId<Character>? linkedCharacterId)
    {
        var entryId = state.ChronicleEntryIds.Issue();
        state.ChronicleEntries.Add(entryId, new ChronicleEntry(
            entryId, householdId, new GameDate(0), category, tier, "Test entry.",
            linkedCharacterId is { } id ? new[] { id } : Array.Empty<RuntimeId<Character>>(),
            "test.seed", ChronicleEntrySource.System));
        return entryId;
    }

    [Test]
    public void AnAchievementAgnomenIsGrantedAfterThreeMajorOrLegendaryLinkedEntries()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        var householdId = Establish(state, headId, new GameDate(0));
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, household: householdId));

        SeedEntry(state, householdId, ChronicleCategory.PoliticsAndOffice, ChronicleTier.Major, headId);
        SeedEntry(state, householdId, ChronicleCategory.PoliticsAndOffice, ChronicleTier.Legendary, headId);
        var thirdEntryId = SeedEntry(state, householdId, ChronicleCategory.PoliticsAndOffice, ChronicleTier.Major, headId);

        var recordedEvent = new ChronicleEntryRecordedEvent(state.EventIds.Issue(), new GameDate(0), thirdEntryId, householdId, ChronicleTier.Major, null);
        var produced = EpithetGenerationSystem.Generate(state, new IDomainEvent[] { recordedEvent });

        Assert.Multiple(() =>
        {
            Assert.That(produced.OfType<AgnomenGrantedEvent>().Count(), Is.EqualTo(1));
            var granted = produced.OfType<AgnomenGrantedEvent>().Single();
            Assert.That(granted.Name, Is.EqualTo(AgnomenCatalog.AchievementAgnomenName));
            Assert.That(granted.CharacterId, Is.EqualTo(headId));

            Assert.That(state.Agnomens.Count, Is.EqualTo(1));
            var agnomen = state.Agnomens.InAscendingOrder().First().Value;
            Assert.That(agnomen.AgnomenType, Is.EqualTo(AgnomenType.VirtueOrAchievement));
            Assert.That(agnomen.GrantMethod, Is.EqualTo(AgnomenGrantMethod.OrganicCrowdOrigin));
            Assert.That(agnomen.SourceChronicleEntryIds, Has.Count.EqualTo(3));
            Assert.That(agnomen.DignitasEffect, Is.Null);
            Assert.That(agnomen.FameEffect, Is.Null);
            Assert.That(agnomen.IsSuppressible, Is.False);
        });
    }

    [Test]
    public void AnAchievementAgnomenIsNeverGrantedTwiceToTheSameCharacter()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        var householdId = Establish(state, headId, new GameDate(0));
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, household: householdId));

        for (var i = 0; i < 4; i++)
            SeedEntry(state, householdId, ChronicleCategory.PoliticsAndOffice, ChronicleTier.Major, headId);
        var lastEntryId = state.ChronicleEntries.InAscendingOrder().Last().Key;

        var recordedEvent = new ChronicleEntryRecordedEvent(state.EventIds.Issue(), new GameDate(0), lastEntryId, householdId, ChronicleTier.Major, null);
        EpithetGenerationSystem.Generate(state, new IDomainEvent[] { recordedEvent });
        var secondPass = EpithetGenerationSystem.Generate(state, new IDomainEvent[] { recordedEvent });

        Assert.Multiple(() =>
        {
            Assert.That(state.Agnomens.Count, Is.EqualTo(1));
            Assert.That(secondPass.OfType<AgnomenGrantedEvent>(), Is.Empty);
        });
    }

    [Test]
    public void FelixIsGrantedToTheWinnerOfAResolvedSuccessionDispute()
    {
        var state = new WorldState(new GameDate(0));
        var winnerId = state.CharacterIds.Issue();
        var householdId = state.HouseholdIds.Issue();
        var disputeId = state.SuccessionDisputeIds.Issue();

        var resolved = new SuccessionDisputeResolvedEvent(
            state.EventIds.Issue(), new GameDate(3), disputeId, householdId, winnerId, SuccessionDisputeStatus.ResolvedByFavor);
        var produced = EpithetGenerationSystem.Generate(state, new IDomainEvent[] { resolved });

        var granted = produced.OfType<AgnomenGrantedEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(granted.Name, Is.EqualTo(AgnomenCatalog.SuccessionVictoryAgnomenName));
            Assert.That(granted.CharacterId, Is.EqualTo(winnerId));

            var agnomen = state.Agnomens.InAscendingOrder().First().Value;
            Assert.That(agnomen.SourceSuccessionDisputeId, Is.EqualTo(disputeId));
        });
    }

    [Test]
    public void ADisputeThatResolvesWithoutAWinnerGrantsNoAgnomen()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var disputeId = state.SuccessionDisputeIds.Issue();

        var resolved = new SuccessionDisputeResolvedEvent(
            state.EventIds.Issue(), new GameDate(3), disputeId, householdId, null, SuccessionDisputeStatus.ResolvedByFavor);
        var produced = EpithetGenerationSystem.Generate(state, new IDomainEvent[] { resolved });

        Assert.That(produced, Is.Empty);
        Assert.That(state.Agnomens.Count, Is.EqualTo(0));
    }

    [Test]
    public void ADynasticEpithetIsSetOnceAHouseholdCrossesTheChronicleThreshold()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();

        for (var i = 0; i < 4; i++)
            SeedEntry(state, householdId, ChronicleCategory.PoliticsAndOffice, ChronicleTier.Major, null);
        var fifthEntryId = SeedEntry(state, householdId, ChronicleCategory.PoliticsAndOffice, ChronicleTier.Legendary, null);

        var recordedEvent = new ChronicleEntryRecordedEvent(state.EventIds.Issue(), new GameDate(0), fifthEntryId, householdId, ChronicleTier.Legendary, null);
        var produced = EpithetGenerationSystem.Generate(state, new IDomainEvent[] { recordedEvent });

        Assert.Multiple(() =>
        {
            Assert.That(produced.OfType<DynasticEpithetChangedEvent>().Count(), Is.EqualTo(1));
            Assert.That(state.DynasticEpithets.TryGet(householdId, out var epithet), Is.True);
            Assert.That(epithet!.EpithetText, Is.EqualTo(DynasticEpithetCatalog.TemplateFor(ChronicleCategory.PoliticsAndOffice)));
            Assert.That(epithet.DerivedFromChronicleEntryIds, Has.Count.EqualTo(5));
        });
    }

    [Test]
    public void BelowThresholdNoDynasticEpithetIsSet()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();

        var entryId = SeedEntry(state, householdId, ChronicleCategory.PoliticsAndOffice, ChronicleTier.Major, null);
        var recordedEvent = new ChronicleEntryRecordedEvent(state.EventIds.Issue(), new GameDate(0), entryId, householdId, ChronicleTier.Major, null);
        EpithetGenerationSystem.Generate(state, new IDomainEvent[] { recordedEvent });

        Assert.That(state.DynasticEpithets.TryGet(householdId, out _), Is.False);
    }

    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Character> HeadId, RuntimeId<Agnomen> AgnomenId) HouseholdWithAgnomen()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        var householdId = Establish(state, headId, new GameDate(0));
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, household: householdId));

        var agnomenId = state.AgnomenIds.Issue();
        state.Agnomens.Add(agnomenId, new Agnomen(
            agnomenId, headId, AgnomenType.VirtueOrAchievement, AgnomenCatalog.AchievementAgnomenName,
            AgnomenGrantMethod.OrganicCrowdOrigin, new GameDate(0), Array.Empty<RuntimeId<ChronicleEntry>>(), null, null, null, false));

        return (state, householdId, headId, agnomenId);
    }

    [Test]
    public void AdoptingAnAgnomenAsCognomenSucceedsAndEmitsCognomenAdoptedEvent()
    {
        var (state, householdId, _, agnomenId) = HouseholdWithAgnomen();

        var command = new AdoptAgnomenAsCognomenCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, agnomenId);
        var result = AdoptAgnomenAsCognomenCommands.Pipeline.Execute(state, command);

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(result.Events.OfType<CognomenAdoptedEvent>().Count(), Is.EqualTo(1));
            Assert.That(state.InheritedCognomenDecisions.Count, Is.EqualTo(1));
            Assert.That(InheritedCognomenResolver.CurrentCognomen(state, householdId), Is.EqualTo(AgnomenCatalog.AchievementAgnomenName));
        });
    }

    [Test]
    public void AdoptingTheSameAgnomenTwiceFails()
    {
        var (state, householdId, _, agnomenId) = HouseholdWithAgnomen();
        var first = new AdoptAgnomenAsCognomenCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, agnomenId);
        AdoptAgnomenAsCognomenCommands.Pipeline.Execute(state, first);

        var second = new AdoptAgnomenAsCognomenCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, householdId, agnomenId);
        var result = AdoptAgnomenAsCognomenCommands.Pipeline.Execute(state, second);

        Assert.That(result.Error, Is.EqualTo(AdoptAgnomenAsCognomenCommands.AlreadyAdopted));
    }

    [Test]
    public void AdoptingAnAgnomenForAHouseholdWithNoHeadFails()
    {
        var (state, _, _, agnomenId) = HouseholdWithAgnomen();
        var otherHouseholdId = state.HouseholdIds.Issue();

        var command = new AdoptAgnomenAsCognomenCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, otherHouseholdId, agnomenId);
        var result = AdoptAgnomenAsCognomenCommands.Pipeline.Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(AdoptAgnomenAsCognomenCommands.HouseholdHasNoHead));
    }

    [Test]
    public void AdoptingAnUnknownAgnomenFails()
    {
        var (state, householdId, _, _) = HouseholdWithAgnomen();
        var unknownAgnomenId = state.AgnomenIds.Issue();

        var command = new AdoptAgnomenAsCognomenCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, unknownAgnomenId);
        var result = AdoptAgnomenAsCognomenCommands.Pipeline.Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(AdoptAgnomenAsCognomenCommands.AgnomenNotFound));
    }

    [Test]
    public void AdoptingAnAgnomenBelongingToAnotherHouseholdsCharacterFails()
    {
        var (state, _, _, agnomenId) = HouseholdWithAgnomen();
        var otherHeadId = state.CharacterIds.Issue();
        var otherHouseholdId = Establish(state, otherHeadId, new GameDate(0));
        state.Characters.Add(otherHeadId, CharacterTestFixtures.Minimal(otherHeadId, household: otherHouseholdId));

        var command = new AdoptAgnomenAsCognomenCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, otherHouseholdId, agnomenId);
        var result = AdoptAgnomenAsCognomenCommands.Pipeline.Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(AdoptAgnomenAsCognomenCommands.AgnomenCharacterNotOfHousehold));
    }

    [Test]
    public void AnAdoptedCognomenOverridesAFutureBirthsGeneratedCognomen()
    {
        var (state, householdId, headId, agnomenId) = HouseholdWithAgnomen();
        var adoptCommand = new AdoptAgnomenAsCognomenCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, agnomenId);
        AdoptAgnomenAsCognomenCommands.Pipeline.Execute(state, adoptCommand);

        var motherId = state.CharacterIds.Issue();
        state.Characters.Add(motherId, CharacterTestFixtures.Minimal(
            motherId, birthDate: new GameDate(300 - 25 * 12), household: householdId));

        var streams = new RandomStreamSet();
        streams.Add(StreamName, 7, 1);
        var pipeline = BirthCharacterCommands.CreatePipeline(streams);
        var birthCommand = new BirthCharacterCommand(
            state.CommandIds.Issue(), "player", new GameDate(300), null,
            motherId, null, Sex.Female, LegalStatus.RomanCitizen, SocialClass.Plebeian,
            NamePoolTestFixtures.Roman, StreamName);

        var result = pipeline.Execute(state, birthCommand);
        var born = (CharacterBornEvent)result.Events.Single();
        state.Characters.TryGet(born.CharacterId, out var child);

        Assert.That(child!.Cognomen, Is.EqualTo(AgnomenCatalog.AchievementAgnomenName));
        _ = headId;
    }

    [Test]
    public void EpithetStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        var householdId = Establish(state, headId, new GameDate(0));
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, household: householdId));

        SeedEntry(state, householdId, ChronicleCategory.PoliticsAndOffice, ChronicleTier.Major, headId);
        SeedEntry(state, householdId, ChronicleCategory.PoliticsAndOffice, ChronicleTier.Major, headId);
        SeedEntry(state, householdId, ChronicleCategory.PoliticsAndOffice, ChronicleTier.Major, headId);
        SeedEntry(state, householdId, ChronicleCategory.PoliticsAndOffice, ChronicleTier.Major, null);
        var fifthEntryId = SeedEntry(state, householdId, ChronicleCategory.PoliticsAndOffice, ChronicleTier.Legendary, headId);

        var recordedEvent = new ChronicleEntryRecordedEvent(state.EventIds.Issue(), new GameDate(0), fifthEntryId, householdId, ChronicleTier.Legendary, null);
        EpithetGenerationSystem.Generate(state, new IDomainEvent[] { recordedEvent });

        var agnomenId = state.Agnomens.InAscendingOrder().First().Key;
        var adoptCommand = new AdoptAgnomenAsCognomenCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, agnomenId);
        AdoptAgnomenAsCognomenCommands.Pipeline.Execute(state, adoptCommand);

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.Agnomens.Count, Is.EqualTo(1));
            Assert.That(restored.InheritedCognomenDecisions.Count, Is.EqualTo(1));
            Assert.That(restored.DynasticEpithets.TryGet(householdId, out var epithet), Is.True);
            Assert.That(epithet!.EpithetText, Is.EqualTo(DynasticEpithetCatalog.TemplateFor(ChronicleCategory.PoliticsAndOffice)));
            Assert.That(InheritedCognomenResolver.CurrentCognomen(restored, householdId), Is.EqualTo(AgnomenCatalog.AchievementAgnomenName));

            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }

    [Test]
    public void DynasticEpithetProvenanceKeepsGrowingEvenWhenTheTextDoesNotChange()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();

        for (var i = 0; i < 4; i++)
            SeedEntry(state, householdId, ChronicleCategory.PoliticsAndOffice, ChronicleTier.Major, null);
        var fifthEntryId = SeedEntry(state, householdId, ChronicleCategory.PoliticsAndOffice, ChronicleTier.Legendary, null);
        var firstRecorded = new ChronicleEntryRecordedEvent(state.EventIds.Issue(), new GameDate(0), fifthEntryId, householdId, ChronicleTier.Legendary, null);
        var firstProduced = EpithetGenerationSystem.Generate(state, new IDomainEvent[] { firstRecorded });

        state.DynasticEpithets.TryGet(householdId, out var afterFirst);
        Assert.Multiple(() =>
        {
            Assert.That(firstProduced.OfType<DynasticEpithetChangedEvent>().Count(), Is.EqualTo(1));
            Assert.That(afterFirst!.DerivedFromChronicleEntryIds, Has.Count.EqualTo(5));
        });

        // A sixth qualifying entry in the same dominant category: the visible text stays the same, but
        // the provenance list must still pick it up rather than freezing at the first five.
        var sixthEntryId = SeedEntry(state, householdId, ChronicleCategory.PoliticsAndOffice, ChronicleTier.Major, null);
        var secondRecorded = new ChronicleEntryRecordedEvent(state.EventIds.Issue(), new GameDate(1), sixthEntryId, householdId, ChronicleTier.Major, null);
        var secondProduced = EpithetGenerationSystem.Generate(state, new IDomainEvent[] { secondRecorded });

        state.DynasticEpithets.TryGet(householdId, out var afterSecond);
        Assert.Multiple(() =>
        {
            Assert.That(secondProduced.OfType<DynasticEpithetChangedEvent>(), Is.Empty, "text did not change, so no second change event");
            Assert.That(afterSecond!.EpithetText, Is.EqualTo(afterFirst.EpithetText));
            Assert.That(afterSecond.DerivedFromChronicleEntryIds, Has.Count.EqualTo(6));
        });
    }

    [Test]
    public void ASameMonthLaterCognomenAdoptionSupersedesAnEarlierOne()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        var householdId = Establish(state, headId, new GameDate(0));
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, household: householdId));

        var firstAgnomenId = state.AgnomenIds.Issue();
        state.Agnomens.Add(firstAgnomenId, new Agnomen(
            firstAgnomenId, headId, AgnomenType.VirtueOrAchievement, "Magnus",
            AgnomenGrantMethod.OrganicCrowdOrigin, new GameDate(0), Array.Empty<RuntimeId<ChronicleEntry>>(), null, null, null, false));
        var secondAgnomenId = state.AgnomenIds.Issue();
        state.Agnomens.Add(secondAgnomenId, new Agnomen(
            secondAgnomenId, headId, AgnomenType.VirtueOrAchievement, "Felix",
            AgnomenGrantMethod.OrganicCrowdOrigin, new GameDate(0), Array.Empty<RuntimeId<ChronicleEntry>>(), null, null, null, false));

        // Both commands are submitted within the same month — same EffectiveFromDate on both decisions.
        var sameMonth = new GameDate(5);
        var first = new AdoptAgnomenAsCognomenCommand(state.CommandIds.Issue(), "player", sameMonth, null, householdId, firstAgnomenId);
        AdoptAgnomenAsCognomenCommands.Pipeline.Execute(state, first);
        var second = new AdoptAgnomenAsCognomenCommand(state.CommandIds.Issue(), "player", sameMonth, null, householdId, secondAgnomenId);
        var secondResult = AdoptAgnomenAsCognomenCommands.Pipeline.Execute(state, second);

        Assert.That(secondResult.Accepted, Is.True);
        Assert.That(InheritedCognomenResolver.CurrentCognomen(state, householdId), Is.EqualTo("Felix"));
    }

    [Test]
    public void AdoptingACognomenCreatesAMajorChronicleEntry()
    {
        var (state, householdId, _, agnomenId) = HouseholdWithAgnomen();

        var command = new AdoptAgnomenAsCognomenCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, agnomenId);
        var result = AdoptAgnomenAsCognomenCommands.Pipeline.Execute(state, command);
        var chronicleEvents = ChronicleGenerationSystem.Generate(state, result.Events);

        Assert.That(chronicleEvents.OfType<ChronicleEntryRecordedEvent>().Count(), Is.EqualTo(1));
        var entry = state.ChronicleEntries.InAscendingOrder().First().Value;
        Assert.Multiple(() =>
        {
            Assert.That(entry.Category, Is.EqualTo(ChronicleCategory.MarriagesAndFamily));
            Assert.That(entry.Tier, Is.EqualTo(ChronicleTier.Major));
            Assert.That(entry.HouseholdId, Is.EqualTo(householdId));
            Assert.That(entry.Prose, Does.Contain(AgnomenCatalog.AchievementAgnomenName));
        });
    }

    [Test]
    public void EpithetQueryProjectsAHouseholdsAgnomenaAndDynasticEpithet()
    {
        var (state, householdId, headId, agnomenId) = HouseholdWithAgnomen();
        state.DynasticEpithets.Add(householdId, new DynasticEpithet(
            householdId, DynasticEpithetCatalog.TemplateFor(ChronicleCategory.PoliticsAndOffice), Array.Empty<RuntimeId<ChronicleEntry>>()));

        var projection = new EpithetQuery(householdId).Execute(state, "player");

        Assert.Multiple(() =>
        {
            Assert.That(projection.HouseholdId, Is.EqualTo(householdId.ToTaggedString()));
            Assert.That(projection.DynasticEpithetText, Is.EqualTo(DynasticEpithetCatalog.TemplateFor(ChronicleCategory.PoliticsAndOffice)));
            Assert.That(projection.Agnomens, Has.Count.EqualTo(1));
            Assert.That(projection.Agnomens[0].AgnomenId, Is.EqualTo(agnomenId.ToTaggedString()));
            Assert.That(projection.Agnomens[0].CharacterId, Is.EqualTo(headId.ToTaggedString()));
            Assert.That(projection.Agnomens[0].Name, Is.EqualTo(AgnomenCatalog.AchievementAgnomenName));
        });
    }

    [Test]
    public void EpithetQueryReturnsNoDynasticEpithetTextWhenNoneHasBeenEarned()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();

        var projection = new EpithetQuery(householdId).Execute(state, "player");

        Assert.Multiple(() =>
        {
            Assert.That(projection.DynasticEpithetText, Is.Null);
            Assert.That(projection.Agnomens, Is.Empty);
        });
    }
}
