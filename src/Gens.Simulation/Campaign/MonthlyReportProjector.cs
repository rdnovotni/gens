using Gens.Simulation.Commands;
using Gens.Simulation.Events;
using Gens.Simulation.Time;

namespace Gens.Simulation.Campaign;

/// <summary>How urgently a report entry needs the player's attention (Phase 4 item 5).</summary>
public enum ReportImportance
{
    Low,
    Medium,
    High,
}

/// <summary>Whether a report entry was resolved automatically or needs the player to look at it
/// (Phase 4 item 5, mirroring the three-tier Auto-Resolved/Flagged-for-Choice/Manual-Mode delivery
/// model in <c>gens-events-design.md</c> §7 — only the two tiers meaningful before any system can
/// actually offer the player a choice).</summary>
public enum ReportAcknowledgementState
{
    AutoResolved,
    Flagged,
}

/// <summary>One projected line in the monthly report: derived entirely from an <see
/// cref="IDomainEvent"/>'s own envelope fields (ADR 0007), never from a report-specific shadow data
/// model. <see cref="SourceInstanceId"/> is the Phase 9 item 4 drill-down link — non-null exactly when
/// the source event implements <see cref="IEventInstanceLinkedEvent"/> — a caller passes it straight
/// to <see cref="Gens.Simulation.Queries.EventInstanceDetailQuery"/> for the full picture (definition, every option
/// offered, which one resolved it).</summary>
public sealed record MonthlyReportEntry(
    string EventId,
    string EventType,
    string Group,
    ReportImportance Importance,
    ReportAcknowledgementState Acknowledgement,
    IReadOnlyList<string> SubjectIds,
    GameDate OccurredDate,
    string? SourceInstanceId = null);

/// <summary>One collapsed group of near-identical Auto-Resolved entries (Phase 9 item 4's "automation
/// summaries"): every entry sharing the same <see cref="Group"/> and <see cref="EventType"/>, once
/// there are enough of them in one month that itemizing each individually would just be noise. <see
/// cref="EventIds"/> preserves every collapsed entry's own ID, so a caller can still drill into any one
/// of them specifically rather than only the aggregate.</summary>
public sealed record MonthlyReportAutomationSummary(
    string Group,
    string EventType,
    int Count,
    IReadOnlyList<string> EventIds);

/// <summary>An immutable, presentation-ready monthly report (Phase 4 item 5; extended Phase 9 item
/// 4): every entry that occurred up to and including <see cref="Date"/>, in the order the events were
/// emitted — already deterministic (the same seed and command log reproduces the same event order),
/// so this class never re-sorts. <see cref="Entries"/> excludes whatever <see
/// cref="AutomationSummaries"/> already collapsed — a caller renders both lists, not <see
/// cref="Entries"/> alone, to see the complete month.</summary>
public sealed record MonthlyReportProjection(
    GameDate Date,
    IReadOnlyList<MonthlyReportEntry> Entries,
    IReadOnlyList<MonthlyReportAutomationSummary> AutomationSummaries);

/// <summary>
/// Projects a list of <see cref="IDomainEvent"/>s into a <see cref="MonthlyReportProjection"/> —
/// entirely from domain events and read models (Phase 9 item 4), never from a report-specific shadow
/// data model. Grouping reads the real §2 scope taxonomy off any event implementing <see
/// cref="IScopedEvent"/> (Phase 9 item 3's own Events-namespace types all do), falling back to the
/// namespace-prefix heuristic Phase 4's first pass used for every event type that predates the scope
/// taxonomy (<see cref="GenericCommandExecutedEvent"/>, <see cref="CampaignBootstrappedEvent"/>,
/// <see cref="Policies.RitesBudgetChangedEvent"/>, and so on) — both rules coexist rather than one
/// replacing the other, since non-Events event types have no scope to read.
/// </summary>
public static class MonthlyReportProjector
{
    /// <summary>The minimum same-(group, type) Auto-Resolved entry count in one month before <see
    /// cref="Project"/> collapses them into one <see cref="MonthlyReportAutomationSummary"/> instead of
    /// itemizing each — an invented, untuned threshold (this codebase's own "§10 untuned numbers"
    /// convention), not a value read from content.</summary>
    public const int AutomationSummaryThreshold = 3;

