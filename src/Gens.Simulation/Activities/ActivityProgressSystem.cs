using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Activities;

/// <summary>Emitted when an Activity begins — its Guest List settled, its exclusions felt, its budget
/// spent. Public.</summary>
public sealed record ActivityBegunEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Activity> ActivityId,
    RuntimeId<Character> HostCharacterId,
    ActivityScaleTier Scale,
    int AcceptedCount,
    int DeclinedCount,
    int ExcludedCount) : IDomainEvent
{
    public string Type => "activities.begun";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HostCharacterId.ToTaggedString() };
    public string? CausationId => null;
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// The Activity Engine's monthly tick (Phase 17 item 4; <c>gens-activities-activity-engine-design.md</c>
/// §3-§9). For every open Activity, in ascending ID order (ADR 0004):
/// <list type="number">
/// <item><b>Begin</b> (Planned, start month reached): answer every still-<see
/// cref="ActivityRsvpStatus.Pending"/> Invitation (<see cref="ActivityRsvpResolver.Resolve"/>), apply
/// §4.2's exclusion snubs (<see cref="ActivityRsvpResolver.ExpectedButExcluded"/>) through the ordinary
/// relationship web, and post a household host's budget to the ledger. A host who died before the
/// start month cancels the Activity instead.</item>
/// <item><b>Interrupt</b> (§3): the host's death, or a Severe-or-worse Natural Disaster in the Venue's
/// settlement this month, routes through <see cref="InterruptActivityCommand"/>.</item>
/// <item><b>Phases</b> (§6): every Phase whose scheduled month has arrived runs in sequence (<see
/// cref="ActivityPhaseRunner.RunPhase"/>) — all of a Quick Activity's in its one month.</item>
/// <item><b>Resolve</b> (§9): once every Phase has run and the end month is reached, <see
/// cref="ActivityOutcomeResolver.Resolve"/>.</item>
/// </list>
/// </summary>
public sealed class ActivityProgressSystem : IMonthlySystem<WorldState>
{
    public static readonly LedgerAccountKey HostingSink = new(LedgerAccountKind.System, "activities:hosting");

    public string Id => "activities.progress";
    public TickPhase Phase => TickPhase.RelationshipsActors;

    public IReadOnlyCollection<string> Reads { get; } =
        new[] { "activities", "activityInvitations", "characters", "relationships", "romanticBonds", "disasterEvents", "seniorPositionAssignments", "actors" };

    /// <summary>Covers the partitions every nested pipeline (relationship, romance, Dignitas, ledger)
    /// touches as well as this system's own, matching <see cref="Romance.AutonomousRomanceSystem"/>'s
    /// identical "the write-set must cover the counters those pipelines touch too" reasoning.</summary>
    public IReadOnlyCollection<string> Writes { get; } = new[]
    {
        "activities", "activityInvitations", "relationships", "romanticBonds", "householdReputations", "actors",
        "ledgerAccounts", "ledgerTransactions", "ledgerTransactionIds", "eventIds", "commandIds", "commandSequence",
    };

    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var open = state.Activities.InAscendingOrder()
            .Where(entry => entry.Value.IsOpen)
            .Select(entry => entry.Key)
            .ToArray();

        foreach (var activityId in open)
        {
            state.Activities.TryGet(activityId, out var activity);

            if (activity!.Status == ActivityStatus.Planned)
            {
                if (activity.StartDate.TotalMonths > context.Date.TotalMonths)
                    continue;

                if (!IsAlive(state, activity.HostCharacterId))
                {
                    events.AddRange(CancelActivityCommands.Pipeline.Execute(
                        state,
                        new CancelActivityCommand(state.CommandIds.Issue(), "system", context.Date, null, activityId,
                            "the host died before the gathering")).Events);
                    continue;
                }

                Begin(state, activityId, context.Date, events);
            }

            if (InterruptionReason(state, activityId, context.Date) is { } reason)
            {
                events.AddRange(InterruptActivityCommands.Pipeline.Execute(
                    state,
                    new InterruptActivityCommand(state.CommandIds.Issue(), "system", context.Date, null, activityId, reason)).Events);
                continue;
            }

            state.Activities.TryGet(activityId, out activity);
            for (var i = 0; i < activity!.Phases.Count; i++)
            {
                state.Activities.TryGet(activityId, out var current);
                var phase = current!.Phases[i];
                if (phase.HasOccurred)
                    continue;
                if (phase.ScheduledDate.TotalMonths > context.Date.TotalMonths)
                    break;

                ActivityPhaseRunner.RunPhase(state, activityId, i, context, events);
            }

            state.Activities.TryGet(activityId, out activity);
            if (activity!.NextPhase is null && activity.EndDate.TotalMonths <= context.Date.TotalMonths)
                ActivityOutcomeResolver.Resolve(state, activityId, context.Date, events);
        }

