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
    public void SurvivedPastRealDateWhenADivergenceAffectedAnEntryNamingTheFigure()
    {
        var catalog = SampleHistoricalTimelineDefinitions.BuildCatalog();
        var entry = catalog.Get(SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry);
        var figureId = entry.InvolvedFigureIds[0];
        var figureDefinition = SampleHistoricalTimelineDefinitions.BuildFigureCatalog().Get(figureId);
        var state = new WorldState(figureDefinition.RealDeathOrEndYear!.Value);
        var householdId = state.HouseholdIds.Issue();
        var divergenceId = state.DivergenceRecordIds.Issue();
        state.DivergenceRecords.Add(divergenceId, new DivergenceRecord(
            divergenceId, state.Date, householdId, "Test trigger",
            new[] { entry.Id }, NewAlternateHistoryBranchActive: true));

        Assert.That(NamedHistoricalFigureQueries.CurrentStatusOf(state, catalog, figureDefinition), Is.EqualTo(HistoricalFigureStatus.SurvivedPastRealDate));
    }
}
