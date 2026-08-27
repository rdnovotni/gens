using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Succession;

/// <summary>
/// A Household's succession bookkeeping (Phase 11 item 1; <c>gens-succession-dynasty-design.md</c>
/// §2-§4): the free/reversible default preference (§2.1), the Formal Declaration (§2.2), Disownment
/// (§2.3), and the two ways a Character joins the eligible-heir pool beyond ordinary legitimate birth
/// (§3's acknowledged-illegitimate and adopted children, §4). One entry per Household, sparse — a
/// Household with none of these ever recorded simply has no entry, matching <see
/// cref="State.WorldState.HouseholdPolicies"/>'s identical "no entry means the catalog/derived
/// default" convention; <see cref="Succession.HeirEligibilityService"/> treats a missing entry as "no
/// preference, no adoptions, no acknowledgments, nobody disowned" rather than throwing.
/// </summary>
/// <param name="PreferredHeirId">§2.1's quiet, always-adjustable default — set/cleared freely by <see
/// cref="SetPreferredHeirCommand"/>.</param>
/// <param name="FormallyDeclaredHeirId">§2.2's Curia announcement — takes priority over <see
/// cref="PreferredHeirId"/> when resolving who inherits, set by <see cref="DeclareHeirCommand"/>.</param>
/// <param name="DeclaredDate">When <see cref="FormallyDeclaredHeirId"/> was declared, or <c>null</c> if
/// none has ever been made.</param>
/// <param name="DisownedCharacterIds">§2.3: every Character this head has disowned. Removed from the
/// eligible-heir pool entirely — reconciliation (undoing a disownment) is an Open Question §10 leaves
/// unresolved, so this implementation offers no command to remove an entry once added.</param>
/// <param name="AdoptedChildIds">§4: every Character this head has adopted, standing identically to a
/// birth child in the eligible-heir pool (§3's "Adopted children — identical standing").</param>
/// <param name="AcknowledgedIllegitimateChildIds">§3: every Illegitimate birth child this head has
/// formally acknowledged, moving them into the eligible-heir pool — an unacknowledged illegitimate
/// child never enters it.</param>
public sealed record HeirDesignation(
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character>? PreferredHeirId,
    RuntimeId<Character>? FormallyDeclaredHeirId,
    GameDate? DeclaredDate,
    IReadOnlyList<RuntimeId<Character>> DisownedCharacterIds,
    IReadOnlyList<RuntimeId<Character>> AdoptedChildIds,
    IReadOnlyList<RuntimeId<Character>> AcknowledgedIllegitimateChildIds)
{
    public static HeirDesignation Empty(RuntimeId<Household> householdId) => new(
        householdId, PreferredHeirId: null, FormallyDeclaredHeirId: null, DeclaredDate: null,
        DisownedCharacterIds: Array.Empty<RuntimeId<Character>>(),
        AdoptedChildIds: Array.Empty<RuntimeId<Character>>(),
        AcknowledgedIllegitimateChildIds: Array.Empty<RuntimeId<Character>>());
}
