using System.IO.Compression;
using System.Text.Json;
using Gens.Simulation.Random;
using Gens.Simulation.Saves.Migrations;
using Gens.Simulation.State;

namespace Gens.Simulation.Saves;

/// <summary>The result of a successful <see cref="SaveReader.Read"/>.</summary>
public sealed record LoadedSave(WorldState State, RandomStreamSet RandomStreams, SaveManifest Manifest);

/// <summary>
/// Reads and verifies a <c>.gens</c> archive (ADR 0010): every manifest checksum is verified before
/// any content is touched — a mismatch is a hard load failure, never a best-effort partial load — and
/// a save at an older format version is migrated to <see cref="SaveFormat.CurrentVersion"/> (ADR
/// 0011) before <see cref="WorldStateMapper"/> ever sees it.
/// </summary>
public static class SaveReader
{
    public static LoadedSave Read(string path, SaveMigrationRegistry? migrations = null)
    {
        if (path is null)
            throw new ArgumentNullException(nameof(path));

        migrations ??= SaveMigrationRegistry.Empty;

        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read, leaveOpen: false);

        var manifestBytes = ReadEntry(archive, SaveFormat.ManifestEntry);
        var manifest = CanonicalJson.Deserialize<SaveManifestDto>(manifestBytes).ToManifest();

        var worldBytes = ReadEntry(archive, SaveFormat.WorldEntry);
        VerifyChecksum(manifest, SaveFormat.WorldEntry, worldBytes);

        var worldJson = worldBytes;
        if (manifest.SaveFormatVersion < SaveFormat.CurrentVersion)
        {
            using var document = JsonDocument.Parse(worldBytes);
            using var migrated = migrations.MigrateToVersion(document, manifest.SaveFormatVersion, SaveFormat.CurrentVersion);
            worldJson = JsonSerializer.SerializeToUtf8Bytes(migrated.RootElement, CanonicalJson.Options);
        }
        else if (manifest.SaveFormatVersion > SaveFormat.CurrentVersion)
        {
            throw new SaveMigrationException(
                $"Save format version {manifest.SaveFormatVersion} is newer than the supported version {SaveFormat.CurrentVersion}.");
        }

        var worldDto = CanonicalJson.Deserialize<WorldSaveDocument>(worldJson);
        var state = WorldStateMapper.ToWorldState(worldDto);
        var randomStreams = RandomStreamSet.Restore(manifest.RandomStreams);

        return new LoadedSave(state, randomStreams, manifest);
    }

    /// <summary>Verifies every manifest checksum without fully loading the save — the shape the
    /// <c>verify-save</c> developer command (Phase 3, item 7) needs.</summary>
    public static void VerifyChecksums(string path)
    {
        if (path is null)
            throw new ArgumentNullException(nameof(path));

        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read, leaveOpen: false);

        var manifestBytes = ReadEntry(archive, SaveFormat.ManifestEntry);
        var manifest = CanonicalJson.Deserialize<SaveManifestDto>(manifestBytes).ToManifest();

        foreach (var entryName in manifest.EntryChecksums.Keys)
            VerifyChecksum(manifest, entryName, ReadEntry(archive, entryName));
    }

    private static void VerifyChecksum(SaveManifest manifest, string entryName, byte[] bytes)
    {
        if (!manifest.EntryChecksums.TryGetValue(entryName, out var expected))
            throw new SaveCorruptedException($"Manifest has no checksum recorded for entry '{entryName}'.");

        var actual = CanonicalJson.Sha256Hex(bytes);
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
            throw new SaveCorruptedException(
                $"Checksum mismatch for entry '{entryName}': expected {expected}, computed {actual}.");
    }

    private static byte[] ReadEntry(ZipArchive archive, string entryName)
    {
        var entry = archive.GetEntry(entryName) ??
            throw new SaveCorruptedException($"Save archive is missing the required entry '{entryName}'.");
        using var entryStream = entry.Open();
        using var buffer = new MemoryStream();
        entryStream.CopyTo(buffer);
        return buffer.ToArray();
    }
}

public sealed class SaveCorruptedException : Exception
{
    public SaveCorruptedException(string message)
        : base(message)
    {
    }
}
