using Gens.Simulation.Goods;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;

namespace Gens.Simulation.Shipping;

/// <summary>Versioned constants for Phase 15 item 8's Shipping Ventures mechanics
/// (<c>gens-private-ships-shipping-ventures-design.md</c>), matching every other Phase 15 item's
/// identical "unsized against real playtest data, but named in one place" convention — §12's Open
/// Questions explicitly leaves "all numeric sizing... unsized." Also the one place this item's own
/// per-<see cref="ShipVesselClass"/> mechanical differentiation actually lives: §2's registry is a real,
/// fully-listed roster (<see cref="ShipVesselClass"/>'s own doc comment), but per this item's own scope
/// decision, only two axes of that registry are mechanically live — <see cref="CapacityTierFor"/> (every
/// class's own §2 capacity reading) and <see cref="StormResistanceMultiplier"/> (§2.2's one explicitly
/// named mechanical trait, the Gallic/Britannic Coaster's "elevated Storm resistance"). Speed (Liburnian,
/// Actuaria) and cargo specialization flavor (Hippago's live animals, Ponto's ferry role, the Nile
/// Riverboat/Pontic Grain Trader's regional identity) are real, named categories with no further
/// mechanical stat block behind them — this item does not invent a Speed axis or a cargo-type system
/// nothing else in this codebase reads yet.</summary>
public static class ShippingCatalog
{
    // --- §2 Capacity Tier, read from Vessel Class ---

    public static ShipCapacityTier CapacityTierFor(ShipVesselClass vesselClass) => vesselClass switch
    {
        ShipVesselClass.NavisCaudicaria => ShipCapacityTier.Low,
        ShipVesselClass.Corbita => ShipCapacityTier.Standard,
        ShipVesselClass.GrainCarrier => ShipCapacityTier.High,
        ShipVesselClass.PunicTrader => ShipCapacityTier.Standard,
        ShipVesselClass.AegeanMerchantman => ShipCapacityTier.Standard,
        ShipVesselClass.GallicBritannicCoaster => ShipCapacityTier.Standard,
        // §2.1's own "Standard to High" — this item's own reading picks the lower, simpler tier,
        // leaving the Red Sea/Nabataean Trader mechanically identical to a Corbita rather than
        // inventing a fifth capacity tier for one row.
        ShipVesselClass.RedSeaNabataeanTrader => ShipCapacityTier.Standard,
        ShipVesselClass.Liburnian => ShipCapacityTier.Low,
        ShipVesselClass.Actuaria => ShipCapacityTier.Standard,
        // §2.2's own "Low, not a trade vessel" — the capacity tier is real (Low), but §2.2's own text
        // is honored separately by <see cref="IsTradeVessel"/> rather than folding "not a trade
        // vessel" into the tier itself.
        ShipVesselClass.Ponto => ShipCapacityTier.Low,
        ShipVesselClass.Hippago => ShipCapacityTier.Low,
        ShipVesselClass.NileRiverboat => ShipCapacityTier.Standard,
        // §2.2's own "Standard to High" for the Bosporan Kingdom's own grain-export identity — this
        // item picks High, matching the Alexandrian Grain Carrier it is explicitly the regional
        // counterpart to.
        ShipVesselClass.PonticGrainTrader => ShipCapacityTier.High,
        ShipVesselClass.PersonalPleasureBarge => ShipCapacityTier.None,
        _ => throw new ArgumentOutOfRangeException(nameof(vesselClass), vesselClass, "Unhandled vessel class."),
    };

    /// <summary>§2.2's Ponto ("not a trade vessel at all") and Personal Pleasure Barge ("not a trade
    /// asset at all") — the two classes <see cref="AssignShipToTradeRouteCommands"/> refuses to attach
    /// to a Trade Route, matching those rows' own explicit text rather than only reading <see
    /// cref="ShipCapacityTier.None"/> (which would miss the Ponto, whose tier is real Low cargo capacity
    /// despite carrying no trade goods).</summary>
    public static bool IsTradeVessel(ShipVesselClass vesselClass) =>
        vesselClass is not (ShipVesselClass.Ponto or ShipVesselClass.PersonalPleasureBarge);

