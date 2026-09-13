using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;

namespace Gens.Simulation.Romance;

/// <summary>
/// The <see cref="State.WorldState.RomanticBonds"/> ordering key. Unlike <see
/// cref="RelationshipKey"/> — which is directed because two Characters can hold two independent,
/// even contradictory, opinions of each other — a <see cref="RomanticBond"/>'s Affection and
/// Attraction are one shared, mutual value for the pair, not per-direction (§3 of
/// <c>gens-romance-sexuality-lineage-design.md</c> reads a bond's two scores as belonging to the
/// pairing itself, never to "A's feelings toward B" versus "B's feelings toward A" as separate
/// records). So this key is deliberately undirected: <see cref="Create"/> is the only supported way
/// to build one, and it always canonicalizes so the numerically smaller <see cref="RuntimeId{T}"/>
/// occupies <see cref="CharacterAId"/> — callers never need to try both orderings to find an existing
/// bond, and <c>(A, B)</c> and <c>(B, A)</c> can never exist as two separate entries.
/// </summary>
public readonly record struct RomanticBondKey(RuntimeId<Character> CharacterAId, RuntimeId<Character> CharacterBId)
    : IComparable<RomanticBondKey>
{
    /// <summary>The only supported construction path — canonicalizes the pair so the smaller ID is
    /// always <see cref="CharacterAId"/>, regardless of the order the caller happens to know the two
    /// Characters in.</summary>
    public static RomanticBondKey Create(RuntimeId<Character> a, RuntimeId<Character> b) =>
        a <= b ? new RomanticBondKey(a, b) : new RomanticBondKey(b, a);

    public int CompareTo(RomanticBondKey other)
    {
        var aComparison = CharacterAId.CompareTo(other.CharacterAId);
        return aComparison != 0 ? aComparison : CharacterBId.CompareTo(other.CharacterBId);
    }

    public static bool operator <(RomanticBondKey left, RomanticBondKey right) => left.CompareTo(right) < 0;
    public static bool operator >(RomanticBondKey left, RomanticBondKey right) => left.CompareTo(right) > 0;
    public static bool operator <=(RomanticBondKey left, RomanticBondKey right) => left.CompareTo(right) <= 0;
    public static bool operator >=(RomanticBondKey left, RomanticBondKey right) => left.CompareTo(right) >= 0;
}
