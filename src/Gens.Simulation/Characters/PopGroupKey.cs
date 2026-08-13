using Gens.Simulation.Identity;

namespace Gens.Simulation.Characters;

/// <summary>
/// The <see cref="State.WorldState.PopGroups"/> ordering key: settlement first, group type second —
/// the same "compare the first field, then the second as a tiebreak" shape <see cref="RelationshipKey"/>
/// and <see cref="State.ScheduledActionKey"/> already use, giving <see
/// cref="Identity.OrderedRegistry{TId,TEntity}"/>'s ascending-order guarantee (ADR 0004) a
/// deterministic "every pop group for settlement X, in a fixed group order" scan for free.
/// </summary>
public readonly record struct PopGroupKey(RuntimeId<Settlement> SettlementId, PopGroupType GroupType) : IComparable<PopGroupKey>
{
    public int CompareTo(PopGroupKey other)
    {
        var settlementComparison = SettlementId.CompareTo(other.SettlementId);
        return settlementComparison != 0 ? settlementComparison : GroupType.CompareTo(other.GroupType);
    }

    public static bool operator <(PopGroupKey left, PopGroupKey right) => left.CompareTo(right) < 0;
    public static bool operator >(PopGroupKey left, PopGroupKey right) => left.CompareTo(right) > 0;
    public static bool operator <=(PopGroupKey left, PopGroupKey right) => left.CompareTo(right) <= 0;
    public static bool operator >=(PopGroupKey left, PopGroupKey right) => left.CompareTo(right) >= 0;
}
