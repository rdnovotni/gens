using System.Linq;
#nullable enable
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Romance;

/// <summary>
/// The single shared gate every Romance mechanic — Affection/Attraction, courtship, the Seduce
/// Scheme, autonomous romance, pregnancy — must call before touching a pair
/// (<c>gens-romance-sexuality-lineage-design.md</c> §2, "The Two Hard Exclusions"). Two rules, each
/// its own named rejection code, are enforced here exactly once so no later command or system
/// reimplements — and risks subtly weakening — either check:
///
/// <list type="bullet">
/// <item><b>The Adult lifecycle gate.</b> Both participants must be at <see
/// cref="LifecycleStage.Adult"/> or <see cref="LifecycleStage.Elderly"/> — Adult is a floor, never a
/// ceiling. §2 states this plainly rather than leaving it to inference: this is a <i>deliberate
/// ethical override of real Roman legal practice, not an application of it</i>. Real Roman law
/// permitted marriage ages far below what this project judges responsible to model in mechanical or
/// numeric detail, and this gate exists precisely so no mechanic built on top of
/// <see cref="RomanticBond"/> can ever reach a Child or Adolescent Character, under any
/// circumstance — not trait rolls, not Scheme success, not autonomous-romance targeting. Historical
/// accuracy is this project's general default everywhere else; §2 is explicit that it is not the
/// operative value here, and there is no tension to resolve — this floor holds without exception.</item>
/// <item><b>The power-imbalance exclusion.</b> Neither participant may hold <see
/// cref="LegalStatus.Enslaved"/> status. §2's own framing is "owner-enslaved," but this codebase has
/// no independent ownership/authority primitive that would let this check distinguish an owner-enslaved
/// pairing from an enslaved-enslaved one — so this is deliberately a conservative superset, excluding
/// every pairing where either side is Enslaved, never only the owner-enslaved subset. A pairing
/// excluded here is not a pairing this document has nothing to say about: it is explicitly routed to
/// Labor &amp; Slavery's own Regimen and Contubernium framework (that document's §9) instead, which is
/// the correct, honest mechanism for both an owner-enslaved power imbalance and a genuine bond between
/// two enslaved individuals alike.</item>
/// </list>
///
/// Pure and read-only: no mutation, no RNG draw, callable freely from validation.
/// </summary>
public static class RomanceEligibility
{
    public static readonly ValidationErrorCode SelfPair = new("romance.eligibility.selfPair");
    public static readonly ValidationErrorCode CharacterANotFound = new("romance.eligibility.characterANotFound");
    public static readonly ValidationErrorCode CharacterBNotFound = new("romance.eligibility.characterBNotFound");
    public static readonly ValidationErrorCode CharacterADeceased = new("romance.eligibility.characterADeceased");
    public static readonly ValidationErrorCode CharacterBDeceased = new("romance.eligibility.characterBDeceased");
    public static readonly ValidationErrorCode CharacterBelowAdultLifecycleStage =
        new("romance.eligibility.characterBelowAdultLifecycleStage");
    public static readonly ValidationErrorCode PowerImbalancedPairing = new("romance.eligibility.powerImbalancedPairing");

    /// <summary>Checks both hard exclusions (plus existence/liveness) for one pair, in a fixed order
    /// so callers get a single, stable rejection code rather than needing to re-derive which check
    /// failed first. Returns <c>null</c> only when every check passes.</summary>
    public static ValidationErrorCode? CheckPair(
        WorldState state, RuntimeId<Character> aId, RuntimeId<Character> bId, GameDate asOf)
    {
        if (aId == bId)
            return SelfPair;
        if (!state.Characters.TryGet(aId, out var a))
            return CharacterANotFound;
        if (!state.Characters.TryGet(bId, out var b))
            return CharacterBNotFound;
        if (!a.IsAlive)
            return CharacterADeceased;
        if (!b.IsAlive)
            return CharacterBDeceased;

        var aStage = a.GetLifecycleStage(asOf);
        var bStage = b.GetLifecycleStage(asOf);
        if (aStage is not (LifecycleStage.Adult or LifecycleStage.Elderly)
            || bStage is not (LifecycleStage.Adult or LifecycleStage.Elderly))
            return CharacterBelowAdultLifecycleStage;

        if (a.LegalStatus == LegalStatus.Enslaved || b.LegalStatus == LegalStatus.Enslaved)
            return PowerImbalancedPairing;

        return null;
    }
}
