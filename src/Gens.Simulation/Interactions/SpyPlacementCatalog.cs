using System.Linq;
#nullable enable
using Gens.Simulation.Ledger;
namespace Gens.Simulation.Interactions;

/// <summary>Numeric constants for <see cref="SpyPlacementProgressSystem"/>, <see
/// cref="PlaceSpyCommands"/>, and <see cref="CounterEspionageSweepCommands"/> (Phase 16 item 1;
/// <c>gens-espionage-design.md</c> §2, §5, §6). Every figure here is this codebase's own deliberately
/// unsized first pass — §10's Open Questions explicitly leaves "Discovery Risk curves, Traceability
/// weighting, Sweep costs, network upkeep, and the Spymaster's actual capacity cap number" unsized —
/// matching rule 10's "content is data, rules are code" applied to numeric tuning specifically, and
/// <see cref="SchemeProgressCatalog"/>'s identical "versioned and named, not left as magic numbers"
/// convention.</summary>
public static class SpyPlacementCatalog
{
    /// <summary>Percentage points of a <see cref="SpyPlacementType.PersistentNetwork"/>'s <see
    /// cref="SpyPlacement.DiscoveryRisk"/> gained per month regardless of the target's own vigilance,
    /// matching <see cref="SchemeProgressCatalog.BaseDiscoveryRiskPerMonthPercent"/>'s identical
    /// baseline.</summary>
    public const int BaseDiscoveryRiskPerMonthPercent = 3;

    /// <summary>Additional DiscoveryRisk percentage points per month, scaled by the target actor's head
    /// Character's own <see cref="Characters.CoreAttributes.Intrigue"/> (§6's "the target's own
    /// investigative capability") — a maximally Intrigue-rated head adds this many points on top of
    /// <see cref="BaseDiscoveryRiskPerMonthPercent"/>.</summary>
    public const int MaxTargetInvestigativeRiskBonusPercent = 6;

    /// <summary>How many DiscoveryRisk percentage points per month a maximally-<see
    /// cref="SpyPlacement.ConcealmentQuality"/>-rated placement shaves off the monthly delta before
    /// <see cref="MaxTargetInvestigativeRiskBonusPercent"/> — a well-concealed spy stays hidden longer
    /// (§2.2's "a higher-Intrigue spy... leaves a cleaner trail").</summary>
    public const int ConcealmentRiskReductionWeightPercent = 50;

    /// <summary>Once a <see cref="SpyPlacementType.PersistentNetwork"/>'s <see
    /// cref="SpyPlacement.DiscoveryRisk"/> reaches this threshold, Discovery resolves immediately (§6),
    /// matching <see cref="SchemeProgressCatalog.DiscoveryRiskThresholdPercent"/>'s identical figure.</summary>
    public const int DiscoveryRiskThresholdPercent = 70;

    /// <summary>A <see cref="SpyPlacementType.QuickOp"/>'s flat chance (0-100), before <see
    /// cref="SpyPlacement.ConcealmentQuality"/> is weighed, of being discovered on its single resolving
    /// tick (§2.1: "one roll, not an accumulating one").</summary>
    public const int QuickOpBaseDiscoveryChancePercent = 35;

    /// <summary>How many percentage points a maximally-<see cref="SpyPlacement.ConcealmentQuality"/>-rated
    /// spy shaves off <see cref="QuickOpBaseDiscoveryChancePercent"/>.</summary>
    public const int QuickOpDiscoveryConcealmentWeightPercent = 50;

    /// <summary>An undiscovered <see cref="SpyPlacementType.QuickOp"/>'s base chance (0-100) of a clean
    /// success — surviving discovery is necessary but not sufficient, matching <see
    /// cref="SchemeProgressCatalog.BaseSuccessChancePercent"/>'s identical reasoning.</summary>
    public const int QuickOpBaseSuccessChancePercent = 50;

    /// <summary>How many percentage points an undiscovered <see cref="SpyPlacementType.QuickOp"/>'s
    /// success chance shifts per point of the spy's own <see cref="SpyPlacement.ConcealmentQuality"/>.</summary>
    public const int QuickOpSuccessConcealmentWeightPercent = 30;

    /// <summary>The target's base chance (0-100), before either side's rating is weighed, that a
    /// discovered placement actually traces back to its sponsor (§6's Traceability roll) — a coin-flip
    /// default, matching <see cref="SchemeProgressCatalog.BaseCounterPlayFoilChancePercent"/>'s identical
    /// "no baseline given" reasoning.</summary>
    public const int BaseTraceabilityChancePercent = 50;

    /// <summary>How many percentage points the Traceability chance shifts per point of difference
    /// between the target's investigative capability and the placement's own <see
    /// cref="SpyPlacement.ConcealmentQuality"/> (target minus concealment, then multiplied by this and
    /// divided by 100) — a more capable target is likelier to trace a less-concealed spy, and vice
    /// versa (§6: "weighted by the spy's own concealment quality... against the target's own
    /// investigative capability").</summary>
    public const int TraceabilityConcealmentVsInvestigativeWeightPercent = 50;

    /// <summary>The flat <see cref="Money"/> cost (in denarii) of one <see
    /// cref="CounterEspionageSweepCommand"/> (§5's "spending time and Influence" — this slice charges
    /// only the denarii half, matching this codebase's convention of not yet wiring an Influence
    /// resource where one does not already exist).</summary>
    public const long SweepCostDenarii = 500;

    /// <summary>The investigator's base chance (0-100), before either side's rating is weighed, of a
    /// Sweep actually finding a real embedded placement it targets.</summary>
    public const int SweepSuccessBaseChancePercent = 50;

    /// <summary>How many percentage points a Sweep's success chance shifts per point of the targeted
    /// placement's own <see cref="SpyPlacement.ConcealmentQuality"/> (subtracted, so a better-concealed
    /// placement is harder to sweep out).</summary>
    public const int SweepSuccessConcealmentWeightPercent = 50;

    /// <summary>The monthly <see cref="Money"/> upkeep (in denarii) posted against a <see
    /// cref="SpyPlacementType.PersistentNetwork"/>'s sponsor for as long as it stays <see
    /// cref="SpyPlacementStatus.InProgress"/> (§2.2: "an embedded spy still needs paying, hiding, or
    /// otherwise supported").</summary>
    public const long PersistentNetworkUpkeepPerMonthDenarii = 50;

    /// <summary>The maximum number of concurrently <see cref="SpyPlacementStatus.InProgress"/>
    /// placements one sponsoring Character may run at once (§2.2's Spymaster capacity cap). A
    /// placeholder pending the real Household Spymaster mechanical bonus (§7, deferred past this
    /// slice) — structured so a future capacity-raising bonus is a single constant/lookup swap here,
    /// not a reshape of <see cref="PlaceSpyCommands"/>.</summary>
    public const int SpymasterCapacityCap = 3;
}
