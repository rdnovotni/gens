using Gens.Simulation.Ledger;
using Gens.Simulation.MerchantFamilies;
using Gens.Simulation.Numerics;

namespace Gens.Simulation.PublicWorks;

/// <summary>Versioned constants for Phase 15 item 9's Public Works &amp; Euergetism mechanics
/// (<c>gens-public-works-euergetism-design.md</c>), matching every other Phase 15 item's identical
/// "unsized against real playtest data, but named in one place" convention — §10's Open Questions
/// explicitly leaves "all numeric sizing... unsized," including the Prominence-to-obligation threshold,
/// each work type's own mechanical benefit magnitude, and upkeep cost curves.</summary>
public static class PublicWorksCatalog
{
    // --- §3 Construction cost & upkeep, one figure per work type ---

    /// <summary>This item's own invented construction cost per <see cref="PublicWorkType"/>, scaled
    /// loosely by real-world scope (a Bridge or Sewer segment cheaper than a settlement-wide Aqueduct or
    /// Harbor works) — §10's own "all numeric sizing... unsized" covers this entirely.</summary>
    public static Money ConstructionCost(PublicWorkType workType) => workType switch
    {
        PublicWorkType.Aqueduct => Money.FromDenarii(600),
        PublicWorkType.Road => Money.FromDenarii(250),
        PublicWorkType.Bridge => Money.FromDenarii(200),
        PublicWorkType.Sewer => Money.FromDenarii(350),
        PublicWorkType.MarketplaceOrBasilica => Money.FromDenarii(500),
        PublicWorkType.Harbor => Money.FromDenarii(800),
        _ => throw new ArgumentOutOfRangeException(nameof(workType), workType, "Unhandled public work type."),
    };

    /// <summary>§6's "modest, real, recurring upkeep cost" — this item's own invented monthly figure,
    /// roughly a fixed fraction of <see cref="ConstructionCost"/> per type, mirroring <see
    /// cref="PrivateInfrastructure.PrivateInfrastructureCatalog"/>'s identical construction-cost-to-
    /// upkeep-cost ratio convention.</summary>
    public static Money MonthlyUpkeep(PublicWorkType workType) => workType switch
    {
        PublicWorkType.Aqueduct => Money.FromDenarii(8),
        PublicWorkType.Road => Money.FromDenarii(3),
        PublicWorkType.Bridge => Money.FromDenarii(2),
        PublicWorkType.Sewer => Money.FromDenarii(4),
        PublicWorkType.MarketplaceOrBasilica => Money.FromDenarii(6),
        PublicWorkType.Harbor => Money.FromDenarii(10),
        _ => throw new ArgumentOutOfRangeException(nameof(workType), workType, "Unhandled public work type."),
    };

    // --- §6 Maintenance & upkeep — condition scale, matching PrivateInfrastructureCatalog's own ---

    public const int PristineCondition = 100;

    /// <summary>Below this reading, a work's own real benefit (§3's Health/Trade/District/Commerce
    /// effect) is treated as fully lapsed, matching <see
    /// cref="PrivateInfrastructure.PrivateInfrastructureCatalog.MinimumOperationalCondition"/>'s
    /// identical "operational or not" binary reading.</summary>
    public const int MinimumOperationalCondition = 30;

    /// <summary>§6's ordinary neglect: how many condition points an unpaid month costs, mirroring <see
    /// cref="PrivateInfrastructure.PrivateInfrastructureCatalog.UnpaidUpkeepConditionLoss"/>'s identical
    /// figure.</summary>
    public const int UnpaidUpkeepConditionLoss = 8;

    /// <summary>§6's "recoverable through the same Repair action" — how many condition points a funded
    /// <see cref="FundPublicWorkUpkeepCommand"/> restores, capped at <see cref="PristineCondition"/>.</summary>
    public const int RepairConditionRestored = 40;

    public static readonly Money RepairCostPerConditionPoint = Money.FromDenarii(1);

    /// <summary>§6's "in a severe case of visible neglect, risks a real Scandal" — this item's own
    /// invented two-part gate: Condition must have fallen below this reading...</summary>
    public const int SevereNeglectConditionThreshold = 20;

    /// <summary>...for at least this many consecutive unpaid months (<see
    /// cref="PublicWork.ConsecutiveNeglectedMonths"/>) — together, "a once-celebrated contribution"
    /// genuinely, sustainedly falling into disrepair, not one bad month.</summary>
    public const int SevereNeglectConsecutiveMonths = 6;

