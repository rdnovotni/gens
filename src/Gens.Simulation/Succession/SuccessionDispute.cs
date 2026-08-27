using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Succession;

/// <summary>A <see cref="SuccessionDispute"/>'s lifecycle position (§5.2-§5.3).</summary>
public enum SuccessionDisputeStatus
{
    Pending,

    /// <summary>The head passed to whichever claimant <see cref="SuccessionDisputeResolutionSystem"/>
    /// favored (§5.2's "the player can favor a side... via the ordinary Interaction Catalog" —
    /// simplified here to the deterministic favor-score resolution that system's own doc comment
    /// describes).</summary>
    ResolvedByFavor,

    /// <summary>A losing claimant founded an independent splinter Household instead of simply losing
    /// (§5.3's "the bitter mirror of Rival Houses §2.2's peaceful cadet-branch split").</summary>
    ResolvedBySplinter,
}

/// <summary>
/// One contested succession (Phase 11 item 1; <c>gens-succession-dynasty-design.md</c> §5.2): opened
/// by <see cref="SuccessionHandoffSystem"/> when a dead head leaves more than one eligible heir and no
/// Formal Declaration to settle it, resolved <see cref="SuccessionCatalog.DisputeResolutionMonths"/>
/// later by <see cref="SuccessionDisputeResolutionSystem"/>. Immutable, like every other <c>WorldState</c>
/// record — a status change replaces the entry in <see cref="State.WorldState.SuccessionDisputes"/>
/// rather than mutating it in place, matching <see cref="Interactions.Scheme"/>'s identical convention.
/// Kept once resolved rather than removed (matching <see cref="Interactions.Scheme"/>'s own "resolved
/// or not, kept for the campaign's lifetime" convention).
/// </summary>
public sealed record SuccessionDispute(
    RuntimeId<SuccessionDispute> DisputeId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> DeceasedHeadId,
    IReadOnlyList<RuntimeId<Character>> ClaimantIds,
    GameDate OpenedDate,
    GameDate ResolutionDueDate,
    SuccessionDisputeStatus Status,
    RuntimeId<Character>? WinnerCharacterId,
    RuntimeId<Character>? SplinterClaimantId,
    RuntimeId<Household>? SplinterHouseholdId)
{
    public bool IsResolved => Status != SuccessionDisputeStatus.Pending;
}
