using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.NotableBusinesses;
using Gens.Simulation.Numerics;
using Gens.Simulation.Societates;

namespace Gens.Simulation.BusinessCompetition;

/// <summary>Versioned constants for Phase 15 item 5's Business Competition mechanics
/// (<c>gens-business-competition-design.md</c>), matching <see
/// cref="NotableBusinesses.NotableBusinessesCatalog"/>'s and every other Phase 15 item's identical
/// "unsized against real playtest data, but named in one place" convention — §10's Open Questions
/// explicitly leaves "all numeric sizing — escalation thresholds between rungs, cartel discovery
/// probability, and market saturation's own exact formula" unsized.</summary>
public static class BusinessCompetitionCatalog
{
    // ---- §2 The Competitive Escalation Ladder --------------------------------------------------

    /// <summary>§2 rung 2's "cuts prices below its own comfortable margin" — this item's own invented
    /// per-month downward nudge applied to the aggressor's own <see
    /// cref="Markets.SettlementMarket.Price"/> for its <see
    /// cref="NotableBusinesses.NotableBusiness.OutputGoodId"/>, only where that resolves to a real
    /// cleared market. Deliberately small and clamped against <see
    /// cref="Markets.MarketPriceBoundConfig.Default"/>'s own 15%/month move ceiling (see <see
    /// cref="CompetitiveEscalationSystem"/>) so this never itself produces an invariant-violating
    /// price swing.</summary>
    public static readonly Fixed64 PriceWarPriceNudgeFraction = Fixed64.FromRaw(30_000); // 0.03

    /// <summary>§2 rung 3's steeper cut — Predatory Pricing is a harder, more sustained push than an
    /// ordinary Price War.</summary>
    public static readonly Fixed64 PredatoryPricingPriceNudgeFraction = Fixed64.FromRaw(70_000); // 0.07

    /// <summary>§2 rung 3's "this carries real risk for the aggressor too — sustained below-margin
    /// operation is a genuine drain on their own finances" — a real monthly Ledger cost posted against
    /// the aggressor while at Predatory Pricing specifically (rung 2's Price War is framed as merely
    /// "reduced profit," not a separate posted cost this item invents a figure for).</summary>
    public static readonly Money PredatoryPricingMonthlyDrain = Money.FromDenarii(25);

    // ---- §3 Breaking Ranks ----------------------------------------------------------------------

    /// <summary>§3's "a real, separate reputational hit from Notable Businesses' own ordinary
    /// competitive consequences" — this item's own invented per-escalation Dignitas penalty on the
    /// instigating household, applied once per actual rung advance run against a fellow Collegium
    /// member (not a one-time flat charge).</summary>
    public const int BreakingRanksDignitasPenalty = 6;

    /// <summary>§3's "social pressure" half — a real Opinion penalty between the two rivals' own
    /// resolved owner Characters, tagged <see cref="BondTag.Rival"/> matching <see
    /// cref="NotableBusinesses.RecordBusinessRivalryActionCommands"/>' own closest analog (that command
    /// grants no bond at all; breaking ranks against a guild brother is graver, per §3's own framing,
    /// so this item grants one here specifically).</summary>
    public const int BreakingRanksOpinionPenalty = -15;

    // ---- §4 Cartels and Market-Sharing Agreements ----------------------------------------------

    /// <summary>§4's "any participant's own Ambition or Greed... creates a real, standing temptation to
    /// secretly break the agreement" — reuses <see cref="SocietatesCatalog.GreedyTraitId"/>
    /// directly (cross-domain reuse, matching that item's own reuse of <see
    /// cref="RealEstate.RealEstateCatalog"/>'s Loyalty threshold) rather than a second, redundant
    /// content-authored Trait meaning the identical thing.</summary>
    public static readonly DefinitionId<Trait> GreedyTraitId = SocietatesCatalog.GreedyTraitId;

    /// <summary>The Ambition (Core Condition) floor this item reads as "temptation," matching <see
    /// cref="SocietatesCatalog.EarlyExitAmbitionThreshold"/>'s own identical reasoning and
    /// magnitude for a comparable Reactive-Trait-driven betrayal risk.</summary>
    public const int CartelDefectionAmbitionThreshold = SocietatesCatalog.EarlyExitAmbitionThreshold;

    /// <summary>§4's real Reputation consequence once a cartel is exposed — every participant's own
    /// Reputation takes a real, felt hit, sized against <see
    /// cref="NotableBusinessesCatalog.BusinessScandalReputationLoss"/>'s own
    /// magnitude for a comparably severe, discovered wrongdoing.</summary>
    public const int CartelDiscoveryReputationLoss = NotableBusinessesCatalog.BusinessScandalReputationLoss;

    // ---- §5 Grain Hoarding ----------------------------------------------------------------------

    /// <summary>§5's "a real risk of mob violence directly against the business and its owner" — this
    /// item's own invented one-time Ledger loss (property damage), posted against the owner's own
    /// tracked account the first month hoarding coincides with a real shortage.</summary>
    public static readonly Money MobViolencePropertyDamage = Money.FromDenarii(150);

    /// <summary>§5's own real, felt Reputation consequence, distinct from and larger than an ordinary
    /// Supply Failure — grain hoarding during a real shortage is "this project's own single most severe
    /// form of economic misconduct."</summary>
    public const int GrainHoardingReputationLoss = 25;

    // ---- §6 Market Entry and Saturation -----------------------------------------------------------

    /// <summary>§6's qualitative saturation read — this item's own invented business-count-per-
    /// Employment-Ratio-point thresholds. Below <see cref="UndersaturatedBusinessCountCeiling"/> at a
    /// healthy (&gt;=1) Employment Ratio reads Undersaturated; at or above <see
    /// cref="SaturatedBusinessCountFloor"/>, or any count at all once Employment Ratio has fallen under
    /// 1 (jobs are already scarce), reads Saturated; everything between reads Balanced.</summary>
    public const int UndersaturatedBusinessCountCeiling = 1;

    public const int SaturatedBusinessCountFloor = 4;
}
