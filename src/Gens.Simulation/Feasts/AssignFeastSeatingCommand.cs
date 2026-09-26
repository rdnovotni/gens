using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Activities;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Feasts;

/// <summary>
/// Seats one guest at a Feast (Phase 17 item 5; §4) — a real, deliberate host decision, finer-grained
/// than and distinct from the Activity Engine's own blunter Exclusion mechanic. Only valid while the
/// Feast is still <see cref="ActivityStatus.Planned"/>, matching every other planning-time act in this
/// domain (RSVP and §4.2 exclusion resolution are likewise deferred to <see
/// cref="ActivityProgressSystem"/>'s own Begin step, never resolved before it). This codebase's command
/// pipeline has no submitting-Character authorization layer anywhere (confirmed against <see
/// cref="PlanActivityCommand"/>, which never checks <see cref="ActorId"/> against the host) — "only the
/// host may assign seating" is therefore enforced the same way every other "only you can do this to your
/// own thing" rule in this codebase is: at the caller/UI layer, not inside this command.
/// </summary>
public sealed record AssignFeastSeatingCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Activity> ActivityId,
    RuntimeId<Character> GuestId,
    FeastCouch Couch,
    FeastCouchPosition Position) : ICommand;

/// <summary>Emitted whenever an <see cref="AssignFeastSeatingCommand"/> is accepted. Private
/// bookkeeping — a seat assignment is not itself a felt social fact until <see
/// cref="FeastSeatingResolutionSystem"/> actually judges it (§4's Insult/Honor only lands once the
/// Feast's Arrival &amp; Seating Phase occurs).</summary>
public sealed record FeastSeatingAssignedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Activity> ActivityId,
    RuntimeId<Character> GuestId,
    FeastCouch Couch,
    FeastCouchPosition Position,
    string? CausationId) : IDomainEvent
{
    public string Type => "feasts.seatingAssigned";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ActivityId.ToTaggedString(), GuestId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(GuestId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="AssignFeastSeatingCommand"/> (ADR 0006).</summary>
public static class AssignFeastSeatingCommands
{
    public static readonly ValidationErrorCode ActivityNotFound = new("feasts.assignSeating.activityNotFound");
    public static readonly ValidationErrorCode NotAFeast = new("feasts.assignSeating.notAFeast");
    public static readonly ValidationErrorCode FeastNotPlanned = new("feasts.assignSeating.feastNotPlanned");
    public static readonly ValidationErrorCode GuestNotInvited = new("feasts.assignSeating.guestNotInvited");
    public static readonly ValidationErrorCode DuplicateSeatForGuest = new("feasts.assignSeating.duplicateSeatForGuest");
    public static readonly ValidationErrorCode SeatAlreadyTaken = new("feasts.assignSeating.seatAlreadyTaken");

    public static readonly CommandPipeline<WorldState, AssignFeastSeatingCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AssignFeastSeatingCommand command)
    {
        if (!state.Activities.TryGet(command.ActivityId, out var activity))
            return ActivityNotFound;
        if (!string.Equals(activity!.TypeKey, FeastCatalog.FeastType.Key, StringComparison.Ordinal))
            return NotAFeast;
        if (activity.Status != ActivityStatus.Planned)
            return FeastNotPlanned;
        if (ActivityInvitationResolver.Get(state, command.ActivityId, command.GuestId) is null)
            return GuestNotInvited;
        if (FeastSeatingResolver.Get(state, command.ActivityId, command.GuestId) is not null)
            return DuplicateSeatForGuest;

        foreach (var existing in FeastSeatingResolver.ForActivity(state, command.ActivityId))
            if (existing.Couch == command.Couch && existing.Position == command.Position)
                return SeatAlreadyTaken;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AssignFeastSeatingCommand command)
    {
        var assignment = new FeastSeatingAssignment(command.ActivityId, command.GuestId, command.Couch, command.Position);
        state.FeastSeatingAssignments.Add(assignment.Key, assignment);

        return new IDomainEvent[]
        {
            new FeastSeatingAssignedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.ActivityId, command.GuestId, command.Couch,
                command.Position, command.CommandId.ToTaggedString()),
        };
    }
}
