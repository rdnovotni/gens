using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.Romance;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Activities;

/// <summary>Emitted once per Phase actually run (§6). Private to the Witness Pool (§7): the people in
/// the room are exactly the people who saw what happened in it.</summary>
public sealed record ActivityPhaseResolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Activity> ActivityId,
    string PhaseKey,
    int Sequence,
    int MomentCount,
    IReadOnlyList<RuntimeId<Character>> WitnessIds) : IDomainEvent
{
    public string Type => "activities.phaseResolved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => WitnessIds.Select(id => id.ToTaggedString()).ToArray();
    public string? CausationId => null;
    public Visibility Visibility => Visibility.Private(WitnessIds.Select(id => id.ToTaggedString()).ToArray());
}

/// <summary>
/// Runs one Phase of an in-progress Activity (§6) — the connective tissue between the Activity's
/// container and the systems this codebase already has. Nothing here grants a new effect: an
/// in-Phase Interaction (§6.1) is an ordinary <see cref="RecordInteractionCommand"/> at Scale-amplified
/// effectiveness (§9's Witness-Pool amplification); a Phase incident (§6.2) is either the same
/// ordinary relationship swing (a Dispute, a Toast) or an ordinary <see
/// cref="RecordRomanticInteractionCommand"/> (a Flirtation, only between a pair <see
/// cref="RomanceEligibility.CheckPair"/> already allows — the "hosted gathering" romance opportunity
/// source <see cref="AutonomousRomanceSystem"/>'s own doc comment deferred, now real); a
/// troublemaker's Disruption (§4.1's sharper case) is an ordinary mutual opinion hit with the host.
///
/// <b>Interaction direction:</b> an Interaction <i>initiated</i> by A against B is recorded as B's
/// reaction — the B→A directed <see cref="Relationship"/> — matching Group Interaction's "the sum of
/// individual per-target reactions" (Characters §9.8).
/// </summary>
public static class ActivityPhaseRunner
{
    /// <summary>The named random stream Phase incidents (§6.2) draw from, kept distinct from every
    /// other stream (rule 8).</summary>
    public const string StreamName = CampaignBootstrapper.ActivityPhaseIncidentStreamName;

    /// <summary>Applies one Interaction inside the Activity's Phase at <paramref name="phaseIndex"/>,
    /// Scale-amplified, and records it as a key moment. Returns the amplified opinion delta actually
    /// applied. Caller guarantees both Characters are present and distinct.</summary>
    public static int ApplyInteraction(
        WorldState state,
        RuntimeId<Activity> activityId,
        int phaseIndex,
        RuntimeId<Character> initiatorId,
        RuntimeId<Character> targetId,
        int opinionDelta,
        BondTag bondsGranted,
        GameDate date,
        string actorId,
        string? causationId,
        List<IDomainEvent> events)
    {
        state.Activities.TryGet(activityId, out var activity);
        var amplified = Amplify(activity!.Scale, opinionDelta);

        if (amplified != 0 || bondsGranted != BondTag.None)
        {
            events.AddRange(RecordInteractionCommands.Pipeline.Execute(
                state,
                new RecordInteractionCommand(
                    state.CommandIds.Issue(), actorId, date, causationId, targetId, initiatorId, amplified,
                    bondsGranted, BondTag.None, RelationshipOrigin.Encounter)).Events);
        }

        AppendMoment(state, activityId, phaseIndex,
            new ActivityMoment(ActivityMomentKind.Interaction, initiatorId, targetId, null, amplified));
        return amplified;
    }

    /// <summary>Runs the Phase at <paramref name="phaseIndex"/>: the Planned Interactions submitted
    /// for it, any troublemaker's Disruption (in the Type's Main Event Phase), and one incident roll.
    /// Marks the Phase occurred.</summary>
    public static void RunPhase(
        WorldState state, RuntimeId<Activity> activityId, int phaseIndex, MonthlyTickContext context, List<IDomainEvent> events)
    {
        state.Activities.TryGet(activityId, out var activity);
        var phase = activity!.Phases[phaseIndex];

        foreach (var planned in activity.PlannedInteractions.Where(p => string.Equals(p.PhaseKey, phase.PhaseKey, StringComparison.Ordinal)))
        {
            state.Activities.TryGet(activityId, out var current);
            if (planned.InitiatorId == planned.TargetId ||
                !ActivityWitnessPool.IsPresent(state, current!, planned.InitiatorId) ||
                !ActivityWitnessPool.IsPresent(state, current!, planned.TargetId))
                continue;

            ApplyInteraction(state, activityId, phaseIndex, planned.InitiatorId, planned.TargetId, planned.OpinionDelta,
                planned.BondsGranted, context.Date, "system", activityId.ToTaggedString(), events);
        }

        if (phaseIndex == DisruptionPhaseIndex(activity))
            ApplyDisruptions(state, activityId, phaseIndex, context.Date, events);

        RollIncident(state, activityId, phaseIndex, context, events);

        state.Activities.TryGet(activityId, out var updated);
        var phases = updated!.Phases.ToArray();
        phases[phaseIndex] = phases[phaseIndex] with { OccurredDate = context.Date };
        ActivityResolver.Replace(state, updated with { Phases = phases });

        var witnesses = ActivityWitnessPool.Of(state, updated);
        events.Add(new ActivityPhaseResolvedEvent(
            state.EventIds.Issue(), context.Date, activityId, phase.PhaseKey, phase.Sequence,
            phases[phaseIndex].Moments.Count, witnesses));
    }

