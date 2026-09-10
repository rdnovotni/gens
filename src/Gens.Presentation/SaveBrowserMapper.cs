#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Gens.Presentation.Models;

namespace Gens.Presentation;

/// <summary>The input row a save browser is built from — deliberately not the desktop client's own
/// <c>SaveSlotMetadata</c> type, since <c>Gens.Presentation</c> must never take a dependency in the
/// adapter → desktop-client direction (only desktop-client → presentation is allowed). The desktop
/// client maps its own slot index into this shape before calling <see cref="SaveBrowserMapper.ToModel"/>.</summary>
public sealed record SaveSlotSource(string SlotId, string DisplayName, bool IsQuicksave, bool Exists, DateTimeOffset? LastSavedUtc, long PlaytimeSeconds);

/// <summary>Pure mapping from a set of save-slot sources to the save browser's screen model — no file
/// I/O, matching <see cref="ProjectionMappers"/>'s role, since there is no active <see
/// cref="CampaignPresentation"/>/<c>CampaignSession</c> to query when a player is browsing saves from
/// the main menu.</summary>
public static class SaveBrowserMapper
{
    public static SaveBrowserModel ToModel(IEnumerable<SaveSlotSource> slots)
    {
        if (slots is null) throw new ArgumentNullException(nameof(slots));
        List<SaveSlotRowModel> rows = slots
            .OrderByDescending(static s => s.LastSavedUtc ?? DateTimeOffset.MinValue)
            .Select(static s => new SaveSlotRowModel(s.SlotId, s.DisplayName, s.IsQuicksave, s.Exists, FormatLastSaved(s.LastSavedUtc), FormatPlaytime(s.PlaytimeSeconds, s.LastSavedUtc)))
            .ToList();
        return new SaveBrowserModel(rows);
    }

    private static string FormatLastSaved(DateTimeOffset? lastSavedUtc) =>
        lastSavedUtc is { } value ? value.ToLocalTime().ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture) : "Never saved";

    private static string FormatPlaytime(long playtimeSeconds, DateTimeOffset? lastSavedUtc)
    {
        if (lastSavedUtc is null) return string.Empty;
        if (playtimeSeconds <= 0) return "Playtime not tracked before this update";
        TimeSpan span = TimeSpan.FromSeconds(playtimeSeconds);
        return span.TotalHours >= 1 ? $"{(int)span.TotalHours}h {span.Minutes}m played" : $"{span.Minutes}m played";
    }
}
