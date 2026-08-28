using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Chronicle;

/// <summary>Emitted whenever <see cref="ChronicleGenerationSystem"/> records a new <see
/// cref="ChronicleEntry"/> from a domain event (Phase 11 item 3). <see cref="CausationId"/> is the
/// source event's own <see cref="IDomainEvent.EventId"/> — traceability back to whichever system
/// actually generated the entry (§2's "source reference" field).</summary>
public sealed record ChronicleEntryRecordedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<ChronicleEntry> EntryId,
    RuntimeId<Household>? HouseholdId,
    ChronicleTier Tier,
    string? CausationId) : IDomainEvent
{
    public string Type => "chronicle.entryRecorded";
    public int SchemaVersion => 1;

    public IReadOnlyList<string> SubjectIds => HouseholdId is { } household
        ? new[] { EntryId.ToTaggedString(), household.ToTaggedString() }
        : new[] { EntryId.ToTaggedString() };

    public Visibility Visibility => Visibility.Public;
}

/// <summary>Emitted whenever a player pins or unpins an entry (§7) — purely personal, never changes
/// any other system's own reading of the entry's tier.</summary>
public sealed record ChronicleEntryPinnedChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<ChronicleEntry> EntryId,
    bool Pinned,
    string? CausationId) : IDomainEvent
{
    public string Type => "chronicle.entryPinnedChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { EntryId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>Emitted whenever a player attaches or clears a free-text annotation on an entry (§7).</summary>
public sealed record ChronicleEntryAnnotatedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<ChronicleEntry> EntryId,
    string? CausationId) : IDomainEvent
{
    public string Type => "chronicle.entryAnnotated";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { EntryId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>Emitted whenever the player adds a personal diary-style note directly (§7's "personal
/// note" entry type, distinguished from a system-generated entry by <see
/// cref="ChronicleEntry.Source"/>).</summary>
public sealed record ChronicleNoteAddedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<ChronicleEntry> EntryId,
    RuntimeId<Household> HouseholdId,
    string? CausationId) : IDomainEvent
{
    public string Type => "chronicle.noteAdded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { EntryId.ToTaggedString(), HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}
