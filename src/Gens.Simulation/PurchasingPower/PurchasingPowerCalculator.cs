using Gens.Simulation.Characters;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.NotableBusinesses;
using Gens.Simulation.Numerics;
using Gens.Simulation.RealEstate;

namespace Gens.Simulation.PurchasingPower;

/// <summary>Pure Population Wealth &amp; Purchasing Power math, factored out of <see
/// cref="AggregatePurchasingPowerSystem"/> and <see cref="BusinessViabilitySystem"/> the same way <see
/// cref="Characters.ContentmentCalculator"/> sits beside <see cref="Characters.ContentmentSystem"/>.</summary>
public static class PurchasingPowerCalculator
{
    /// <summary>
    /// §2's Wealth Pyramid, read directly onto <see cref="PopGroupType"/> and <see
    /// cref="PopGroup.EmploymentRatio"/> — no new population data invented, per §2's own "mapped
    /// directly onto Settlement Demographics' own existing eight pop groups" framing. An unfavorable
    /// Employment Ratio (below 1.0) forces <see cref="WealthBand.Subsistence"/> regardless of group
    /// type first, matching §2's own literal "and any pop group currently reading an unfavorable
    /// Employment Ratio" clause — a real, meaningful downgrade (an unemployed Curiales household has no
    /// real discretionary spending left either) rather than a type-locked mapping. Veterani and the
    /// Non-Household Enslaved cohort are not named anywhere in §2's own three bullets; this item reads
    /// Veterani (land-tenant discharged soldiers) as economically closest to Coloni and the wholly
    /// enslaved cohort as carrying literally zero discretionary spending — both a real, disclosed
    /// reading rather than a literal §2 citation, since §2 only enumerates six of the eight groups.
    /// </summary>
    public static WealthBand ClassifyPopGroup(PopGroupType groupType, Fixed64 employmentRatio)
    {
        if (employmentRatio < Fixed64.One)
            return WealthBand.Subsistence;

        return groupType switch
        {
            PopGroupType.Coloni => WealthBand.Subsistence,
            PopGroupType.Operarii => WealthBand.Subsistence,
            PopGroupType.Veterans => WealthBand.Subsistence,
            PopGroupType.NonHouseholdEnslaved => WealthBand.Subsistence,
            PopGroupType.Opifices => WealthBand.ModestSurplus,
            PopGroupType.Negotiatores => WealthBand.ModestSurplus,
            PopGroupType.Aeditui => WealthBand.EliteDiscretionary,
            PopGroupType.Curiales => WealthBand.EliteDiscretionary,
            _ => WealthBand.Subsistence,
        };
    }

    /// <summary>§3's own weighted aggregate — three raw per-tier population counts folded into a single
    /// <see cref="AggregateDemandReading"/> via <see cref="PurchasingPowerCatalog"/>'s own per-capita
    /// weights, deliberately <i>not</i> a plain population-times-average-wealth calculation per §3's own
    /// explicit rejection of that shape. <see cref="AggregateDemandReading.TotalDemandIndex"/> is the
    /// weighted sum divided by total population — an average per-capita demand reading, comparable
    /// across settlements of different sizes.</summary>
    public static AggregateDemandReading ComputeAggregateDemand(
        RuntimeId<Settlement> settlementId, int subsistencePopulation, int modestSurplusPopulation, int eliteDiscretionaryPopulation)
    {
        if (subsistencePopulation < 0 || modestSurplusPopulation < 0 || eliteDiscretionaryPopulation < 0)
            throw new ArgumentOutOfRangeException(nameof(subsistencePopulation), "Population counts cannot be negative.");

        var subsistenceWeight = Fixed64.Multiply(PurchasingPowerCatalog.SubsistenceDemandWeight, Fixed64.FromInt(subsistencePopulation));
        var modestSurplusWeight = Fixed64.Multiply(PurchasingPowerCatalog.ModestSurplusDemandWeight, Fixed64.FromInt(modestSurplusPopulation));
        var eliteDiscretionaryWeight = Fixed64.Multiply(PurchasingPowerCatalog.EliteDiscretionaryDemandWeight, Fixed64.FromInt(eliteDiscretionaryPopulation));

        var totalPopulation = subsistencePopulation + modestSurplusPopulation + eliteDiscretionaryPopulation;
        var totalWeight = subsistenceWeight + modestSurplusWeight + eliteDiscretionaryWeight;
        var totalDemandIndex = totalPopulation > 0
            ? Fixed64.Divide(totalWeight, Fixed64.FromInt(totalPopulation))
            : PurchasingPowerCatalog.NeutralDemandIndex;

        return new AggregateDemandReading(
            settlementId, subsistencePopulation, modestSurplusPopulation, eliteDiscretionaryPopulation,
            subsistenceWeight, modestSurplusWeight, eliteDiscretionaryWeight, totalDemandIndex);
    }

