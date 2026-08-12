using System.Text.Json.Serialization;

namespace Gens.ContentCompiler.Content;

/// <summary>The compiled content pack manifest (ADR 0012): schema version, compiler version, a
/// SHA-256 source hash per input file, and the computed family dependency order.</summary>
public sealed record ContentManifestDto
{
    [JsonPropertyOrder(0)]
    public required int SchemaVersion { get; init; }

    [JsonPropertyOrder(1)]
    public required string CompilerVersion { get; init; }

    [JsonPropertyOrder(2)]
    public required IReadOnlyList<SourceFileHashDto> SourceHashes { get; init; }

    [JsonPropertyOrder(3)]
    public required IReadOnlyList<string> DependencyOrder { get; init; }
}

public sealed record SourceFileHashDto
{
    [JsonPropertyOrder(0)]
    public required string Path { get; init; }

    [JsonPropertyOrder(1)]
    public required string Sha256 { get; init; }
}

/// <summary>The full compiled artifact: the manifest plus every definition, grouped by family in
/// <see cref="ContentFamilies"/> dependency order and ordinal-<c>id</c> order within each family
/// (ADR 0004's ordering discipline applied to content).</summary>
public sealed record CompiledContentPackDto
{
    [JsonPropertyOrder(0)]
    public required ContentManifestDto Manifest { get; init; }

    [JsonPropertyOrder(1)]
    public required IReadOnlyList<CompiledFamilyDto> Families { get; init; }
}

public sealed record CompiledFamilyDto
{
    [JsonPropertyOrder(0)]
    public required string Name { get; init; }

    [JsonPropertyOrder(1)]
    public required IReadOnlyList<System.Text.Json.JsonElement> Definitions { get; init; }
}
