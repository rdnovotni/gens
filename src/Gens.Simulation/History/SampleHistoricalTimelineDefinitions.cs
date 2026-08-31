using Gens.Simulation.Events;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.History;

/// <summary>
/// A small, self-contained worked example exercising every field of both content schemas end-to-end
/// (Phase 13 item 5), mirroring <see cref="SampleEventDefinitions"/>/<see
/// cref="Gens.Simulation.Regions.SampleRegionProfileDefinitions"/>'s identical "vertical slice, not the
/// final content ceiling" framing and <c>sample-*</c> ID precedent. This is fixture content, not
/// authored history — <see cref="KnownWorldHistoricalTimeline"/>/<see
/// cref="KnownWorldHistoricalFigures"/> carry the real, authored 133 BC – AD 235 catalog.
/// </summary>
public static class SampleHistoricalTimelineDefinitions
{
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> SampleFigureOne = new("sample-historical-figure-one");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> SampleFigureTwo = new("sample-historical-figure-two");

    /// <summary>Sits exactly at <see cref="HistoricalTimelineRange.Start"/> — the earliest legal date.</summary>
    public static readonly DefinitionId<HistoricalTimelineEntryDefinition> SampleRangeOpeningEntry = new("sample-range-opening-entry");

    /// <summary>Sits in the last legal month before <see cref="HistoricalTimelineRange.End"/>.</summary>
    public static readonly DefinitionId<HistoricalTimelineEntryDefinition> SampleRangeClosingEntry = new("sample-range-closing-entry");

    /// <summary>Divergence-eligible, linked to a real sample <see cref="EventDefinition"/> (<see
    /// cref="SampleEventDefinitions.DomesticMurmur"/>), and names both sample figures — the one entry
    /// exercising <see cref="HistoricalTimelineEntryDefinition.DivergenceEligible"/>, <see
    /// cref="HistoricalTimelineEntryDefinition.LinkedEventDefinitionRef"/>, and a multi-figure <see
    /// cref="HistoricalTimelineEntryDefinition.InvolvedFigureIds"/> list all at once.</summary>
    public static readonly DefinitionId<HistoricalTimelineEntryDefinition> SampleDivergenceEligibleEntry = new("sample-divergence-eligible-entry");

    /// <summary>Not Divergence-eligible (a <see cref="HistoricalEventType.NaturalDisaster"/>) — the
    /// contrasting case.</summary>
    public static readonly DefinitionId<HistoricalTimelineEntryDefinition> SampleIneligibleEntry = new("sample-ineligible-entry");

    public static NamedHistoricalFigureCatalog BuildFigureCatalog() => new(new[]
    {
        new NamedHistoricalFigureDefinition(
            SampleFigureOne, "Sample Figure the First", HistoricalFigureRole.HeadOfState,
            realAccessionOrStartYear: HistoricalYear.ToGameDate(80, isBce: true),
            realDeathOrEndYear: HistoricalYear.ToGameDate(50, isBce: true)),
        new NamedHistoricalFigureDefinition(
            SampleFigureTwo, "Sample Figure the Second", HistoricalFigureRole.General,
            realAccessionOrStartYear: null,
            realDeathOrEndYear: HistoricalYear.ToGameDate(40, isBce: true)),
    });

    public static HistoricalTimelineCatalog BuildCatalog(EventCatalog? eventCatalog = null) => new(
        new[]
        {
            new HistoricalTimelineEntryDefinition(
                SampleRangeOpeningEntry, HistoricalTimelineRange.Start, HistoricalEventType.Other,
                "Sample Range-Opening Entry", new[] { "Roman" }, Array.Empty<DefinitionId<NamedHistoricalFigureDefinition>>(),
                linkedEventDefinitionRef: null, divergenceEligible: false),
            new HistoricalTimelineEntryDefinition(
                SampleRangeClosingEntry, new GameDate(HistoricalTimelineRange.End.TotalMonths - 1), HistoricalEventType.Other,
                "Sample Range-Closing Entry", new[] { "Roman" }, Array.Empty<DefinitionId<NamedHistoricalFigureDefinition>>(),
                linkedEventDefinitionRef: null, divergenceEligible: false),
            new HistoricalTimelineEntryDefinition(
                SampleDivergenceEligibleEntry, HistoricalYear.ToGameDate(60, isBce: true), HistoricalEventType.WarOrRevolt,
                "Sample Divergence-Eligible War", new[] { "Roman", "Gallic" }, new[] { SampleFigureOne, SampleFigureTwo },
                linkedEventDefinitionRef: SampleEventDefinitions.DomesticMurmur, divergenceEligible: true),
            new HistoricalTimelineEntryDefinition(
                SampleIneligibleEntry, HistoricalYear.ToGameDate(45, isBce: true), HistoricalEventType.NaturalDisaster,
                "Sample Non-Divergence-Eligible Disaster", new[] { "Empire-wide" }, Array.Empty<DefinitionId<NamedHistoricalFigureDefinition>>(),
                linkedEventDefinitionRef: null, divergenceEligible: false),
        },
        BuildFigureCatalog(),
        eventCatalog);
}
