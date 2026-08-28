using Gens.Simulation.Chronicle;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Queries;

/// <summary>One rendered <see cref="ChronicleEntry"/> row (Phase 11 item 3).</summary>
public readonly record struct ChronicleEntryRow(
    string EntryId,
    int MonthTotalMonths,
    ChronicleCategory Category,
    ChronicleTier Tier,
    string Prose,
    IReadOnlyList<string> LinkedCharacterIds,
    string SourceSystem,
    ChronicleEntrySource Source,
    bool Pinned,
    string? PlayerAnnotation);

/// <summary>One head's tenure and every filtered entry that fell inside it (§4's "generational
/// chapters" default read) — <see cref="EndMonthTotalMonths"/> is <c>null</c> for the household's
/// current, still-open chapter.</summary>
public readonly record struct ChronicleChapterRow(
    string HeadCharacterId,
    int StartMonthTotalMonths,
    int? EndMonthTotalMonths,
    string ChapterSummary,
    IReadOnlyList<ChronicleEntryRow> Entries);

/// <summary>Both readings of the same filtered entry set at once (§4: "neither view is the 'real'
/// one; they're the same underlying entries read two different ways") — a caller renders whichever
/// fits the current screen.</summary>
public readonly record struct ChronicleProjection(
    string HouseholdId,
    IReadOnlyList<ChronicleChapterRow> Chapters,
    IReadOnlyList<ChronicleEntryRow> Entries);

/// <summary>
/// Projects one household's Chronicle (Phase 11 item 3), filtered per §3/§4: by default, Minor-tier
/// entries are excluded ("logged, but filtered out of the default read") unless <see
/// cref="_includeMinor"/> is set or the entry is individually <see cref="ChronicleEntry.Pinned"/> (§7:
/// a pin overrides the entry's own tier for the player's personal read regardless of what tier it was
/// assigned). An optional <see cref="_categoryFilter"/> narrows to one category across every chapter
/// at once; an optional <see cref="_pinnedOnly"/> narrows to only pinned entries.
/// </summary>
public sealed class ChronicleQuery : IWorldQuery<ChronicleProjection>
{
    private readonly RuntimeId<Household> _householdId;
    private readonly ChronicleCategory? _categoryFilter;
    private readonly bool _includeMinor;
    private readonly bool _pinnedOnly;

    public ChronicleQuery(
        RuntimeId<Household> householdId,
        ChronicleCategory? categoryFilter = null,
        bool includeMinor = false,
        bool pinnedOnly = false)
    {
        _householdId = householdId;
        _categoryFilter = categoryFilter;
        _includeMinor = includeMinor;
        _pinnedOnly = pinnedOnly;
    }

    public ChronicleProjection Execute(WorldState state, string observerId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var entries = new List<ChronicleEntryRow>();
        foreach (var kvp in state.ChronicleEntries.InAscendingOrder())
        {
            var entry = kvp.Value;
            if (entry.HouseholdId != _householdId)
                continue;
            if (!entry.Pinned && !_includeMinor && entry.Tier == ChronicleTier.Minor)
                continue;
            if (_categoryFilter is { } category && entry.Category != category)
                continue;
            if (_pinnedOnly && !entry.Pinned)
                continue;

            entries.Add(ToRow(entry));
        }

        entries.Sort(static (a, b) => a.MonthTotalMonths.CompareTo(b.MonthTotalMonths));

        var chapters = new List<ChronicleChapterRow>();
        foreach (var kvp in state.GenerationalChapters.InAscendingOrder())
        {
            if (kvp.Key.HouseholdId != _householdId)
                continue;

            var chapter = kvp.Value;
            var chapterEntries = entries
                .Where(row => row.MonthTotalMonths >= chapter.StartMonth.TotalMonths &&
                              (chapter.EndMonth is null || row.MonthTotalMonths < chapter.EndMonth.Value.TotalMonths))
                .ToArray();

            chapters.Add(new ChronicleChapterRow(
                chapter.HeadCharacterId.ToTaggedString(),
                chapter.StartMonth.TotalMonths,
                chapter.EndMonth?.TotalMonths,
                chapter.ChapterSummary,
                chapterEntries));
        }

        return new ChronicleProjection(_householdId.ToTaggedString(), chapters, entries);
    }

    private static ChronicleEntryRow ToRow(ChronicleEntry entry) => new(
        entry.EntryId.ToTaggedString(),
        entry.Month.TotalMonths,
        entry.Category,
        entry.Tier,
        entry.Prose,
        entry.LinkedCharacterIds.Select(id => id.ToTaggedString()).ToArray(),
        entry.SourceSystem,
        entry.Source,
        entry.Pinned,
        entry.PlayerAnnotation);
}
