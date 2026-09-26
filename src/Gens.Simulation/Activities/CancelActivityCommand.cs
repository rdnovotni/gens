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

/// <summary>The host calls off an Activity before it begins (Phase 17 item 4). Because invitations are
/// only answered, the budget only posted, and §4.2's exclusion snubs only applied once an Activity
/// actually begins, cancelling a <see cref="ActivityStatus.Planned"/> Activity costs nothing and
/// offends no one; an Activity already underway can only be <see cref="InterruptActivityCommand"/>ed.</summary>
public sealed record CancelActivityCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Activity> ActivityId,
    string Reason) : ICommand;

/// <summary>Emitted when an Activity ends without concluding — cancelled before it began or
/// interrupted while underway (§3). Public: the gathering's guests all learn it is off.</summary>
public sealed record ActivityEndedWithoutConclusionEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Activity> ActivityId,
    RuntimeId<Character> HostCharacterId,
    RuntimeId<Household>? HostHouseholdId,
    ActivityStatus Status,
    string Reason,
    string? CausationId) : IDomainEvent
{
    public string Type => "activities.endedWithoutConclusion";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HostCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="CancelActivityCommand"/> (ADR 0006).</summary>
public static class CancelActivityCommands
{
    public static readonly ValidationErrorCode ActivityNotFound = new("activities.cancel.activityNotFound");
    public static readonly ValidationErrorCode ActivityNotPlanned = new("activities.cancel.activityNotPlanned");

    public static readonly CommandPipeline<WorldState, CancelActivityCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, CancelActivityCommand command)
    {
        if (!state.Activities.TryGet(command.ActivityId, out var activity))
            return ActivityNotFound;
        if (activity!.Status != ActivityStatus.Planned)
            return ActivityNotPlanned;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, CancelActivityCommand command)
    {
        state.Activities.TryGet(command.ActivityId, out var activity);
        ActivityResolver.Replace(state, activity! with { Status = ActivityStatus.Cancelled, TerminationReason = command.Reason });

        return new IDomainEvent[]
        {
            new ActivityEndedWithoutConclusionEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.ActivityId, activity.HostCharacterId,
                activity.HostHouseholdId, ActivityStatus.Cancelled, command.Reason, command.CommandId.ToTaggedString()),
        };
    }
}