    /// <summary>The Phase a troublemaker acts in: the Type's Main Event if it has one, otherwise its
    /// middle Phase.</summary>
    public static int DisruptionPhaseIndex(HostedActivity activity)
    {
        for (var i = 0; i < activity.Phases.Count; i++)
            if (string.Equals(activity.Phases[i].PhaseKey, ActivityPhaseKeys.MainEvent, StringComparison.Ordinal))
                return i;

        return activity.Phases.Count / 2;
    }

    public static int Amplify(ActivityScaleTier scale, int delta) =>
        Math.Clamp(delta * ActivityCatalog.WitnessAmplificationPercent(scale) / 100, Relationship.MinOpinion, Relationship.MaxOpinion);

    private static void ApplyDisruptions(
        WorldState state, RuntimeId<Activity> activityId, int phaseIndex, GameDate date, List<IDomainEvent> events)
    {
        state.Activities.TryGet(activityId, out var activity);
        var troublemakers = ActivityInvitationResolver.GuestList(state, activityId)
            .Where(invitation => invitation.IsAttending && invitation.AttendsToCauseTrouble)
            .Select(invitation => invitation.InviteeId)
            .ToArray();

        foreach (var troublemakerId in troublemakers)
        {
            if (!ActivityWitnessPool.IsPresent(state, activity!, troublemakerId) ||
                !ActivityWitnessPool.IsPresent(state, activity!, activity!.HostCharacterId))
                continue;

            var delta = Amplify(activity.Scale, ActivityCatalog.DisruptionOpinionDelta);
            MutualOpinion(state, activity.HostCharacterId, troublemakerId, delta, date, activityId, events);
            AppendMoment(state, activityId, phaseIndex,
                new ActivityMoment(ActivityMomentKind.Disruption, troublemakerId, activity.HostCharacterId, null, delta));
        }
    }

    private static void RollIncident(
        WorldState state, RuntimeId<Activity> activityId, int phaseIndex, MonthlyTickContext context, List<IDomainEvent> events)
    {
        state.Activities.TryGet(activityId, out var activity);
        var present = ActivityWitnessPool.Of(state, activity!);
        if (present.Count < 2)
            return;

        var streams = context.RandomStreams;
        if (streams.NextUInt(StreamName, 100) >= (uint)ActivityCatalog.PhaseIncidentChancePercent(activity!.Scale))
            return;

        var kindRoll = streams.NextUInt(StreamName, 100);
        var firstIndex = (int)streams.NextUInt(StreamName, (uint)present.Count);
        var secondIndex = (int)streams.NextUInt(StreamName, (uint)(present.Count - 1));
        if (secondIndex >= firstIndex)
            secondIndex++;
        var firstId = present[firstIndex];
        var secondId = present[secondIndex];

        var kind = kindRoll < ActivityCatalog.ToastBandUpper ? ActivityIncidentKind.Toast
            : kindRoll < ActivityCatalog.DisputeBandUpper ? ActivityIncidentKind.Dispute
            : ActivityIncidentKind.Flirtation;

        if (kind == ActivityIncidentKind.Flirtation && RomanceEligibility.CheckPair(state, firstId, secondId, context.Date) is not null)
            kind = ActivityIncidentKind.Toast;

        int magnitude;
        if (kind == ActivityIncidentKind.Flirtation)
        {
            magnitude = ActivityCatalog.FlirtationAffectionDelta;
            events.AddRange(RecordRomanticInteractionCommands.Pipeline.Execute(
                state,
                new RecordRomanticInteractionCommand(
                    state.CommandIds.Issue(), "system", context.Date, activityId.ToTaggedString(), firstId, secondId,
                    ActivityCatalog.FlirtationAffectionDelta, ActivityCatalog.FlirtationAttractionDelta, null)).Events);
        }
        else
        {
            magnitude = Amplify(activity.Scale, kind == ActivityIncidentKind.Dispute
                ? ActivityCatalog.DisputeOpinionDelta
                : ActivityCatalog.ToastOpinionDelta);
            MutualOpinion(state, firstId, secondId, magnitude, context.Date, activityId, events);
        }

        AppendMoment(state, activityId, phaseIndex,
            new ActivityMoment(ActivityMomentKind.Incident, firstId, secondId, kind, magnitude));
    }

    private static void MutualOpinion(
        WorldState state, RuntimeId<Character> a, RuntimeId<Character> b, int delta, GameDate date,
        RuntimeId<Activity> activityId, List<IDomainEvent> events)
    {
        if (delta == 0)
            return;

        foreach (var (from, to) in new[] { (a, b), (b, a) })
        {
            events.AddRange(RecordInteractionCommands.Pipeline.Execute(
                state,
                new RecordInteractionCommand(
                    state.CommandIds.Issue(), "system", date, activityId.ToTaggedString(), from, to, delta,
                    BondTag.None, BondTag.None, RelationshipOrigin.Encounter)).Events);
        }
    }

    private static void AppendMoment(WorldState state, RuntimeId<Activity> activityId, int phaseIndex, ActivityMoment moment)
    {
        state.Activities.TryGet(activityId, out var activity);
        var phases = activity!.Phases.ToArray();
        phases[phaseIndex] = phases[phaseIndex] with { Moments = phases[phaseIndex].Moments.Append(moment).ToArray() };
        ActivityResolver.Replace(state, activity with { Phases = phases });
    }
}
