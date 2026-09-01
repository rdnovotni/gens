namespace Gens.Simulation.Health;

/// <summary>Pure, RNG-free monthly affliction-probability math for §2's Endemic Illness layer, one
/// function per <see cref="EndemicExposureDriver"/> — extending <see
/// cref="HealthConditionProgressionCalculator"/>'s own "documented as invented, pending playtesting"
/// precedent to Exposure itself. No Exposure curve exists anywhere in the design corpus (§12's own "All
/// numeric sizing" open question, same citation item 1's progression calculator already used) — every
/// figure below is this implementation's own invented number, deliberately small (a fraction of a
/// percent per Character per month at typical inputs) so that seven simultaneous endemic rolls per
/// living Character every month (<see cref="EndemicIllnessSystem"/>) do not turn every settlement into
/// a constant hospital ward. Every probability is scaled by <see
/// cref="SanitationInvestmentCalculator.ExposureMultiplier"/> by the caller, not here, so this file
/// never needs to know about Sanitation Investment's own tier enum.</summary>
public static class EndemicExposureCalculator
{
    private const double MaxMonthlyProbability = 0.06;

    /// <summary>Roman Fever (§2's Marsh/Poor-land terrain driver). <paramref name="marshFraction"/> is
    /// the settlement's own Marsh-terrain <see cref="Land.Plot"/> share (0-1).</summary>
    public static double RomanFeverMonthlyProbability(double marshFraction) =>
        Clamp(0.05 * marshFraction);

    /// <summary>The Flux (§2's poor-sanitation driver). No Public Latrines/Fountains or Aqueduct/
    /// Cistern building exists in this codebase yet (this file's own top-level and <see
    /// cref="EndemicExposureDriver"/>'s doc comments disclose this), so the only real mitigating input
    /// today is the Sanitation Investment tier the caller folds in afterward — this function's own
    /// baseline models "poor sanitation" as the default, unmitigated state.</summary>
    public static double TheFluxMonthlyProbability() => Clamp(0.012);

    /// <summary>Consumption (§2's population-density driver). <paramref name="crowdingRatio"/> is a
    /// settlement's living population (named Characters plus background <see
    /// cref="Characters.PopGroup"/> headcount) divided by its total Plot capacity — the closest real
    /// proxy this codebase has to Settlement Demographics' own not-yet-built Overcrowding/Insulae
    /// mechanic (§10's own cross-system note), disclosed here rather than faked as a real Overcrowding
    /// score.</summary>
    public static double ConsumptionMonthlyProbability(double crowdingRatio) =>
        Clamp(0.008 * Math.Max(0.0, crowdingRatio));

    /// <summary>Leprosy (§2's own "not terrain-driven at all... purely a matter of exposure and time").
    /// A flat, deliberately rare baseline with no driver at all, matching the design doc's own framing
    /// literally.</summary>
    public static double LeprosyMonthlyProbability() => Clamp(0.0015);

    /// <summary>Gout (§2's wealth driver — "a household or individual sustained at Lavish consumption
    /// tier... heavy Wine intake specifically"). <paramref name="settlementIsLavish"/> is whether any
    /// <see cref="Characters.PopGroup"/> at the settlement sits at <see
    /// cref="Characters.WealthBand.EliteDiscretionary"/> with <see cref="Characters.DietTier.Generous"/>
    /// — a settlement-level proxy, not a per-Character wealth check, since named Characters carry no
    /// wealth/diet field of their own; disclosed as a deliberate simplification rather than a precise
    /// per-household Lavish-tier gate.</summary>
    public static double GoutMonthlyProbability(bool settlementIsLavish) =>
        Clamp(settlementIsLavish ? 0.02 : 0.001);

    /// <summary>Ophthalmia (§2's arid/dust-region driver). No Region in this codebase carries a real,
    /// gameplay-reachable arid/dust flag yet (<see cref="EndemicExposureDriver.RegionalFlavorUnmodeled"/>'s
    /// own doc comment) — this is a flat, region-agnostic baseline standing in for that eventual driver,
    /// not a faked regional gate.</summary>
    public static double OphthalmiaMonthlyProbability() => Clamp(0.006);

    /// <summary>Saturnism (§2's own two-unrelated-drivers shape). <paramref name="settlementIsLavish"/>
    /// mirrors <see cref="GoutMonthlyProbability"/>'s own wealth proxy (elite lead-sweetened wine/
    /// cookware); <paramref name="hillsFraction"/> is the settlement's own Hills-terrain <see
    /// cref="Land.Plot"/> share, standing in for "living in or operating a Mine in the Iberian colony
    /// specifically" (§2) — this codebase has no Mine building and no reachable Iberian-region flag yet
    /// (<see cref="DiseaseCatalog"/>'s own top-level doc comment discloses this widening explicitly), so
    /// Hills terrain anywhere is used as the honest, broader proxy for mining-adjacent occupational
    /// exposure. The two drivers are independent (an OR, not a sum): a settlement can be both lavish and
    /// hilly without doubling this specific roll, matching §2's "two entirely unrelated real drivers"
    /// framing — only the larger of the two probabilities applies.</summary>
    public static double SaturnismMonthlyProbability(bool settlementIsLavish, double hillsFraction)
    {
        var wealthDriven = settlementIsLavish ? 0.015 : 0.0;
        var miningDriven = 0.03 * Math.Max(0.0, hillsFraction);
        return Clamp(Math.Max(wealthDriven, miningDriven));
    }

    private static double Clamp(double probability) => Math.Clamp(probability, 0.0, MaxMonthlyProbability);
}