    // --- §4 Inscription & Dignitas credit ---

    /// <summary>§4's real Dignitas credit for a private patron's own funded work — this item's own
    /// invented flat award, deliberately more modest than <see
    /// cref="PrivateInfrastructure.PrivateInfrastructureCatalog.FullReclamationDignitasAward"/>'s
    /// one-time achievement figure, since euergetism (unlike a rare Land Reclamation success) is meant to
    /// be a genuinely repeatable, ongoing civic practice rather than a singular triumph.</summary>
    public const int PrivateFundingDignitasAward = 8;

    // --- §2 Euergetism Obligation ---

    /// <summary>Events §5's own Prominence concept is confirmed, by direct search, not to exist as a
    /// real field anywhere in this codebase — the same gap <see
    /// cref="Scandal.ScandalScope"/>'s own doc comment already names for Provincial/RomeWide spread. This
    /// item reads the identical real, checkable proxy <see
    /// cref="MerchantFamilies.EquestrianStatusQuery"/> already established for "sufficiently wealthy and
    /// sufficiently prominent" — a household's own tracked Net Worth — set meaningfully above <see
    /// cref="MerchantFamiliesCatalog.EquestrianNetWorthThreshold"/> so only a household genuinely wealthier
    /// than the ordinary Equestrian bar carries this item's own quieter, further-reaching civic
    /// expectation.</summary>
    public static readonly Money ObligationNetWorthThreshold = Money.FromDenarii(35_000);

    /// <summary>§2's "never funds a single public work across a long playthrough" — this item's own
    /// invented grace period: a household must have carried <see cref="ObligationNetWorthThreshold"/>
    /// or more, continuously, for this many months with zero <see
    /// cref="EuergetismObligation.PublicWorksFundedCount"/> before it is read as neglectful.</summary>
    public const int ObligationGracePeriodMonths = 24;

    /// <summary>§2's "a real, quiet Dignitas cost" — a small, invented monthly trickle (not a one-time
    /// hit) applied by <see cref="EuergetismObligationSystem"/> for every month a qualifying household
    /// stays neglectful, matching §2's own "quiet, ongoing" framing directly rather than a single
    /// discrete penalty.</summary>
    public const int ObligationMonthlyDignitasPenalty = -1;

    // --- §5 Competitive Euergetism ---

    /// <summary>§10's own open question — "Competitive Euergetism's own natural stopping point... short
    /// of one household simply running out of will or wealth" — resolved as a practical, invented ceiling
    /// rather than an unbounded ladder, matching <see
    /// cref="BusinessCompetition.CompetitiveEscalation"/>'s own four-rung ladder precedent for "a real
    /// escalation needs a real top."</summary>
    public const int MaxEscalationRounds = 5;

    /// <summary>§5's "raising the real... cost of the next contribution" — each further round's funded
    /// work costs this fraction more than the base <see cref="ConstructionCost"/>, compounding per
    /// round.</summary>
    public static readonly Fixed64 EscalationCostMultiplierPerRound = Fixed64.FromRaw(250_000); // 0.25 (25%) per round.

    /// <summary>§5's "raising the real Dignitas stakes" — a scaled Dignitas award on top of the ordinary
    /// <see cref="PrivateFundingDignitasAward"/>, multiplied by the current escalation round.</summary>
    public const int EscalationDignitasPerRound = 4;

    // --- §3 Per-work-type mechanical effects, this item's own invented magnitudes ---

    /// <summary>§3's Aqueduct — "a real, direct improvement to Disease &amp; Public Health outcomes." A
    /// multiplier under <see cref="Fixed64.One"/> applied on top of <see
    /// cref="Health.SanitationInvestmentCalculator.ExposureMultiplier"/> by <see
    /// cref="PublicWorksHealthQuery.SanitationMultiplier"/> — an operational Aqueduct alone reduces
    /// exposure by a modest fraction; see <see cref="SewerSanitationMultiplier"/> for the further, larger
    /// Sewer contribution §3 names as "distinct from an aqueduct's own clean-water contribution."</summary>
    public static readonly Fixed64 AqueductSanitationMultiplier = Fixed64.FromRaw(850_000); // 0.85.

    /// <summary>§3's Sewer — "a further real Disease &amp; Public Health improvement, distinct from an
    /// aqueduct's own clean-water contribution" — a larger reduction than the Aqueduct's own, since real
    /// sanitation (waste removal) historically mattered at least as much as clean-water supply.</summary>
    public static readonly Fixed64 SewerSanitationMultiplier = Fixed64.FromRaw(800_000); // 0.80.

