using System.Text.Json.Serialization;

namespace Gens.Simulation.Saves;

/// <summary>The canonical, explicitly-ordered <c>manifest.json</c> shape (ADR 0010) — <see
/// cref="SaveManifest"/> is the friendly in-memory record; this DTO is the only thing <see
/// cref="CanonicalJson"/> ever serializes for a manifest entry.</summary>
public sealed record SaveManifestDto
{
    [JsonPropertyOrder(0)]
    public required int SaveFormatVersion { get; init; }

    [JsonPropertyOrder(1)]
    public required string GameVersion { get; init; }

    [JsonPropertyOrder(2)]
    public required string ContentPackHash { get; init; }

    /// <summary>Ordinally sorted by stream name (ADR 0004), matching <see
    /// cref="Random.RandomStreamSet.CaptureStates"/>'s own ordering.</summary>
    [JsonPropertyOrder(3)]
    public required IReadOnlyList<RandomStreamEntryDto> RandomStreams { get; init; }

    [JsonPropertyOrder(4)]
    public required IReadOnlyList<string> GeneratedAssetReferences { get; init; }

    /// <summary>Ordinally sorted by entry name (e.g. <c>world.json</c>) so the manifest itself is
    /// byte-stable.</summary>
    [JsonPropertyOrder(5)]
    public required IReadOnlyList<EntryChecksumDto> EntryChecksums { get; init; }

    public static SaveManifestDto FromManifest(SaveManifest manifest) => new()
    {
        SaveFormatVersion = manifest.SaveFormatVersion,
        GameVersion = manifest.GameVersion,
        ContentPackHash = manifest.ContentPackHash,
        RandomStreams = manifest.RandomStreams
            .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
            .Select(static pair => new RandomStreamEntryDto { Name = pair.Key, State = pair.Value })
            .ToArray(),
        GeneratedAssetReferences = manifest.GeneratedAssetReferences,
        EntryChecksums = manifest.EntryChecksums
            .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
            .Select(static pair => new EntryChecksumDto { Entry = pair.Key, Sha256 = pair.Value })
            .ToArray(),
    };

    public SaveManifest ToManifest() => new(
        SaveFormatVersion,
        GameVersion,
        ContentPackHash,
        RandomStreams.ToDictionary(static entry => entry.Name, static entry => entry.State, StringComparer.Ordinal),
        GeneratedAssetReferences,
        EntryChecksums.ToDictionary(static entry => entry.Entry, static entry => entry.Sha256, StringComparer.Ordinal));
}

public sealed record RandomStreamEntryDto
{
    [JsonPropertyOrder(0)]
    public required string Name { get; init; }

    [JsonPropertyOrder(1)]
    public required Random.Pcg32State State { get; init; }
}

public sealed record EntryChecksumDto
{
    [JsonPropertyOrder(0)]
    public required string Entry { get; init; }

    [JsonPropertyOrder(1)]
    public required string Sha256 { get; init; }
}
