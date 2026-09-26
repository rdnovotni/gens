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
/// Breaks off an Activity already underway (Phase 17 item 4; §3: "an Extended Activity is a real,
/// standing state for its duration, and can be genuinely interrupted by an unrelated event — a Natural
/// Disaster, a Scheme reaching resolution, a Piracy raid"). The one command path any such source
/// routes through (<see cref="ICommand.ActorId"/> is typically <c>"system"</c>); <see
/// cref="ActivityProgressSystem"/> itself submits it for the two interruptions it detects directly —
/// the host's death and a Severe-or-worse Natural Disaster striking the Venue's settlement. An
/// interrupted Activity resolves no Outcome: no Quality payoff, no Dignitas, no guest opinion swing —
/// whatever already happened in its completed Phases stands, and nothing more.
/// </summary>
public sealed record InterruptActivityCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Activity> ActivityId,
    string Reason) : ICommand;

/// <summary>The validate/mutate pipeline for <see cref="InterruptActivityCommand"/> (ADR 0006).</summary>
public static class InterruptActivityCommands
{
    public static readonly ValidationErrorCode ActivityNotFound = new("activities.interrupt.activityNotFound");
    public static readonly ValidationErrorCode ActivityNotInProgress = new("activities.interrupt.activityNotInProgress");

    public static readonly CommandPipeline<WorldState, InterruptActivityCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, InterruptActivityCommand command)
    {
        if (!state.Activities.TryGet(command.ActivityId, out var activity))
            return ActivityNotFound;
        if (activity!.Status != ActivityStatus.InProgress)
            return ActivityNotInProgress;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, InterruptActivityCommand command)
    {
        state.Activities.TryGet(command.ActivityId, out var activity);
        ActivityResolver.Replace(state, activity! with { Status = ActivityStatus.Interrupted, TerminationReason = command.Reason });

        return new IDomainEvent[]
        {
            new ActivityEndedWithoutConclusionEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.ActivityId, activity.HostCharacterId,
                activity.HostHouseholdId, ActivityStatus.Interrupted, command.Reason, command.CommandId.ToTaggedString()),
        };
    }
}
