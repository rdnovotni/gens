namespace Gens.Simulation.History;

/// <summary>A <see cref="HistoricalTimelineEntryDefinition"/>'s real-world category (§10's own
/// <c>eventType</c> enum, <c>gens-events-design.md</c>).</summary>
public enum HistoricalEventType
{
    ImperialSuccession,
    WarOrRevolt,
    NaturalDisaster,
    ReligiousObservance,
    PoliticalTrial,
    Other,
}

/// <summary>A <see cref="NamedHistoricalFigureDefinition"/>'s real historical office/function (§10's
/// own <c>role</c> enum).</summary>
public enum HistoricalFigureRole
{
    HeadOfState,
    General,
    Senator,
    Governor,
    RebelLeader,
    Orator,
    WriterOrHistorian,
    PhilosopherOrScholar,
    ReligiousFigure,
    PhysicianOrNaturalist,
    Jurist,
    ExplorerOrWanderer,
    ArchitectOrEngineer,
    Patron,
    Other,
}

/// <summary>The four-value counterfactual read on one <see cref="HistoricalTimelineEntryDefinition"/>
/// for a given campaign (§10's own <c>divergenceState</c> enum), derived by <see
/// cref="HistoricalTimelineQueries.DivergenceStateOf"/> — never stored, matching this item's own
/// "counterfactual flags are queries against the one real <see cref="DivergenceRecord"/> list"
/// design.</summary>
public enum HistoricalDivergenceState
{
    /// <summary>Reached in-campaign (its real date has arrived or passed) and unaffected by any
    /// recorded <see cref="DivergenceRecord"/> — history proceeded as documented.</summary>
    OnTrack,

    /// <summary>Named in at least one recorded <see cref="DivergenceRecord.AffectedTimelineEntryIds"/>
    /// — this thread of history no longer draws on the real historical roster (§6.7).</summary>
    Diverged,

    /// <summary>Its real date is still in the campaign's future.</summary>
    NotYetReached,

    /// <summary>Its real date falls before the campaign's own Starting Year — already history by the
    /// time play began, and it never fires (§6.4).</summary>
    PredatesStart,
}

/// <summary>A <see cref="NamedHistoricalFigureDefinition"/>'s counterfactual read for a given campaign
/// (§10's own <c>currentStatus</c> enum), derived by <see
/// cref="NamedHistoricalFigureQueries.CurrentStatusOf"/> — never stored, matching <see
/// cref="HistoricalDivergenceState"/>'s identical "derived, not stored" convention.</summary>
public enum HistoricalFigureStatus
{
    /// <summary>Still alive per the real historical record as of the campaign's current date (or has
    /// no recorded death/end year at all).</summary>
    AliveOnTrack,

    /// <summary>Reached or passed its real death/end year with no Divergence affecting it — died
    /// exactly on the schedule the real historical record sets.</summary>
    DeceasedOnSchedule,

    /// <summary>Only reachable through Divergence (§6.7): a recorded <see cref="DivergenceRecord"/>
    /// affected a <see cref="HistoricalTimelineEntryDefinition"/> naming this figure, and the
    /// campaign's current date is at or past what would have been their real death/end year.</summary>
    SurvivedPastRealDate,
}
