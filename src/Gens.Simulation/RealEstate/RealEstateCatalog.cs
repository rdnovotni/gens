using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;
using Gens.Simulation.Regions;
using Gens.Simulation.Stewardship;

namespace Gens.Simulation.RealEstate;

/// <summary>Versioned constants for Phase 15 item 1's Land/Property Market mechanics
/// (<c>gens-land-ownership-real-estate-design.md</c>), matching <see cref="Scandal.ScandalCatalog"/>'s
/// and <see cref="Fame.FameCatalog"/>'s identical "unsized against real playtest data, but named in one
/// place" convention — §14's Open Questions explicitly leaves "all numeric sizing" unresolved.</summary>
public static class RealEstateCatalog
{
    /// <summary>§4's "a Vicus might have just one; a full City, four or five" — the soft cap on how
    /// many Districts <see cref="EstablishDistrictCommands"/> allows per settlement, scaling with <see
    /// cref="SettlementStage"/>. This item's own invented mapping of that prose into a concrete count
    /// per stage.</summary>
    public static int MaxDistrictsForStage(SettlementStage stage) => stage switch
    {
        SettlementStage.Villa => 1,
        SettlementStage.Vicus => 1,
        SettlementStage.Town => 3,
        SettlementStage.City => 5,
        _ => 1,
    };

    /// <summary>§4: Districts exist only "at Vicus stage or above" — a Villa (the single-household
    /// starting state) has no urban geography to subdivide yet.</summary>
    public static readonly SettlementStage MinimumStageForDistricts = SettlementStage.Vicus;

    // --- District Property Value trend (§4, §9) ---

    /// <summary>The neutral starting reading every new District begins at — "nothing pulling it up or
    /// down yet."</summary>
    public static readonly Fixed64 BaselinePropertyValue = Fixed64.One;

    /// <summary>Never lets a District's Property Value collapse to zero or below — a real, if
    /// depressed, District still has some standing value, matching §10's own "gentrifying... a real,
    /// felt consequence" framing implying the inverse (a declining District) is a felt but bounded
    /// consequence too.</summary>
    public static readonly Fixed64 MinimumPropertyValue = Fixed64.FromRaw(100_000); // 0.1.

    /// <summary>How much of the gap between a District's current Property Value and this month's
    /// freshly computed target it actually closes, each month — smoothing so one month's disaster or
    /// population blip doesn't instantly re-price every property in the District.</summary>
    public static readonly Fixed64 PropertyValueSmoothing = Fixed64.FromRaw(100_000); // 0.1 (10%/month).

    /// <summary>§4's population-growth input, scaled: each 1% of settlement-wide population growth
    /// this month nudges the District's own target Property Value by this much.</summary>
    public static readonly Fixed64 PopulationGrowthWeight = Fixed64.FromRaw(2_000_000); // 2.0.

    /// <summary>§4's Contentment input, scaled: the settlement's own size-weighted average Contentment,
    /// read against a neutral 0.5 midpoint (below depresses the target, above lifts it), multiplied by
    /// this weight.</summary>
    public static readonly Fixed64 ContentmentWeight = Fixed64.FromRaw(400_000); // 0.4.

    /// <summary>§4's Natural Disaster damage input: each building this item's own lookback window
    /// (<see cref="DisasterDamageLookbackMonths"/>) recorded as damaged in the District's settlement
    /// depresses the target Property Value by this much, matching <see
    /// cref="Hazards.DisasterEvent.BuildingsDamaged"/>'s own already-tracked aggregate.</summary>
    public static readonly Fixed64 DisasterDamagePerBuildingWeight = Fixed64.FromRaw(20_000); // 0.02.

    /// <summary>How many months back <see cref="DistrictPropertyValueSystem"/> still counts a <see
    /// cref="Hazards.DisasterEvent"/>'s damage against a District's Property Value — a real disaster's
    /// mark fades rather than depressing Value forever.</summary>
    public const int DisasterDamageLookbackMonths = 24;

    /// <summary>§4/§12's Gazetteer Prominence Tier input, only read when a District's <see
    /// cref="District.LinkedGazetteerLocationId"/> resolves.</summary>
    public static Fixed64 ProminenceTierBonus(ProminenceTier tier) => tier switch
    {
        ProminenceTier.Outpost => Fixed64.Zero,
        ProminenceTier.RegionalCenter => Fixed64.FromRaw(100_000), // 0.1.
        ProminenceTier.ProvincialSeat => Fixed64.FromRaw(250_000), // 0.25.
        _ => Fixed64.Zero,
    };

    // --- §10 Displacement: District Property Value as a rent-burden input into Contentment ---

