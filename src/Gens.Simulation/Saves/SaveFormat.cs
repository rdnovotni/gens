namespace Gens.Simulation.Saves;

public static class SaveFormat
{
    public const int CurrentVersion = 1;
    public const string Extension = ".gens";
    public const string ManifestEntry = "manifest.json";
    public const string WorldEntry = "world.json";
    public const string HistoryEntry = "history.json";
}

public sealed record SaveManifest(
    int SaveFormatVersion,
    string GameVersion,
    IReadOnlyDictionary<string, Random.Pcg32State> RandomStreams,
    IReadOnlyList<string> GeneratedAssetReferences);

