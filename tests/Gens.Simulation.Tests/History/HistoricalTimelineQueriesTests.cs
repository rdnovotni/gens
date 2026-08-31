using Gens.Simulation.History;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.History;

public sealed class HistoricalTimelineQueriesTests
{
    private static HistoricalTimelineCatalog Catalog => SampleHistoricalTimelineDefinitions.BuildCatalog();

    [Test]
    public void PredatesStartWhenEntryDateIsBeforeTheCampaignStartingDate()
    {
        var catalog = Catalog;
        var entry = catalog.Get(SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry);
        var state = new WorldState(entry.Date);
        var startingDateAfterEntry = new GameDate(entry.Date.TotalMonths + 12);

        var result = HistoricalTimelineQueries.DivergenceStateOf(state, catalog, entry, startingDateAfterEntry);

        Assert.That(result, Is.EqualTo(HistoricalDivergenceState.PredatesStart));
    }

    [Test]
    public void NotYetReachedWhenEntryDateIsAfterTheCampaignClock()
    {
        var catalog = Catalog;
        var entry = catalog.Get(SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry);
        var beforeEntry = new GameDate(entry.Date.TotalMonths - 6);
        var state = new WorldState(beforeEntry);

        var result = HistoricalTimelineQueries.DivergenceStateOf(state, catalog, entry, HistoricalTimelineRange.Start);

        Assert.That(result, Is.EqualTo(HistoricalDivergenceState.NotYetReached));
    }

    [Test]
    public void OnTrackWhenTheCampaignClockHasReachedTheEntryAndNoDivergenceAffectsIt()
    {
        var catalog = Catalog;
        var entry = catalog.Get(SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry);
        var state = new WorldState(entry.Date);

        var result = HistoricalTimelineQueries.DivergenceStateOf(state, catalog, entry, HistoricalTimelineRange.Start);

        Assert.That(result, Is.EqualTo(HistoricalDivergenceState.OnTrack));
    }

    [Test]
    public void DivergedWhenARecordedDivergenceAffectsTheEntry()
    {
        var catalog = Catalog;
        var entry = catalog.Get(SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry);
        var state = new WorldState(entry.Date);
        var householdId = state.HouseholdIds.Issue();
        var divergenceId = state.DivergenceRecordIds.Issue();
        state.DivergenceRecords.Add(divergenceId, new DivergenceRecord(
            divergenceId, entry.Date, householdId, "Test trigger",
            new[] { entry.Id }, NewAlternateHistoryBranchActive: true));

        var result = HistoricalTimelineQueries.DivergenceStateOf(state, catalog, entry, HistoricalTimelineRange.Start);

        Assert.That(result, Is.EqualTo(HistoricalDivergenceState.Diverged));
    }
}

public sealed class NamedHistoricalFigureQueriesTests
{
    [Test]
    public void AliveOnTrackWhenTheFigureHasNoRecordedDeathYear()
    {
        var catalog = SampleHistoricalTimelineDefinitions.BuildCatalog();
        var figure = new NamedHistoricalFigureDefinition(
            new DefinitionId<NamedHistoricalFigureDefinition>("no-death-year"), "Test", HistoricalFigureRole.Other, null, null);
        var state = new WorldState(HistoricalTimelineRange.Start);

        Assert.That(NamedHistoricalFigureQueries.CurrentStatusOf(state, catalog, figure), Is.EqualTo(HistoricalFigureStatus.AliveOnTrack));
    }

    [Test]
    public void AliveOnTrackWhenTheCampaignClockHasNotReachedTheDeathYear()
    {
        var catalog = SampleHistoricalTimelineDefinitions.BuildCatalog();
        var figure = catalog.Get(SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry).InvolvedFigureIds[0];
        var figureDefinition = SampleHistoricalTimelineDefinitions.BuildFigureCatalog().Get(figure);
        var beforeDeath = new GameDate(figureDefinition.RealDeathOrEndYear!.Value.TotalMonths - 12);
        var state = new WorldState(beforeDeath);

        Assert.That(NamedHistoricalFigureQueries.CurrentStatusOf(state, catalog, figureDefinition), Is.EqualTo(HistoricalFigureStatus.AliveOnTrack));
    }