    /// <summary>Below this District Property Value, no extra rent burden is felt at all — only a
    /// District that has genuinely gentrified past the baseline actually depresses lower-tier
    /// Contentment (§10's "sharply rising Property Value").</summary>
    public static readonly Fixed64 RentBurdenPropertyValueThreshold = Fixed64.FromRaw(1_300_000); // 1.3.

    /// <summary>The rent-burden fraction subtracted from a lower-tier resident pop group's Contentment
    /// per full unit of District Property Value above <see cref="RentBurdenPropertyValueThreshold"/> —
    /// §10's "higher rent burden depressing Contentment... exactly the way overcrowding or low
    /// Contentment already does."</summary>
    public static readonly Fixed64 RentBurdenWeight = Fixed64.FromRaw(500_000); // 0.5.

    // --- §6 Leasing & Operators ---

    /// <summary>§6's "a skimming Operator" — reused directly rather than inventing a second Loyalty
    /// threshold, matching <see cref="Land.DistantHoldingMismanagementRiskSystem"/>'s own identical
    /// reuse of this exact constant for "is the person running things unsupervised a real
    /// liability."</summary>
    public const int SkimmingLoyaltyThreshold = StewardIncidentCatalog.LoyaltyRiskThreshold;

    /// <summary>§6.1's "a decade" of steady tenure before a real buyout offer becomes plausible — one
    /// hundred and twenty months, read literally.</summary>
    public const int BuyoutMinimumTenureMonths = 120;

    /// <summary>§6.1's "particularly plausible for a freedman... genuinely capable" — the Ambition
    /// (Core Condition, 0-100) and Stewardship (Core Attribute, 0-100) floors an Operator must clear,
    /// on top of never having skimmed, before <see cref="OperatorLifecycleSystem"/> ever flags a real
    /// buyout offer.</summary>
    public const int BuyoutAmbitionThreshold = 65;

    public const int BuyoutStewardshipThreshold = 60;

    /// <summary>§6.1's "the District's own Property Value keeps climbing" — the buyout offer only
    /// fires once the Operator's own District has genuinely gentrified past this reading, tying the
    /// worked example's two conditions (a capable Operator, a rising District) together rather than
    /// letting either alone trigger it.</summary>
    public static readonly Fixed64 BuyoutDistrictPropertyValueThreshold = Fixed64.FromRaw(1_100_000); // 1.1.

    /// <summary>§6's "a reliable, agreed share of income" a steady Operator remits monthly, as a
    /// fraction of the property's own tracked Value — this item's own invented monthly-yield rate,
    /// deliberately modest (a Directly Managed property keeps the whole thing; leasing trades margin
    /// for passive income, per §11's own opening premise).</summary>
    public static readonly Fixed64 SteadyOperatorMonthlyYield = Fixed64.FromRaw(5_000); // 0.005 (0.5%/month).

    /// <summary>A skimming Operator still remits something, just less — §6's "quietly under-reports
    /// income" rather than remitting nothing at all (which would be instantly obvious without an
    /// audit).</summary>
    public static readonly Fixed64 SkimmingOperatorMonthlyYield = Fixed64.FromRaw(2_000); // 0.002 (0.2%/month).

    /// <summary>§6's audit consequence for a false accusation: the Loyalty hit an honest Operator takes
    /// when audited and cleared — "a relationship-web hit if the Operator turns out to have been
    /// honest all along."</summary>
    public const int FalseAuditAccusationLoyaltyPenalty = 10;

    // --- §9 Property Value & the Market ---

    /// <summary>§9's "current Value minus a standard friction" for a sale to the abstract market.</summary>
    public static readonly Fixed64 MarketSaleFriction = Fixed64.FromRaw(150_000); // 0.15 (15%).

    /// <summary>Converts a tracked <see cref="Fixed64"/> Property Value reading into a <see
    /// cref="Money"/> price for a property whose own base worth is <paramref name="baseValue"/> —
    /// e.g. a District trend of 1.2 against a 10,000-denarii base villa prices it at 12,000. This
    /// item's own invented linear read of §9's "value... moving from... the District's own trend."</summary>
    public static Money PriceFor(Money baseValue, Fixed64 propertyValueTrend) => baseValue.Scale(propertyValueTrend);

    // --- §11 Portfolio Scale & Oversight ---

    /// <summary>§11's "each additional significant Property Record beyond a soft threshold" — the
    /// count of Directly Managed properties (§11's own closing line: leasing delegates management, so
    /// a Leased Out property does not count against this) a household can run before Administrative
    /// Burden starts costing anything.</summary>
    public const int AdministrativeBurdenFreeThreshold = 3;

    /// <summary>§11's "a genuine Economy &amp; Finance expense line" — the monthly denarii cost per
    /// Directly Managed property held past <see cref="AdministrativeBurdenFreeThreshold"/>.</summary>
    public static readonly Money AdministrativeBurdenCostPerProperty = Money.FromDenarii(5);
}
