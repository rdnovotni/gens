using Gens.Simulation.Events;
using Gens.Simulation.Identity;

namespace Gens.Simulation.History;

/// <summary>
/// The in-memory lookup over every registered <see cref="HistoricalTimelineEntryDefinition"/> —
/// mirrors <see cref="EventCatalog"/>'s identical shape, plus the date-aware, cross-referencing content
/// validation this item's own name calls for: every <see
/// cref="HistoricalTimelineEntryDefinition.InvolvedFigureIds"/> entry must resolve against <paramref
/// name="figures"/>, and — when an <see cref="EventCatalog"/> is supplied — every non-null <see
/// cref="HistoricalTimelineEntryDefinition.LinkedEventDefinitionRef"/> must resolve against it,
/// matching <see cref="Gens.Simulation.Regions.RegionProfileCatalog"/>'s own Home Anchor/capital-
/// uniqueness cross-reference validation convention. <paramref name="eventCatalog"/> is optional
/// because most callers (a fresh campaign that hasn't yet loaded a full Events catalog, or a test
/// exercising this catalog in isolation) have no reason to validate that cross-reference at all.
/// </summary>
public sealed class HistoricalTimelineCatalog
{
    private readonly Dictionary<DefinitionId<HistoricalTimelineEntryDefinition>, HistoricalTimelineEntryDefinition> _byId;

    public HistoricalTimelineCatalog(
        IEnumerable<HistoricalTimelineEntryDefinition> entries,
        NamedHistoricalFigureCatalog figures,
        EventCatalog? eventCatalog = null)
    {
        if (entries is null)
            throw new ArgumentNullException(nameof(entries));
        if (figures is null)
            throw new ArgumentNullException(nameof(figures));

        var byId = new Dictionary<DefinitionId<HistoricalTimelineEntryDefinition>, HistoricalTimelineEntryDefinition>();
        foreach (var entry in entries)
        {
            if (!byId.TryAdd(entry.Id, entry))
                throw new ArgumentException($"Duplicate historical timeline entry ID '{entry.Id}' in catalog.", nameof(entries));
        }

        foreach (var entry in byId.Values)
        {
            foreach (var figureId in entry.InvolvedFigureIds)
            {
                if (!figures.TryGet(figureId, out _))
                {
                    throw new ArgumentException(
                        $"Historical timeline entry '{entry.Id}' names unknown figure '{figureId}'.",
                        nameof(entries));
                }
            }

            if (eventCatalog is not null && entry.LinkedEventDefinitionRef is { } linkedId && !eventCatalog.TryGet(linkedId, out _))
            {
                throw new ArgumentException(
                    $"Historical timeline entry '{entry.Id}' links unknown event definition '{linkedId}'.",
                    nameof(entries));
            }
        }

        _byId = byId;
    }

    public bool TryGet(DefinitionId<HistoricalTimelineEntryDefinition> id, out HistoricalTimelineEntryDefinition definition) =>
        _byId.TryGetValue(id, out definition!);

    public HistoricalTimelineEntryDefinition Get(DefinitionId<HistoricalTimelineEntryDefinition> id) =>
        TryGet(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"No historical timeline entry '{id}' is registered in this catalog.");

    public IEnumerable<HistoricalTimelineEntryDefinition> All() => _byId.Values;

    /// <summary>Every registered entry in ascending real-date order, then by ID as a stable tiebreak —
    /// <see cref="All"/> alone doesn't guarantee this, since content authors won't necessarily list
    /// entries pre-sorted.</summary>
    public IReadOnlyList<HistoricalTimelineEntryDefinition> Chronological() =>
        _byId.Values
            .OrderBy(entry => entry.Date.TotalMonths)
            .ThenBy(entry => entry.Id.Value, StringComparer.Ordinal)
            .ToArray();
}
