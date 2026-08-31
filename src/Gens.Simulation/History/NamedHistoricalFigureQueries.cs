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

        // A figure "survives past their real date" only when a recorded Divergence affected the specific
        // Timeline entry dated at their own real death/end year and naming them — not just any entry
        // that happens to name them at some earlier point in their life. Diverging an unrelated earlier
        // entry (e.g. the AD 101 Dacian Wars naming Trajan) must never itself imply Trajan survived past
        // his real AD 117 death; only diverging the entry actually scheduled on that death date can.
        var survivedViaDivergence = state.DivergenceRecords.InAscendingOrder()
            .Any(record => record.Value.AffectedTimelineEntryIds.Any(entryId =>
                catalog.TryGet(entryId, out var entry) &&
                entry.Date.TotalMonths == deathDate.TotalMonths &&
                entry.InvolvedFigureIds.Contains(figure.Id)));

        return survivedViaDivergence ? HistoricalFigureStatus.SurvivedPastRealDate : HistoricalFigureStatus.DeceasedOnSchedule;
    }
}