    public static MonthlyReportProjection Project(GameDate date, IReadOnlyList<IDomainEvent> events)
    {
        if (events is null)
            throw new ArgumentNullException(nameof(events));

        var rawEntries = events
            .Select(evt => new MonthlyReportEntry(
                evt.EventId.ToTaggedString(),
                evt.Type,
                GroupOf(evt),
                ImportanceOf(evt),
                AcknowledgementOf(evt),
                evt.SubjectIds,
                evt.OccurredDate,
                (evt as IEventInstanceLinkedEvent)?.InstanceId.ToTaggedString()))
            .ToArray();

        var collapsedKeys = new HashSet<(string Group, string EventType)>();
        var summaries = new List<MonthlyReportAutomationSummary>();
        foreach (var group in rawEntries
                     .Where(entry => entry.Acknowledgement == ReportAcknowledgementState.AutoResolved)
                     .GroupBy(entry => (entry.Group, entry.EventType)))
        {
            var members = group.ToArray();
            if (members.Length < AutomationSummaryThreshold)
                continue;

            collapsedKeys.Add(group.Key);
            summaries.Add(new MonthlyReportAutomationSummary(
                group.Key.Group, group.Key.EventType, members.Length, members.Select(m => m.EventId).ToArray()));
        }

        var entries = rawEntries.Where(entry => !collapsedKeys.Contains((entry.Group, entry.EventType))).ToArray();

        return new MonthlyReportProjection(date, entries, summaries);
    }

    /// <summary>Reads the real §2 scope off <see cref="IScopedEvent"/> when the event implements it;
    /// otherwise falls back to the event type's namespace segment (e.g. <c>"campaign.bootstrapped"</c>
    /// → <c>"campaign"</c>), matching this projector's first pass.</summary>
    private static string GroupOf(IDomainEvent evt)
    {
        if (evt is IScopedEvent scoped)
            return scoped.Scope switch
            {
                EventScope.Personal => "personal",
                EventScope.Household => "household",
                EventScope.Settlement => "settlement",
                EventScope.Imperial => "imperial",
                _ => scoped.Scope.ToString(),
            };

        var separatorIndex = evt.Type.IndexOf('.');
        return separatorIndex < 0 ? evt.Type : evt.Type[..separatorIndex];
    }

    /// <summary>A freshly-fired Household or Imperial-scope event is the report's own "something the
    /// player should notice" moment (§5's Prominence-driven weighting is out of this pass's scope, so
    /// every Household/Imperial firing reads as equally High for now); every other Events-namespace
    /// type is informational follow-up to something already reported. <see
    /// cref="CampaignBootstrappedEvent"/> keeps its first pass's fixed High baseline.</summary>
    private static ReportImportance ImportanceOf(IDomainEvent evt) => evt switch
    {
        CampaignBootstrappedEvent => ReportImportance.High,
        EventFiredEvent { Scope: EventScope.Household or EventScope.Imperial } => ReportImportance.High,
        EventFiredEvent => ReportImportance.Medium,
        EventExpiredEvent or EventStageAdvancedEvent => ReportImportance.Medium,
        EventOptionResolvedEvent or SampleEventOptionOutcomeEvent => ReportImportance.Low,

        // Phase 14 item 5: a hazard/epidemic/wanderer event's own severity, not just its namespace,
        // decides how loudly the report treats it — matching this switch's own existing "a fired
        // Household/Imperial event is High, its own later follow-up reads as ordinary" shape.
        Hazards.DisasterEventOccurredEvent { Severity: Hazards.DisasterSeverity.Severe or Hazards.DisasterSeverity.Catastrophic } =>
            ReportImportance.High,
        Hazards.DisasterEventOccurredEvent => ReportImportance.Medium,
        Hazards.HazardElevatedExposureWarningEvent => ReportImportance.Medium,
        Health.EpidemicOutbreakIgnitedEvent => ReportImportance.High,
        Health.AntoninePlagueOnsetEvent or Health.AntoninePlagueWaningEvent => ReportImportance.High,
        Wanderers.WandererRecruitedEvent => ReportImportance.High,

        _ => ReportImportance.Medium,
    };

    /// <summary>Flags a newly-fired Household-scope event for player choice, matching
    /// <c>gens-events-design.md</c> §7's "Household-scope events...Flagged for Choice" rule — only the
    /// firing itself (still awaiting an option) is flagged; that same instance's own later resolution/
    /// expiry/stage-advance announcements are already resolved and read as ordinary Auto-Resolved
    /// follow-up. The pre-Events <c>"household."</c>-prefixed heuristic is kept as a fallback for any
    /// future non-Events household-scope event type.</summary>
    private static ReportAcknowledgementState AcknowledgementOf(IDomainEvent evt)
    {
        if (evt is EventFiredEvent { Scope: EventScope.Household })
            return ReportAcknowledgementState.Flagged;

        return evt.Type.StartsWith("household.", StringComparison.Ordinal)
            ? ReportAcknowledgementState.Flagged
            : ReportAcknowledgementState.AutoResolved;
    }
}
