using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Doctrine;

/// <summary>The <see cref="State.WorldState.HouseholdDoctrines"/> ordering key: household first,
/// Doctrine type second, matching <see cref="Characters.HouseholdRegimenKey"/>'s identical "compare the
/// first field, then the second as a tiebreak" shape.</summary>
public readonly record struct HouseholdDoctrineKey(RuntimeId<Household> HouseholdId, HouseholdDoctrineType DoctrineType)
    : IComparable<HouseholdDoctrineKey>
{
    public int CompareTo(HouseholdDoctrineKey other)
    {
        var householdComparison = HouseholdId.CompareTo(other.HouseholdId);
        return householdComparison != 0 ? householdComparison : ((int)DoctrineType).CompareTo((int)other.DoctrineType);
    }

    public static bool operator <(HouseholdDoctrineKey left, HouseholdDoctrineKey right) => left.CompareTo(right) < 0;
    public static bool operator >(HouseholdDoctrineKey left, HouseholdDoctrineKey right) => left.CompareTo(right) > 0;
    public static bool operator <=(HouseholdDoctrineKey left, HouseholdDoctrineKey right) => left.CompareTo(right) <= 0;
    public static bool operator >=(HouseholdDoctrineKey left, HouseholdDoctrineKey right) => left.CompareTo(right) >= 0;
}

/// <summary>
/// One household's standing against one <see cref="HouseholdDoctrineType"/> (Phase 12 item 9; §3.1).
/// Sparse, per-(household, Doctrine) partition — present only once <see
/// cref="DoctrineResolutionSystem"/> has actually touched it, matching <see
/// cref="Reputation.HouseholdReputation"/>'s identical "no entry means the default" convention (here,
/// <see cref="AffinityScore"/> 0 and <see cref="Tier"/> <see cref="DoctrineTier.None"/>).
///
/// <see cref="AffinityScore"/> is clamped 0-100 per §3.1's own "hidden Affinity score (0-100)" —
/// unlike Dignitas's deliberately unclamped total, matching <see
/// cref="Clientela.HouseholdInfluence"/>'s own "a bounded, resettable figure, not a reputation score"
/// reasoning applied to a 0-100 range instead of a zero floor.
/// </summary>
/// <param name="CapstoneUnlocked">§3.1: true once <see cref="Tier"/> has ever reached <see
/// cref="DoctrineTier.Defining"/> — kept even if Affinity later decays back below that threshold,
/// since a capstone once earned is not narratively un-earned by a quiet month; only <see
/// cref="CapstoneUsedThisGeneration"/> actually gates a real capstone command from firing twice.</param>
/// <param name="CapstoneUsedThisGeneration">§9's own <c>capstoneUsedThisGeneration</c> field. This item
/// sets it once a capstone command fires and never clears it — §3.3's own per-generation reset needs a
/// real succession-event hook this item does not build (see <see
/// cref="DoctrineResolutionSystem"/>'s own doc comment for why Apex itself is the deliberate cut that
/// reset would exist to serve); until that exists, a household earns each real capstone exactly once
/// per campaign rather than once per generation, a narrower but honestly-stated version of §3.1's own
/// "capstone" framing.</param>
public sealed record HouseholdDoctrineState(
    RuntimeId<Household> HouseholdId,
    HouseholdDoctrineType DoctrineType,
    int AffinityScore,
    DoctrineTier Tier,
    bool CapstoneUnlocked = false,
    bool CapstoneUsedThisGeneration = false);

/// <summary>Read-side helpers over <see cref="WorldState.HouseholdDoctrines"/>, matching <see
/// cref="Reputation.DignitasResolver"/>'s identical "no entry means the default" and "replace, don't
/// mutate in place" conventions.</summary>
public static class HouseholdDoctrineResolver
{
    public static HouseholdDoctrineState Current(WorldState state, RuntimeId<Household> householdId, HouseholdDoctrineType type)
    {
        var key = new HouseholdDoctrineKey(householdId, type);
        return state.HouseholdDoctrines.TryGet(key, out var entry)
            ? entry!
            : new HouseholdDoctrineState(householdId, type, AffinityScore: 0, Tier: DoctrineTier.None);
    }

    /// <summary>Replaces (remove-then-add) a household's stored state for one Doctrine, creating the
    /// entry if none exists yet — matching every other immutable-record partition in <see
    /// cref="WorldState"/>.</summary>
    public static void Set(WorldState state, HouseholdDoctrineState next)
    {
        var key = new HouseholdDoctrineKey(next.HouseholdId, next.DoctrineType);
        if (state.HouseholdDoctrines.TryGet(key, out _))
            state.HouseholdDoctrines.Remove(key);
        state.HouseholdDoctrines.Add(key, next);
    }
}