    /// <summary>§3's Sewer — "a genuine Settlement Demographics Contentment boost for the District it
    /// actually serves." <see cref="Characters.PopGroupKey"/> carries no
    /// District attribution at all (confirmed by direct search of that key's own two-field shape), so
    /// this item honestly reads this as a settlement-wide bonus — any operational Sewer anywhere in the
    /// settlement — rather than fabricating a per-District PopGroup split this codebase has never
    /// modeled.</summary>
    public static readonly Fixed64 SewerContentmentBonus = Fixed64.FromRaw(30_000); // 0.03.

    /// <summary>§3's Road — "a real, direct improvement to Travel efficiency and... Trade Route
    /// effectiveness, reducing the felt cost of both." Both targets are confirmed, by direct search, to
    /// carry no live figure this item could actually multiply: Travel's own <see
    /// cref="Travel.DistanceTierCatalog"/> is an authored inter-region table, not a per-settlement
    /// mechanism a local Road could plausibly move, and <see cref="Economy.StandingContract.
    /// TradeRouteInvestment"/> is Phase 15 item 7/8's own already-confirmed "one-off commitment stub with
    /// no live effectiveness figure" (see <see
    /// cref="PrivateInfrastructure.PrivateInfrastructureCatalog.TradeProximityMonthlyBonus"/>'s own doc
    /// comment). This item follows that same item's own precedent instead: a real, felt monthly Treasury
    /// income credit standing in for reduced trade friction, posted directly to the settlement's own
    /// Treasury account.</summary>
    public static readonly Money RoadTreasuryMonthlyBonus = Money.FromDenarii(6);

    /// <summary>§3's Harbor — "a real, substantial improvement to a coastal settlement's own trade
    /// capacity." Resources &amp; Goods' own import/export flow carries no live per-settlement figure
    /// this item could multiply either (confirmed by direct search — no such flow exists in <c>Gens.
    /// Simulation.Goods</c>), so this item realizes the same kind of real Treasury income credit <see
    /// cref="RoadTreasuryMonthlyBonus"/> already does for Roads, sized larger per §3's own "substantial"
    /// framing.</summary>
    public static readonly Money HarborTreasuryMonthlyBonus = Money.FromDenarii(15);

    /// <summary>§3's Bridge — "feeding directly into Land Ownership &amp; Real Estate's own... District-
    /// value calculations for the newly-accessible area." <see cref="RealEstate.District.PropertyValue"/>
    /// is recomputed wholesale every month by <see cref="RealEstate.DistrictPropertyValueSystem"/> from
    /// its own named inputs, so a direct additive nudge to that field would simply be overwritten the
    /// same tick — this item instead realizes the bump the same way Private Infrastructure's own item 7
    /// already did for a newly-built structure (<see
    /// cref="PrivateInfrastructure.PrivateInfrastructureCatalog.PropertyValueBonusPerStructure"/>'s own
    /// doc comment): a one-time addition to every already-Districted Plot's own tracked <see
    /// cref="RealEstate.PlotPropertyExtension.Value"/> in the Bridge's linked District, through that
    /// item's own public <see cref="RealEstate.PlotPropertyResolver"/> — this item's own file is never
    /// touched.</summary>
    public static readonly Money BridgePropertyValueBonusPerPlot = Money.FromDenarii(30);

    /// <summary>§3's Marketplace/Basilica — "a real, direct boost to Economy &amp; Finance's own Market
    /// Dynamics and Notable Businesses' own available District-level Purchasing Power." Business
    /// Competition (Phase 15 item 5) already establishes that item as "the one Phase 15 item actually
    /// allowed to move <see cref="Markets.SettlementMarket.Price"/>," so this item never touches that
    /// field directly; Population Wealth &amp; Purchasing Power (Phase 15 item 10, this item's own very
    /// next roadmap item) is confirmed unbuilt, so no live District-level Purchasing Power figure exists
    /// yet either. This item's own real, concrete realization instead: a monthly Ledger income credit to
    /// every <see cref="NotableBusinesses.NotableBusiness"/> resolved to a real household in the
    /// Marketplace's own District — genuine new commerce capacity felt by the businesses actually working
    /// that District, per §3's own "giving local commerce genuine new capacity rather than only
    /// prestige."</summary>
    public static readonly Money MarketplaceBusinessMonthlyBonus = Money.FromDenarii(3);
}
