namespace Gens.Simulation.Schemes;

/// <summary>Numeric constants for <see cref="SchemeProgressSystem"/> (Phase 10 item 12). §10's own
/// text never sizes a progress rate, a discovery-risk curve, or a counter-play window; this catalog is
/// where that original engineering choice lives, matching every other Phase 10 catalog's identical
/// convention.</summary>
public static class SchemeProgressCatalog
{
    /// <summary>Base monthly Progress gain (0-100 scale) before the initiator's Intrigue/Boldness
    /// contribution.</summary>
    public const int BaseProgressPerMonth = 8;

    /// <summary>Extra Progress per month at the initiator's maximum Intrigue (0-100).</summary>
    public const int IntrigueProgressWeight = 10;

    /// <summary>Extra Progress per month at the initiator's maximum Boldness axis score (rescaled to
    /// 0-100).</summary>
    public const int BoldnessProgressWeight = 6;

    /// <summary>Base monthly Discovery Risk gain (0-100 scale) — a scheme is never perfectly
    /// undetectable no matter how skilled the initiator (§10 stage 3: "rises with scheme duration").</summary>
    public const int BaseDiscoveryPerMonth = 5;

    /// <summary>Extra Discovery Risk per month at the target's maximum Intrigue (0-100) — the more
    /// perceptive the target, the faster suspicion builds.</summary>
    public const int TargetIntrigueDiscoveryWeight = 8;

    /// <summary>Extra Discovery Risk per month when an <see cref="SchemeInstance.AssistingAgentCharacterId"/>
    /// is involved (§10 stage 3: "assisting client = leak risk").</summary>
    public const int AssistingAgentDiscoveryBonus = 4;

    /// <summary>The Discovery Risk (0-100) at which the target's suspicion crosses the threshold and
    /// the scheme moves to <see cref="SchemeStage.AwaitingCounterPlay"/> (§10 stage 4).</summary>
    public const int DiscoveryThreshold = 70;

    /// <summary>How many months a target has to submit a <see cref="CounterPlaySchemeCommand"/> once
    /// <see cref="SchemeStage.AwaitingCounterPlay"/> begins, before the scheme resolves on its own.</summary>
    public const int CounterPlayWindowMonths = 3;

    /// <summary>Percent chance (0-100) a scheme that reaches Progress 100 actually succeeds, at
    /// Progress exactly 100 — success is likely but never certain even at full Progress.</summary>
    public const int MaxSuccessChancePercent = 85;
}
