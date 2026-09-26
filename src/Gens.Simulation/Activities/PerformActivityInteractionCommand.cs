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
/// An attendee opens an ordinary Interaction against another attendee inside an Activity's Phase
/// (Phase 17 item 4; §6.1: "at elevated availability or effectiveness... this document doesn't grant
/// any of these a new effect — it simply gives them a real, appropriate stage to happen on").
///
/// Two shapes, because a Quick Activity's Phases all run inside one tick (§3, and §12's open "player
/// control granularity" question): against a <see cref="ActivityStatus.Planned"/> Activity it records
/// an intent for the named <see cref="PhaseKey"/>, which <see cref="ActivityPhaseRunner"/> carries out
/// when that Phase runs (provided both parties actually attend); against an <see
/// cref="ActivityStatus.InProgress"/> Activity (an Extended one between its Phases) it applies at once,
/// recorded on the most recent Phase that has run, and <see cref="PhaseKey"/> is ignored.
/// </summary>
public sealed record PerformActivityInteractionCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Activity> ActivityId,
    RuntimeId<Character> InitiatorId,
    RuntimeId<Character> TargetId,
    string PhaseKey,
    int OpinionDelta,
    BondTag BondsGranted) : ICommand;

/// <summary>Emitted when a <see cref="PerformActivityInteractionCommand"/> is accepted — recording
/// an intent (<see cref="Applied"/> false) or applying it immediately (<see cref="Applied"/> true).</summary>
public sealed record ActivityInteractionSubmittedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Activity> ActivityId,
    RuntimeId<Character> InitiatorId,
    RuntimeId<Character> TargetId,
    string PhaseKey,
    bool Applied,
    int AppliedOpinionDelta,
    string? CausationId) : IDomainEvent
{
    public string Type => "activities.interactionSubmitted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { InitiatorId.ToTaggedString(), TargetId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(InitiatorId.ToTaggedString(), TargetId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="PerformActivityInteractionCommand"/> (ADR 0006).</summary>
public static class PerformActivityInteractionCommands
{
    public static readonly ValidationErrorCode ActivityNotFound = new("activities.interaction.activityNotFound");
    public static readonly ValidationErrorCode ActivityNotOpen = new("activities.interaction.activityNotOpen");
    public static readonly ValidationErrorCode SelfInteraction = new("activities.interaction.selfInteraction");
    public static readonly ValidationErrorCode InitiatorNotParticipant = new("activities.interaction.initiatorNotParticipant");
    public static readonly ValidationErrorCode TargetNotParticipant = new("activities.interaction.targetNotParticipant");
    public static readonly ValidationErrorCode UnknownOrPastPhase = new("activities.interaction.unknownOrPastPhase");
    public static readonly ValidationErrorCode NothingToRecord = new("activities.interaction.nothingToRecord");
    public static readonly ValidationErrorCode OpinionDeltaOutOfRange = new("activities.interaction.opinionDeltaOutOfRange");

    public static readonly CommandPipeline<WorldState, PerformActivityInteractionCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, PerformActivityInteractionCommand command)
    {
        if (!state.Activities.TryGet(command.ActivityId, out var activity))
            return ActivityNotFound;
        if (!activity!.IsOpen)
            return ActivityNotOpen;
        if (command.InitiatorId == command.TargetId)
            return SelfInteraction;
        if (command.OpinionDelta == 0 && command.BondsGranted == BondTag.None)
            return NothingToRecord;
        if (command.OpinionDelta is < Relationship.MinOpinion or > Relationship.MaxOpinion)
            return OpinionDeltaOutOfRange;

        if (activity.Status == ActivityStatus.Planned)
        {
            if (!IsExpectedParticipant(state, activity, command.InitiatorId))
                return InitiatorNotParticipant;
            if (!IsExpectedParticipant(state, activity, command.TargetId))
                return TargetNotParticipant;
            if (!activity.Phases.Any(phase => string.Equals(phase.PhaseKey, command.PhaseKey, StringComparison.Ordinal)))
                return UnknownOrPastPhase;
        }
        else
        {
            if (!ActivityWitnessPool.IsPresent(state, activity, command.InitiatorId))
                return InitiatorNotParticipant;
            if (!ActivityWitnessPool.IsPresent(state, activity, command.TargetId))
                return TargetNotParticipant;
            if (!activity.Phases.Any(phase => phase.HasOccurred))
                return UnknownOrPastPhase;
        }

        return null;
    }

    /// <summary>Before the Activity begins: the host, or any invitee who has not declined.</summary>
    private static bool IsExpectedParticipant(WorldState state, HostedActivity activity, RuntimeId<Character> characterId)
    {
        if (!state.Characters.TryGet(characterId, out var character) || !character.IsAlive)
            return false;
        if (activity.HostCharacterId == characterId)
            return true;

        return ActivityInvitationResolver.Get(state, activity.Id, characterId) is
        { RsvpStatus: ActivityRsvpStatus.Pending or ActivityRsvpStatus.Accepted };
    }

    private static IDomainEvent[] Mutate(WorldState state, PerformActivityInteractionCommand command)
    {
        state.Activities.TryGet(command.ActivityId, out var activity);
        var events = new List<IDomainEvent>();

        if (activity!.Status == ActivityStatus.Planned)
        {
            var planned = new ActivityPlannedInteraction(
                command.InitiatorId, command.TargetId, command.PhaseKey, command.OpinionDelta, command.BondsGranted);
            ActivityResolver.Replace(state, activity with { PlannedInteractions = activity.PlannedInteractions.Append(planned).ToArray() });
            events.Add(new ActivityInteractionSubmittedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.ActivityId, command.InitiatorId, command.TargetId,
                command.PhaseKey, Applied: false, AppliedOpinionDelta: 0, command.CommandId.ToTaggedString()));
            return events.ToArray();
        }

        var phaseIndex = activity.Phases.Select((phase, index) => (phase, index)).Last(pair => pair.phase.HasOccurred).index;
        var applied = ActivityPhaseRunner.ApplyInteraction(
            state, command.ActivityId, phaseIndex, command.InitiatorId, command.TargetId, command.OpinionDelta,
            command.BondsGranted, command.SubmittedDate, command.ActorId, command.CommandId.ToTaggedString(), events);
        events.Add(new ActivityInteractionSubmittedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.ActivityId, command.InitiatorId, command.TargetId,
            activity.Phases[phaseIndex].PhaseKey, Applied: true, applied, command.CommandId.ToTaggedString()));
        return events.ToArray();
    }
}
