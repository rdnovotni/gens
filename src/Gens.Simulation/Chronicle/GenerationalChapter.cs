using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Chronicle;

/// <summary>Composite key for <see cref="GenerationalChapter"/>: a household can accumulate many
/// chapters over a long campaign, one per head's tenure (§4), so — unlike every other per-household
/// partition in <see cref="State.WorldState"/> — this one is keyed by (household, start month) rather
/// than household alone.</summary>
public readonly record struct GenerationalChapterKey(RuntimeId<Household> HouseholdId, int StartMonthTotalMonths)
    : IComparable<GenerationalChapterKey>
{
    public int CompareTo(GenerationalChapterKey other)
    {
        var householdComparison = HouseholdId.CompareTo(other.HouseholdId);
        return householdComparison != 0 ? householdComparison : StartMonthTotalMonths.CompareTo(other.StartMonthTotalMonths);
    }
}

/// <summary>
/// One head's tenure, read as its own chapter of the household record (Phase 11 item 3; §4's "default
/// read is generational chapters — each head's tenure is its own chapter... opening with a short
/// summary of how they came to hold the position"). <see cref="EndMonth"/> is <c>null</c> for the
/// household's current, still-open chapter. Immutable: <see cref="ChronicleGenerationSystem"/> closes
/// an open chapter by removing and re-adding it under the same key with <see cref="EndMonth"/> set,
/// matching <see cref="Succession.HouseholdHeadship"/>'s identical convention.
/// </summary>
public sealed record GenerationalChapter(
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> HeadCharacterId,
    GameDate StartMonth,
    GameDate? EndMonth,
    string ChapterSummary);
