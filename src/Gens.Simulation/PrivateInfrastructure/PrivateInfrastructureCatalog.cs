using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;

namespace Gens.Simulation.PrivateInfrastructure;

/// <summary>Versioned constants for Phase 15 item 7's Private Infrastructure mechanics
/// (<c>gens-private-infrastructure-design.md</c>), matching <see cref="RealEstate.RealEstateCatalog"/>'s
/// and every other Phase 15 item's identical "unsized against real playtest data, but named in one
/// place" convention — §11's Open Questions explicitly leaves "all numeric sizing," Land Reclamation's
/// resolution odds, the Road Cluster threshold's own tuning, and every upkeep cost unresolved.</summary>
public static class PrivateInfrastructureCatalog
{
    // --- §2 Private Roads ---

    /// <summary>§2's Paved Road as "a genuine construction project" — this item's own invented capital
    /// cost per Plot-to-Plot connection, deliberately substantial (real Pozzolana/Concrete Works
    /// materials, per §2's own "real Roman road-engineering flavor").</summary>
    public static readonly Money PavedRoadConstructionCost = Money.FromDenarii(200);

    /// <summary>§8's "modest, real, recurring upkeep cost" for a Paved Road connection.</summary>
    public static readonly Money PavedRoadMonthlyUpkeep = Money.FromDenarii(2);

    // --- §2.1 Trade-Proximity Bonus ---

    /// <summary>§2.1's "a real, formalized Commerce/logistics bonus" for a Plot that is River-adjacent,
    /// Coast-adjacent, or Paved-Road-connected to one that is — this item's own invented monthly income
    /// figure, posted directly to the owning household (§9's honest finding: no live "Trade Route
    /// effectiveness" figure exists anywhere in this codebase to multiply instead — <see
    /// cref="Economy.StandingContract.TradeRouteInvestment"/> is a one-off commitment stub with no
    /// recurring effectiveness computation of its own — so this item posts a real, felt Ledger income
    /// line as its own concrete realization of that bonus rather than modifying a figure that does not
    /// exist).</summary>
    public static readonly Money TradeProximityMonthlyBonus = Money.FromDenarii(4);

    // --- §4 Road Clusters & the Connected Estate bonus ---

    /// <summary>§4's own "three or more Plots" threshold, taken literally per that section's own
    /// "deliberately simple, round number" framing — §11 leaves whether that's the right number for a
    /// large estate genuinely untested.</summary>
    public const int RoadClusterThreshold = 3;

    /// <summary>§4's Connected Estate bonus — "a small aggregate efficiency lift across every building
    /// in the cluster" — read here as a flat monthly income figure per <see
    /// cref="Buildings.BuildingInstance"/> standing on a qualifying cluster's own Plots.</summary>
    public static readonly Money ConnectedEstateBonusPerBuilding = Money.FromDenarii(1);

    /// <summary>This item's own bump to a Plot's tracked Property Value (<see
    /// cref="RealEstate.PlotPropertyExtension.Value"/>) the moment any structure in this namespace is
    /// built there — §9's "a Plot's own private infrastructure investment is a real, direct input to...
    /// Property Value," realized as an immediate, one-time addition rather than a second monthly
    /// recompute system layered on top of <see cref="RealEstate.DistrictPropertyValueSystem"/>'s own
    /// District-level trend.</summary>
    public static readonly Money PropertyValueBonusPerStructure = Money.FromDenarii(50);

    // --- §3 Irrigation Canal ---

    public static readonly Money IrrigationCanalConstructionCost = Money.FromDenarii(150);
    public static readonly Money IrrigationCanalMonthlyUpkeep = Money.FromDenarii(3);

    /// <summary>§3's Soil Fertility recovery bonus. No Soil Fertility track exists anywhere in this
    /// codebase yet (<c>Hazards.HazardExposureCalculator</c>'s own top-level disclosure: "no Soil
    /// Fertility track exists... Phase 14 item 4/a future item's own territory" — the same honest gap
    /// <c>Hazards.DormantVolcano</c>'s own post-eruption fertility boost already names) — this figure is
    /// carried on the record as a real, documented reading of §3's own text for whichever future item
    /// builds that track to read, not consumed by anything in this item.</summary>
    public static readonly Fixed64 IrrigationCanalFertilityRecoveryBonus = Fixed64.FromRaw(300_000); // 0.30.

