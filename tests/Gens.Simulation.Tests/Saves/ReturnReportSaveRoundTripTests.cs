using Gens.Simulation.Ledger;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

/// <summary>Phase 10 item 11 save round-trip coverage.</summary>
public sealed class ReturnReportSaveRoundTripTests
{
    [Test]
    public void ReturnReportsRoundTripThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(5));
        var assignmentId = state.StewardshipAssignmentIds.Issue();
        var reportId = state.ReturnReportIds.Issue();
        var report = new ReturnReport(
            reportId, assignmentId,
            new[] { new ReturnReportSummaryEntry(new GameDate(1), "fund-festival", "Funded a Festival.") },
            Money.FromDenarii(45),
            new[] { new ReturnReportIncidentEntry(new GameDate(2), StewardIncidentType.Skimming, Money.FromDenarii(5)) },
            ChronicleWorthy: true);
        state.ReturnReports.Add(reportId, report);

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.ReturnReports.TryGet(reportId, out var stored), Is.True);
            Assert.That(stored!.ReportId, Is.EqualTo(report.ReportId));
            Assert.That(stored.AssignmentId, Is.EqualTo(report.AssignmentId));
            Assert.That(stored.SummaryEntries, Is.EqualTo(report.SummaryEntries));
            Assert.That(stored.TotalTreasuryImpact, Is.EqualTo(report.TotalTreasuryImpact));
            Assert.That(stored.IncidentsDiscovered, Is.EqualTo(report.IncidentsDiscovered));
            Assert.That(stored.ChronicleWorthy, Is.EqualTo(report.ChronicleWorthy));
            Assert.That(restored.ReturnReportIds.Peek, Is.EqualTo(state.ReturnReportIds.Peek));
        });

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restored));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    [Test]
    public void AnEmptyPreExistingSaveFixtureStillLoadsWithoutAnyReturnReportData()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Saves", "Fixtures", "v1-empty-campaign.gens");
        var loaded = SaveReader.Read(path);

        Assert.That(loaded.State.ReturnReports.Count, Is.EqualTo(0));
        Assert.That(loaded.State.ReturnReportIds.Peek, Is.EqualTo(0));
    }
}
