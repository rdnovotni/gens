using Gens.Simulation.Numerics;

namespace Gens.Simulation.Characters;

/// <summary>
/// Pure social-mobility math (<c>gens-settlement-demographics-design.md</c> §5), factored out of <see
/// cref="SocialMobilitySystem"/> the same way <see cref="BackgroundJobCapacityCalculator"/> sits beside
/// <see cref="JobCapacitySystem"/>. Every rate below is this implementation's own invented figure — §5
/// names the qualitative drivers (relative Employment Ratios, wealth accumulation) but no numeric rate,
/// matching every other Phase 7 system's "documented as invented" convention.
/// </summary>
public static class SocialMobilityCalculator
{
    /// <summary>How much higher a neighboring group's Employment Ratio must be, relative to the
    /// source group's own, before any population is pulled toward it (§5's Coloni⇄Operarii,
    /// Operarii⇄Opifices, Opifices⇄Negotiatores links) — a 20% edge, so ordinary month-to-month noise
    /// doesn't trigger constant back-and-forth churn.</summary>
    private static readonly Fixed64 EmploymentPullThreshold = Fixed64.FromRaw(1_200_000); // 1.2x.

    /// <summary>How much of the losing group's Size moves per month once the pull threshold is met —
    /// a slow drift, not a mass exodus.</summary>
    private static readonly Fixed64 EmploymentPullRate = Fixed64.FromRaw(20_000); // 0.02.

    /// <summary>Monthly share of an <see cref="WealthBand.EliteDiscretionary"/> Negotiatores/Opifices
    /// group that rises into Curiales, gated on Curiales itself having open capacity (its own
    /// Employment Ratio &gt;= 1, i.e., unfilled slots) — §5's "wealth accumulation; the upward
    /// ceiling."</summary>
    private static readonly Fixed64 WealthCeilingRate = Fixed64.FromRaw(10_000); // 0.01.

    /// <summary>Monthly share of Curiales that drifts back down to Operarii/Coloni — §5's own "rare —
    /// bankruptcy, scandal" framing, so this is deliberately the smallest rate here.</summary>
    private static readonly Fixed64 CurialesAttritionRate = Fixed64.FromRaw(2_000); // 0.002.

    /// <summary>Monthly share of the Non-Household Enslaved cohort manumitted into free background
    /// labor (§5, §9's "shrinks via ambient manumission by other owners").</summary>
    private static readonly Fixed64 ManumissionRate = Fixed64.FromRaw(5_000); // 0.005.

    /// <summary>Monthly share of Veterans that drifts back to Coloni "absent a recruitment drive" (§5's
    /// own phrase) — the ordinary, non-military-gated half of the Veterans loop this Phase 7 item
    /// implements; see <see cref="SocialMobilitySystem"/>'s own doc comment for the military-gated half
    /// this deliberately leaves as a hook.</summary>
    private static readonly Fixed64 VeteranDriftRate = Fixed64.FromRaw(15_000); // 0.015.

    /// <summary>How many of <paramref name="sourceSize"/> move toward a neighboring group whose
    /// Employment Ratio is favorable enough, or zero if the pull threshold isn't met.</summary>
    public static int EmploymentPullCount(int sourceSize, Fixed64 sourceEmploymentRatio, Fixed64 destEmploymentRatio)
    {
        if (sourceSize <= 0)
            return 0;
        var required = Fixed64.Multiply(sourceEmploymentRatio, EmploymentPullThreshold);
        if (destEmploymentRatio < required)
            return 0;
        return FloorCount(sourceSize, EmploymentPullRate);
    }

    /// <summary>How many of an EliteDiscretionary Negotiatores/Opifices group rises into Curiales this
    /// month, or zero if Curiales itself has no open capacity.</summary>
    public static int WealthCeilingCount(int sourceSize, WealthBand sourceWealthBand, Fixed64 curialesEmploymentRatio)
    {
        if (sourceSize <= 0 || sourceWealthBand != WealthBand.EliteDiscretionary || curialesEmploymentRatio < Fixed64.One)
            return 0;
        return FloorCount(sourceSize, WealthCeilingRate);
    }

    public static int CurialesAttritionCount(int curialesSize) => FloorCount(curialesSize, CurialesAttritionRate);

    public static int ManumissionCount(int enslavedSize) => FloorCount(enslavedSize, ManumissionRate);

    public static int VeteranDriftCount(int veteransSize) => FloorCount(veteransSize, VeteranDriftRate);

    /// <summary>Whole-unit floor of <c>size * rate</c> — deliberately not <see
    /// cref="StochasticRounding"/>'s probabilistic remainder: §5 frames every one of these pathways as
    /// a steady, deterministic drift rather than a per-month roll, so a fractional remainder below one
    /// whole person is simply dropped, the same plain-integer-math convention <see
    /// cref="JobCapacitySystem"/> and <see cref="EmploymentMatchingSystem"/> already use.</summary>
    private static int FloorCount(int size, Fixed64 rate)
    {
        if (size <= 0)
            return 0;
        var raw = Fixed64.Multiply(Fixed64.FromInt(size), rate).RawValue;
        return (int)(raw / Fixed64.ScaleFactor);
    }
}
