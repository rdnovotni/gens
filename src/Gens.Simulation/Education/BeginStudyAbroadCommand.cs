using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Regions;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Education;

/// <summary>
/// Begins a Study Abroad Journey (Phase 17 item 2; §4): creates the underlying <see cref="TravelTrip"/>
/// against <see cref="LocationKind.InstitutionOfRenown"/> exactly like any other <see
/// cref="Travel.BeginTravelCommand"/> trip, plus this domain's own <see cref="StudyAbroadJourney"/> side
/// record. No special guard excludes the player's own actively-controlled Character — Travel already
/// treats unavailability uniformly via <see cref="TravelTripQueries.IsReserved"/>, per this ticket's own
/// scope note.
/// </summary>
public sealed record BeginStudyAbroadCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> TravelerCharacterId,
    RuntimeId<Household> HouseholdId,
    DefinitionId<InstitutionOfRenown> InstitutionId) : ICommand;

/// <summary>Emitted whenever a <see cref="BeginStudyAbroadCommand"/> is accepted.</summary>
public sealed record StudyAbroadBegunEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<TravelTrip> TripId,
    RuntimeId<Character> TravelerCharacterId,
    DefinitionId<InstitutionOfRenown> InstitutionId,
    string? CausationId) : IDomainEvent
{
    public string Type => "education.studyAbroadBegun";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { TravelerCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="BeginStudyAbroadCommand"/> (ADR 0006). Built per
/// <see cref="RegionProfileCatalog"/>/<see cref="DistanceTierCatalog"/>, matching <see
/// cref="Travel.BeginTravelCommands.BuildPipeline"/>'s identical "caller-loaded content, not embedded in
/// the save-state graph" shape — neither catalog is actually consulted for an Institution destination
/// (see <see cref="TravelRoute.Resolve"/>'s own dedicated branch), but both are still required so this
/// command's own signature stays uniform with every other Travel-issuing command.</summary>
public static class BeginStudyAbroadCommands
{
    public static readonly ValidationErrorCode TravelerNotFound = new("education.beginStudyAbroad.travelerNotFound");
    public static readonly ValidationErrorCode TravelerDeceased = new("education.beginStudyAbroad.travelerDeceased");
    public static readonly ValidationErrorCode TravelerAlreadyTraveling = new("education.beginStudyAbroad.travelerAlreadyTraveling");
    public static readonly ValidationErrorCode UnknownInstitution = new("education.beginStudyAbroad.unknownInstitution");

    public static CommandPipeline<WorldState, BeginStudyAbroadCommand> BuildPipeline(
        RegionProfileCatalog regions, DistanceTierCatalog distanceTiers)
    {
        if (regions is null)
            throw new ArgumentNullException(nameof(regions));
        if (distanceTiers is null)
            throw new ArgumentNullException(nameof(distanceTiers));

        return new CommandPipeline<WorldState, BeginStudyAbroadCommand>(
            validate: Validate,
            mutate: (state, command) => Mutate(state, command, regions, distanceTiers),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, BeginStudyAbroadCommand command)
    {
        if (!state.Characters.TryGet(command.TravelerCharacterId, out var traveler))
            return TravelerNotFound;
        if (!traveler!.IsAlive)
            return TravelerDeceased;
        if (TravelTripQueries.IsReserved(state, command.TravelerCharacterId))
            return TravelerAlreadyTraveling;
        if (!KnownInstitutionsOfRenown.Catalog.TryGet(command.InstitutionId, out _))
            return UnknownInstitution;

        return null;
    }

    private static IDomainEvent[] Mutate(
        WorldState state, BeginStudyAbroadCommand command, RegionProfileCatalog regions, DistanceTierCatalog distanceTiers)
    {
        state.Characters.TryGet(command.TravelerCharacterId, out var traveler);

        var party = TravelParty.Create(command.TravelerCharacterId);
        var origin = TravelLocation.Home(traveler!.Location);
        var destination = TravelLocation.InstitutionOfRenown(command.InstitutionId);
        // homeRegionId is unused by TravelRoute.Resolve's Institution branch; the traveler's own current
        // Settlement's region has no reachable catalog entry here, so this passes the destination's own
        // (equally unused) region slot rather than inventing a lookup this command has no real need for.
        var route = TravelRoute.Resolve(origin, destination, homeRegionId: default, regions, distanceTiers);

        var tripId = state.TravelTripIds.Issue();
        var trip = TravelTrip.Begin(tripId, party, route, command.SubmittedDate);
        state.TravelTrips.Add(tripId, trip);
        state.StudyAbroadJourneys.Add(
            tripId, new StudyAbroadJourney(tripId, command.TravelerCharacterId, command.InstitutionId, command.HouseholdId));

        return new IDomainEvent[]
        {
            new StudyAbroadBegunEvent(
                state.EventIds.Issue(), command.SubmittedDate, tripId, command.TravelerCharacterId, command.InstitutionId,
                command.CommandId.ToTaggedString()),
        };
    }
}
