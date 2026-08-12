namespace Gens.Simulation.Saves;

public static class SaveFormat
{
    public const int CurrentVersion = 1;
    public const string Extension = ".gens";
    public const string ManifestEntry = "manifest.json";
    public const string WorldEntry = "world.json";
    public const string HistoryEntry = "history.json";
}

/// <summary>The <c>manifest.json</c> entry's shape (ADR 0010): format/game/content versions, every
/// RNG stream's captured state (already ordinally sorted by <see
/// cref="Random.RandomStreamSet.CaptureStates"/> per ADR 0004), generated-asset references, and a
/// SHA-256 checksum for every other entry in the archive, computed over that entry's canonical bytes
/// before compression.</summary>
public sealed record SaveManifest(
    int SaveFormatVersion,
    string GameVersion,
    string ContentPackHash,
    IReadOnlyDictionary<string, Random.Pcg32State> RandomStreams,
    IReadOnlyList<string> GeneratedAssetReferences,
    IReadOnlyDictionary<string, string> EntryChecksums);

