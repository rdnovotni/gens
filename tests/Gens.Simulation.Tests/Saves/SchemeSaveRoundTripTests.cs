using Gens.Simulation.Saves;
using Gens.Simulation.Schemes;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

/// <summary>Phase 10 item 12 save round-trip coverage.</summary>
public sealed class SchemeSaveRoundTripTests
{
    [Test]
    public void SchemeInstancesRoundTripThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(5));
        var initiatorId = state.CharacterIds.Issue();
        var targetId = state.CharacterIds.Issue();
        var agentId = state.CharacterIds.Issue();

        var progressingId = state.SchemeIds.Issue();
        var progressing = new SchemeInstance(
            progressingId, SchemeType.FabricateHook, initiatorId, targetId, agentId, new GameDate(1), 40, 30, SchemeStage.Progressing);
        state.Schemes.Add(progressingId, progressing);

        var resolvedId = state.SchemeIds.Issue();
        var resolved = new SchemeInstance(
            resolvedId, SchemeType.Assassinate, initiatorId, targetId, null, new GameDate(0), 100, 80,
            SchemeStage.Resolved, new GameDate(6), SchemeOutcome.DiscoveredAndEscalated, new GameDate(9));
        state.Schemes.Add(resolvedId, resolved);

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.Schemes.TryGet(progressingId, out var restoredProgressing), Is.True);
            Assert.That(restoredProgressing, Is.EqualTo(progressing));
            Assert.That(restored.Schemes.TryGet(resolvedId, out var restoredResolved), Is.True);
            Assert.That(restoredResolved, Is.EqualTo(resolved));
            Assert.That(restored.SchemeIds.Peek, Is.EqualTo(state.SchemeIds.Peek));
        });

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restored));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    [Test]
    public void AnEmptyPreExistingSaveFixtureStillLoadsWithoutAnySchemeData()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Saves", "Fixtures", "v1-empty-campaign.gens");
        var loaded = SaveReader.Read(path);

        Assert.That(loaded.State.Schemes.Count, Is.EqualTo(0));
        Assert.That(loaded.State.SchemeIds.Peek, Is.EqualTo(0));
    }
}
