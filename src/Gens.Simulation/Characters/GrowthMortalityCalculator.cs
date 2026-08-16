using Gens.Simulation.Numerics;

namespace Gens.Simulation.Characters;

/// <summary>
/// Pure background-population growth/mortality math, extending <see cref="MortalityCalculator"/>'s
/// pattern (Phase 5 item 3's named-character curve) to the aggregate <see cref="PopGroup"/> tier
/// (Roadmap Phase 7 item 4). No numeric growth/mortality curve for background pops exists anywhere in
/// the design corpus — every constant below is this implementation's own invented figure, chosen only
/// so that high Contentment yields modest positive growth, low Contentment or high HealthExposure
/// yields net decline, and neither runs away to an extreme in a single month, matching <see
/// cref="MortalityCalculator"/>'s own "documented as invented, pending playtesting" framing.
/// </summary>
public static class GrowthMortalityCalculator
{
    /// <summary>Contentment at which the growth/mortality modifier is neutral, the same "midpoint is
    /// neutral" shape <see cref="MortalityCalculator"/>'s own Health factor uses.</summary>
    private static readonly Fixed64 NeutralContentment = Fixed64.FromRaw(500_000); // 0.5.

    /// <summary>A small baseline natural-increase rate applied even at neutral Contentment (births
    /// modestly outpacing deaths absent any other pressure) — 0.1%/month, roughly 1.2%/year.</summary>
    private static readonly Fixed64 BaselineMonthlyRate = Fixed64.FromRaw(1_000);

    /// <summary>How strongly Contentment above/below neutral pushes the rate — at maximum Contentment
    /// (1.0, a full 0.5 above neutral) this alone contributes +1.0%/month; at zero Contentment it
    /// contributes -1.0%/month.</summary>
    private static readonly Fixed64 ContentmentSensitivity = Fixed64.FromRaw(20_000); // 0.02.

    /// <summary>How strongly HealthExposure (§7.3 overcrowding) drags the rate down — at maximum
    /// exposure (1.0) this alone contributes -1.5%/month, on top of whatever Contentment already did.</summary>
    private static readonly Fixed64 HealthExposureSensitivity = Fixed64.FromRaw(15_000); // 0.015.

    /// <summary>Clamped so no single month's shock (however low Contentment or high HealthExposure)
    /// can more than halve or double a group inside a handful of months.</summary>
    private static readonly Fixed64 MinMonthlyRate = Fixed64.FromRaw(-20_000); // -2%/month.
    private static readonly Fixed64 MaxMonthlyRate = Fixed64.FromRaw(15_000); // +1.5%/month.

    /// <summary>This month's net growth (positive) or mortality (negative) rate for one <see
    /// cref="PopGroup"/>, from its already-computed <see cref="PopGroup.Contentment"/> and <see
    /// cref="PopGroup.HealthExposure"/>.</summary>
    public static Fixed64 MonthlyNetRate(Fixed64 contentment, Fixed64 healthExposure)
    {
        var contentmentTerm = Fixed64.Multiply(contentment - NeutralContentment, ContentmentSensitivity);
        var healthTerm = Fixed64.Multiply(healthExposure, HealthExposureSensitivity);
        var rate = BaselineMonthlyRate + contentmentTerm - healthTerm;

        if (rate < MinMonthlyRate)
            return MinMonthlyRate;
        return rate > MaxMonthlyRate ? MaxMonthlyRate : rate;
    }
}
