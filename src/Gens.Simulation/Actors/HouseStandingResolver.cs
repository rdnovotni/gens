using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Actors;

/// <summary>Resolves the <see cref="HouseStandingLevel"/> actually in effect between two actors,
/// matching <see cref="Policies.HouseholdPolicyResolver"/>'s identical "no entry yet means the
/// catalog default, not an error" convention — <see cref="WorldState.HouseStandings"/> is sparse: a
/// pair that has never interacted simply has no entry, and defaults to <see
/// cref="HouseStandingLevel.Neutral"/>.</summary>
public static class HouseStandingResolver
{
    public static HouseStandingLevel GetEffectiveStanding(WorldState state, RuntimeId<Actor> actorAId, RuntimeId<Actor> actorBId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var key = HouseStandingKey.Between(actorAId, actorBId);
        return state.HouseStandings.TryGet(key, out var standing) ? standing!.Standing : HouseStandingLevel.Neutral;
    }
}
