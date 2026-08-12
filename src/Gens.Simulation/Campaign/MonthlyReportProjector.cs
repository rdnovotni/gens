using Gens.Simulation.Commands;
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
/// model.</summary>
public sealed record MonthlyReportEntry(
    string EventId,
    string EventType,
    string Group,
    ReportImportance Importance,
    ReportAcknowledgementState Acknowledgement,
    IReadOnlyList<string> SubjectIds,
    GameDate OccurredDate);

/// <summary>An immutable, presentation-ready monthly report (Phase 4 item 5): every entry that
/// occurred up to and including <see cref="Date"/>, in the order the events were emitted — already
/// deterministic (the same seed and command log reproduces the same event order), so this class
/// never re-sorts.</summary>
public sealed record MonthlyReportProjection(GameDate Date, IReadOnlyList<MonthlyReportEntry> Entries);

/// <summary>
/// Projects a list of <see cref="IDomainEvent"/>s into a <see cref="MonthlyReportProjection"/>. The
/// importance/grouping/acknowledgement rules here are deliberately minimal: no real gameplay event
/// type exists yet outside <see cref="GenericCommandExecutedEvent"/> and <see
/// cref="CampaignBootstrappedEvent"/>, so this is the mechanism proven end-to-end, not a tuned
/// content-authored importance table (that arrives once real event types need one, per ADR 0007).
/// </summary>
public static class MonthlyReportProjector
{
    public static MonthlyReportProjection Project(GameDate date, IReadOnlyList<IDomainEvent> events)
    {
        if (events is null)
            throw new ArgumentNullException(nameof(events));

        var entries = events
            .Select(evt => new MonthlyReportEntry(
                evt.EventId.ToTaggedString(),
                evt.Type,
                GroupOf(evt.Type),
                ImportanceOf(evt.Type),
                AcknowledgementOf(evt.Type),
                evt.SubjectIds,
                evt.OccurredDate))
            .ToArray();

        return new MonthlyReportProjection(date, entries);
    }

    /// <summary>Groups by the event type's namespace segment (e.g. <c>"campaign.bootstrapped"</c> →
    /// <c>"campaign"</c>), matching <c>gens-events-design.md</c>'s scope taxonomy in spirit until real
    /// event types exist to assign an actual Personal/Household/Settlement/Imperial scope.</summary>
    private static string GroupOf(string eventType)
    {
        var separatorIndex = eventType.IndexOf('.');
        return separatorIndex < 0 ? eventType : eventType[..separatorIndex];
    }

    private static ReportImportance ImportanceOf(string eventType) =>
        eventType == "campaign.bootstrapped" ? ReportImportance.High : ReportImportance.Medium;

    /// <summary>Flags a household-scope event type for player choice, matching <c>gens-events-design.md</c>
    /// §7's "Household-scope events...Flagged for Choice" rule; everything else auto-resolves. No
    /// household-scope event type is emitted yet, so this is currently a no-op rule proven ready for
    /// the event types later phases add.</summary>
    private static ReportAcknowledgementState AcknowledgementOf(string eventType) =>
        eventType.StartsWith("household.", StringComparison.Ordinal)
            ? ReportAcknowledgementState.Flagged
            : ReportAcknowledgementState.AutoResolved;
}
