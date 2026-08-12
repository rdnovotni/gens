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

    /// <summary>The calendar queue (Phase 4 item 4): future-dated work not yet due. Already in
    /// ascending (due date, action ID) order (ADR 0004) via <see
    /// cref="Identity.OrderedRegistry{TId,TEntity}.InAscendingOrder"/>. Not <c>required</c>, and
    /// defaults to empty: ADR 0011's "additive only until v1 ships" policy means the permanent
    /// <c>v1-empty-campaign.gens</c> fixture — written before this field existed — must still load at
    /// <see cref="SaveFormat.CurrentVersion"/> without a migration.</summary>
    [JsonPropertyOrder(5)]
    public IReadOnlyList<ScheduledActionEntryDto> ScheduledActions { get; init; } = Array.Empty<ScheduledActionEntryDto>();

    /// <summary>Every Character's full record (Phase 5 item 1), already in ascending-<see
    /// cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and defaults to empty, for the same
    /// additive-only reason as <see cref="ScheduledActions"/> above: <see cref="CharacterIds"/> alone
    /// still fully describes a pre-Phase-5 save (its Characters partition was always empty), so no
    /// migration is needed for this field to appear. <see cref="CharacterIds"/> itself is kept,
    /// redundantly derivable from this list's IDs, purely for that same backward compatibility.</summary>
    [JsonPropertyOrder(6)]
    public IReadOnlyList<CharacterDto> Characters { get; init; } = Array.Empty<CharacterDto>();
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

    /// <summary>Not <c>required</c>, and defaults to 0: see <see cref="WorldSaveDocument.ScheduledActions"/>'s
    /// doc comment for why this field must tolerate a pre-Phase-4 save with no scheduled actions.</summary>
    [JsonPropertyOrder(11)]
    public long ScheduledActionIds { get; init; }
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

/// <summary>One <see cref="State.ScheduledActionEntry"/>, in the same tagged-string-ID convention as
/// every other cross-referenced runtime ID (ADR 0001).</summary>
public sealed record ScheduledActionEntryDto
{
    [JsonPropertyOrder(0)]
    public required string ActionId { get; init; }

    [JsonPropertyOrder(1)]
    public required int DueDateTotalMonths { get; init; }

    [JsonPropertyOrder(2)]
    public required string ActorId { get; init; }

    [JsonPropertyOrder(3)]
    public required string ActionType { get; init; }

    [JsonPropertyOrder(4)]
    public required string PayloadJson { get; init; }

    [JsonPropertyOrder(5)]
    public string? CausationId { get; init; }
}

/// <summary>One <see cref="Characters.Character"/>'s full record (Phase 5 item 1), in the same
/// declared-field order the record itself uses.</summary>
public sealed record CharacterDto
{
    [JsonPropertyOrder(0)]
    public required string Id { get; init; }

    [JsonPropertyOrder(1)]
    public required string Praenomen { get; init; }

    [JsonPropertyOrder(2)]
    public required string Nomen { get; init; }

    [JsonPropertyOrder(3)]
    public string? Cognomen { get; init; }

    [JsonPropertyOrder(4)]
    public required string Sex { get; init; }

    [JsonPropertyOrder(5)]
    public required int BirthDateTotalMonths { get; init; }

    [JsonPropertyOrder(6)]
    public required string LegalStatus { get; init; }

    [JsonPropertyOrder(7)]
    public string? SocialClass { get; init; }

    [JsonPropertyOrder(8)]
    public required string Culture { get; init; }

    [JsonPropertyOrder(9)]
    public required string Location { get; init; }

    [JsonPropertyOrder(10)]
    public string? Household { get; init; }

    [JsonPropertyOrder(11)]
    public required CoreAttributesDto Attributes { get; init; }

    [JsonPropertyOrder(12)]
    public required LaborSkillsDto Skills { get; init; }

    [JsonPropertyOrder(13)]
    public required ConditionDto Condition { get; init; }

    [JsonPropertyOrder(14)]
    public required string Source { get; init; }

    [JsonPropertyOrder(15)]
    public required int InstantiatedAtMonth { get; init; }
}

/// <summary>One <see cref="Characters.CoreAttributes"/> (<c>gens-familia-design.md</c> §2.1).</summary>
public sealed record CoreAttributesDto
{
    [JsonPropertyOrder(0)]
    public required int Diplomacy { get; init; }

    [JsonPropertyOrder(1)]
    public required int Martial { get; init; }

    [JsonPropertyOrder(2)]
    public required int Stewardship { get; init; }

    [JsonPropertyOrder(3)]
    public required int Intrigue { get; init; }

    [JsonPropertyOrder(4)]
    public required int Learning { get; init; }
}

/// <summary>One <see cref="Characters.LaborSkills"/> (<c>gens-familia-design.md</c> §2.2).</summary>
public sealed record LaborSkillsDto
{
    [JsonPropertyOrder(0)]
    public required int Fieldwork { get; init; }

    [JsonPropertyOrder(1)]
    public required int DomesticService { get; init; }

    [JsonPropertyOrder(2)]
    public required int Craft { get; init; }

    [JsonPropertyOrder(3)]
    public required int Culinary { get; init; }

    [JsonPropertyOrder(4)]
    public required int Medicine { get; init; }
}

/// <summary>One <see cref="Characters.Condition"/> (<c>gens-familia-design.md</c> §2.3).</summary>
public sealed record ConditionDto
{
    [JsonPropertyOrder(0)]
    public required int Health { get; init; }

    [JsonPropertyOrder(1)]
    public required int Fatigue { get; init; }

    [JsonPropertyOrder(2)]
    public required int Loyalty { get; init; }

    [JsonPropertyOrder(3)]
    public required int Ambition { get; init; }

    [JsonPropertyOrder(4)]
    public required int Fertility { get; init; }
}
