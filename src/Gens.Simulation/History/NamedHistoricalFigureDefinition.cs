using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.History;

/// <summary>
/// One real historical figure who drives Historical Timeline content by name but is never instantiated
/// as an interactive <see cref="Gens.Simulation.Characters.Character"/> (Phase 13 item 5; §6.5, §10's
/// own <c>NamedHistoricalFigure{}</c> sketch). Tracks only real, documented biographical facts, used
/// purely to flavor and gate which <see cref="HistoricalTimelineEntryDefinition"/>s fire.
/// </summary>
public sealed record NamedHistoricalFigureDefinition
{
    public NamedHistoricalFigureDefinition(
        DefinitionId<NamedHistoricalFigureDefinition> id,
        string realName,
        HistoricalFigureRole role,
        GameDate? realAccessionOrStartYear,
        GameDate? realDeathOrEndYear)
    {
        if (string.IsNullOrWhiteSpace(realName))
            throw new ArgumentException("A named historical figure requires a non-empty real name.", nameof(realName));
        if (realAccessionOrStartYear is { } start && realDeathOrEndYear is { } end && start.TotalMonths > end.TotalMonths)
        {
            throw new ArgumentException(
                "A figure's real accession/start year cannot fall after their real death/end year.",
                nameof(realAccessionOrStartYear));
        }

        Id = id;
        RealName = realName;
        Role = role;
        RealAccessionOrStartYear = realAccessionOrStartYear;
        RealDeathOrEndYear = realDeathOrEndYear;
    }

    public DefinitionId<NamedHistoricalFigureDefinition> Id { get; }
    public string RealName { get; }
    public HistoricalFigureRole Role { get; }

    /// <summary>Null when this figure's real tenure has no single clean starting year worth recording
    /// (many of §6's non-Emperor figures only have one real, dated moment at all), or — for a figure
    /// entirely outside political office, such as Jesus of Nazareth (§6's own careful-treatment note) —
    /// when neither this field nor <see cref="RealDeathOrEndYear"/> fits at all.</summary>
    public GameDate? RealAccessionOrStartYear { get; }

    public GameDate? RealDeathOrEndYear { get; }
}
