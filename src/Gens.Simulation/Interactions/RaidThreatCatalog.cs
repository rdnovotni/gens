using System.Linq;
#nullable enable
namespace Gens.Simulation.Interactions;

/// <summary>Numeric constants for <see cref="RaidThreatSystem"/> and <see
/// cref="AdjustEstateSecurityInvestmentCommands"/> (Phase 16 item 2; <c>gens-piracy-banditry-design.md</c>
/// §2, §3, §9). Every figure here is this codebase's own deliberately unsized first pass — §12's Open
/// Questions explicitly leaves "raid-trigger frequency" and "the security-investment-to-risk curve"
/// unsized — matching rule 10's "content is data, rules are code" applied to numeric tuning specifically,
/// and <see cref="SpyPlacementCatalog"/>'s identical "versioned and named, not left as magic numbers"
/// convention.</summary>
public static class RaidThreatCatalog
{
    /// <summary>A <see cref="Actors.LivingWorldActorType.BanditConfederation"/>'s base chance (0-100),
    /// before <see cref="Actors.LivingWorldActor.StandingTrend"/> or <see
    /// cref="Actors.LivingWorldActor.MilitaryStrength"/> are weighed, of attempting a raid against some
    /// household in its own region this month (§3).</summary>
    public const int BaseRaidChancePercent = 8;

    /// <summary>Percentage points added to <see cref="BaseRaidChancePercent"/> when the Confederation's
    /// own <see cref="Actors.LivingWorldActorStandingTrend"/> is <see
    /// cref="Actors.LivingWorldActorStandingTrend.Rising"/> — §2's "bolder, more frequent... raids as it
    /// grows in strength and confidence".</summary>
    public const int RisingTrendRaidChanceBonusPercent = 6;

    /// <summary>Percentage points subtracted from <see cref="BaseRaidChancePercent"/> when the
    /// Confederation's own trend is <see cref="Actors.LivingWorldActorStandingTrend.Declining"/> — §2's
    /// "weaker and more risk-averse until it either rebuilds or fades out".</summary>
    public const int DecliningTrendRaidChancePenaltyPercent = 5;

    /// <summary>Percentage points added to the raid chance per <see
    /// cref="Actors.MilitaryStrengthBand"/> step above <see
    /// cref="Actors.MilitaryStrengthBand.Negligible"/> — a stronger Confederation raids more readily.</summary>
    public const int MilitaryStrengthBandRaidChanceBonusPercent = 4;

    /// <summary>The defender's base chance (0-100), before <see
    /// cref="EstateSecurityInvestment.SecurityLevel"/> or the raider's own strength are weighed, of
    /// intercepting a raid outright (§3's "a well-defended target can repel or capture the raiders
    /// outright") — a coin-flip default, matching <see
    /// cref="SpyPlacementCatalog.BaseTraceabilityChancePercent"/>'s identical "no baseline given"
    /// reasoning.</summary>
    public const int BaseInterceptionChancePercent = 30;

    /// <summary>How many percentage points a maximally-invested (<see
    /// cref="EstateSecurityInvestment.MaxValue"/>) household's <see
    /// cref="EstateSecurityInvestment.SecurityLevel"/> adds to <see
    /// cref="BaseInterceptionChancePercent"/> — §9's "far more likely to repel one outright... if
    /// targeted anyway".</summary>
    public const int MaxSecurityInterceptionBonusPercent = 55;

    /// <summary>Percentage points subtracted from the interception chance per <see
    /// cref="Actors.MilitaryStrengthBand"/> step the raiding Confederation carries above <see
    /// cref="Actors.MilitaryStrengthBand.Negligible"/> — a stronger raiding party is harder to repel.</summary>
    public const int MilitaryStrengthBandInterceptionPenaltyPercent = 8;

    /// <summary>Given a raid was intercepted, the chance (0-100) the raiders are actually captured
    /// rather than merely repelled and driven off (§3: "a captured raider is a real Character... sale
    /// into slavery"; this slice tracks the capture outcome without yet generating that Character — see
    /// <see cref="RaidOutcome.RaidersCaptured"/>'s own doc comment).</summary>
    public const int CaptureGivenInterceptedChancePercent = 40;

    /// <summary>The base <see cref="Ledger.Money"/> loss, in denarii, a successful raid inflicts on its
    /// target household before <see cref="EstateSecurityInvestment.SecurityLevel"/> reduces it (§3, §8's
    /// goods/livestock loss).</summary>
    public const long BaseSpoilsLostDenarii = 400;

    /// <summary>How many percentage points of <see cref="BaseSpoilsLostDenarii"/> a maximally-invested
    /// household's own <see cref="EstateSecurityInvestment.SecurityLevel"/> shaves off a successful
    /// raid's actual loss — a well-defended household that still loses a raid loses less than an
    /// undefended one.</summary>
    public const int MaxSecuritySpoilsReductionPercent = 60;

    /// <summary>Multiplier (percent of <see cref="BaseSpoilsLostDenarii"/>) applied when <see
    /// cref="RaidTargetType.Settlement"/> is the roll outcome rather than <see
    /// cref="RaidTargetType.TradeGoods"/> or <see cref="RaidTargetType.Livestock"/> — a direct strike on
    /// the settlement itself is the heavier of the three (§3's "or a settlement directly").</summary>
    public const int SettlementRaidSpoilsMultiplierPercent = 175;

    /// <summary>Percent chance (0-100), after any raid resolution, that the Confederation's own <see
    /// cref="Actors.LivingWorldActorStandingTrend"/> steps one notch toward <see
    /// cref="Actors.LivingWorldActorStandingTrend.Rising"/> on an uncontested <see
    /// cref="RaidOutcome.RaidSucceeded"/>, or toward <see
    /// cref="Actors.LivingWorldActorStandingTrend.Declining"/> on <see
    /// cref="RaidOutcome.InterceptedRepelled"/>/<see cref="RaidOutcome.RaidersCaptured"/> — §2's "ignoring
    /// a real threat indefinitely is never a neutral, cost-free choice" realized as a direct outcome-fed
    /// nudge, rather than <see cref="Actors.BackgroundHouseDriftSystem"/>'s unrelated ambient roll (which
    /// only ever considers <see cref="Actors.LivingWorldActorType.Gens"/> actors).</summary>
    public const int StandingTrendShiftChancePercent = 20;

    /// <summary>The flat <see cref="Ledger.Money"/> cost (in denarii) of raising a household's <see
    /// cref="EstateSecurityInvestment.SecurityLevel"/> by one point via <see
    /// cref="AdjustEstateSecurityInvestmentCommand"/> (§9's "the defensive investment"). Lowering the
    /// level is free — this slice does not model recovering spent coin from stood-down guards.</summary>
    public const long SecurityLevelCostPerPointDenarii = 20;
}
