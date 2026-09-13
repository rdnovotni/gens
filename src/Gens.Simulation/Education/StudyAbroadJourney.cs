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
/// <param name="StudyCompleted">True once the full <see cref="InstitutionOfRenown.JourneyDurationMonths"/>
/// stay has been completed and the underlying <see cref="TravelTrip"/> has been sent home on its return
/// leg (correctness fix: <see cref="StudyAbroadProgressSystem"/> used to grant the permanent <see
/// cref="CharacterInstitutionCredential"/> at this same moment — while the traveler was still mid-return,
/// an entire travel leg outstanding). While this is false, an <see cref="TravelTripStatus"/> transition
/// away from <see cref="TravelTripStatus.Arrived"/> means the trip left early (<c>RecallTravelCommand</c>
/// or an early <c>BeginReturnCommand</c>, cut short before the stay finished) and this journey record is
/// cleaned up with no credential; while true, the same system instead watches for the trip's return leg to
/// reach <see cref="TravelTripStatus.Completed"/> before finally granting the credential and cleaning up.</param>
public sealed record StudyAbroadJourney(
    RuntimeId<TravelTrip> TripId,
    RuntimeId<Character> CharacterId,
    DefinitionId<InstitutionOfRenown> InstitutionId,
    RuntimeId<Household> HouseholdId,
    int MonthsAtInstitution = 0,
    bool StudyCompleted = false);

/// <summary>Read-side helpers over <see cref="WorldState.StudyAbroadJourneys"/>.</summary>
public static class StudyAbroadJourneyResolver
{
    public static bool TryGet(WorldState state, RuntimeId<TravelTrip> tripId, out StudyAbroadJourney journey) =>
        state.StudyAbroadJourneys.TryGet(tripId, out journey);
}
