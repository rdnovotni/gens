using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

/// <summary>Phase 10 item 10 save round-trip coverage.</summary>
public sealed class AutonomousDecisionLogSaveRoundTripTests
{
    [Test]
    public void AutonomousDecisionLogsRoundTripThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(5));
        var assignmentId = state.StewardshipAssignmentIds.Issue();
        var logId = state.AutonomousDecisionLogIds.Issue();
        var log = new AutonomousDecisionLog(
            logId, assignmentId, new GameDate(5), "change-rites-budget", "Restored the Rites Budget to its Standard default.",
            0, 0, StewardIncidentType.Skimming);
        state.AutonomousDecisionLogs.Add(logId, log);

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.AutonomousDecisionLogs.TryGet(logId, out var stored), Is.True);
            Assert.That(stored, Is.EqualTo(log));
            Assert.That(restored.AutonomousDecisionLogIds.Peek, Is.EqualTo(state.AutonomousDecisionLogIds.Peek));
        });

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restored));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    [Test]
    public void AnEmptyPreExistingSaveFixtureStillLoadsWithoutAnyDecisionLogData()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Saves", "Fixtures", "v1-empty-campaign.gens");
        var loaded = SaveReader.Read(path);

        Assert.That(loaded.State.AutonomousDecisionLogs.Count, Is.EqualTo(0));
        Assert.That(loaded.State.AutonomousDecisionLogIds.Peek, Is.EqualTo(0));
    }
}
