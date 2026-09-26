using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Activities;

/// <summary>
/// Convenes an Activity (Phase 17 item 4; <c>gens-activities-activity-engine-design.md</c> §2-§5): a
/// specific host, Type, Venue, Guest List, and start month. Sends every Invitation (§4.1) as <see
/// cref="ActivityRsvpStatus.Pending"/>; <see cref="ActivityProgressSystem"/> resolves whichever are
/// still unanswered, applies §4.2's exclusion snubs, and posts <see cref="Budget"/> when the Activity
/// actually begins — so cancelling beforehand (<see cref="CancelActivityCommand"/>) costs nothing and
/// snubs no one. Scale is derived here, once, from Guest List size and Venue (§5.1: "set by the host at
/// planning time... not rolled").
///
/// Exactly one of <see cref="HostHouseholdId"/> (a household host — the player's own) or <see
/// cref="HostActorId"/> (§8's NPC host, whose <see cref="LivingWorldActor.HeadCharacterId"/> must be
/// <see cref="HostCharacterId"/>) is set. A household host's <see cref="ActivityVenueKind.VillaRoom"/>
/// must name a real room in a real <see cref="Holding"/>'s <see cref="Villas.Villa"/>, whose tier
/// becomes the Venue tier; an NPC host's Villa room is its own untracked residence (tier 1).
/// </summary>
public sealed record PlanActivityCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> HostCharacterId,
    RuntimeId<Household>? HostHouseholdId,
    RuntimeId<Actor>? HostActorId,
    string TypeKey,
    ActivityVenueKind VenueKind,
    string VenueKey,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Holding>? HoldingId,
    GameDate StartDate,
    int DurationMonths,
    IReadOnlyList<RuntimeId<Character>> GuestIds,
    IReadOnlyList<ActivityQualityInput> QualityInputs,
    Money Budget) : ICommand;

