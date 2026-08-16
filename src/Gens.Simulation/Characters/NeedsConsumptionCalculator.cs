using Gens.Simulation.Identity;
using Gens.Simulation.Numerics;

namespace Gens.Simulation.Characters;

/// <summary>Pure needs-demand/consumption math (<c>gens-settlement-demographics-design.md</c> §6.1),
/// factored out of <see cref="NeedsConsumptionSystem"/> the same way <see
/// cref="BackgroundJobCapacityCalculator"/> sits beside <see cref="JobCapacitySystem"/>.</summary>
public static class NeedsConsumptionCalculator
{
    /// <summary>The good this implementation's own invented consumption figures are denominated in —
    /// grain, the one staple good every sample content set already defines, standing in for §6.1's
    /// unnamed generic "consumption" until Resources &amp; Goods (Phase 8) authors a real per-good
    /// household needs basket.</summary>
    public static readonly DefinitionId<Good> ConsumptionGood = new("grain");

    /// <summary>Monthly per-capita grain-equivalent demand by <see cref="DietTier"/>. No numeric
    /// figure exists anywhere in the design corpus for this — this implementation's own invented
    /// baseline, in the same "documented as invented, pending playtesting" spirit as <see
    /// cref="EmploymentMatchingSystem"/>'s wage table and <see cref="RegimenCatalog"/>'s Diet-axis
    /// Health trends, which this table's own ordering (Meager &lt; Adequate &lt; Generous) matches.</summary>
    private static readonly Dictionary<DietTier, long> PerCapitaMonthlyDemand = new()
    {
        [DietTier.Meager] = 1,
        [DietTier.Adequate] = 2,
        [DietTier.Generous] = 3,
    };

    /// <summary>This group's total monthly consumption demand (§6.1's "demand-side contribution to
    /// Resources &amp; Goods' Market Dynamics") — <c>Size * PerCapitaMonthlyDemand[NeedsProfile]</c>,
    /// ledger-ready groundwork for Phase 8's market to eventually consume, matching <see
    /// cref="BackgroundJobCapacityComputedEvent"/>'s identical "always emitted, always re-derived"
    /// convention. No Stockpile is actually debited here — Resources &amp; Goods doesn't exist yet to
    /// receive this demand, so this is a reported figure only (this class's own doc comment).</summary>
    public static long ComputeDemand(DietTier needsProfile, int size) => PerCapitaMonthlyDemand[needsProfile] * size;

    /// <summary>How well a group's needs are being met, as a <see cref="Fixed64"/> in <c>[0, 1]</c>,
    /// standing in for §6.2's "needs satisfaction" contentment input. Nothing yet checks <see
    /// cref="ComputeDemand"/> against an actual Stockpile (no per-settlement goods pool feeds the
    /// background population — Phase 8's Economy &amp; Finance market is what will eventually clear
    /// this demand against real supply), so this reads <see cref="DietTier"/> itself as the proxy: a
    /// group whose <see cref="NeedsProfile"/> already reflects Meager/Adequate/Generous consumption
    /// is, by construction, "getting" that tier — the figures below are this implementation's own
    /// invented mapping of that tier to a satisfaction level, replaced once Phase 8 can measure actual
    /// shortfall instead of assuming the authored tier is being met.</summary>
    public static Fixed64 SatisfactionFor(DietTier needsProfile) => needsProfile switch
    {
        DietTier.Meager => Fixed64.FromRaw(600_000), // 0.6 — fed, but with no margin.
        DietTier.Adequate => Fixed64.FromRaw(850_000), // 0.85.
        DietTier.Generous => Fixed64.One, // 1.0.
        _ => Fixed64.FromRaw(600_000),
    };
}
