using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Travel;

/// <summary>§5's Reservation mechanism: a Character already away on a trip cannot be booked onto a
/// second, concurrent one. "Away" means any non-terminal <see cref="TravelTripStatus"/> — <see
/// cref="TravelTripStatus.Completed"/> is the only status that frees a Character back up.</summary>
public static class TravelTripQueries
{
    public static bool IsReserved(WorldState state, RuntimeId<Character> characterId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.TravelTrips.InAscendingOrder())
        {
            if (entry.Value.Status == TravelTripStatus.Completed)
                continue;
            if (entry.Value.Party.AllMembers.Contains(characterId))
                return true;
        }

        return false;
    }
}
