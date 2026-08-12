using System.IO.Compression;
using Gens.Simulation.Random;
using Gens.Simulation.State;

namespace Gens.Simulation.Saves;

/// <summary>
/// Writes a <c>.gens</c> archive atomically (ADR 0010): the archive is built at a temporary path in
/// the same directory as the final target, fully written and flushed, then moved into place with a
/// single rename — a save that fails partway through never leaves a corrupted or partial file at the
/// target path.
/// </summary>
public static class SaveWriter
{
    public static void Write(
        string path,
        WorldState state,
        RandomStreamSet randomStreams,
        string gameVersion,
        string contentPackHash,
        IReadOnlyList<string>? generatedAssetReferences = null)
    {
        if (path is null)
            throw new ArgumentNullException(nameof(path));
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));

        var worldBytes = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(state));

        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (string.IsNullOrEmpty(directory))
            throw new ArgumentException($"'{path}' has no containing directory.", nameof(path));
        Directory.CreateDirectory(directory);

        var tempPath = Path.Combine(directory, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
        try
        {
            using (var fileStream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.ReadWrite))
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, leaveOpen: false))
            {
                WriteEntry(archive, SaveFormat.WorldEntry, worldBytes);

                var manifest = new SaveManifest(
                    SaveFormat.CurrentVersion,
                    gameVersion,
                    contentPackHash,
                    randomStreams.CaptureStates(),
                    generatedAssetReferences ?? Array.Empty<string>(),
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        [SaveFormat.WorldEntry] = CanonicalJson.Sha256Hex(worldBytes),
                    });
                var manifestBytes = CanonicalJson.SerializeToCanonicalBytes(SaveManifestDto.FromManifest(manifest));
                WriteEntry(archive, SaveFormat.ManifestEntry, manifestBytes);
            }

            // Atomic rename: on the platforms this project targets, File.Move onto an existing path
            // fails, so remove any prior save first — there is still no window where neither the old
            // nor the new file is a complete, valid archive, since the temp file is already fully
            // written and flushed above.
            if (File.Exists(path))
                File.Delete(path);
            File.Move(tempPath, path);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    // A fixed timestamp so two writes of byte-identical content produce a byte-identical archive
    // (ZIP entries otherwise stamp DateTime.Now, which would fail the "save, re-save, diff bytes"
    // regression test ADR 0010 calls for even when the underlying world.json content is unchanged).
    private static readonly DateTimeOffset FixedEntryTimestamp = new(1980, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static void WriteEntry(ZipArchive archive, string entryName, byte[] bytes)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        entry.LastWriteTime = FixedEntryTimestamp;
        using var entryStream = entry.Open();
        entryStream.Write(bytes, 0, bytes.Length);
    }
}