/// <summary>Emitted whenever a <see cref="PlanActivityCommand"/> is accepted. Public — sending
/// invitations is itself a social fact (§4.1: "sending an Invitation is a real, meaningful act").</summary>
public sealed record ActivityPlannedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Activity> ActivityId,
    string TypeKey,
    RuntimeId<Character> HostCharacterId,
    RuntimeId<Household>? HostHouseholdId,
    RuntimeId<Actor>? HostActorId,
    ActivityScaleTier Scale,
    GameDate StartDate,
    int GuestCount,
    string? CausationId) : IDomainEvent
{
    public string Type => "activities.planned";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HostCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="PlanActivityCommand"/> (ADR 0006).</summary>
public static class PlanActivityCommands
{
    public static readonly ValidationErrorCode UnknownType = new("activities.plan.unknownType");
    public static readonly ValidationErrorCode HostNotFound = new("activities.plan.hostNotFound");
    public static readonly ValidationErrorCode HostDeceased = new("activities.plan.hostDeceased");
    public static readonly ValidationErrorCode HostNotAdult = new("activities.plan.hostNotAdult");
    public static readonly ValidationErrorCode HostOwnerAmbiguous = new("activities.plan.hostOwnerAmbiguous");
    public static readonly ValidationErrorCode HostNotHouseholdMember = new("activities.plan.hostNotHouseholdMember");
    public static readonly ValidationErrorCode HostActorNotFound = new("activities.plan.hostActorNotFound");
    public static readonly ValidationErrorCode HostNotActorHead = new("activities.plan.hostNotActorHead");
    public static readonly ValidationErrorCode HostAlreadyHosting = new("activities.plan.hostAlreadyHosting");
    public static readonly ValidationErrorCode StartDateInPast = new("activities.plan.startDateInPast");
    public static readonly ValidationErrorCode InvalidDuration = new("activities.plan.invalidDuration");
    public static readonly ValidationErrorCode VenueKindNotAllowed = new("activities.plan.venueKindNotAllowed");
    public static readonly ValidationErrorCode VenueKeyRequired = new("activities.plan.venueKeyRequired");
    public static readonly ValidationErrorCode SettlementNotFound = new("activities.plan.settlementNotFound");
    public static readonly ValidationErrorCode HoldingRequired = new("activities.plan.holdingRequired");
    public static readonly ValidationErrorCode HoldingNotFound = new("activities.plan.holdingNotFound");
    public static readonly ValidationErrorCode HoldingNotInSettlement = new("activities.plan.holdingNotInSettlement");
    public static readonly ValidationErrorCode VillaRoomNotFound = new("activities.plan.villaRoomNotFound");
    public static readonly ValidationErrorCode NoGuests = new("activities.plan.noGuests");
    public static readonly ValidationErrorCode GuestNotFound = new("activities.plan.guestNotFound");
    public static readonly ValidationErrorCode GuestDeceased = new("activities.plan.guestDeceased");
    public static readonly ValidationErrorCode HostOnGuestList = new("activities.plan.hostOnGuestList");
    public static readonly ValidationErrorCode DuplicateGuest = new("activities.plan.duplicateGuest");
    public static readonly ValidationErrorCode QualityInputsMismatch = new("activities.plan.qualityInputsMismatch");
    public static readonly ValidationErrorCode QualityInputOutOfRange = new("activities.plan.qualityInputOutOfRange");
    public static readonly ValidationErrorCode InvalidBudget = new("activities.plan.invalidBudget");

    public static readonly CommandPipeline<WorldState, PlanActivityCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, PlanActivityCommand command)
    {
        if (!ActivityTypeCatalog.TryGet(command.TypeKey, out var type))
            return UnknownType;
        if (!state.Characters.TryGet(command.HostCharacterId, out var host))
            return HostNotFound;
        if (!host.IsAlive)
            return HostDeceased;
        if (host.GetLifecycleStage(command.SubmittedDate) < LifecycleStage.Adult)
            return HostNotAdult;
        if ((command.HostHouseholdId is null) == (command.HostActorId is null))
            return HostOwnerAmbiguous;
        if (command.HostHouseholdId is { } householdId && host.Household != householdId)
            return HostNotHouseholdMember;
        if (command.HostActorId is { } actorId)
        {
            if (!state.Actors.TryGet(actorId, out var actor))
                return HostActorNotFound;
            if (actor!.HeadCharacterId != command.HostCharacterId)
                return HostNotActorHead;
        }

        if (ActivityResolver.OpenActivityHostedBy(state, command.HostCharacterId) is not null)
            return HostAlreadyHosting;
        if (command.StartDate.TotalMonths < command.SubmittedDate.TotalMonths)
            return StartDateInPast;
        if (command.DurationMonths < type.MinimumMonths || command.DurationMonths > type.MaximumMonths)
            return InvalidDuration;

        var venueError = ValidateVenue(state, command, type);
        if (venueError is not null)
            return venueError;

        if (command.GuestIds is null || command.GuestIds.Count == 0)
            return NoGuests;
        var seen = new HashSet<RuntimeId<Character>>();
        foreach (var guestId in command.GuestIds)
        {
            if (guestId == command.HostCharacterId)
                return HostOnGuestList;
            if (!seen.Add(guestId))
                return DuplicateGuest;
            if (!state.Characters.TryGet(guestId, out var guest))
                return GuestNotFound;
            if (!guest.IsAlive)
                return GuestDeceased;
        }

        if (command.QualityInputs is null || command.QualityInputs.Count != type.QualityInputs.Count)
            return QualityInputsMismatch;
        foreach (var definition in type.QualityInputs)
        {
            var matches = command.QualityInputs.Where(input => string.Equals(input.Key, definition.Key, StringComparison.Ordinal)).ToArray();
            if (matches.Length != 1)
                return QualityInputsMismatch;
            if (matches[0].Score is < 0 or > 100)
                return QualityInputOutOfRange;
        }

        if (command.Budget.IsNegative || (command.HostActorId is not null && command.Budget != Money.Zero))
            return InvalidBudget;

        return null;
    }

    private static ValidationErrorCode? ValidateVenue(WorldState state, PlanActivityCommand command, ActivityTypeDefinition type)
    {
        if (!type.AllowedVenueKinds.Contains(command.VenueKind))
            return VenueKindNotAllowed;
        if (string.IsNullOrWhiteSpace(command.VenueKey))
            return VenueKeyRequired;
        if (!state.Settlements.TryGet(command.SettlementId, out _))
            return SettlementNotFound;

        if (command.VenueKind != ActivityVenueKind.VillaRoom || command.HostActorId is not null)
            return null;

        if (command.HoldingId is not { } holdingId)
            return HoldingRequired;
        if (!state.Holdings.TryGet(holdingId, out var holding))
            return HoldingNotFound;
        if (holding!.SettlementId != command.SettlementId)
            return HoldingNotInSettlement;
        if (holding.Villa is null || !holding.Villa.TryGetRoom(command.VenueKey, out _))
            return VillaRoomNotFound;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, PlanActivityCommand command)
    {
        var type = ActivityTypeCatalog.Get(command.TypeKey);
        var activityId = state.ActivityIds.Issue();

        var venueTier = 1;
        RuntimeId<Holding>? venueHoldingId = null;
        if (command.VenueKind == ActivityVenueKind.VillaRoom && command.HostActorId is null && command.HoldingId is { } holdingId)
        {
            state.Holdings.TryGet(holdingId, out var holding);
            holding!.Villa!.TryGetRoom(command.VenueKey, out var room);
            venueTier = room.Tier;
            venueHoldingId = holdingId;
        }

        var venue = new ActivityVenue(command.VenueKind, command.VenueKey, command.SettlementId, venueHoldingId, venueTier);
        var scale = ActivityCatalog.DeriveScale(command.GuestIds.Count, command.VenueKind, command.VenueKey);
        var endDate = new GameDate(command.StartDate.TotalMonths + command.DurationMonths - 1);

        // Canonical Type order, not submission order, so two commands differing only in how the
        // caller listed the inputs produce identical state (ADR 0004).
        var qualityInputs = type.QualityInputs
            .Select(definition => command.QualityInputs.Single(input => string.Equals(input.Key, definition.Key, StringComparison.Ordinal)))
            .ToArray();

        var activity = new HostedActivity(
            activityId, type.Key, command.HostCharacterId, command.HostHouseholdId, command.HostActorId, venue,
            type.DurationMode, command.SubmittedDate, command.StartDate, endDate, scale, qualityInputs, command.Budget,
            ActivityStatus.Planned, SchedulePhases(type, command.StartDate, command.DurationMonths),
            Array.Empty<ActivityPlannedInteraction>());
        state.Activities.Add(activityId, activity);

        foreach (var guestId in command.GuestIds.OrderBy(id => id))
        {
            var invitation = new ActivityInvitation(
                activityId, guestId, ActivityRsvpStatus.Pending, WasExpectedInvite: false, ExclusionInsultApplied: false,
                RespondedExplicitly: false, AttendsToCauseTrouble: false, RespondedDate: null);
            state.ActivityInvitations.Add(invitation.Key, invitation);
        }

        return new IDomainEvent[]
        {
            new ActivityPlannedEvent(
                state.EventIds.Issue(), command.SubmittedDate, activityId, type.Key, command.HostCharacterId,
                command.HostHouseholdId, command.HostActorId, scale, command.StartDate, command.GuestIds.Count,
                command.CommandId.ToTaggedString()),
        };
    }

    /// <summary>§3: a Quick Activity's Phases all fall in its one month; an Extended Activity's are
    /// spread evenly from its first month to its last, the final Phase always on the last month.</summary>
    internal static IReadOnlyList<ActivityPhase> SchedulePhases(ActivityTypeDefinition type, GameDate startDate, int durationMonths)
    {
        var count = type.PhaseKeys.Count;
        var phases = new ActivityPhase[count];
        for (var i = 0; i < count; i++)
        {
            var offset = count == 1 ? durationMonths - 1 : i * (durationMonths - 1) / (count - 1);
            phases[i] = new ActivityPhase(
                type.PhaseKeys[i], i, new GameDate(startDate.TotalMonths + offset), OccurredDate: null,
                Array.Empty<ActivityMoment>());
        }

        return phases;
    }
}
