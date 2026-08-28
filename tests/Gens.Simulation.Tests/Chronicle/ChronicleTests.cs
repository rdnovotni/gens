using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Queries;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Chronicle;

/// <summary>Phase 11 item 3 coverage: entries generated from domain events, significance tiers,
/// generational chapters, category/tier/pin filtering, player annotation/pinning/notes, and rival
/// cross-posting (<c>gens-dynasty-chronicle-design.md</c>).</summary>
public sealed class ChronicleTests
{
    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Character> HeadId) HouseholdWithHead()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, birthDate: new GameDate(-360)));
        var householdId = state.HouseholdIds.Issue();
        return (state, householdId, headId);
    }

    [Test]
    public void BirthEventProjectsAMinorTierBirthsAndDeathsEntry()
    {
        var (state, householdId, motherId) = HouseholdWithHead();
        state.Characters.Remove(motherId);
        state.Characters.Add(motherId, CharacterTestFixtures.Minimal(motherId, household: householdId, birthDate: new GameDate(-360)));
        var childId = state.CharacterIds.Issue();
        state.Characters.Add(childId, CharacterTestFixtures.Minimal(childId, motherId: motherId, household: householdId, birthDate: new GameDate(5)));

        var born = new CharacterBornEvent(state.EventIds.Issue(), new GameDate(5), childId, motherId, null, Legitimacy.Legitimate, null);
        var events = ChronicleGenerationSystem.Generate(state, new IDomainEvent[] { born });

        Assert.That(events, Has.Count.EqualTo(1));
        Assert.That(state.ChronicleEntries.Count, Is.EqualTo(1));
        var entry = state.ChronicleEntries.InAscendingOrder().First().Value;
        Assert.Multiple(() =>
        {
            Assert.That(entry.Tier, Is.EqualTo(ChronicleTier.Minor));
            Assert.That(entry.Category, Is.EqualTo(ChronicleCategory.BirthsAndDeaths));
            Assert.That(entry.HouseholdId, Is.EqualTo(householdId));
            Assert.That(entry.LinkedCharacterIds, Is.EquivalentTo(new[] { childId, motherId }));
            Assert.That(entry.Source, Is.EqualTo(ChronicleEntrySource.System));
        });
    }

    [Test]
    public void DeathOfAPlayerControlledCharacterIsLegendaryOtherwiseMajor()
    {
        var (state, householdId, headId) = HouseholdWithHead();
        state.Characters.Remove(headId);
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(
            headId, household: householdId, birthDate: new GameDate(-360),
            deathRecord: new DeathRecord(new GameDate(10), DeathCause.OldAge, 30)));
        state.PlayerControls.Add(householdId, new PlayerControlState(householdId, headId, PlayerControlMode.DirectHead));

        var died = new CharacterDiedEvent(state.EventIds.Issue(), new GameDate(10), headId, null, new DeathRecord(new GameDate(10), DeathCause.OldAge, 30));
        ChronicleGenerationSystem.Generate(state, new IDomainEvent[] { died });

        var entry = state.ChronicleEntries.InAscendingOrder().First().Value;
        Assert.That(entry.Tier, Is.EqualTo(ChronicleTier.Legendary));
    }

    [Test]
    public void DeathIsStillLegendaryWhenPlayerControlHasAlreadyHandedOffToTheSuccessorThisSameTick()
    {
        // Mirrors a full monthly tick's real ordering: PlayerControlHandoffSystem runs (and already
        // updates PlayerControls to the successor) before ChronicleGenerationSystem.Generate ever sees
        // state — so a plain "is state.PlayerControls currently pointing at the deceased" check would
        // wrongly read this as an ordinary death.
        var (state, householdId, headId) = HouseholdWithHead();
        state.Characters.Remove(headId);
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(
            headId, household: householdId, birthDate: new GameDate(-360),
            deathRecord: new DeathRecord(new GameDate(10), DeathCause.OldAge, 30)));
        var heirId = state.CharacterIds.Issue();
        state.Characters.Add(heirId, CharacterTestFixtures.Minimal(heirId, household: householdId, birthDate: new GameDate(-240)));
        state.PlayerControls.Add(householdId, new PlayerControlState(householdId, heirId, PlayerControlMode.DirectHead));

        var died = new CharacterDiedEvent(state.EventIds.Issue(), new GameDate(10), headId, null, new DeathRecord(new GameDate(10), DeathCause.OldAge, 30));
        var handoff = new PlayerControlChangedEvent(
            state.EventIds.Issue(), new GameDate(10), householdId, headId, heirId, PlayerControlMode.DirectHead, PlayerControlMode.DirectHead);
        ChronicleGenerationSystem.Generate(state, new IDomainEvent[] { died, handoff });

        var deathEntry = state.ChronicleEntries.InAscendingOrder().First(e => e.Value.SourceSystem == died.Type).Value;
        Assert.That(deathEntry.Tier, Is.EqualTo(ChronicleTier.Legendary));
    }

    [Test]
    public void HeadshipEstablishedAndTransferredOpenAndCloseGenerationalChapters()
    {
        var (state, householdId, headId) = HouseholdWithHead();
        var heirId = state.CharacterIds.Issue();
        state.Characters.Add(heirId, CharacterTestFixtures.Minimal(heirId, household: householdId, birthDate: new GameDate(-240)));

        var established = new HouseholdHeadEstablishedEvent(state.EventIds.Issue(), new GameDate(0), householdId, headId, null);
        ChronicleGenerationSystem.Generate(state, new IDomainEvent[] { established });

        Assert.That(state.GenerationalChapters.Count, Is.EqualTo(1));
        var openChapter = state.GenerationalChapters.InAscendingOrder().First().Value;
        Assert.That(openChapter.EndMonth, Is.Null);

        var transferred = new HouseholdHeadTransferredEvent(
            state.EventIds.Issue(), new GameDate(20), householdId, headId, heirId, HandoffTrigger.OrdinaryInheritance);
        ChronicleGenerationSystem.Generate(state, new IDomainEvent[] { transferred });

        Assert.That(state.GenerationalChapters.Count, Is.EqualTo(2));
        var chapters = state.GenerationalChapters.InAscendingOrder().Select(e => e.Value).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(chapters[0].EndMonth, Is.EqualTo(new GameDate(20)));
            Assert.That(chapters[1].HeadCharacterId, Is.EqualTo(heirId));
            Assert.That(chapters[1].EndMonth, Is.Null);
        });
    }

    [Test]
    public void ExtinctionClosesTheOpenChapterAndNeverOpensANewOne()
    {
        var (state, householdId, headId) = HouseholdWithHead();
        var established = new HouseholdHeadEstablishedEvent(state.EventIds.Issue(), new GameDate(0), householdId, headId, null);
        ChronicleGenerationSystem.Generate(state, new IDomainEvent[] { established });

        var extinguished = new HouseholdExtinguishedEvent(state.EventIds.Issue(), new GameDate(30), householdId, headId);
        ChronicleGenerationSystem.Generate(state, new IDomainEvent[] { extinguished });

        Assert.That(state.GenerationalChapters.Count, Is.EqualTo(1));
        var chapter = state.GenerationalChapters.InAscendingOrder().First().Value;
        Assert.That(chapter.EndMonth, Is.EqualTo(new GameDate(30)));

        var extinctionEntry = state.ChronicleEntries.InAscendingOrder().Last().Value;
        Assert.That(extinctionEntry.Tier, Is.EqualTo(ChronicleTier.Legendary));
    }

    [Test]
    public void RivalHouseExtinctionCrossPostsToItsOwnDossierWithNoOwningHousehold()
    {
        var state = new WorldState(new GameDate(0));
        var actorId = state.ActorIds.Issue();

        var extinguished = new LivingWorldActorExtinguishedEvent(state.EventIds.Issue(), new GameDate(4), actorId, "Gens Cornelia", LivingWorldActorTier.Background);
        ChronicleGenerationSystem.Generate(state, new IDomainEvent[] { extinguished });

        var entry = state.ChronicleEntries.InAscendingOrder().First().Value;
        Assert.Multiple(() =>
        {
            Assert.That(entry.HouseholdId, Is.Null);
            Assert.That(entry.Tier, Is.EqualTo(ChronicleTier.Legendary));
            Assert.That(state.RivalDossiers.TryGet(actorId, out var dossier), Is.True);
            Assert.That(dossier!.RecentChronicleEntries, Is.EquivalentTo(new[] { entry.EntryId }));
        });
    }

    [Test]
    public void RuinedInsolvencyIsTheOnlyChronicleWorthyInsolvencyStage()
    {
        var (state, householdId, _) = HouseholdWithHead();

        var atRisk = new InsolvencyStageChangedEvent(state.EventIds.Issue(), new GameDate(2), householdId, InsolvencyStage.AtRisk, 2);
        var ruined = new InsolvencyStageChangedEvent(state.EventIds.Issue(), new GameDate(8), householdId, InsolvencyStage.Ruined, 8);
        ChronicleGenerationSystem.Generate(state, new IDomainEvent[] { atRisk, ruined });

        Assert.That(state.ChronicleEntries.Count, Is.EqualTo(1));
        var entry = state.ChronicleEntries.InAscendingOrder().First().Value;
        Assert.That(entry.Tier, Is.EqualTo(ChronicleTier.Major));
        Assert.That(entry.Category, Is.EqualTo(ChronicleCategory.WealthAndBuilding));
    }

    [Test]
    public void PinAnnotateAndAddNoteCommandsRoundTrip()
    {
        var (state, householdId, motherId) = HouseholdWithHead();
        var childId = state.CharacterIds.Issue();
        state.Characters.Add(childId, CharacterTestFixtures.Minimal(childId, motherId: motherId, household: householdId, birthDate: new GameDate(1)));
        var born = new CharacterBornEvent(state.EventIds.Issue(), new GameDate(1), childId, motherId, null, Legitimacy.Legitimate, null);
        ChronicleGenerationSystem.Generate(state, new IDomainEvent[] { born });
        var entryId = state.ChronicleEntries.InAscendingOrder().First().Key;

        var pinResult = SetChronicleEntryPinnedCommands.Pipeline.Execute(
            state, new SetChronicleEntryPinnedCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, entryId, true));
        var annotateResult = AnnotateChronicleEntryCommands.Pipeline.Execute(
            state, new AnnotateChronicleEntryCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, entryId, "A day I remember."));

        state.ChronicleEntries.TryGet(entryId, out var updated);
        Assert.Multiple(() =>
        {
            Assert.That(pinResult.Accepted, Is.True);
            Assert.That(annotateResult.Accepted, Is.True);
            Assert.That(updated!.Pinned, Is.True);
            Assert.That(updated.PlayerAnnotation, Is.EqualTo("A day I remember."));
        });

        var noteResult = AddChronicleNoteCommands.Pipeline.Execute(
            state,
            new AddChronicleNoteCommand(
                state.CommandIds.Issue(), "player", new GameDate(3), null, householdId,
                "This is the day I decided never to trust the Cornelii again.", ChronicleCategory.Other, Array.Empty<RuntimeId<Character>>()));

        Assert.That(noteResult.Accepted, Is.True);
        Assert.That(state.ChronicleEntries.Count, Is.EqualTo(2));
        var note = state.ChronicleEntries.InAscendingOrder().Last().Value;
        Assert.That(note.Source, Is.EqualTo(ChronicleEntrySource.PlayerNote));
    }

    [Test]
    public void QueryExcludesMinorTierByDefaultButKeepsPinnedMinorEntries()
    {
        var (state, householdId, motherId) = HouseholdWithHead();
        var childId = state.CharacterIds.Issue();
        state.Characters.Add(childId, CharacterTestFixtures.Minimal(childId, motherId: motherId, household: householdId, birthDate: new GameDate(1)));
        var born = new CharacterBornEvent(state.EventIds.Issue(), new GameDate(1), childId, motherId, null, Legitimacy.Legitimate, null);
        ChronicleGenerationSystem.Generate(state, new IDomainEvent[] { born });
        var entryId = state.ChronicleEntries.InAscendingOrder().First().Key;

        var query = new ChronicleQuery(householdId);
        var projection = query.Execute(state, "player");
        Assert.That(projection.Entries, Is.Empty);

        SetChronicleEntryPinnedCommands.Pipeline.Execute(
            state, new SetChronicleEntryPinnedCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, entryId, true));

        var afterPin = query.Execute(state, "player");
        Assert.That(afterPin.Entries, Has.Count.EqualTo(1));
    }

    [Test]
    public void ChronicleEntriesAndChaptersRoundTripThroughTheDtoAndStateHash()
    {
        var (state, householdId, headId) = HouseholdWithHead();
        var established = new HouseholdHeadEstablishedEvent(state.EventIds.Issue(), new GameDate(0), householdId, headId, null);
        ChronicleGenerationSystem.Generate(state, new IDomainEvent[] { established });
        var entryId = state.ChronicleEntries.InAscendingOrder().First().Key;
        SetChronicleEntryPinnedCommands.Pipeline.Execute(
            state, new SetChronicleEntryPinnedCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, entryId, true));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);
        var afterHash = StateHasher.Hash(restored);

        Assert.Multiple(() =>
        {
            Assert.That(afterHash, Is.EqualTo(beforeHash));
            Assert.That(restored.ChronicleEntries.Count, Is.EqualTo(state.ChronicleEntries.Count));
            Assert.That(restored.GenerationalChapters.Count, Is.EqualTo(state.GenerationalChapters.Count));
        });
    }
}
