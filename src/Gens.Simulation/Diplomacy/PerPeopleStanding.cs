using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Actors;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Diplomacy;

/// <summary>The <see cref="WorldState.PerPeopleStandings"/> ordering key: a household's standing with
/// one Foreign People (<c>gens-diplomacy-non-roman-peoples-design.md</c> §4). Unlike <see
/// cref="HouseStandingKey"/>, this key is directed rather than normalized — a player household is never
/// itself a <see cref="LivingWorldActor"/> (see <see cref="Crime.RansomNegotiationResolver.TryFindActorForHousehold"/>'s
/// own doc comment), so the two sides are different entity kinds and there is no "lower/higher ID"
/// symmetry to normalize away.</summary>
public readonly record struct PerPeopleStandingKey(
    RuntimeId<Household> HouseholdId, RuntimeId<Actor> ForeignPeopleActorId) : IComparable<PerPeopleStandingKey>
{
    public int CompareTo(PerPeopleStandingKey other)
    {
        var householdComparison = HouseholdId.CompareTo(other.HouseholdId);
        return householdComparison != 0 ? householdComparison : ForeignPeopleActorId.CompareTo(other.ForeignPeopleActorId);
    }

    public static bool operator <(PerPeopleStandingKey left, PerPeopleStandingKey right) => left.CompareTo(right) < 0;
    public static bool operator >(PerPeopleStandingKey left, PerPeopleStandingKey right) => left.CompareTo(right) > 0;
    public static bool operator <=(PerPeopleStandingKey left, PerPeopleStandingKey right) => left.CompareTo(right) <= 0;
    public static bool operator >=(PerPeopleStandingKey left, PerPeopleStandingKey right) => left.CompareTo(right) >= 0;
}

/// <summary>One household's tracked standing with one Foreign People (§4: "a household can hold
/// genuinely different Standing with different neighboring peoples simultaneously, using Rival Houses'
/// own tiered scale") — reuses <see cref="HouseStandingLevel"/> verbatim rather than a parallel enum,
/// plus a repeatable <see cref="Goodwill"/> accumulator so a lighter-weight action (Diplomatic Gifts)
/// can nudge standing without needing to cross a full tier every time. Sparse and stored keyed by <see
/// cref="PerPeopleStandingKey"/>: an untracked pair simply has no entry — see <see
/// cref="PerPeopleStandingResolver"/> for the default that applies to a missing entry.</summary>
public sealed record PerPeopleStanding(
    RuntimeId<Household> HouseholdId,
    RuntimeId<Actor> ForeignPeopleActorId,
    HouseStandingLevel Standing,
    int Goodwill,
    GameDate LastChangedDate);

/// <summary>Resolves the <see cref="HouseStandingLevel"/> actually in effect between a household and a
/// Foreign People, matching <see cref="HouseStandingResolver"/>'s identical "no entry yet means the
/// catalog default, not an error" convention.</summary>
public static class PerPeopleStandingResolver
{
    public static PerPeopleStanding GetEffective(WorldState state, RuntimeId<Household> householdId, RuntimeId<Actor> foreignPeopleActorId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var key = new PerPeopleStandingKey(householdId, foreignPeopleActorId);
        return state.PerPeopleStandings.TryGet(key, out var standing)
            ? standing
            : new PerPeopleStanding(householdId, foreignPeopleActorId, HouseStandingLevel.Neutral, Goodwill: 0, LastChangedDate: default);
    }
}

/// <summary>The single internal write path for <see cref="PerPeopleStanding"/> — both commands
/// (<see cref="AdjustPerPeopleStandingCommand"/>, <see cref="SendDiplomaticGiftCommand"/>) and future
/// systems (raiding, rival competition) apply goodwill/tier changes through this one helper rather than
/// each re-deriving the tier-step arithmetic independently.</summary>
public static class PerPeopleStandingMutator
{
    /// <summary>Adds <paramref name="goodwillDelta"/> to the pair's running <see
    /// cref="PerPeopleStanding.Goodwill"/> and steps <see cref="PerPeopleStanding.Standing"/> one tier
    /// toward Allied or Rivalry/Feuding whenever the accumulator crosses a <see
    /// cref="FrontierDiplomacyCatalog.GoodwillPerTierStep"/> threshold in that direction, then resets the
    /// accumulator to the remainder — matching a simple odometer-rollover shape rather than an unbounded
    /// running total.</summary>
    public static PerPeopleStanding Apply(WorldState state, PerPeopleStandingKey key, int goodwillDelta, GameDate date)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var existing = PerPeopleStandingResolver.GetEffective(state, key.HouseholdId, key.ForeignPeopleActorId);
        var goodwill = existing.Goodwill + goodwillDelta;
        var standing = existing.Standing;

        while (goodwill >= FrontierDiplomacyCatalog.GoodwillPerTierStep && standing != HouseStandingLevel.Allied)
        {
            standing = (HouseStandingLevel)((int)standing - 1);
            goodwill -= FrontierDiplomacyCatalog.GoodwillPerTierStep;
        }

        while (goodwill <= -FrontierDiplomacyCatalog.GoodwillPerTierStep && standing != HouseStandingLevel.Feuding)
        {
            standing = (HouseStandingLevel)((int)standing + 1);
            goodwill += FrontierDiplomacyCatalog.GoodwillPerTierStep;
        }

        // At either extreme, goodwill can still run the other way (working back toward Neutral) but
        // never further past the extreme it is already at — there is no tier left to step into.
        if (standing == HouseStandingLevel.Allied)
            goodwill = Math.Min(goodwill, 0);
        if (standing == HouseStandingLevel.Feuding)
            goodwill = Math.Max(goodwill, 0);

        var updated = new PerPeopleStanding(key.HouseholdId, key.ForeignPeopleActorId, standing, goodwill, date);
        if (state.PerPeopleStandings.TryGet(key, out _))
            state.PerPeopleStandings.Remove(key);
        state.PerPeopleStandings.Add(key, updated);
        return updated;
    }
}
