using Gens.Simulation.Events;
using Gens.Simulation.History;
using Gens.Simulation.Random;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.History;

public sealed class HistoricalTimelineSchedulerTests
{
    private static MonthlyTickContext Context(GameDate date) => new(date, new RandomStreamSet());

    [Test]
    public void FiresAnOnTrackUnlinkedEntryAsADigestEventExactlyOnce()
    {
        var catalog = SampleHistoricalTimelineDefinitions.BuildCatalog();
        var entry = catalog.Get(SampleHistoricalTimelineDefinitions.SampleIneligibleEntry);
        var state = new WorldState(entry.Date);
        var scheduler = new HistoricalTimelineScheduler(catalog, HistoricalTimelineRange.Start);

        var firstTick = scheduler.Tick(state, Context(entry.Date));
        var secondTick = scheduler.Tick(state, Context(entry.Date));

        Assert.Multiple(() =>
        {
            Assert.That(firstTick, Has.Count.EqualTo(1));
            Assert.That(firstTick[0], Is.InstanceOf<HistoricalTimelineEntryOccurredEvent>());
            Assert.That(((HistoricalTimelineEntryOccurredEvent)firstTick[0]).EntryId, Is.EqualTo(entry.Id));
            Assert.That(secondTick, Is.Empty, "an already-fired entry must never fire a second time in the same campaign");
        });
    }

    [Test]
    public void DoesNotFireAnEntryOutsideItsOwnMonth()
    {
        var catalog = SampleHistoricalTimelineDefinitions.BuildCatalog();
        var entry = catalog.Get(SampleHistoricalTimelineDefinitions.SampleIneligibleEntry);
        var state = new WorldState(entry.Date);
        var scheduler = new HistoricalTimelineScheduler(catalog, HistoricalTimelineRange.Start);

        var oneMonthEarly = new GameDate(entry.Date.TotalMonths - 1);
        var events = scheduler.Tick(state, Context(oneMonthEarly));

        Assert.That(events, Is.Empty);
    }

    [Test]
    public void NeverFiresAnAlreadyDivergedEntry()
    {
        var catalog = SampleHistoricalTimelineDefinitions.BuildCatalog();
        var entry = catalog.Get(SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry);
        var earlierDate = new GameDate(entry.Date.TotalMonths - 3);
        var state = new WorldState(earlierDate);
        var householdId = state.HouseholdIds.Issue();

        var divergePipeline = RecordDivergenceCommands.BuildPipeline(catalog);
        var divergeResult = divergePipeline.Execute(
            state,
            new RecordDivergenceCommand(
                state.CommandIds.Issue(), "player", earlierDate, null, householdId, "A consequential act",
                new[] { entry.Id }));
        Assume.That(divergeResult.Accepted, Is.True);

        for (var i = 0; i < 3; i++)
            state.AdvanceMonth();
        Assume.That(state.Date, Is.EqualTo(entry.Date));

        // No eventCatalog supplied: even though the sample entry itself carries a real
        // LinkedEventDefinitionRef, this exercises the pure "never fires once Diverged" gate
        // independent of the linked-firing path (covered separately below).
        var scheduler = new HistoricalTimelineScheduler(catalog, HistoricalTimelineRange.Start);
        var events = scheduler.Tick(state, Context(entry.Date));

        Assert.Multiple(() =>
        {
            Assert.That(events, Is.Empty);
            Assert.That(state.FiredHistoricalTimelineEntryIds.TryGet(entry.Id.Value, out _), Is.False);
        });
    }

    [Test]
    public void FiresALinkedEntryThroughTheExistingEventsPipeline()
    {
        var eventCatalog = SampleEventDefinitions.BuildCatalog();
        var catalog = SampleHistoricalTimelineDefinitions.BuildCatalog(eventCatalog);
        var entry = catalog.Get(SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry);
        var state = new WorldState(entry.Date);
        // SampleEventDefinitions.DomesticMurmur (the sample entry's own linked definition) is
        // Personal-scope, so the scheduler resolves its subjects off every named Character (mirroring
        // EventPoolSystem's own per-scope candidate resolution) — a real Character must exist for it to
        // have anyone to fire against.
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        var scheduler = new HistoricalTimelineScheduler(catalog, HistoricalTimelineRange.Start, eventCatalog);

        var events = scheduler.Tick(state, Context(entry.Date));

        Assert.Multiple(() =>
        {
            Assert.That(events, Has.Count.EqualTo(1));
            Assert.That(events[0], Is.InstanceOf<EventFiredEvent>());
            Assert.That(state.EventInstances.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public void DoesNotMarkFiredWhenTheLinkedFireIsRejectedSoALaterTickCanRetry()
    {
        var eventCatalog = SampleEventDefinitions.BuildCatalog();
        var catalog = SampleHistoricalTimelineDefinitions.BuildCatalog(eventCatalog);
        var entry = catalog.Get(SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry);
        var state = new WorldState(entry.Date);
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        var subjectId = characterId.ToTaggedString();

        // Pre-seed an already-active instance of the same linked definition/subject so
        // FireEventCommand's own AlreadyActive rule rejects the scheduler's own attempt this tick.
        var instanceId = state.EventInstanceIds.Issue();
        state.EventInstances.Add(instanceId, new EventInstance(
            instanceId, SampleEventDefinitions.DomesticMurmur, EventScope.Personal, new[] { subjectId },
            subjectId, CurrentStageIndex: 0, FiredDate: entry.Date,
            ExpiresDate: new GameDate(entry.Date.TotalMonths + 2), Status: EventInstanceStatus.Pending));

        var scheduler = new HistoricalTimelineScheduler(catalog, HistoricalTimelineRange.Start, eventCatalog);
        var events = scheduler.Tick(state, Context(entry.Date));

        Assert.Multiple(() =>
        {
            Assert.That(events, Is.Empty, "a rejected fire must not emit an event");
            Assert.That(state.FiredHistoricalTimelineEntryIds.TryGet(entry.Id.Value, out _), Is.False,
                "a rejected fire must not permanently mark this entry as fired");
        });
    }

    [Test]
    public void SaveLoadRoundTripPreservesFiredHistoryAndDoesNotRefire()
    {
        var catalog = SampleHistoricalTimelineDefinitions.BuildCatalog();
        var entry = catalog.Get(SampleHistoricalTimelineDefinitions.SampleIneligibleEntry);
        var state = new WorldState(entry.Date);
        var householdId = state.HouseholdIds.Issue();
        var divergenceId = state.DivergenceRecordIds.Issue();
        state.DivergenceRecords.Add(divergenceId, new DivergenceRecord(
            divergenceId, entry.Date, householdId, "Test trigger",
            new[] { SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry }, NewAlternateHistoryBranchActive: true));

        var scheduler = new HistoricalTimelineScheduler(catalog, HistoricalTimelineRange.Start);
        scheduler.Tick(state, Context(entry.Date));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.FiredHistoricalTimelineEntryIds.Count, Is.EqualTo(state.FiredHistoricalTimelineEntryIds.Count));
            Assert.That(restored.DivergenceRecords.Count, Is.EqualTo(1));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));

            var restoredEvents = scheduler.Tick(restored, Context(entry.Date));
            Assert.That(restoredEvents, Is.Empty, "a restored save must not re-fire an already-resolved entry");
        });
    }
}