    /// <summary>§3's Drought/Famine severity reduction — this item's one real, live hazard hook: <see
    /// cref="Hazards.HazardExposureProfile.Compute"/> now reads the settlement's own irrigated-Plot
    /// fraction (Irrigation Canals and Wells/Cisterns alike) and <see
    /// cref="Hazards.HazardExposureCalculator.DroughtFamineExposure"/> lowers its baseline Exposure
    /// score proportionally — a real, felt reduction in how often Drought/Famine actually ignites on an
    /// irrigated estate's settlement, not merely a stored, unconsumed number.</summary>
    public static readonly Fixed64 IrrigationCanalDroughtExposureReduction = Fixed64.FromRaw(400_000); // 0.40 at full settlement coverage.

    // --- §3.1 Well & Cistern ---

    public static readonly Money WellConstructionCost = Money.FromDenarii(40);
    public static readonly Money CisternConstructionCost = Money.FromDenarii(80);
    public static readonly Money WellMonthlyUpkeep = Money.FromDenarii(1);
    public static readonly Money CisternMonthlyUpkeep = Money.FromDenarii(2);

    /// <summary>§3.1's "a real, if more modest, version" of <see
    /// cref="IrrigationCanalFertilityRecoveryBonus"/> — half its magnitude for a Well, three-quarters
    /// for the larger Cistern, this item's own reading of "a step up in both cost and effect."</summary>
    public static readonly Fixed64 WellFertilityRecoveryBonus = Fixed64.FromRaw(150_000); // 0.15.
    public static readonly Fixed64 CisternFertilityRecoveryBonus = Fixed64.FromRaw(220_000); // 0.22.

    /// <summary>The Well/Cistern's own, lower-magnitude counterpart to <see
    /// cref="IrrigationCanalDroughtExposureReduction"/> — also a real, live input to <see
    /// cref="Hazards.HazardExposureProfile.Compute"/>'s irrigated-fraction reading.</summary>
    public static readonly Fixed64 WellDroughtExposureReduction = Fixed64.FromRaw(180_000); // 0.18.
    public static readonly Fixed64 CisternDroughtExposureReduction = Fixed64.FromRaw(260_000); // 0.26.

    // --- §5 Land Reclamation ---

    /// <summary>§5.1's "a real, substantial, multi-month project" — this item's own reading of a genuine
    /// multi-year drainage effort, closer to the Pontine Marshes' own real historical timescale than an
    /// ordinary building project.</summary>
    public const int LandReclamationDurationMonths = 24;

    /// <summary>This item's own invented per-month Labor figure a Land Reclamation project consumes
    /// while actively progressing — a plain, unhooked int (§10's own <c>laborAssigned</c> field), not a
    /// live draw against a Duty-slot roster, since no per-project Labor-reservation mechanism exists
    /// anywhere in this codebase for a capital project of this shape.</summary>
    public const int LandReclamationMonthlyLaborRequired = 4;

    public static readonly Money LandReclamationMonthlyCost = Money.FromDenarii(15);

    /// <summary>§5.1's own honest framing — "a real chance of landing as only a Partial Reclamation...
    /// rather than a guaranteed full terrain change every time" — read as a genuine minority outcome,
    /// deliberately weighted toward Partial the way the real Pontine Marshes' own centuries-long,
    /// only-ever-partially-successful history suggests. This item's own invented probability; §11 leaves
    /// the actual weighting, and whether continued investment after a Partial result can ever push
    /// toward Full, both explicitly open — this item does not build a continuation path.</summary>
    public static readonly Fixed64 FullReclamationProbability = Fixed64.FromRaw(250_000); // 0.25 (25%).

    public const uint ReclamationRollPrecision = 1_000_000;

    /// <summary>§5.1's Partial Reclamation's own "modest yield floor raised" — read here as the same
    /// Land Condition scale <see cref="Land.LandCondition"/> already uses, a real floor <see
    /// cref="LandReclamationResolutionSystem"/> raises a Marsh Plot's own <see cref="Land.Plot.Condition"/>
    /// to on a Partial result (short of the Full result's own terrain reclassification).</summary>
    public const int PartialReclamationConditionFloor = 60;