    [Test]
    public void DeceasedOnScheduleWhenTheClockHasReachedTheDeathYearWithNoDivergence()
    {
        var catalog = SampleHistoricalTimelineDefinitions.BuildCatalog();
        var figureId = SampleHistoricalTimelineDefinitions.SampleFigureOne;
        var figureDefinition = SampleHistoricalTimelineDefinitions.BuildFigureCatalog().Get(figureId);
        var state = new WorldState(figureDefinition.RealDeathOrEndYear!.Value);

        Assert.That(NamedHistoricalFigureQueries.CurrentStatusOf(state, catalog, figureDefinition), Is.EqualTo(HistoricalFigureStatus.DeceasedOnSchedule));
    }

    [Test]
    public void SurvivedPastRealDateWhenTheDivergedEntryIsDatedAtTheFiguresOwnRealDeathYear()
    {
        var catalog = SampleHistoricalTimelineDefinitions.BuildCatalog();
        var entry = catalog.Get(SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry);
        // A standalone figure whose own real death year is dated exactly at this entry — survival can
        // only ever be inferred from diverging the entry actually scheduled on a figure's own death
        // date, never merely an entry that happens to name them at some other point in their life.
        var figureDefinition = new NamedHistoricalFigureDefinition(
            new DefinitionId<NamedHistoricalFigureDefinition>("test-figure-dying-at-entry-date"),
            "Test Figure", HistoricalFigureRole.Other, realAccessionOrStartYear: null, realDeathOrEndYear: entry.Date);
        var state = new WorldState(entry.Date);
        var householdId = state.HouseholdIds.Issue();
        var divergenceId = state.DivergenceRecordIds.Issue();
        state.DivergenceRecords.Add(divergenceId, new DivergenceRecord(
            divergenceId, state.Date, householdId, "Test trigger",
            new[] { entry.Id }, NewAlternateHistoryBranchActive: true));

        Assert.That(NamedHistoricalFigureQueries.CurrentStatusOf(state, catalog, figureDefinition), Is.EqualTo(HistoricalFigureStatus.SurvivedPastRealDate));
    }

    [Test]
    public void DeceasedOnScheduleWhenTheDivergedEntryOnlyNamesTheFigureBeforeTheirOwnLaterRealDeathYear()
    {
        var catalog = SampleHistoricalTimelineDefinitions.BuildCatalog();
        var entry = catalog.Get(SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry);
        var figureId = entry.InvolvedFigureIds[0];
        var figureDefinition = SampleHistoricalTimelineDefinitions.BuildFigureCatalog().Get(figureId);
        // SampleFigureOne's own real death year (50 BC) is later than the entry it's named on (60 BC) —
        // diverging that earlier, unrelated entry must never itself imply the figure survived past its
        // own, separate, later real death date (the bug Codex flagged: e.g. diverging the AD 101 Dacian
        // Wars entry naming Trajan must not mark him as surviving past his real AD 117 death).
        var state = new WorldState(figureDefinition.RealDeathOrEndYear!.Value);
        var householdId = state.HouseholdIds.Issue();
        var divergenceId = state.DivergenceRecordIds.Issue();
        state.DivergenceRecords.Add(divergenceId, new DivergenceRecord(
            divergenceId, entry.Date, householdId, "Test trigger",
            new[] { entry.Id }, NewAlternateHistoryBranchActive: true));

        Assert.That(NamedHistoricalFigureQueries.CurrentStatusOf(state, catalog, figureDefinition), Is.EqualTo(HistoricalFigureStatus.DeceasedOnSchedule));
    }
}
