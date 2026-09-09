using System.Text.Json;
using Gens.Simulation.Campaign;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Commands;
using Gens.Simulation.Epithets;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

/// <summary>
/// UR-01 (Unity retirement blocker) / ADR 0019: the permanent shared-scenario fixtures
/// (<c>ur01-current-native.gens</c>, <c>ur01-legacy.gens</c>, <c>ur01-migrated.gens</c>) each
/// reproduce the exact hash their recorded checkpoint in <c>ur01-hash-transcript-native.json</c>
/// promises, and each independently continues deterministically after a reload — the "save fixture
/// matrix" and "save-&gt;load-&gt;advance continuation matches for every supported fixture" acceptance
/// criteria ADR 0019 defines as achievable without a Unity Editor.
/// </summary>
public sealed class Ur01SharedScenarioFixtureTests
{
    private static readonly string FixturesDirectory =
        Path.Combine(TestContext.CurrentContext.TestDirectory, "Saves", "Fixtures");

    private static string FixturePath(string fileName) => Path.Combine(FixturesDirectory, fileName);

    private static ulong CheckpointHash(string label)
    {
        using var document = JsonDocument.Parse(File.ReadAllBytes(FixturePath("ur01-hash-transcript-native.json")));
        foreach (var checkpoint in document.RootElement.GetProperty("checkpoints").EnumerateArray())
        {
            if (checkpoint.GetProperty("label").GetString() == label)
                return Convert.ToUInt64(checkpoint.GetProperty("stateHashHex").GetString(), 16);
        }

        throw new InvalidOperationException($"Checkpoint '{label}' was not found in the recorded transcript.");
    }

    /// <summary>Mirrors <c>AdvanceCommand</c>'s own tick pairing: one month via
    /// <see cref="ScheduledActionSystem"/>, then <see cref="WorldState.AdvanceMonth"/>.</summary>
    private static ulong AdvanceOneMonthAndHash(LoadedSave loaded)
    {
        var state = loaded.State;
        var simulation = new WriteSetVerifyingSimulation(new IMonthlySystem<WorldState>[] { new ScheduledActionSystem() });
        var tickEvents = simulation.Tick(state, state.Date, loaded.RandomStreams);
        var chronicleEvents = ChronicleGenerationSystem.Generate(state, tickEvents);
        var combined = new List<IDomainEvent>(tickEvents);
        combined.AddRange(chronicleEvents);
        EpithetGenerationSystem.Generate(state, combined);
        state.AdvanceMonth();
        return StateHasher.Hash(state);
    }

    [TestCase("ur01-current-native.gens", "final")]
    [TestCase("ur01-legacy.gens", "initial")]
    [TestCase("ur01-migrated.gens", "final")]
    public void FixtureHashMatchesItsRecordedCheckpoint(string fixtureFileName, string checkpointLabel)
    {
        var loaded = SaveReader.Read(FixturePath(fixtureFileName));

        Assert.That(StateHasher.Hash(loaded.State), Is.EqualTo(CheckpointHash(checkpointLabel)));
    }

    [TestCase("ur01-current-native.gens")]
    [TestCase("ur01-legacy.gens")]
    [TestCase("ur01-migrated.gens")]
    public void FixtureContinuesDeterministicallyAfterReload(string fixtureFileName)
    {
        var first = AdvanceOneMonthAndHash(SaveReader.Read(FixturePath(fixtureFileName)));
        var second = AdvanceOneMonthAndHash(SaveReader.Read(FixturePath(fixtureFileName)));

        Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public void MigratedFixtureIsByteIdenticalToCurrentNativeFixture()
    {
        var currentNativeBytes = File.ReadAllBytes(FixturePath("ur01-current-native.gens"));
        var migratedBytes = File.ReadAllBytes(FixturePath("ur01-migrated.gens"));

        Assert.That(migratedBytes, Is.EqualTo(currentNativeBytes),
            "SaveMigrationRegistry.Empty means v1 is the only schema version today (ADR 0019); " +
            "migrating a v1 save must be a byte-identical no-op, not a real transformation.");
    }
}
