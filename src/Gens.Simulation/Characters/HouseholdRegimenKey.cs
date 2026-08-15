using Gens.Simulation.Identity;

namespace Gens.Simulation.Characters;

/// <summary>The <see cref="State.WorldState.HouseholdRegimenDefaults"/> ordering key: household first,
/// duty slot second, the same "compare the first field, then the second as a tiebreak" shape <see
/// cref="PopGroupKey"/> and <see cref="RelationshipKey"/> already use. <see cref="Slot"/> of
/// <c>null</c> is the whole-household fallback group (<c>gens-labor-slavery-design.md</c> §5: "all
/// Household Slaves"); a specific slot is a per-duty group default (e.g. "all Field Hands") and sorts
/// after the household-wide entry.</summary>
public readonly record struct HouseholdRegimenKey(RuntimeId<Household> HouseholdId, DutySlot? Slot) : IComparable<HouseholdRegimenKey>
{
    public int CompareTo(HouseholdRegimenKey other)
    {
        var householdComparison = HouseholdId.CompareTo(other.HouseholdId);
        if (householdComparison != 0)
            return householdComparison;
        if (Slot is null)
            return other.Slot is null ? 0 : -1;
        if (other.Slot is null)
            return 1;
        return ((int)Slot.Value).CompareTo((int)other.Slot.Value);
    }

    public static bool operator <(HouseholdRegimenKey left, HouseholdRegimenKey right) => left.CompareTo(right) < 0;
    public static bool operator >(HouseholdRegimenKey left, HouseholdRegimenKey right) => left.CompareTo(right) > 0;
    public static bool operator <=(HouseholdRegimenKey left, HouseholdRegimenKey right) => left.CompareTo(right) <= 0;
    public static bool operator >=(HouseholdRegimenKey left, HouseholdRegimenKey right) => left.CompareTo(right) >= 0;
}
