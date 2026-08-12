using System.Text.Json.Serialization;

namespace Gens.Simulation.Saves;

/// <summary>The <c>world.json</c> entry's canonical shape (ADR 0010): every <see
/// cref="State.WorldState"/> counter, ordered partition, and the command sequence number, in an
/// explicit property order. Collections are pre-sorted by <see cref="WorldStateMapper"/> before
/// reaching this DTO — <see cref="CanonicalJson"/> never sorts on a DTO's behalf.</summary>
public sealed record WorldSaveDocument
{
    [JsonPropertyOrder(0)]
    public required int DateTotalMonths { get; init; }

    [JsonPropertyOrder(1)]
    public required long NextCommandSequenceNumber { get; init; }

    [JsonPropertyOrder(2)]
    public required CounterSetDto Counters { get; init; }

    [JsonPropertyOrder(3)]
    public required IReadOnlyList<string> CharacterIds { get; init; }

    [JsonPropertyOrder(4)]
    public required IReadOnlyList<KnowledgeEntryDto> Knowledge { get; init; }
}

/// <summary>The next-value of every per-entity-kind <see cref="Identity.RuntimeIdCounter{T}"/> (ADR
/// 0001) — itself campaign state, persisted like any other field.</summary>
public sealed record CounterSetDto
{
    [JsonPropertyOrder(0)]
    public required long RegionIds { get; init; }

    [JsonPropertyOrder(1)]
    public required long SettlementIds { get; init; }

    [JsonPropertyOrder(2)]
    public required long PlotIds { get; init; }

    [JsonPropertyOrder(3)]
    public required long HouseholdIds { get; init; }

    [JsonPropertyOrder(4)]
    public required long ActorIds { get; init; }

    [JsonPropertyOrder(5)]
    public required long CharacterIds { get; init; }

    [JsonPropertyOrder(6)]
    public required long BuildingIds { get; init; }

    [JsonPropertyOrder(7)]
    public required long ContractIds { get; init; }

    [JsonPropertyOrder(8)]
    public required long ActivityIds { get; init; }

    [JsonPropertyOrder(9)]
    public required long CommandIds { get; init; }

    [JsonPropertyOrder(10)]
    public required long EventIds { get; init; }
}

/// <summary>One <see cref="State.KnowledgeState"/> entry. <see cref="ValueJson"/> holds the fact's
/// value as a pre-serialized JSON string: <see cref="State.KnowledgeEntry.Value"/> is <c>object</c>
/// (Phase 2's storage-only placeholder, per that type's own doc comment — no system has emitted a real
/// payload shape yet), so this DTO round-trips whatever JSON shape a future writer produces without
/// needing to know its type ahead of time.</summary>
public sealed record KnowledgeEntryDto
{
    [JsonPropertyOrder(0)]
    public required string ObserverId { get; init; }

    [JsonPropertyOrder(1)]
    public required string SubjectId { get; init; }

    [JsonPropertyOrder(2)]
    public required string Topic { get; init; }

    [JsonPropertyOrder(3)]
    public required string ValueJson { get; init; }

    [JsonPropertyOrder(4)]
    public required string Confidence { get; init; }

    [JsonPropertyOrder(5)]
    public required int AsOfDateTotalMonths { get; init; }

    [JsonPropertyOrder(6)]
    public string? ProvenanceEventId { get; init; }
}
