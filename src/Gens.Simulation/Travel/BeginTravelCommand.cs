using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Regions;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Travel;

/// <summary>Commits a <see cref="TravelParty"/> to a trip (§3's "committing to a trip"): resolves the
/// route, reserves every party member (§5), and starts the outbound leg.</summary>
public sealed record BeginTravelCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> TravelerCharacterId,
    IReadOnlyList<RuntimeId<Character>> RetinueCharacterIds,
    DefinitionId<RegionProfileDefinition> HomeRegionId,
    TravelLocation Destination) : ICommand;

/// <summary>Emitted whenever a <see cref="BeginTravelCommand"/> is accepted.</summary>
public sealed record TravelBegunEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<TravelTrip> TripId,
    RuntimeId<Character> TravelerCharacterId,
    LocationKind DestinationKind,
    DistanceTier DistanceTier,
    int TravelTimeMonths,
    string? CausationId) : IDomainEvent
{
    public string Type => "travel.begun";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { TravelerCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="BeginTravelCommand"/> (ADR 0006). Built per
/// <see cref="RegionProfileCatalog"/>/<see cref="DistanceTierCatalog"/>, matching <see
/// cref="Events.FireEventCommands.BuildPipeline"/>'s identical "caller-loaded content, not embedded in
/// the save-state graph" shape.</summary>
public static class BeginTravelCommands
{
    public static readonly ValidationErrorCode TravelerNotFound = new("travel.begin.travelerNotFound");
    public static readonly ValidationErrorCode TravelerDeceased = new("travel.begin.travelerDeceased");
    public static readonly ValidationErrorCode RetinueMemberNotFound = new("travel.begin.retinueMemberNotFound");
    public static readonly ValidationErrorCode RetinueMemberDeceased = new("travel.begin.retinueMemberDeceased");
    public static readonly ValidationErrorCode RetinueMemberIsTraveler = new("travel.begin.retinueMemberIsTraveler");
    public static readonly ValidationErrorCode DuplicateRetinueMember = new("travel.begin.duplicateRetinueMember");
    public static readonly ValidationErrorCode PartyMemberAlreadyTraveling = new("travel.begin.partyMemberAlreadyTraveling");
    public static readonly ValidationErrorCode DestinationMustNotBeHome = new("travel.begin.destinationMustNotBeHome");
    public static readonly ValidationErrorCode DestinationUnsupported = new("travel.begin.destinationUnsupported");
    public static readonly ValidationErrorCode DestinationRegionRequired = new("travel.begin.destinationRegionRequired");
    public static readonly ValidationErrorCode CapitalRegionNotFound = new("travel.begin.capitalRegionNotFound");

    public static CommandPipeline<WorldState, BeginTravelCommand> BuildPipeline(RegionProfileCatalog regions, DistanceTierCatalog distanceTiers)
    {
        if (regions is null)
            throw new ArgumentNullException(nameof(regions));
        if (distanceTiers is null)
            throw new ArgumentNullException(nameof(distanceTiers));

        return new CommandPipeline<WorldState, BeginTravelCommand>(
            validate: (state, command) => Validate(state, command, regions),
            mutate: (state, command) => Mutate(state, command, regions, distanceTiers),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, BeginTravelCommand command, RegionProfileCatalog regions)
    {
        if (!state.Characters.TryGet(command.TravelerCharacterId, out var traveler))
            return TravelerNotFound;
        if (!traveler.IsAlive)
            return TravelerDeceased;

        if (command.RetinueCharacterIds.Contains(command.TravelerCharacterId))
            return RetinueMemberIsTraveler;
        if (command.RetinueCharacterIds.Distinct().Count() != command.RetinueCharacterIds.Count)
            return DuplicateRetinueMember;

        foreach (var retinueId in command.RetinueCharacterIds)
        {
            if (!state.Characters.TryGet(retinueId, out var member))
                return RetinueMemberNotFound;
            if (!member.IsAlive)
                return RetinueMemberDeceased;
        }

        if (TravelTripQueries.IsReserved(state, command.TravelerCharacterId))
            return PartyMemberAlreadyTraveling;
        foreach (var retinueId in command.RetinueCharacterIds)
        {
            if (TravelTripQueries.IsReserved(state, retinueId))
                return PartyMemberAlreadyTraveling;
        }

        if (command.Destination.Kind == LocationKind.Home)
            return DestinationMustNotBeHome;
        if (command.Destination.Kind == LocationKind.Campaign)
            return DestinationUnsupported;

        if (command.Destination.Kind == LocationKind.Rome)
        {
            if (!regions.All().Any(region => region.Gazetteer.Any(entry => entry.Roles.Contains(GazetteerRole.Capital))))
                return CapitalRegionNotFound;
        }
        else if (command.Destination.RegionId is null)
        {
            return DestinationRegionRequired;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, BeginTravelCommand command, RegionProfileCatalog regions, DistanceTierCatalog distanceTiers)
    {
        state.Characters.TryGet(command.TravelerCharacterId, out var traveler);

        var party = TravelParty.Create(command.TravelerCharacterId, command.RetinueCharacterIds);
        var origin = TravelLocation.Home(traveler.Location);
        var route = TravelRoute.Resolve(origin, command.Destination, command.HomeRegionId, regions, distanceTiers);

        var tripId = state.TravelTripIds.Issue();
        var trip = TravelTrip.Begin(tripId, party, route, command.SubmittedDate);
        state.TravelTrips.Add(tripId, trip);

        // CurrentTravelLocation is left null (§10: "defaults to a 'home' Location") while a leg is
        // actually underway — it is set once the party genuinely arrives somewhere (§7's Arrival), by
        // TravelProgressSystem, and cleared again once the trip completes.

        return new IDomainEvent[]
        {
            new TravelBegunEvent(
                state.EventIds.Issue(), command.SubmittedDate, tripId, command.TravelerCharacterId,
                route.Destination.Kind, route.DistanceTier, route.TravelTimeMonths, command.CommandId.ToTaggedString()),
        };
    }
}
