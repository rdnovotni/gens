using Gens.Simulation.History;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.History;

public sealed class RecordDivergenceCommandTests
{
    private static (WorldState State, HistoricalTimelineCatalog Catalog, RuntimeId<Household> HouseholdId) Fixture(GameDate date)
    {
        var state = new WorldState(date);
        var householdId = state.HouseholdIds.Issue();
        return (state, SampleHistoricalTimelineDefinitions.BuildCatalog(), householdId);
    }

    private static RecordDivergenceCommand Command(
        WorldState state, RuntimeId<Household> householdId,
        params DefinitionId<HistoricalTimelineEntryDefinition>[] entryIds) =>
        new(state.CommandIds.Issue(), "player", state.Date, null, householdId, "A consequential act", entryIds);

    [Test]
    public void NoAffectedEntriesIsRejected()
    {
        var (state, catalog, householdId) = Fixture(HistoricalTimelineRange.Start);
        var pipeline = RecordDivergenceCommands.BuildPipeline(catalog);

        var result = pipeline.Execute(state, Command(state, householdId));

        Assert.That(result.Error, Is.EqualTo(RecordDivergenceCommands.NoAffectedEntries));
    }

    [Test]
    public void AnUnknownTimelineEntryIsRejected()
    {
        var (state, catalog, householdId) = Fixture(HistoricalTimelineRange.Start);
        var pipeline = RecordDivergenceCommands.BuildPipeline(catalog);

        var result = pipeline.Execute(state, Command(state, householdId, new DefinitionId<HistoricalTimelineEntryDefinition>("unregistered")));

        Assert.That(result.Error, Is.EqualTo(RecordDivergenceCommands.UnknownTimelineEntry));
    }

    [Test]
    public void ANonEligibleEntryIsRejected()
    {
        var (state, catalog, householdId) = Fixture(HistoricalTimelineRange.Start);
        var pipeline = RecordDivergenceCommands.BuildPipeline(catalog);

        var result = pipeline.Execute(
            state, Command(state, householdId, SampleHistoricalTimelineDefinitions.SampleIneligibleEntry));

        Assert.That(result.Error, Is.EqualTo(RecordDivergenceCommands.NotDivergenceEligible));
    }

    [Test]
    public void AnEntryWhoseRealDateHasAlreadyPassedIsRejected()
    {
        var entryDate = SampleHistoricalTimelineDefinitions.BuildCatalog()
            .Get(SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry).Date;
        var afterEntry = new GameDate(entryDate.TotalMonths + 12);
        var (state, catalog, householdId) = Fixture(afterEntry);
        var pipeline = RecordDivergenceCommands.BuildPipeline(catalog);

        var result = pipeline.Execute(
            state, Command(state, householdId, SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry));

        Assert.That(result.Error, Is.EqualTo(RecordDivergenceCommands.EntryAlreadyPast));
    }

    [Test]
    public void AnEntryAlreadyFiredThisMonthIsRejected()
    {
        // Strict "<" alone would still accept this: the entry's own real date equals state.Date exactly
        // (HistoricalTimelineScheduler fires same-month entries), so only the FiredHistoricalTimelineEntryIds
        // check actually catches an already-emitted historical fact from retroactively becoming "diverged."
        var entry = SampleHistoricalTimelineDefinitions.BuildCatalog()
            .Get(SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry);
        var (state, catalog, householdId) = Fixture(entry.Date);
        state.FiredHistoricalTimelineEntryIds.Add(entry.Id.Value, entry.Date);
        var pipeline = RecordDivergenceCommands.BuildPipeline(catalog);

        var result = pipeline.Execute(
            state, Command(state, householdId, SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry));

        Assert.That(result.Error, Is.EqualTo(RecordDivergenceCommands.EntryAlreadyPast));
    }

    [Test]
    public void AnAlreadyDivergedEntryIsRejected()
    {
        var (state, catalog, householdId) = Fixture(HistoricalTimelineRange.Start);
        var pipeline = RecordDivergenceCommands.BuildPipeline(catalog);
        var firstResult = pipeline.Execute(
            state, Command(state, householdId, SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry));
        Assume.That(firstResult.Accepted, Is.True);

        var secondResult = pipeline.Execute(
            state, Command(state, householdId, SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry));

        Assert.That(secondResult.Error, Is.EqualTo(RecordDivergenceCommands.EntryAlreadyDiverged));
    }

    [Test]
    public void AValidDivergenceIsAcceptedAndRecorded()
    {
        var (state, catalog, householdId) = Fixture(HistoricalTimelineRange.Start);
        var pipeline = RecordDivergenceCommands.BuildPipeline(catalog);

        var result = pipeline.Execute(
            state, Command(state, householdId, SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(state.DivergenceRecords.Count, Is.EqualTo(1));
            Assert.That(result.Events, Has.Count.EqualTo(1));
            Assert.That(result.Events[0], Is.InstanceOf<DivergenceRecordedEvent>());

            var recorded = state.DivergenceRecords.InAscendingOrder().First().Value;
            Assert.That(recorded.TriggeringHouseholdId, Is.EqualTo(householdId));
            Assert.That(recorded.AffectedTimelineEntryIds, Does.Contain(SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry));
            Assert.That(recorded.NewAlternateHistoryBranchActive, Is.True);
        });
    }
}