    /// <summary>§2.2's own explicitly named mechanical trait: the Gallic/Britannic Coaster's "elevated
    /// Storm resistance... trading some cargo efficiency for real, mechanical hardiness." Read by <see
    /// cref="ShipVoyageRiskSystem"/> as a flat multiplier on the Storm hit probability every other class
    /// rolls against unmodified.</summary>
    public static Fixed64 StormResistanceMultiplier(ShipVesselClass vesselClass) =>
        vesselClass == ShipVesselClass.GallicBritannicCoaster ? Fixed64.FromRaw(750_000) : Fixed64.One; // 0.75, else 1.0.

    // --- §3.1 Custom Commissioning ---

    /// <summary>§3's "a real, deliberate construction project rather than an instant purchase" — this
    /// item's own invented per-tier duration, deliberately short relative to <see
    /// cref="PrivateInfrastructure.LandReclamationProject"/>'s own multi-year Land Reclamation timescale
    /// (§12's own open "commissioning time relative to Estate &amp; Settlement's own building-
    /// construction timeline," left unresolved by the design doc itself): a vessel, unlike drainage or a
    /// road network, is a shipyard's ordinary, if substantial, product.</summary>
    public static int CommissionDurationMonths(ShipCapacityTier tier) => tier switch
    {
        ShipCapacityTier.None => 2,
        ShipCapacityTier.Low => 3,
        ShipCapacityTier.Standard => 6,
        ShipCapacityTier.High => 10,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unhandled capacity tier."),
    };

    private static readonly Money BaseCommissionCostNone = Money.FromDenarii(100);
    private static readonly Money BaseCommissionCostLow = Money.FromDenarii(150);
    private static readonly Money BaseCommissionCostStandard = Money.FromDenarii(400);
    private static readonly Money BaseCommissionCostHigh = Money.FromDenarii(900);

    private static Money BaseCommissionCost(ShipCapacityTier tier) => tier switch
    {
        ShipCapacityTier.None => BaseCommissionCostNone,
        ShipCapacityTier.Low => BaseCommissionCostLow,
        ShipCapacityTier.Standard => BaseCommissionCostStandard,
        ShipCapacityTier.High => BaseCommissionCostHigh,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unhandled capacity tier."),
    };

    /// <summary>§3.1's Build Quality premium — "a real, proportionally higher commissioning cost" for
    /// Fine or Exceptional over Common, this item's own invented multiplier.</summary>
    private static Fixed64 QualityCostMultiplier(GoodQuality quality) => quality switch
    {
        GoodQuality.Common => Fixed64.One,
        GoodQuality.Fine => Fixed64.FromRaw(1_500_000), // 1.5x.
        GoodQuality.Exceptional => Fixed64.FromRaw(2_250_000), // 2.25x.
        _ => throw new ArgumentOutOfRangeException(nameof(quality), quality, "Unhandled build quality."),
    };

    /// <summary>The commission's total capital cost, spread evenly across <see
    /// cref="CommissionDurationMonths"/> by <see cref="ShipCommissionResolutionSystem"/> — declared as a
    /// total rather than a monthly figure so the Build Quality premium reads naturally against the whole
    /// project, matching how §3.1 frames the premium against "the new Ship's own... cost," not a
    /// per-month rate.</summary>
    public static Money TotalCommissionCost(ShipCapacityTier tier, GoodQuality quality) =>
        BaseCommissionCost(tier).Scale(QualityCostMultiplier(quality));

    /// <summary>§3.1's Build Quality "starting Condition ceiling" — this item's own invented reading:
    /// Common starts short of pristine, Fine higher, Exceptional at the same <see
    /// cref="Land.LandCondition.Pristine"/> reading every other Phase 15 asset starts a new build at.</summary>
    public static int StartingCondition(GoodQuality quality) => quality switch
    {
        GoodQuality.Common => 80,
        GoodQuality.Fine => 90,
        GoodQuality.Exceptional => 100,
        _ => throw new ArgumentOutOfRangeException(nameof(quality), quality, "Unhandled build quality."),
    };

    /// <summary>§3.1's Build Quality "long-run resistance to ordinary wear" — how many condition points
    /// an unpaid upkeep month costs, lower for a higher-quality hull, mirroring <see
    /// cref="PrivateInfrastructure.PrivateInfrastructureCatalog.UnpaidUpkeepConditionLoss"/>'s own flat
    /// figure but split by quality per §3.1's own explicit text.</summary>
    public static int UnpaidUpkeepConditionLoss(GoodQuality quality) => quality switch
    {
        GoodQuality.Common => 10,
        GoodQuality.Fine => 7,
        GoodQuality.Exceptional => 5,
        _ => throw new ArgumentOutOfRangeException(nameof(quality), quality, "Unhandled build quality."),
    };

