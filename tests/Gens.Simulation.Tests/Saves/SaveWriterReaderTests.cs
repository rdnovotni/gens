using Gens.Simulation.Random;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

public sealed class SaveWriterReaderTests
{
    [Test]
    public void RoundTripPreservesStateHashAndRandomStreamStates()
    {
        var (state, streams) = BuildPopulatedScenario();
        var beforeHash = StateHasher.Hash(state);
        var beforeStreamStates = streams.CaptureStates();

        var path = Path.Combine(Path.GetTempPath(), $"gens-save-test-{Guid.NewGuid():N}.gens");
        try
        {
            SaveWriter.Write(path, state, streams, "0.0.0-test", "content-hash-placeholder");
            var loaded = SaveReader.Read(path);

            Assert.That(StateHasher.Hash(loaded.State), Is.EqualTo(beforeHash));
            Assert.That(loaded.RandomStreams.CaptureStates(), Is.EqualTo(beforeStreamStates));
            Assert.That(loaded.Manifest.SaveFormatVersion, Is.EqualTo(SaveFormat.CurrentVersion));
            Assert.That(loaded.Manifest.ContentPackHash, Is.EqualTo("content-hash-placeholder"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void ReSavingUnchangedStateProducesByteIdenticalArchive()
    {
        var (state, streams) = BuildPopulatedScenario();
        var pathA = Path.Combine(Path.GetTempPath(), $"gens-save-test-{Guid.NewGuid():N}.gens");
        var pathB = Path.Combine(Path.GetTempPath(), $"gens-save-test-{Guid.NewGuid():N}.gens");
        try
        {
            SaveWriter.Write(pathA, state, streams, "0.0.0-test", "content-hash-placeholder");
            SaveWriter.Write(pathB, state, streams, "0.0.0-test", "content-hash-placeholder");

            Assert.That(File.ReadAllBytes(pathB), Is.EqualTo(File.ReadAllBytes(pathA)));
        }
        finally
        {
            File.Delete(pathA);
            File.Delete(pathB);
        }
    }

    [Test]
    public void SaveLoadResaveProducesByteIdenticalWorldEntry()
    {
        var (state, streams) = BuildPopulatedScenario();
        var pathA = Path.Combine(Path.GetTempPath(), $"gens-save-test-{Guid.NewGuid():N}.gens");
        var pathB = Path.Combine(Path.GetTempPath(), $"gens-save-test-{Guid.NewGuid():N}.gens");
        try
        {
            SaveWriter.Write(pathA, state, streams, "0.0.0-test", "content-hash-placeholder");
            var loaded = SaveReader.Read(pathA);
            SaveWriter.Write(pathB, loaded.State, loaded.RandomStreams, "0.0.0-test", "content-hash-placeholder");

            Assert.That(File.ReadAllBytes(pathB), Is.EqualTo(File.ReadAllBytes(pathA)));
        }
        finally
        {
            File.Delete(pathA);
            File.Delete(pathB);
        }
    }

    [Test]
    public void LoadFailsHardOnAChecksumMismatch()
    {
        var (state, streams) = BuildPopulatedScenario();
        var path = Path.Combine(Path.GetTempPath(), $"gens-save-test-{Guid.NewGuid():N}.gens");
        try
        {
            SaveWriter.Write(path, state, streams, "0.0.0-test", "content-hash-placeholder");
            CorruptWorldEntry(path);

            Assert.That(() => SaveReader.Read(path), Throws.TypeOf<SaveCorruptedException>());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void WriteNeverLeavesAPartialFileAtTheTargetPathOnFailure()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"gens-save-test-dir-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "campaign.gens");
        try
        {
            // A null RandomStreamSet is rejected before any file IO happens.
            Assert.That(
                () => SaveWriter.Write(path, new WorldState(new GameDate(0)), null!, "0.0.0-test", "hash"),
                Throws.ArgumentNullException);

            Assert.That(File.Exists(path), Is.False);
            Assert.That(Directory.GetFiles(directory), Is.Empty);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Test]
    public void WriteOverwritesAnExistingSaveAtomicallyWithoutALeftoverTempFile()
    {
        var (state, streams) = BuildPopulatedScenario();
        var path = Path.Combine(Path.GetTempPath(), $"gens-save-test-{Guid.NewGuid():N}.gens");
        try
        {
            SaveWriter.Write(path, state, streams, "0.0.0-test", "content-hash-placeholder");
            state.AdvanceMonth();
            SaveWriter.Write(path, state, streams, "0.0.0-test", "content-hash-placeholder");

            var loaded = SaveReader.Read(path);
            Assert.That(loaded.State.Date.TotalMonths, Is.EqualTo(8));

            var leftoverTempFiles = Directory.GetFiles(Path.GetDirectoryName(path)!, ".*.tmp");
            Assert.That(leftoverTempFiles, Is.Empty);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static (WorldState State, RandomStreamSet Streams) BuildPopulatedScenario()
    {
        var state = new WorldState(new GameDate(7));
        var firstCharacter = state.CharacterIds.Issue();
        state.Characters.Add(firstCharacter, new object());
        state.PlotIds.Issue();
        state.IssueCommandSequenceNumber();
        state.Knowledge.Set(
            new KnowledgeKey("player", firstCharacter.ToTaggedString(), "location"),
            new KnowledgeEntry("Rome", KnowledgeConfidence.Certain, state.Date, "event_0000001"));

        var streams = new RandomStreamSet();
        streams.AddDerived("growth", 987654321UL);
        streams.NextUInt("growth");

        return (state, streams);
    }

    private static void CorruptWorldEntry(string path)
    {
        using var archive = System.IO.Compression.ZipFile.Open(path, System.IO.Compression.ZipArchiveMode.Update);
        var entry = archive.GetEntry(SaveFormat.WorldEntry)!;
        using var stream = entry.Open();
        stream.SetLength(0);
        var corrupted = System.Text.Encoding.UTF8.GetBytes("{\"corrupted\":true}");
        stream.Write(corrupted, 0, corrupted.Length);
    }
}
