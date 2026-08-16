using Gens.Simulation.Numerics;

namespace Gens.Simulation.Characters;

/// <summary>Pure contentment math (<c>gens-settlement-demographics-design.md</c> §6.2), factored out
/// of <see cref="ContentmentSystem"/> the same way <see cref="BackgroundJobCapacityCalculator"/> sits
/// beside <see cref="JobCapacitySystem"/>.</summary>
public static class ContentmentCalculator
{
    /// <summary>§6.2 names Employment Ratio, needs satisfaction, and housing as Contentment's inputs
    /// but specifies no formula — this implementation's own invented equal-weighted average of the
    /// three (Employment Ratio and Housing Satisfaction each capped at <see cref="Fixed64.One"/> first,
    /// so an oversupplied job market or empty Insula can't push Contentment past "fully satisfied" on
    /// its own). Policies &amp; Edicts' Annona/Games levers (§6.3) and Natural Disasters/Disease
    /// inputs are deliberately not modeled here — this document supplies the number those future
    /// systems will adjust, per §6.2's own "this document supplies the number, not the political
    /// consequences" framing.</summary>
    public static Fixed64 ComputeContentment(Fixed64 employmentRatio, Fixed64 housingSatisfaction, Fixed64 needsSatisfaction)
    {
        var cappedEmployment = Min(employmentRatio, Fixed64.One);
        var cappedHousing = Min(housingSatisfaction, Fixed64.One);
        var sum = cappedEmployment + cappedHousing + needsSatisfaction;
        return Fixed64.Divide(sum, Fixed64.FromInt(3));
    }

    /// <summary>§7.3's overcrowding cross-reference: a group whose housing capacity has fallen short
    /// of its size (<paramref name="housingSatisfaction"/> below 1.0) carries elevated
    /// disease/public-health risk proportional to the shortfall — this implementation's own invented
    /// 1:1 mapping (a housing pool at exactly half capacity reads as 0.5 HealthExposure), since no
    /// numeric overcrowding-to-exposure curve exists in the design corpus. Never negative: adequate or
    /// surplus housing contributes zero exposure rather than a "credit."</summary>
    public static Fixed64 ComputeHealthExposure(Fixed64 housingSatisfaction) =>
        housingSatisfaction >= Fixed64.One ? Fixed64.Zero : Fixed64.One - housingSatisfaction;

    private static Fixed64 Min(Fixed64 a, Fixed64 b) => a < b ? a : b;
}
