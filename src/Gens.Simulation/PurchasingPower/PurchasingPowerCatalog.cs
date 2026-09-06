using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Characters;
using Gens.Simulation.Numerics;

namespace Gens.Simulation.PurchasingPower;

/// <summary>Versioned constants for Phase 15 item 10's Population Wealth &amp; Purchasing Power
/// mechanics (<c>gens-population-wealth-purchasing-power-design.md</c>), matching every other Phase 15
/// item's identical "unsized against real playtest data, but named in one place" convention — §10's
/// Open Questions explicitly leaves "all numeric sizing — the exact weighting curve favoring Elite
/// Discretionary demand, and every tier's own precise boundary conditions" unsized.</summary>
public static class PurchasingPowerCatalog
{
    // --- §3 Aggregate Purchasing Power — per-capita demand weight by WealthBand ---

    /// <summary>§3's "weighted heavily toward the top" — a Subsistence resident's own per-capita
    /// discretionary demand contribution, deliberately the smallest of the three: real, but "almost
    /// entirely food and bare shelter" (§2).</summary>
    public static readonly Fixed64 SubsistenceDemandWeight = Fixed64.FromRaw(200_000); // 0.2.

    /// <summary>A Modest Surplus resident's own per-capita contribution — "real, if limited, discretionary
    /// spending" (§2), read as the baseline unit the other two tiers are weighted against.</summary>
    public static readonly Fixed64 ModestSurplusDemandWeight = Fixed64.One; // 1.0.

    /// <summary>An Elite Discretionary resident's own per-capita contribution — §3's own explicit
    /// instruction that a linear population-times-average-wealth calculation "would understate how much
    /// economic weight the narrow Elite Discretionary tier actually carries." This item's own invented
    /// 5x-over-Modest-Surplus reading: a real, historically-plausible order-of-magnitude gap between an
    /// ordinary shopkeeper's thin discretionary margin and a genuinely wealthy household's, without
    /// claiming a precise historical ratio §10 never supplies.</summary>
    public static readonly Fixed64 EliteDiscretionaryDemandWeight = Fixed64.FromRaw(5_000_000); // 5.0.

    // --- §7 Business Viability thresholds ---

    /// <summary>§7's "a large enough Elite Discretionary population actually existing nearby" — this
    /// item's own invented flat population floor (not a fraction: even a huge, overwhelmingly
    /// Subsistence settlement can carry this many real elite residents and still sustain one luxury
    /// business) below which an Elite Discretionary-tier Output business reads as genuinely starved of
    /// local customers.</summary>
    public const int MinEliteDiscretionaryPopulationForLuxury = 15;

    /// <summary>§7's the same test for a Modest Surplus-tier Output business — read as a population
    /// fraction rather than a flat count, since "a better cut of meat, a slightly finer cloth" demand
    /// scales with how much of the settlement is above bare Subsistence at all, not with the raw count
    /// of any one tier.</summary>
    public static readonly Fixed64 MinNonSubsistenceFractionForModestSurplus = Fixed64.FromRaw(50_000); // 0.05 (5%).

    /// <summary>§7's own recurring "long-term viability" drag — unlike a one-shot Scandal/Crime
    /// consequence, a genuinely mismatched business keeps losing real Reputation every month the
    /// mismatch persists (clamped at <see cref="NotableBusinesses.NotableBusinessesCatalog.MinReputation"/>,
    /// so this never runs away unbounded), matching §7's own "long-term viability" framing directly
    /// rather than a single discrete penalty.</summary>
    public const int LocalDemandMismatchMonthlyReputationLoss = 1;

    // --- §4 Subsistence-good political sensitivity ---

    /// <summary>§4's "a direct Contentment crisis" — this item's own invented Contentment penalty
    /// applied (via <see cref="Characters.ContentmentCalculator"/>'s new overload) to every Subsistence-
    /// tier <see cref="PopGroup"/> while its settlement's <see cref="Markets.SettlementMarket"/> for
    /// <see cref="Characters.NeedsConsumptionCalculator.ConsumptionGood"/> carries genuine unsatisfied
    /// demand (the same real "genuine shortage" signal <see
    /// cref="BusinessCompetition.GrainHoardingResolutionSystem"/> already established), matching <see
    /// cref="RealEstate.DistrictRentBurdenCalculator.ComputeRentBurden"/>'s own identical "a real, felt
    /// subtracted term" shape.</summary>
    public static readonly Fixed64 SubsistenceShortageContentmentPenalty = Fixed64.FromRaw(50_000); // 0.05.

    /// <summary>§4's own named "hoardingRiskMultiplier" (§9's data model) — a real, computed reading
    /// applied on top of the baseline reading of 1.0 while a genuine shortage is underway, exposed for a
    /// future autonomous NPC decision layer to read. No such layer exists anywhere in this codebase yet
    /// (matching <see cref="BusinessCompetition.MarketSaturationSystem"/>'s own identical "a real,
    /// computed primitive with no autonomous caller yet" precedent) — <see
    /// cref="BusinessCompetition.DeclareGrainHoardingCommand"/> is always player/scenario-issued, never
    /// auto-fired from this reading.</summary>
    public static readonly Fixed64 ShortageHoardingRiskMultiplier = Fixed64.FromRaw(2_000_000); // 2.0.

    // --- §6 Market Capacity integration (Business Competition §6) ---

    /// <summary>Below this <see cref="AggregateDemandReading.TotalDemandIndex"/> reading, <see
    /// cref="BusinessCompetition.MarketSaturationSystem"/> reads a settlement's own trade as genuinely
    /// thinner than its raw business count alone would suggest — §6's own "a crowded, flat-population
    /// settlement genuinely dilutes... demand" extended from population *trend* (already scoped out by
    /// that system's own doc comment) to population *purchasing power*, the one further real input this
    /// item can compute without inventing a per-District population-growth allocation rule.</summary>
    public static readonly Fixed64 ThinPurchasingPowerCeiling = Fixed64.FromRaw(400_000); // 0.4.

    // --- §6 District Property Value integration (Land Ownership & Real Estate §4) ---

    /// <summary>The neutral reading <see cref="RealEstate.DistrictPropertyValueSystem"/> treats as
    /// "no purchasing-power pull either way" — a settlement with no recorded <see
    /// cref="AggregateDemandReading"/> yet (this item's own system has not ticked) contributes exactly
    /// zero here, reading identically to every District Property Value computation that shipped before
    /// this item.</summary>
    public static readonly Fixed64 NeutralDemandIndex = Fixed64.FromRaw(500_000); // 0.5.

    /// <summary>§6's "a real, concrete new input" into District Property Value — this item's own invented
    /// weight, deliberately smaller than <see cref="RealEstate.RealEstateCatalog.PopulationGrowthWeight"/>
    /// since Purchasing Power is a slow-moving structural reading, not a month-to-month shock like a
    /// disaster or a migration swing.</summary>
    public static readonly Fixed64 PurchasingPowerPropertyValueWeight = Fixed64.FromRaw(600_000); // 0.6.
}
