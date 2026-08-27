using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

/// <summary>Phase 10 package 13 save round-trip coverage for <see cref="ReturnReport"/>, mirroring
/// <see cref="StewardshipSaveRoundTripTests"/>'s identical pattern.</summary>
public sealed class ReturnReportSaveRoundTripTests
{
    [Test]
    public void ReturnReportsRoundTripThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(5));
        var householdId = state.HouseholdIds.Issue();

        var assignmentId = state.StewardshipAssignmentIds.Issue();
        var assignment = StewardshipAssignment.Create(
            assignmentId, householdId, StewardshipContext.Travel, StewardshipMode.SingleSteward,
            state.CharacterIds.Issue(), null, null, StewardAutonomyLevel.Standard, new GameDate(1)) with
        { EndDate = new GameDate(4) };
        state.StewardshipAssignments.Add(assignmentId, assignment);

        var logId = state.AutonomousDecisionLogIds.Issue();
        state.AutonomousDecisionLogs.Add(
            logId,
            new AutonomousDecisionLog(
                logId, assignmentId, new GameDate(2), "none", "The steward was discovered quietly skimming 3.00 denarii.",
                42, 10, StewardIncidentType.Skimming));

        var reportId = state.ReturnReportIds.Issue();
        var report = new ReturnReport(
            reportId, assignmentId,
            new[] { "held", "The steward was discovered quietly skimming 3.00 denarii." },
            -Money.FromDenarii(3),
            new[] { logId },
            ChronicleWorthy: false);
        state.ReturnReports.Add(reportId, report);

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.ReturnReports.TryGet(reportId, out var restoredReport), Is.True);
            Assert.That(restoredReport!.ReportId, Is.EqualTo(report.ReportId));
            Assert.That(restoredReport.AssignmentId, Is.EqualTo(report.AssignmentId));
            Assert.That(restoredReport.SummaryEntries, Is.EqualTo(report.SummaryEntries));
            Assert.That(restoredReport.TotalTreasuryImpact, Is.EqualTo(report.TotalTreasuryImpact));
            Assert.That(restoredReport.IncidentsDiscovered, Is.EqualTo(report.IncidentsDiscovered));
            Assert.That(restoredReport.ChronicleWorthy, Is.EqualTo(report.ChronicleWorthy));

            Assert.That(restored.ReturnReportIds.Peek, Is.EqualTo(state.ReturnReportIds.Peek));
        });

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restored));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    [Test]
    public void AnEmptyPreExistingSaveFixtureStillLoadsWithoutAnyReturnReports()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Saves", "Fixtures", "v1-empty-campaign.gens");
        var loaded = SaveReader.Read(path);

        Assert.That(loaded.State.ReturnReports.Count, Is.EqualTo(0));
        Assert.That(loaded.State.ReturnReportIds.Peek, Is.EqualTo(0));
    }
}