    /// <summary>
    /// §5/§7's own good-tier classification. Resources &amp; Goods' own content registry authors a
    /// <c>category</c> field (<c>raw-materials</c> / <c>intermediate</c> / <c>finished</c> / <c>luxury</c>
    /// / <c>imported</c> — <c>content/schemas/goods.schema.json</c>) but no C# type anywhere in this
    /// codebase actually reads it (confirmed by direct search of <see cref="GoodDefinition"/>, which
    /// carries only <see cref="GoodDefinition.Perishability"/>/<see cref="GoodDefinition.QualityEligible"/>/
    /// <see cref="GoodDefinition.ConditionTracked"/>/<see cref="GoodDefinition.ShelfLifeTicks"/>), and no
    /// content good is actually authored under the <c>luxury</c> or <c>imported</c> category today
    /// (confirmed by direct search of every file under <c>content/source/goods/</c> — six goods total,
    /// none tagged either way). This item cannot honestly classify goods by that unused field, so it
    /// instead reuses <see cref="Characters.NeedsConsumptionCalculator.ConsumptionGood"/> directly — the
    /// one real, already-established "the subsistence good" this codebase tracks, the same proxy <see
    /// cref="BusinessCompetition.GrainHoardingResolver.IsGrainTrading"/> already reads for an identical
    /// question. Every other good defaults to <see cref="WealthBand.ModestSurplus"/>; no good reads as
    /// <see cref="WealthBand.EliteDiscretionary"/> today since none exists to classify that way — a real,
    /// disclosed gap this function will need revisiting once Resources &amp; Goods authors a genuine
    /// luxury/imported good.
    /// </summary>
    public static WealthBand GetGoodTier(DefinitionId<Good> goodId) =>
        goodId == NeedsConsumptionCalculator.ConsumptionGood ? WealthBand.Subsistence : WealthBand.ModestSurplus;

    /// <summary>
    /// §7's Business Viability — "whether its Output actually matches the Purchasing Power tier
    /// genuinely available in its own District." A Subsistence-tier Output always matches: §7's own "a
    /// bakery serving Subsistence-tier bread can thrive almost anywhere a real population exists at all."
    /// A Modest Surplus-tier Output needs a real, non-trivial non-Subsistence population fraction (<see
    /// cref="PurchasingPowerCatalog.MinNonSubsistenceFractionForModestSurplus"/>). An Elite Discretionary-
    /// tier Output needs a real, if small, Elite Discretionary population floor (<see
    /// cref="PurchasingPowerCatalog.MinEliteDiscretionaryPopulationForLuxury"/>) — §5's own "a large
    /// enough Elite Discretionary population actually existing nearby." A settlement with no recorded
    /// population at all is read as an honest "no verdict yet" match rather than a false mismatch.
    /// </summary>
    public static BusinessViabilityCheck EvaluateBusinessViability(
        RuntimeId<NotableBusiness> businessId, RuntimeId<District> districtId,
        WealthBand outputGoodTier, AggregateDemandReading reading)
    {
        var totalPopulation = reading.SubsistencePopulation + reading.ModestSurplusPopulation + reading.EliteDiscretionaryPopulation;

        bool localDemandMatch;
        string? recommendedAction;

        if (totalPopulation <= 0)
        {
            localDemandMatch = true;
            recommendedAction = null;
        }
        else if (outputGoodTier == WealthBand.Subsistence)
        {
            localDemandMatch = true;
            recommendedAction = null;
        }
        else if (outputGoodTier == WealthBand.ModestSurplus)
        {
            var nonSubsistence = reading.ModestSurplusPopulation + reading.EliteDiscretionaryPopulation;
            var fraction = Fixed64.Divide(Fixed64.FromInt(nonSubsistence), Fixed64.FromInt(totalPopulation));
            localDemandMatch = fraction >= PurchasingPowerCatalog.MinNonSubsistenceFractionForModestSurplus;
            recommendedAction = localDemandMatch ? null : "specialize";
        }
        else
        {
            localDemandMatch = reading.EliteDiscretionaryPopulation >= PurchasingPowerCatalog.MinEliteDiscretionaryPopulationForLuxury;
            recommendedAction = localDemandMatch ? null : "move";
        }

        return new BusinessViabilityCheck(businessId, districtId, outputGoodTier, localDemandMatch, recommendedAction);
    }
}
