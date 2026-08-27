using Gens.Simulation.Actors;

namespace Gens.Simulation.Succession;

/// <summary>
/// Every numeric baseline <c>gens-succession-dynasty-design.md</c> §10 leaves as an Open Question
/// (Dignitas/Loyalty amounts, drama-toggle weighting, Regency duration, splinter-house terms) — this
/// implementation's own invented values, matching how <see cref="Actors.RivalAmbitionCatalog"/> and
/// <see cref="Actors.LivingWorldActorDriftCatalog"/> invent baselines for their own phase's untuned
/// numbers. A plain C# static class, not content-authored JSON: no compiled-content-to-runtime-catalog
/// loader exists yet for any content family (see <see cref="Actors.RivalHouseCreationService"/>'s own
/// doc comment), so this follows that same established convention rather than building one solely for
/// Succession's numbers.
/// </summary>
public static class SuccessionCatalog
{
    /// <summary>How many months a <see cref="SuccessionDispute"/> stays <see
    /// cref="SuccessionDisputeStatus.Pending"/> before <see cref="SuccessionDisputeResolutionSystem"/>
    /// resolves it (§5.2's "the contest" as a process, not an instant).</summary>
    public const int DisputeResolutionMonths = 6;

    /// <summary>The monthly chance (out of 100) that a dead head with more than one eligible heir and
    /// no Formal Declaration triggers a <see cref="SuccessionDispute"/> rather than a quiet handoff to
    /// the default-order heir (§5.1's layered triggers, collapsed to one flat roll for this phase's
    /// scope).</summary>
    public const uint DisputeTriggerChancePercent = 20;

    /// <summary>The chance (out of 100) that the runner-up claimant in a resolved dispute founds an
    /// independent splinter Household (§5.3) instead of simply losing and remaining in the original
    /// Household.</summary>
    public const uint SplinterHouseChancePercent = 40;

    /// <summary>The share (out of 100) of the original Household's Denarii ledger balance a splinter
    /// claimant takes with them (§5.3's "a losing claimant can take a share").</summary>
    public const long SplinterHouseAssetSharePercent = 25;

    /// <summary>The Loyalty penalty a disowned Character's own Condition takes (§2.3's "damages
    /// opinion" — this implementation's own scoped stand-in: the full relationship-graph opinion
    /// adjustment against "everyone who stayed loyal" that §2.3 describes needs a per-pair
    /// Relationship write for every remaining pool member, which is deferred pending a concrete case
    /// that needs it).</summary>
    public const int DisownedLoyaltyPenalty = 25;

    /// <summary>The age (in whole years) below which a chosen heir is a minor under §6.2 Regency
    /// rather than an outright new head — the same Adult-stage floor <see
    /// cref="Characters.LifecycleStage"/> already uses.</summary>
    public const int MinimumAdultAgeYears = 18;
}
