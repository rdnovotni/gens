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
    public static Fixed64 ComputeContentment(Fixed64 employmentRatio, Fixed64 housingSatisfaction, Fixed64 needsSatisfaction) =>
        ComputeContentment(employmentRatio, housingSatisfaction, needsSatisfaction, rentBurden: Fixed64.Zero);

    /// <summary>Phase 15 item 1's overload (<c>gens-land-ownership-real-estate-design.md</c> §10): the
    /// same three-way average above, with <paramref name="rentBurden"/> — <see
    /// cref="RealEstate.DistrictRentBurdenCalculator.ComputeRentBurden"/>'s own read of a lower-tier
    /// resident pop group's District Property Value exposure — subtracted afterward and floored at
    /// zero. §10's own explicit framing ("this document adds no new tracked displacement mechanic...
    /// feeds directly into Settlement Demographics' existing Contentment... formula as a new input")
    /// is why this is an added term on the existing formula rather than a parallel calculation: a pop
    /// group this item never touches (every group outside a Districted settlement, or a group <see
    /// cref="Characters.ContentmentSystem"/> never flags as rent-exposed) always passes <see
    /// cref="Fixed64.Zero"/> here and reads identically to before this item shipped.</summary>
    public static Fixed64 ComputeContentment(
        Fixed64 employmentRatio, Fixed64 housingSatisfaction, Fixed64 needsSatisfaction, Fixed64 rentBurden) =>
        ComputeContentment(employmentRatio, housingSatisfaction, needsSatisfaction, rentBurden, civicInfrastructureBonus: Fixed64.Zero);

    /// <summary>Phase 15 item 9's overload (<c>gens-public-works-euergetism-design.md</c> §3): the same
    /// formula above, with <paramref name="civicInfrastructureBonus"/> — <see
    /// cref="PublicWorks.PublicWorksContentmentQuery.CivicInfrastructureBonus"/>'s own read of an
    /// operational Sewer Public Work at the settlement — added afterward and capped at <see
    /// cref="Fixed64.One"/>, matching <paramref name="rentBurden"/>'s own identical "an added term on the
    /// existing formula rather than a parallel calculation" precedent. Every pre-item-9 call site passes
    /// <see cref="Fixed64.Zero"/> here and reads identically to before this item shipped.</summary>
    public static Fixed64 ComputeContentment(
        Fixed64 employmentRatio, Fixed64 housingSatisfaction, Fixed64 needsSatisfaction, Fixed64 rentBurden, Fixed64 civicInfrastructureBonus)
    {
        var cappedEmployment = Min(employmentRatio, Fixed64.One);
        var cappedHousing = Min(housingSatisfaction, Fixed64.One);
        var sum = cappedEmployment + cappedHousing + needsSatisfaction;
        var baseline = Fixed64.Divide(sum, Fixed64.FromInt(3));
        var withBurden = baseline - rentBurden;
        var floored = withBurden < Fixed64.Zero ? Fixed64.Zero : withBurden;
        var withBonus = floored + civicInfrastructureBonus;
        return withBonus > Fixed64.One ? Fixed64.One : withBonus;
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
