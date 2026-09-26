using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Activities;

/// <summary>
/// Whether a Character is physically able to be at an Activity's Venue. Reads active <see
/// cref="TravelTrip"/> membership rather than <see cref="Character.CurrentTravelLocation"/> alone:
/// <see cref="BeginTravelCommand"/> deliberately leaves that field null while a leg is underway, so it
/// cannot tell a Character in transit from one at home. A Character on any non-<see
/// cref="TravelTripStatus.Completed"/> trip can attend only once that trip has <see
/// cref="TravelTripStatus.Arrived"/> at the Venue's own settlement (arriving somewhere to attend an
/// Activity already underway — the design doc's §10 Travel bullet); a Character not travelling is
/// treated as able to attend, travel to a gathering being abstracted like every other local social
/// overture in this codebase.
/// </summary>
public static class ActivityAvailability
{
    public static bool CanAttend(WorldState state, RuntimeId<Character> characterId, RuntimeId<Settlement> venueSettlementId)
    {
        foreach (var entry in state.TravelTrips.InAscendingOrder())
        {
            var trip = entry.Value;
            if (trip.Status == TravelTripStatus.Completed || !trip.Party.AllMembers.Contains(characterId))
                continue;

            return trip.Status == TravelTripStatus.Arrived &&
                   state.Characters.TryGet(characterId, out var character) &&
                   character.CurrentTravelLocation?.SettlementId == venueSettlementId;
        }

        return true;
    }
}
