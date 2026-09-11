using System.Linq;
#nullable enable
namespace Gens.Simulation.Diplomacy;

/// <summary>Numeric constants for Phase 16 item 5 slice 1 (<c>gens-diplomacy-non-roman-peoples-design.md</c>
/// §6, §14). Every figure here is this codebase's own deliberately unsized first pass — §14's Open
/// Questions explicitly leaves "Interpreter Quality's exact modifier values, Diplomatic Gift costs/
/// effects... unsized" — matching <see cref="Interactions.RaidThreatCatalog"/>'s identical "versioned
/// and named, not left as magic numbers" convention.</summary>
public static class FrontierDiplomacyCatalog
{
    /// <summary>How many accumulated <see cref="PerPeopleStanding.Goodwill"/> points cross one
    /// <see cref="Actors.HouseStandingLevel"/> tier step, in either direction.</summary>
    public const int GoodwillPerTierStep = 100;

    /// <summary>The flat <see cref="Ledger.Money"/> cost, in denarii, of a minimum-value Diplomatic
    /// Gift (§6) — a caller may send a larger gift for proportionally more goodwill.</summary>
    public const long MinimumGiftDenarii = 50;

    /// <summary>Goodwill gained per <see cref="MinimumGiftDenarii"/> of gift value sent, before <see
    /// cref="FrontierNegotiationQuality.QualityModifierPercent"/> scales it — §6's "a modest, real
    /// Standing improvement."</summary>
    public const int GiftGoodwillPerMinimumUnit = 5;

    /// <summary>The base chance (0-100), before negotiation quality/treaty-type/standing modifiers, that
    /// a <see cref="ProposeFrontierTreatyCommand"/> succeeds — a coin-flip default, matching <see
    /// cref="Interactions.RaidThreatCatalog.BaseInterceptionChancePercent"/>'s identical "no baseline
    /// given" reasoning.</summary>
    public const int NegotiationBaseSuccessChancePercent = 50;

    /// <summary>Percentage points added when <see cref="FrontierNegotiationQualitySource.CulturalFamiliarity"/>
    /// conducts the negotiation (§5's full-strength, no-penalty baseline).</summary>
    public const int CulturalFamiliarityBonusPercent = 20;

    /// <summary>Percentage points added when <see cref="FrontierNegotiationQualitySource.NegotiatorFluency"/>
    /// conducts the negotiation — the same full-strength baseline as Cultural Familiarity (§5 treats
    /// both as "full-strength" once the gate clears on fluency); this codebase keeps them numerically
    /// equal for now, leaving Cultural Familiarity's own distinct payoff to whatever future pass adds
    /// Cultural Drift's second acquisition path.</summary>
    public const int NegotiatorFluencyBonusPercent = 20;

    /// <summary>Percentage points subtracted when only an Interpres (formal or informal) clears the
    /// gate — §5's "closes most, but not all, of the gap."</summary>
    public const int InterpresPenaltyPercent = 10;

    /// <summary>Percentage points added or subtracted per <see cref="Actors.HouseStandingLevel"/> step
    /// away from Neutral — Allied is friendlier ground for a negotiation, Rivalrous/Feuding harder.</summary>
    public const int StandingSuccessChancePercentPerTierTowardAllied = 10;

    /// <summary>The default treaty term, in months, applied by <see cref="ProposeFrontierTreatyCommand"/>
    /// when accepted.</summary>
    public const int DefaultTreatyTermMonths = 60;

    /// <summary>Goodwill lost when a Tribute treaty's monthly payment cannot be posted for insufficient
    /// funds (§7's "diplomatic failure" theme, applied narrowly here — the treaty itself is not broken,
    /// only strained).</summary>
    public const int TributeArrearsGoodwillPenalty = 15;

    /// <summary>Goodwill lost when a household abrogates its own active treaty early.</summary>
    public const int AbrogationGoodwillPenalty = 40;

    /// <summary>Goodwill gained on a successfully concluded treaty, before the negotiation-quality
    /// modifier is applied.</summary>
    public const int TreatyConcludedGoodwillGain = 30;

    /// <summary>Goodwill lost on a rejected treaty proposal.</summary>
    public const int TreatyRejectedGoodwillLoss = 10;
}
