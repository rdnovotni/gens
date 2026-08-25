using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Stewardship;

/// <summary>One month's line in a <see cref="ReturnReport"/> (§8's <c>summaryEntries[]</c>) — a
/// narrative summary, not a raw stat delta, matching the Monthly Report's own "narrative summary"
/// framing (§8: "mirrors Events' Monthly Report").</summary>
public sealed record ReturnReportSummaryEntry(GameDate Month, string DecisionType, string Outcome);

/// <summary>One incident actually discovered on return (§8's <c>incidentsDiscovered[]</c>) — the
/// dramatic reveal itself. Never surfaced before this point: <see cref="AutonomousDecisionLog"/>
/// entries carry the same information the whole time, but nothing reads it back out until the
/// assignment ends.</summary>
public sealed record ReturnReportIncidentEntry(GameDate Month, StewardIncidentType Type, Money Amount);

/// <summary>
/// The narrative summary delivered on Travel return or Regency's natural end (Phase 10 item 11; §8's
/// <c>ReturnReport</c> data model) — built once, by <see cref="ReturnReportGenerator"/>, when <see
/// cref="StewardshipCommands.EndPipeline"/> accepts an <see cref="EndStewardshipAssignmentCommand"/>.
/// </summary>
public sealed record ReturnReport(
    RuntimeId<ReturnReport> ReportId,
    RuntimeId<StewardshipAssignment> AssignmentId,
    IReadOnlyList<ReturnReportSummaryEntry> SummaryEntries,
    Money TotalTreasuryImpact,
    IReadOnlyList<ReturnReportIncidentEntry> IncidentsDiscovered,
    bool ChronicleWorthy);

/// <summary>Builds a <see cref="ReturnReport"/> by reading back every <see
/// cref="AutonomousDecisionLog"/> entry recorded for one assignment — a pure projection over already-
/// stored state, matching <see cref="State.WorldState"/>'s Monthly Report's own "generated entirely
/// from domain events and read models" precedent (Phase 9 item 4) applied to logs instead of
/// events.</summary>
public static class ReturnReportGenerator
{
    public static ReturnReport Generate(WorldState state, RuntimeId<ReturnReport> reportId, RuntimeId<StewardshipAssignment> assignmentId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var logs = state.AutonomousDecisionLogs.InAscendingOrder()
            .Select(entry => entry.Value)
            .Where(log => log.AssignmentId == assignmentId)
            .OrderBy(log => log.Month.TotalMonths)
            .ToArray();

        var summaryEntries = logs.Select(log => new ReturnReportSummaryEntry(log.Month, log.DecisionType, log.Outcome)).ToArray();

        var incidents = logs
            .Where(log => log.IncidentType is not null)
            .Select(log => new ReturnReportIncidentEntry(log.Month, log.IncidentType!.Value, -log.TreasuryImpact))
            .ToArray();

        var totalTreasuryImpact = logs.Aggregate(Money.Zero, (total, log) => total + log.TreasuryImpact);

        return new ReturnReport(reportId, assignmentId, summaryEntries, totalTreasuryImpact, incidents, ChronicleWorthy: incidents.Length > 0);
    }
}
