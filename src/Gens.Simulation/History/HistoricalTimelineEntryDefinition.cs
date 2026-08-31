using Gens.Simulation.Events;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.History;

/// <summary>
/// One curated, chronologically real, date-locked entry on the Historical Timeline (Phase 13 item 5;
/// <c>gens-events-design.md</c> §6.4, §10's own <c>HistoricalTimelineEntry{}</c> sketch), mirroring
/// <see cref="Gens.Simulation.Regions.RegionProfileDefinition"/>/<see
/// cref="Gens.Simulation.Events.EventDefinition"/>'s identical "sealed record, constructor validates,
/// content is data" shape. <see cref="Date"/> replaces §10's separate <c>realYear</c>/<c>realMonth</c>
/// pair with this codebase's one canonical <see cref="GameDate"/> (see <see cref="HistoricalYear"/> for
/// the BCE/CE-to-<see cref="GameDate"/> conversion every authored entry goes through).
/// </summary>
public sealed record HistoricalTimelineEntryDefinition
{
    public HistoricalTimelineEntryDefinition(
        DefinitionId<HistoricalTimelineEntryDefinition> id,
        GameDate date,
        HistoricalEventType eventType,
        string realWorldName,
        IReadOnlyList<string> regionRelevance,
        IReadOnlyList<DefinitionId<NamedHistoricalFigureDefinition>> involvedFigureIds,
        DefinitionId<EventDefinition>? linkedEventDefinitionRef,
        bool divergenceEligible)
    {
        if (string.IsNullOrWhiteSpace(realWorldName))
            throw new ArgumentException("A historical timeline entry requires a non-empty real-world name.", nameof(realWorldName));
        if (!HistoricalTimelineRange.Contains(date))
        {
            throw new ArgumentException(
                $"'{realWorldName}' at {date.ToDisplayYearLabel()} falls outside the supported " +
                $"{HistoricalTimelineRange.Start.ToDisplayYearLabel()} – {HistoricalTimelineRange.End.ToDisplayYearLabel()} range.",
                nameof(date));
        }
        if (regionRelevance is null || regionRelevance.Count == 0)
            throw new ArgumentException("A historical timeline entry requires at least one region/culture relevance tag.", nameof(regionRelevance));
        if (regionRelevance.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("A region relevance tag cannot be empty.", nameof(regionRelevance));
        if (involvedFigureIds is null)
            throw new ArgumentNullException(nameof(involvedFigureIds));

        Id = id;
        Date = date;
        EventType = eventType;
        RealWorldName = realWorldName;
        RegionRelevance = regionRelevance;
        InvolvedFigureIds = involvedFigureIds;
        LinkedEventDefinitionRef = linkedEventDefinitionRef;
        DivergenceEligible = divergenceEligible;
    }

    public DefinitionId<HistoricalTimelineEntryDefinition> Id { get; }
    public GameDate Date { get; }
    public HistoricalEventType EventType { get; }
    public string RealWorldName { get; }

    /// <summary>Qualitative region/culture tags (e.g. <c>"Iberian"</c>, <c>"Empire-wide"</c>), the same
    /// loose-string convention <see cref="Gens.Simulation.Regions.RegionProfileDefinition"/>'s §4.1-§4.6
    /// fields use — always at least one entry (validated above).</summary>
    public IReadOnlyList<string> RegionRelevance { get; }

    public IReadOnlyList<DefinitionId<NamedHistoricalFigureDefinition>> InvolvedFigureIds { get; }

    /// <summary>Null for most entries: authoring a real, full multi-stage <see cref="EventDefinition"/>
    /// for each of the roughly ninety authored entries is out of this item's own scope — an unlinked
    /// entry still fires as a lightweight digest event (<see
    /// cref="HistoricalTimelineEntryOccurredEvent"/>) via <see cref="HistoricalTimelineScheduler"/>.</summary>
    public DefinitionId<EventDefinition>? LinkedEventDefinitionRef { get; }

    /// <summary>Whether Divergence (§6.7) could plausibly branch this entry: content-authored, generally
    /// <c>true</c> for <see cref="HistoricalEventType.ImperialSuccession"/>, <see
    /// cref="HistoricalEventType.WarOrRevolt"/>, and <see cref="HistoricalEventType.PoliticalTrial"/>
    /// (§6.7's own examples — a claimant's win, a war's resolution, a figure's scheduled death — are all
    /// consequential political/succession/war moments), generally <c>false</c> for <see
    /// cref="HistoricalEventType.NaturalDisaster"/>/<see cref="HistoricalEventType.ReligiousObservance"/>
    /// (a real eruption or festival date isn't the kind of thing a household's political action
    /// branches).</summary>
    public bool DivergenceEligible { get; }
}
