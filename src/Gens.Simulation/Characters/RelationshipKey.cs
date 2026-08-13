using Gens.Simulation.Identity;

namespace Gens.Simulation.Characters;

/// <summary>
/// The <see cref="State.WorldState.Relationships"/> ordering key: source Character first, target
/// second. Because a <see cref="Relationship"/> is directed, <c>(A, B)</c> and <c>(B, A)</c> are two
/// distinct keys that may independently exist, not exist, or hold different values — mirroring
/// <see cref="State.ScheduledActionKey"/>'s identical "compare the first field, then the second as a
/// tiebreak" shape so <see cref="Identity.OrderedRegistry{TId,TEntity}"/>'s ascending-order guarantee
/// (ADR 0004) gives a deterministic "all of A's outgoing relationships, in target order" scan for free.
/// </summary>
public readonly record struct RelationshipKey(RuntimeId<Character> From, RuntimeId<Character> To) : IComparable<RelationshipKey>
{
    public int CompareTo(RelationshipKey other)
    {
        var fromComparison = From.CompareTo(other.From);
        return fromComparison != 0 ? fromComparison : To.CompareTo(other.To);
    }

    public static bool operator <(RelationshipKey left, RelationshipKey right) => left.CompareTo(right) < 0;
    public static bool operator >(RelationshipKey left, RelationshipKey right) => left.CompareTo(right) > 0;
    public static bool operator <=(RelationshipKey left, RelationshipKey right) => left.CompareTo(right) <= 0;
    public static bool operator >=(RelationshipKey left, RelationshipKey right) => left.CompareTo(right) >= 0;
}
