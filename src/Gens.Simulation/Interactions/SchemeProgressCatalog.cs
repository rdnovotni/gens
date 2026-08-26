namespace Gens.Simulation.Interactions;

/// <summary>Numeric constants for <see cref="SchemeProgressSystem"/> (Phase 10 item 6's Scheme engine;
/// <c>gens-characters-design.md</c> §10). Every figure here is this codebase's own deliberately
/// unsized first pass — §10 itself specifies the shape of the formula (Intrigue/Boldness-driven
/// progress, risk that rises with elapsed time and awareness) but never the actual rates — matching
/// rule 10's "content is data, rules are code" applied to numeric tuning specifically, and
/// <see cref="Actors.LivingWorldActorDriftCatalog"/>'s identical "this is where that original
/// engineering choice lives, versioned and named" convention.</summary>
public static class SchemeProgressCatalog
{
    /// <summary>Percentage points of <see cref="Scheme.Progress"/> gained per month regardless of the
    /// initiator's own Intrigue (§10.2's "advances by an amount driven by... Core Attribute").</summary>
    public const int BaseProgressPerMonthPercent = 4;

    /// <summary>Additional Progress percentage points per month, scaled by the initiator's <see
    /// cref="Characters.CoreAttributes.Intrigue"/> (0-100) — a maximally Intrigue-rated initiator adds
    /// this many points on top of <see cref="BaseProgressPerMonthPercent"/>.</summary>
    public const int MaxIntrigueProgressBonusPercent = 8;

    /// <summary>Percentage points of <see cref="Scheme.DiscoveryRisk"/> gained per month regardless of
    /// the target's own vigilance (§10.3's "rises the longer a Scheme runs").</summary>
    public const int BaseDiscoveryRiskPerMonthPercent = 3;

    /// <summary>Additional DiscoveryRisk percentage points per month, scaled by the target's own <see
    /// cref="Characters.CoreAttributes.Intrigue"/> (§10.3's "scaled against the target's own
    /// Intrigue") — this codebase has no Perceptive/Oblivious trait check wired to a numeric score yet,
    /// so the target's Intrigue attribute alone stands in for that half of §10.3's formula for now.</summary>
    public const int MaxTargetIntrigueRiskBonusPercent = 6;

    /// <summary>Once <see cref="Scheme.DiscoveryRisk"/> reaches this threshold, the target's suspicion
    /// has "crossed a threshold" (§10.4) and counter-play resolves the Scheme immediately, regardless
    /// of how much <see cref="Scheme.Progress"/> it had made.</summary>
    public const int DiscoveryRiskThresholdPercent = 70;

    /// <summary>The target's base chance (0-100), before either party's Intrigue is weighed, of
    /// foiling a Scheme once discovery risk crosses <see cref="DiscoveryRiskThresholdPercent"/> — a
    /// coin-flip default since §10.4 gives no baseline of its own.</summary>
    public const int BaseCounterPlayFoilChancePercent = 50;

    /// <summary>How many percentage points the counter-play foil chance shifts per point of Intrigue
    /// difference between target and initiator (target's Intrigue minus initiator's, then multiplied by
    /// this and divided by 100) — a more Intrigue-capable target is likelier to foil a less capable
    /// initiator, and vice versa.</summary>
    public const int CounterPlayIntrigueDifferenceWeightPercent = 50;

    /// <summary>The initiator's base chance (0-100) of a clean success once <see cref="Scheme.Progress"/>
    /// reaches 100 without discovery risk ever crossing <see cref="DiscoveryRiskThresholdPercent"/> —
    /// completing the plan is necessary but not sufficient (§10.2 describes Progress as advancing
    /// *toward* the attempt, not guaranteeing its result).</summary>
    public const int BaseSuccessChancePercent = 50;

    /// <summary>How many percentage points the success chance shifts per point of the initiator's own
    /// Intrigue (0-100) once Progress has completed cleanly.</summary>
    public const int SuccessChanceIntrigueWeightPercent = 30;
}
