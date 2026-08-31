using Gens.Simulation.State;

namespace Gens.Simulation.History;

/// <summary>The counterfactual-flag query for <see cref="NamedHistoricalFigureDefinition"/>, mirroring
/// <see cref="HistoricalTimelineQueries"/>'s identical "derive from the one real <see
/// cref="DivergenceRecord"/> list, never store" shape.</summary>
public static class NamedHistoricalFigureQueries
{
    public static HistoricalFigureStatus CurrentStatusOf(
        WorldState state, HistoricalTimelineCatalog catalog, NamedHistoricalFigureDefinition figure)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (catalog is null)
            throw new ArgumentNullException(nameof(catalog));
        if (figure is null)
            throw new ArgumentNullException(nameof(figure));

        if (figure.RealDeathOrEndYear is not { } deathDate || state.Date.TotalMonths < deathDate.TotalMonths)
            return HistoricalFigureStatus.AliveOnTrack;

        // A figure "survives past their real date" only when a recorded Divergence actually affected a
        // Timeline entry naming them — otherwise reaching or passing their real death/end year with no
        // Divergence means they died exactly on schedule.
        var survivedViaDivergence = state.DivergenceRecords.InAscendingOrder()
            .Any(record => record.Value.AffectedTimelineEntryIds.Any(entryId =>
                catalog.TryGet(entryId, out var entry) && entry.InvolvedFigureIds.Contains(figure.Id)));

        return survivedViaDivergence ? HistoricalFigureStatus.SurvivedPastRealDate : HistoricalFigureStatus.DeceasedOnSchedule;
    }
}
