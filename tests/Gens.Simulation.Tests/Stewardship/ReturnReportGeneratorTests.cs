using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Stewardship;

/// <summary>Phase 10 item 11 coverage for <see cref="ReturnReportGenerator"/>.</summary>
public sealed class ReturnReportGeneratorTests
{
    [Test]
    public void GenerateSummarizesOnlyEntriesForTheGivenAssignment()
    {
        var state = new WorldState(new GameDate(0));
        var assignmentId = state.StewardshipAssignmentIds.Issue();
        var otherAssignmentId = state.StewardshipAssignmentIds.Issue();

        void AddLog(RuntimeId<StewardshipAssignment> forAssignment, int month, string decisionType, string outcome)
        {
            var logId = state.AutonomousDecisionLogIds.Issue();
            state.AutonomousDecisionLogs.Add(logId, new AutonomousDecisionLog(logId, forAssignment, new GameDate(month), decisionType, outcome, 100, 0));
        }

        AddLog(assignmentId, 1, "fund-festival", "Funded a Festival.");
        AddLog(assignmentId, 2, "none", "held");
        AddLog(otherAssignmentId, 1, "change-rites-budget", "Restored the Rites Budget.");

        var report = ReturnReportGenerator.Generate(state, state.ReturnReportIds.Issue(), assignmentId);

        Assert.That(report.SummaryEntries, Has.Count.EqualTo(2));
    }

    [Test]
    public void GenerateOrdersSummaryEntriesByMonthRegardlessOfLogInsertionOrder()
    {
        var state = new WorldState(new GameDate(0));
        var assignmentId = state.StewardshipAssignmentIds.Issue();

        var laterLogId = state.AutonomousDecisionLogIds.Issue();
        state.AutonomousDecisionLogs.Add(laterLogId, new AutonomousDecisionLog(laterLogId, assignmentId, new GameDate(5), "none", "held", 100, 0));
        var earlierLogId = state.AutonomousDecisionLogIds.Issue();
        state.AutonomousDecisionLogs.Add(earlierLogId, new AutonomousDecisionLog(earlierLogId, assignmentId, new GameDate(1), "none", "held", 100, 0));

        var report = ReturnReportGenerator.Generate(state, state.ReturnReportIds.Issue(), assignmentId);

        Assert.That(report.SummaryEntries.Select(e => e.Month.TotalMonths), Is.EqualTo(new[] { 1, 5 }));
    }

    [Test]
    public void GenerateSumsTreasuryImpactAndCollectsIncidents()
    {
        var state = new WorldState(new GameDate(0));
        var assignmentId = state.StewardshipAssignmentIds.Issue();

        var spendLogId = state.AutonomousDecisionLogIds.Issue();
        state.AutonomousDecisionLogs.Add(
            spendLogId,
            new AutonomousDecisionLog(spendLogId, assignmentId, new GameDate(1), "fund-festival", "Funded.", 100, 0, null, Money.FromDenarii(50)));

        var incidentLogId = state.AutonomousDecisionLogIds.Issue();
        state.AutonomousDecisionLogs.Add(
            incidentLogId,
            new AutonomousDecisionLog(
                incidentLogId, assignmentId, new GameDate(2), "none", "held", 100, 15,
                StewardIncidentType.Skimming, -Money.FromDenarii(5)));

        var report = ReturnReportGenerator.Generate(state, state.ReturnReportIds.Issue(), assignmentId);

        Assert.Multiple(() =>
        {
            Assert.That(report.TotalTreasuryImpact, Is.EqualTo(Money.FromDenarii(45)));
            Assert.That(report.IncidentsDiscovered, Has.Count.EqualTo(1));
            Assert.That(report.IncidentsDiscovered[0].Type, Is.EqualTo(StewardIncidentType.Skimming));
            Assert.That(report.IncidentsDiscovered[0].Amount, Is.EqualTo(Money.FromDenarii(5)));
            Assert.That(report.ChronicleWorthy, Is.True);
        });
    }

    [Test]
    public void GenerateIsNotChronicleWorthyWithNoIncidents()
    {
        var state = new WorldState(new GameDate(0));
        var assignmentId = state.StewardshipAssignmentIds.Issue();
        var logId = state.AutonomousDecisionLogIds.Issue();
        state.AutonomousDecisionLogs.Add(logId, new AutonomousDecisionLog(logId, assignmentId, new GameDate(1), "none", "held", 100, 0));

        var report = ReturnReportGenerator.Generate(state, state.ReturnReportIds.Issue(), assignmentId);

        Assert.That(report.ChronicleWorthy, Is.False);
    }
}
