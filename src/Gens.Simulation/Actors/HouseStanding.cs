using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Actors;

/// <summary>The single tracked state between two <see cref="LivingWorldActor"/>s
/// (<c>gens-rival-houses-design.md</c> §5.2) — sits above individual relationship-web opinions, the
/// Faction concept applied at house level. Feuding authorizes Private Feuds (Military &amp; Combat §6,
/// unbuilt as of Phase 10); Allied unlocks joint ventures.</summary>
public enum HouseStandingLevel
{
    Allied,
    Neutral,
    Rivalrous,
    Feuding,
}

/// <summary>A standing modifier on a house-to-house relationship that outlives either house's current
/// Head, decaying far slower than ordinary opinion (<c>gens-rival-houses-design.md</c> §5.2) — left
/// only by a Feud resolving in Catastrophic Defeat, a battlefield death, or an execution (not Ransom).
/// Stores only <see cref="OriginEngagementId"/> and <see cref="OriginDate"/>, not a running magnitude
/// or a stored decay countdown: matching <c>Policies.HouseholdPolicyState</c>'s "derive elapsed time
/// from a recorded date, never store a countdown" idiom, the decay system that reads this (Phase 10
/// package 8) computes current magnitude from elapsed months against a versioned catalog rate.</summary>
/// <param name="OriginEngagementId">A plain string reference: no Military &amp; Combat engagement
/// record exists in this codebase yet (Phase 16), so this cannot be a typed <see cref="RuntimeId{T}"/>
/// — matching <see cref="LivingWorldActorMilitaryStrength.ResolvedForceId"/>'s identical convention.</param>
public readonly record struct AncestralGrudge(string OriginEngagementId, GameDate OriginDate);

/// <summary>The <see cref="Gens.Simulation.State.WorldState.HouseStandings"/> ordering key. Unlike
/// <see cref="Characters.RelationshipKey"/>, a <see cref="HouseStanding"/> is undirected — "a single
/// tracked state per house-pair" (§5.2) — so <see cref="Between"/> is the only way to construct one:
/// it always normalizes to (lower ID, higher ID) so <c>(A, B)</c> and <c>(B, A)</c> resolve to the
/// same key rather than silently becoming two independent entries.</summary>
public readonly record struct HouseStandingKey : IComparable<HouseStandingKey>
{
    private HouseStandingKey(RuntimeId<Actor> actorAId, RuntimeId<Actor> actorBId)
    {
        ActorAId = actorAId;
        ActorBId = actorBId;
    }

    public RuntimeId<Actor> ActorAId { get; }
    public RuntimeId<Actor> ActorBId { get; }

    public static HouseStandingKey Between(RuntimeId<Actor> x, RuntimeId<Actor> y)
    {
        if (x == y)
            throw new ArgumentException("A HouseStanding requires two distinct actors.", nameof(y));

        return x.CompareTo(y) <= 0 ? new HouseStandingKey(x, y) : new HouseStandingKey(y, x);
    }

    public int CompareTo(HouseStandingKey other)
    {
        var actorAComparison = ActorAId.CompareTo(other.ActorAId);
        return actorAComparison != 0 ? actorAComparison : ActorBId.CompareTo(other.ActorBId);
    }

    public static bool operator <(HouseStandingKey left, HouseStandingKey right) => left.CompareTo(right) < 0;
    public static bool operator >(HouseStandingKey left, HouseStandingKey right) => left.CompareTo(right) > 0;
    public static bool operator <=(HouseStandingKey left, HouseStandingKey right) => left.CompareTo(right) <= 0;
    public static bool operator >=(HouseStandingKey left, HouseStandingKey right) => left.CompareTo(right) >= 0;
}

/// <summary>One house-pair's tracked <see cref="HouseStandingLevel"/> plus its optional <see
/// cref="Grudge"/> (Phase 10 item 5; <c>gens-rival-houses-design.md</c> §9's <c>HouseStanding</c>).
/// Sparse and stored keyed by <see cref="HouseStandingKey"/>: an untracked pair simply has no entry —
/// see <see cref="HouseStandingResolver"/> for the default that applies to a missing entry.</summary>
public sealed record HouseStanding(HouseStandingLevel Standing, AncestralGrudge? Grudge = null);
