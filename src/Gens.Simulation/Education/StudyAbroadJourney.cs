using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Education;

/// <summary>
/// The Education-domain side-record for a Study Abroad Journey (Phase 17 item 2; §4, §12's own
/// <c>StudyAbroadJourney</c> model), keyed by the underlying <see cref="TravelTrip"/>'s own <see
/// cref="RuntimeId{T}"/> rather than minting a redundant id of its own — matching <see
/// cref="Languages.LiteracyRecord"/>'s identical "the owning entity is already a unique key" precedent,
/// here applied to a Travel trip instead of a Character. <see cref="MonthsAtInstitution"/> accrues once
/// <see cref="TravelTripStatus.Arrived"/> (see <see cref="StudyAbroadProgressSystem"/>); the underlying
/// <see cref="TravelTrip.MonthsElapsed"/> already counts the outbound/return legs, so this domain tracks
/// only the time actually spent at the institution.
/// </summary>
public sealed record StudyAbroadJourney(
    RuntimeId<TravelTrip> TripId,
    RuntimeId<Character> CharacterId,
    DefinitionId<InstitutionOfRenown> InstitutionId,
    RuntimeId<Household> HouseholdId,
    int MonthsAtInstitution = 0);

/// <summary>Read-side helpers over <see cref="WorldState.StudyAbroadJourneys"/>.</summary>
public static class StudyAbroadJourneyResolver
{
    public static bool TryGet(WorldState state, RuntimeId<TravelTrip> tripId, out StudyAbroadJourney journey) =>
        state.StudyAbroadJourneys.TryGet(tripId, out journey);
}