    // --- §3.2 Consecrated Launch ---

    /// <summary>§3.2's real Religion Funded Action — a flat, invented cost bundled into the
    /// commissioning project's own completion rather than a separately negotiated donation amount (§5 of
    /// <c>gens-religion-sacred-calendar-design.md</c>'s own Funded Action framing, reused directly per
    /// <see cref="Religion.FundFestivalCelebrationCommand"/>'s own doc comment: "not invented here" as a
    /// generic abstraction, only reused where this item needs the same Favor/Dignitas payoff shape).</summary>
    public static readonly Money ConsecratedLaunchCost = Money.FromDenarii(50);

    public const int ConsecratedLaunchFavorGain = 10;
    public const int ConsecratedLaunchDignitasGain = 5;

    // --- §4 Flagship ---

    /// <summary>§4's "real, standing Dignitas material simply by existing prominently" — this item's own
    /// invented one-time award the moment a Ship first becomes a household's Flagship, posted through
    /// <see cref="Reputation.AdjustDignitasCommand"/>'s own established path, matching <see
    /// cref="PrivateInfrastructure.PrivateInfrastructureCatalog.FullReclamationDignitasAward"/>'s
    /// identical "real, invented, one-time achievement award" shape rather than an ongoing monthly
    /// trickle this item does not build.</summary>
    public const int FlagshipDesignationDignitasAward = 20;

    // --- §7 Upkeep & Repair ---

    public static Money MonthlyUpkeep(ShipCapacityTier tier) => tier switch
    {
        ShipCapacityTier.None => Money.FromDenarii(1),
        ShipCapacityTier.Low => Money.FromDenarii(2),
        ShipCapacityTier.Standard => Money.FromDenarii(4),
        ShipCapacityTier.High => Money.FromDenarii(8),
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unhandled capacity tier."),
    };

    public const int RepairConditionRestored = 40;
    public static readonly Money RepairCostPerConditionPoint = Money.FromDenarii(2);

    // --- §8 Loss, §9 Reputation ---

    /// <summary>§9's "lucky ship" reputation — this item's own invented threshold: enough consecutive
    /// discrete Voyage Events resolved <see cref="VoyageOutcome.ArrivedSafely"/> to read as a real,
    /// earned track record rather than a lucky first crossing.</summary>
    public const int LuckyShipVoyageThreshold = 5;

    /// <summary>§9's "conversely... a genuinely harder ship to crew" — this item's own invented
    /// threshold on <see cref="MerchantShip.ConsecutiveBadOutcomes"/>.</summary>
    public const int BadReputationVoyageThreshold = 2;

    public const int LuckyShipDignitasAward = 5;

    /// <summary>§3.2's Blessed Launch and §9's earned lucky reputation each contribute their own
    /// standing reduction in future Voyage Event severity — both real, invented, and, per §3.2's own
    /// "distinct from and stacking with," multiplicative rather than exclusive.</summary>
    public static readonly Fixed64 BlessedLaunchRiskMultiplier = Fixed64.FromRaw(850_000); // 0.85.
    public static readonly Fixed64 LuckyShipRiskMultiplier = Fixed64.FromRaw(850_000); // 0.85.
    public static readonly Fixed64 BadReputationRiskMultiplier = Fixed64.FromRaw(1_150_000); // 1.15.

    /// <summary>§8's ordinary Storm loss — an invented, modest Dignitas penalty posted through <see
    /// cref="Reputation.AdjustDignitasCommand"/>.</summary>
    public const int OrdinaryLossDignitasPenalty = -10;

    /// <summary>§4's "a real, sharper Dignitas hit than an ordinary Ship's own loss" for a lost
    /// Flagship — this item's own invented, larger penalty.</summary>
    public const int FlagshipLossDignitasPenalty = -25;

    /// <summary>§8's disaster-vulnerability reuse of <see
    /// cref="Hazards.DisasterDamageCalculator.BuildingConditionStepsLost"/>, converted from that
    /// calculator's 0-4 step scale to this item's own 0-100 <see cref="Land.LandCondition"/> scale,
    /// matching <see
    /// cref="PrivateInfrastructure.PrivateInfrastructureCatalog.ConditionPointsPerBuildingConditionStep"/>'s
    /// identical conversion figure.</summary>
    public const int ConditionPointsPerBuildingConditionStep = 20;
}
