namespace Gens.Client.Desktop.Saves;

/// <summary>The persisted shape of <c>saves/index.json</c> (ADR 0020): player-facing save-slot
/// metadata (display name, last-saved time, playtime), kept entirely outside the checksummed
/// <c>.gens</c> archive so a player-editable name or a wall-clock timestamp never enters the
/// canonical, hashed save content (ADR 0010).</summary>
public sealed record SaveIndex
{
    public const int CurrentVersion = 1;
    public int Version { get; init; } = CurrentVersion;
    public IReadOnlyList<SaveSlotMetadata> Slots { get; init; } = Array.Empty<SaveSlotMetadata>();
}

/// <summary>One save slot's metadata. <see cref="FileName"/> is always derived from a GUID, never
/// from <see cref="DisplayName"/>, so the player-chosen name can hold arbitrary Unicode without
/// ever touching filesystem-reserved-name or path-traversal concerns.</summary>
public sealed record SaveSlotMetadata
{
    public required string SlotId { get; init; }
    public required string FileName { get; init; }
    public required string DisplayName { get; init; }
    public required bool IsQuicksave { get; init; }
    public DateTimeOffset? LastSavedUtc { get; init; }
    public long PlaytimeSeconds { get; init; }
}
