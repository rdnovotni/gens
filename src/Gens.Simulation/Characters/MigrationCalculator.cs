using Gens.Simulation.Numerics;

namespace Gens.Simulation.Characters;

/// <summary>
/// Pure immigration/emigration math (<c>gens-settlement-demographics-design.md</c> §8), factored out
/// of <see cref="MigrationSystem"/> the same way <see cref="BackgroundJobCapacityCalculator"/> sits
/// beside <see cref="JobCapacitySystem"/>. Every constant below is this implementation's own invented
/// figure — §8 names the qualitative drivers (Contentment, capacity, Employment Ratio for
/// immigration; sustained low Contentment, famine, disease, or war for emigration) but no numeric
/// rate, matching every other Phase 7 system's "documented as invented" convention.
/// </summary>
public static class MigrationCalculator
{
    /// <summary>Below this Contentment, a group starts losing population to emigration (§8.2).</summary>
    private static readonly Fixed64 EmigrationContentmentThreshold = Fixed64.FromRaw(350_000); // 0.35.

    /// <summary>The emigration rate at Contentment = 0 — a group in true crisis loses at most 3% of
    /// its population per month; the rate scales linearly down to zero as Contentment rises to <see
    /// cref="EmigrationContentmentThreshold"/>.</summary>
    private static readonly Fixed64 MaxEmigrationRate = Fixed64.FromRaw(30_000); // 0.03.

    /// <summary>Above this settlement-wide Contentment, immigration starts pulling new population in
    /// (§8.1).</summary>
    private static readonly Fixed64 ImmigrationContentmentThreshold = Fixed64.FromRaw(600_000); // 0.6.

    /// <summary>The immigration rate at Contentment = 1.0 (full pull) — at most 1% of the settlement's
    /// total population arrives per month; the rate scales linearly up from zero as Contentment rises
    /// past <see cref="ImmigrationContentmentThreshold"/>.</summary>
    private static readonly Fixed64 MaxImmigrationRate = Fixed64.FromRaw(10_000); // 0.01.

    /// <summary>§8.2's sustained-low-Contentment push, simplified to this month's own Contentment
    /// reading — a true "sustained" trigger would need multi-month Contentment history, which no <see
    /// cref="PopGroup"/> field currently tracks; left as a documented simplification for whenever that
    /// history exists. §8.2's famine/disease/war inputs have no corresponding systems yet either
    /// (Natural Disasters/Disease, Military &amp; Combat) — Contentment alone (already depressed by
    /// low needs satisfaction or overcrowding per §6.2/§7.3) is the only "hardship" signal available
    /// today, and this is that general hook.</summary>
    public static Fixed64 EmigrationRate(Fixed64 contentment)
    {
        if (contentment >= EmigrationContentmentThreshold)
            return Fixed64.Zero;
        var shortfall = EmigrationContentmentThreshold - contentment;
        var normalized = Fixed64.Divide(shortfall, EmigrationContentmentThreshold);
        return Fixed64.Multiply(normalized, MaxEmigrationRate);
    }

    /// <summary>§8.1's ambient pull: ordinary immigration only flows when the settlement is both
    /// content (<paramref name="settlementContentment"/>) and actually has room for a new arrival at
    /// its default Coloni/Operarii entry point — a favorable Employment Ratio (<paramref
    /// name="entryEmploymentRatio"/> &gt;= 1, open job slots) and available housing/land (<paramref
    /// name="entryHousingSatisfaction"/> &gt;= 1, uncrowded). Either gate failing yields zero: a
    /// content settlement with no jobs or no housing left doesn't pull people in just because it's
    /// pleasant, matching §8.1's "driven by Contentment, available capacity, and favorable Employment
    /// Ratios" — all three, not any one alone.</summary>
    public static Fixed64 ImmigrationRate(Fixed64 settlementContentment, Fixed64 entryEmploymentRatio, Fixed64 entryHousingSatisfaction)
    {
        if (settlementContentment < ImmigrationContentmentThreshold)
            return Fixed64.Zero;
        if (entryEmploymentRatio < Fixed64.One || entryHousingSatisfaction < Fixed64.One)
            return Fixed64.Zero;

        var excess = settlementContentment - ImmigrationContentmentThreshold;
        var headroom = Fixed64.One - ImmigrationContentmentThreshold;
        var normalized = Fixed64.Divide(excess, headroom);
        return Fixed64.Multiply(normalized, MaxImmigrationRate);
    }
}
