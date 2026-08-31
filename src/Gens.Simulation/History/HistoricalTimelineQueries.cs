using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.History;

/// <summary>The counterfactual-flag query this item's own name calls for: <see
/// cref="HistoricalDivergenceState"/> is always derived against the one real <see
/// cref="DivergenceRecord"/> list, never stored — mirrors how <see
/// cref="Gens.Simulation.Travel.TravelLocation"/>/<see
/// cref="Gens.Simulation.Characters.Character.CurrentTravelLocation"/> favor deriving over storing
/// where possible.</summary>
public static class HistoricalTimelineQueries
{
    /// <param name="catalog">Unused by this query's own logic today — kept in the signature per this
    /// item's own architecture so a future refinement (e.g. validating <paramref name="entry"/> is
    /// actually registered) has somewhere to read from without an API break.</param>
    /// <param name="campaignStartingDate">The campaign's own Starting Year (§6.2) — not a per-household
    /// <c>GameCalendar</c> concept (explicitly out of this item's scope; see <see
    /// cref="HistoricalTimelineScheduler"/>'s own doc comment), just the one date every entry's
    /// <see cref="HistoricalDivergenceState.PredatesStart"/> check resolves against.</param>
    public static HistoricalDivergenceState DivergenceStateOf(
        WorldState state, HistoricalTimelineCatalog catalog, HistoricalTimelineEntryDefinition entry, GameDate campaignStartingDate)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (catalog is null)
            throw new ArgumentNullException(nameof(catalog));
        if (entry is null)
            throw new ArgumentNullException(nameof(entry));

        if (entry.Date.TotalMonths < campaignStartingDate.TotalMonths)
            return HistoricalDivergenceState.PredatesStart;

        var diverged = state.DivergenceRecords.InAscendingOrder()
            .Any(record => record.Value.AffectedTimelineEntryIds.Contains(entry.Id));
        if (diverged)
            return HistoricalDivergenceState.Diverged;

        if (entry.Date.TotalMonths > state.Date.TotalMonths)
            return HistoricalDivergenceState.NotYetReached;

        return HistoricalDivergenceState.OnTrack;
    }
}