        return events;
    }

    private static void Begin(WorldState state, RuntimeId<Activity> activityId, GameDate date, List<IDomainEvent> events)
    {
        state.Activities.TryGet(activityId, out var activity);

        var pending = ActivityInvitationResolver.GuestList(state, activityId)
            .Where(invitation => invitation.RsvpStatus == ActivityRsvpStatus.Pending)
            .ToArray();
        foreach (var invitation in pending)
        {
            var (status, troublemaker) = ActivityRsvpResolver.Resolve(state, activity!, invitation.InviteeId);
            ActivityInvitationResolver.Replace(state, invitation with
            {
                RsvpStatus = status,
                AttendsToCauseTrouble = troublemaker,
                RespondedDate = date,
            });
            events.Add(new ActivityInvitationAnsweredEvent(
                state.EventIds.Issue(), date, activityId, invitation.InviteeId, activity!.HostCharacterId, status,
                RespondedExplicitly: false, troublemaker, activityId.ToTaggedString()));
        }

        var excluded = ActivityRsvpResolver.ExpectedButExcluded(state, activity!, date);
        var penalty = -ActivityCatalog.ExclusionOpinionPenalty(activity!.Scale);
        foreach (var excludedId in excluded)
        {
            var record = new ActivityInvitation(
                activityId, excludedId, ActivityRsvpStatus.NotInvited, WasExpectedInvite: true, ExclusionInsultApplied: true,
                RespondedExplicitly: false, AttendsToCauseTrouble: false, RespondedDate: null);
            state.ActivityInvitations.Add(record.Key, record);

            events.AddRange(RecordInteractionCommands.Pipeline.Execute(
                state,
                new RecordInteractionCommand(
                    state.CommandIds.Issue(), "system", date, activityId.ToTaggedString(), excludedId, activity.HostCharacterId,
                    penalty, BondTag.None, BondTag.None, RelationshipOrigin.Encounter)).Events);
            events.Add(new ActivityExclusionSnubEvent(
                state.EventIds.Issue(), date, activityId, excludedId, activity.HostCharacterId, activity.Scale, penalty));
        }

        if (activity.HostHouseholdId is { } householdId && activity.Budget > Money.Zero)
        {
            events.Add(LedgerService.Post(
                state, date, LedgerTransactionCategory.Purchases,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), -activity.Budget),
                    new LedgerPosting(HostingSink, activity.Budget),
                },
                reference: $"activities:hosting:{activityId.ToTaggedString()}"));
        }

        ActivityResolver.Replace(state, activity with { Status = ActivityStatus.InProgress });

        var guestList = ActivityInvitationResolver.GuestList(state, activityId).ToArray();
        events.Add(new ActivityBegunEvent(
            state.EventIds.Issue(), date, activityId, activity.HostCharacterId, activity.Scale,
            guestList.Count(invitation => invitation.RsvpStatus == ActivityRsvpStatus.Accepted),
            guestList.Count(invitation => invitation.RsvpStatus == ActivityRsvpStatus.Declined),
            excluded.Count));
    }

    private static string? InterruptionReason(WorldState state, RuntimeId<Activity> activityId, GameDate date)
    {
        state.Activities.TryGet(activityId, out var activity);
        if (activity!.Status != ActivityStatus.InProgress)
            return null;
        if (!IsAlive(state, activity.HostCharacterId))
            return "the host died mid-gathering";

        foreach (var entry in state.DisasterEvents.InAscendingOrder())
        {
            var disaster = entry.Value;
            if (disaster.SettlementId == activity.Venue.SettlementId &&
                disaster.OccurredDate == date &&
                disaster.Severity >= ActivityCatalog.InterruptingDisasterSeverity)
                return $"a {disaster.Severity} {disaster.HazardType} struck the venue's settlement";
        }

        return null;
    }

    private static bool IsAlive(WorldState state, RuntimeId<Character> characterId) =>
        state.Characters.TryGet(characterId, out var character) && character.IsAlive;
}
