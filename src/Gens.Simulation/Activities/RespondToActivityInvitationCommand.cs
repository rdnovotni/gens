using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Activities;

/// <summary>
/// An invitee answers an Invitation themselves (Phase 17 item 4; §4.1, §8.1) — the player accepting or
/// declining a Rival House's feast, or answering for a member of their own household. Any Invitation
/// still <see cref="ActivityRsvpStatus.Pending"/> when the Activity begins is answered by <see
/// cref="ActivityRsvpResolver"/> instead. §8.1: "declining is a real, legible social choice of its
/// own, reading exactly the way declining any other social overture already does" — so an explicit
/// decline costs the host's opinion of the invitee through the ordinary relationship web, where an
/// NPC's own auto-resolved decline (an absence, not a refusal) does not.
/// </summary>
public sealed record RespondToActivityInvitationCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Activity> ActivityId,
    RuntimeId<Character> InviteeId,
    bool Accept) : ICommand;

/// <summary>Emitted whenever an Invitation is answered, explicitly or by <see
/// cref="ActivityRsvpResolver"/>. Private to the invitee and host.</summary>
public sealed record ActivityInvitationAnsweredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Activity> ActivityId,
    RuntimeId<Character> InviteeId,
    RuntimeId<Character> HostCharacterId,
    ActivityRsvpStatus RsvpStatus,
    bool RespondedExplicitly,
    bool AttendsToCauseTrouble,
    string? CausationId) : IDomainEvent
{
    public string Type => "activities.invitationAnswered";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { InviteeId.ToTaggedString(), HostCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(InviteeId.ToTaggedString(), HostCharacterId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="RespondToActivityInvitationCommand"/> (ADR 0006).</summary>
public static class RespondToActivityInvitationCommands
{
    /// <summary>The host's opinion hit toward an invitee who explicitly declines (§8.1).</summary>
    public const int ExplicitDeclineOpinionDelta = -3;

    public static readonly ValidationErrorCode ActivityNotFound = new("activities.respond.activityNotFound");
    public static readonly ValidationErrorCode ActivityNotPlanned = new("activities.respond.activityNotPlanned");
    public static readonly ValidationErrorCode NotInvited = new("activities.respond.notInvited");
    public static readonly ValidationErrorCode AlreadyAnswered = new("activities.respond.alreadyAnswered");
    public static readonly ValidationErrorCode InviteeDeceased = new("activities.respond.inviteeDeceased");

    public static readonly CommandPipeline<WorldState, RespondToActivityInvitationCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RespondToActivityInvitationCommand command)
    {
        if (!state.Activities.TryGet(command.ActivityId, out var activity))
            return ActivityNotFound;
        if (activity!.Status != ActivityStatus.Planned)
            return ActivityNotPlanned;
        var invitation = ActivityInvitationResolver.Get(state, command.ActivityId, command.InviteeId);
        if (invitation is null || invitation.RsvpStatus == ActivityRsvpStatus.NotInvited)
            return NotInvited;
        if (invitation.RsvpStatus != ActivityRsvpStatus.Pending)
            return AlreadyAnswered;
        if (!state.Characters.TryGet(command.InviteeId, out var invitee) || !invitee.IsAlive)
            return InviteeDeceased;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, RespondToActivityInvitationCommand command)
    {
        state.Activities.TryGet(command.ActivityId, out var activity);
        var invitation = ActivityInvitationResolver.Get(state, command.ActivityId, command.InviteeId)!;
        var status = command.Accept ? ActivityRsvpStatus.Accepted : ActivityRsvpStatus.Declined;
        ActivityInvitationResolver.Replace(state, invitation with
        {
            RsvpStatus = status,
            RespondedExplicitly = true,
            RespondedDate = command.SubmittedDate,
        });

        var events = new List<IDomainEvent>
        {
            new ActivityInvitationAnsweredEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.ActivityId, command.InviteeId,
                activity!.HostCharacterId, status, RespondedExplicitly: true, AttendsToCauseTrouble: false,
                command.CommandId.ToTaggedString()),
        };

        if (!command.Accept)
        {
            events.AddRange(RecordInteractionCommands.Pipeline.Execute(
                state,
                new RecordInteractionCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    activity.HostCharacterId, command.InviteeId, ExplicitDeclineOpinionDelta,
                    BondTag.None, BondTag.None, RelationshipOrigin.Encounter)).Events);
        }

        return events.ToArray();
    }
}