    /// <summary>§5.1's "a genuine, rare achievement worth real Dignitas... when it lands" — this item's
    /// own invented award, posted through <see cref="Reputation.AdjustDignitasCommand"/>'s own
    /// established path the moment a Full Reclamation resolves.</summary>
    public const int FullReclamationDignitasAward = 15;

    // --- §6 Private Bridges ---

    public static readonly Money PrivateBridgeConstructionCost = Money.FromDenarii(120);
    public static readonly Money PrivateBridgeMonthlyUpkeep = Money.FromDenarii(2);

    // --- §7 Boundary & Security Infrastructure ---

    public static readonly Money FenceConstructionCost = Money.FromDenarii(60);
    public static readonly Money WallConstructionCost = Money.FromDenarii(140);
    public static readonly Money FenceMonthlyUpkeep = Money.FromDenarii(1);
    public static readonly Money WallMonthlyUpkeep = Money.FromDenarii(3);

    /// <summary>§7's livestock-rustling risk reduction. Piracy &amp; Banditry (Resources &amp; Goods
    /// §3.2's own raid category) is Phase 16, confirmed unbuilt by direct search — no live raid roll
    /// exists anywhere in this codebase yet to actually consume this figure against. Carried here as a
    /// real, documented, queryable fact (matching <see cref="Societates.PartnerDisputeRiskQuery"/>'s own
    /// "the primitive ships, the caller doesn't exist yet" precedent) for the Phase 16 raid system that
    /// will eventually read it, not a faked live effect.</summary>
    public static readonly Fixed64 FenceRustlingRiskReduction = Fixed64.FromRaw(150_000); // 0.15.
    public static readonly Fixed64 WallRustlingRiskReduction = Fixed64.FromRaw(350_000); // 0.35.

    // --- §8 Maintenance & Disaster Vulnerability ---

    /// <summary>§8's "condition field Estate &amp; Settlement's own Plot data model already tracks" —
    /// every structure in this namespace starts at the same reading <see
    /// cref="Land.LandCondition.Pristine"/> uses.</summary>
    public const int PristineCondition = 100;

    /// <summary>Below this reading, a structure's own real effect (Commerce bonus, Fertility/Drought
    /// reduction, rustling-risk reduction) is treated as fully lapsed rather than partially degraded —
    /// this item's own invented single threshold rather than a continuously-scaled effect curve, matching
    /// <see cref="Buildings.BuildingInstance.IsOperational"/>'s own "operational or not" binary reading
    /// applied to condition instead of staffing.</summary>
    public const int MinimumOperationalCondition = 30;

    /// <summary>§8's ordinary neglect: how many condition points an unpaid month's upkeep costs a
    /// structure — mirrors <see cref="Buildings.BuildingInstance.ApplyUpkeep"/>'s own per-missed-month
    /// decay in spirit, sized independently since this item's condition scale is 0-100, not a five-step
    /// enum.</summary>
    public const int UnpaidUpkeepConditionLoss = 8;

    /// <summary>§8's Repair action — how many condition points a funded repair restores, capped at <see
    /// cref="PristineCondition"/>.</summary>
    public const int RepairConditionRestored = 40;

    public static readonly Money RepairCostPerConditionPoint = Money.FromDenarii(1);

    /// <summary>§8's disaster-vulnerability hit-or-miss reuse of <see
    /// cref="Hazards.DisasterDamageCalculator.BuildingHitProbability"/> and <see
    /// cref="Hazards.DisasterDamageCalculator.BuildingConditionStepsLost"/> — this item's own conversion
    /// from that calculator's 0-4 <c>BuildingCondition</c> step scale to this namespace's own 0-100
    /// scale, so a Catastrophic hit costs this structure roughly the same proportional share of its own
    /// condition range a Catastrophic hit costs a <see cref="Buildings.BuildingInstance"/> (4 of 4
    /// non-Ruined steps).</summary>
    public const int ConditionPointsPerBuildingConditionStep = 20;
}
