using System.Text.Json.Serialization;

namespace Gens.ContentCompiler.Content;

/// <summary>The UR-01 shared scenario spec (<c>shared-scenario.json</c>): a fixed seed, region,
/// ruleset, difficulty, ordered command sequence, and month count that <c>run-shared-scenario</c>
/// runs natively; during the Unity retirement migration a human could reproduce the same inputs
/// manually inside the (now-retired) Unity Editor (ADR 0019) to compare the two clients.</summary>
public sealed record SharedScenarioSpec
{
    public required ulong Seed { get; init; }

    public required string Region { get; init; }

    public string Ruleset { get; init; } = "default";

    public string Difficulty { get; init; } = "standard";

    public string? StartProfileId { get; init; }

    public int StartMonths { get; init; }

    public required int Months { get; init; }

    public required IReadOnlyList<SharedScenarioCommandSpec> Commands { get; init; }
}

/// <summary>One entry in a <see cref="SharedScenarioSpec"/>'s command sequence, executed immediately
/// (mirroring <c>submit-command</c> with <c>--due-in-months 0</c>) in list order.</summary>
public sealed record SharedScenarioCommandSpec
{
    public required string ActorId { get; init; }

    public required string Type { get; init; }

    public string Payload { get; init; } = "{}";
}

/// <summary>One recorded checkpoint in a <see cref="HashTranscriptDto"/>: a human-readable label
/// (<c>"initial"</c>, <c>"post-command:&lt;type&gt;"</c>, <c>"month:&lt;n&gt;"</c>, <c>"final"</c>),
/// the in-campaign date it was captured at, and the hex-encoded <c>StateHasher</c> hash.</summary>
public sealed record HashCheckpointDto
{
    [JsonPropertyOrder(0)]
    public required string Label { get; init; }

    [JsonPropertyOrder(1)]
    public required long DateTotalMonths { get; init; }

    [JsonPropertyOrder(2)]
    public required string StateHashHex { get; init; }
}

/// <summary>The full hash transcript <c>run-shared-scenario</c> produces: every checkpoint captured
/// while running <see cref="SharedScenarioSpec"/>, in the order they occurred. Comparing two
/// transcripts checkpoint-by-checkpoint is the cross-client parity check ADR 0019 defines.</summary>
public sealed record HashTranscriptDto
{
    [JsonPropertyOrder(0)]
    public required string ScenarioSpecPath { get; init; }

    [JsonPropertyOrder(1)]
    public required IReadOnlyList<HashCheckpointDto> Checkpoints { get; init; }
}
