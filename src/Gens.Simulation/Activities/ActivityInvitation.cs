using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Activities;

/// <summary>§4.1/§11's RSVP state. <see cref="NotInvited"/> exists only for §4.2's meaningful
/// exclusion — a Character who reasonably expected an invitation and was left off.</summary>
public enum ActivityRsvpStatus
{
    Pending,
    Accepted,
    Declined,
    NotInvited,
}

/// <summary>The <see cref="WorldState.ActivityInvitations"/> ordering key: Activity first, invitee
/// second, so "every invitation to one Activity, in invitee order" is a contiguous ascending scan
/// (ADR 0004), mirroring <see cref="RelationshipKey"/>'s identical two-field shape.</summary>
public readonly record struct ActivityInvitationKey(RuntimeId<Activity> ActivityId, RuntimeId<Character> InviteeId)
    : IComparable<ActivityInvitationKey>
{
    public int CompareTo(ActivityInvitationKey other)
    {
        var activityComparison = ActivityId.CompareTo(other.ActivityId);
        return activityComparison != 0 ? activityComparison : InviteeId.CompareTo(other.InviteeId);
    }

    public static bool operator <(ActivityInvitationKey left, ActivityInvitationKey right) => left.CompareTo(right) < 0;
    public static bool operator >(ActivityInvitationKey left, ActivityInvitationKey right) => left.CompareTo(right) > 0;
    public static bool operator <=(ActivityInvitationKey left, ActivityInvitationKey right) => left.CompareTo(right) <= 0;
    public static bool operator >=(ActivityInvitationKey left, ActivityInvitationKey right) => left.CompareTo(right) >= 0;
}

/// <summary>
/// One Character's place on (or pointedly off) an Activity's Guest List (Phase 17 item 4; §4, §11's
/// <c>ActivityInvitation</c>). Created <see cref="ActivityRsvpStatus.Pending"/> for every invitee at
/// planning time; created <see cref="ActivityRsvpStatus.NotInvited"/> with <see
/// cref="WasExpectedInvite"/> set when the Activity begins and §4.2's exclusion check finds someone
/// who should have been asked.
/// </summary>
/// <param name="RespondedExplicitly">True when the invitee answered through <see
/// cref="RespondToActivityInvitationCommand"/> (typically the player) rather than having <see
/// cref="ActivityRsvpResolver"/> answer for them when the Activity began.</param>
/// <param name="AttendsToCauseTrouble">§4.1's "sharper case": a hostile invitee who accepts
/// specifically to cause trouble once there.</param>
public sealed record ActivityInvitation(
    RuntimeId<Activity> ActivityId,
    RuntimeId<Character> InviteeId,
    ActivityRsvpStatus RsvpStatus,
    bool WasExpectedInvite,
    bool ExclusionInsultApplied,
    bool RespondedExplicitly,
    bool AttendsToCauseTrouble,
    GameDate? RespondedDate)
{
    public ActivityInvitationKey Key => new(ActivityId, InviteeId);

    public bool IsAttending => RsvpStatus == ActivityRsvpStatus.Accepted;
}

/// <summary>Read-side helpers over <see cref="WorldState.ActivityInvitations"/>.</summary>
public static class ActivityInvitationResolver
{
    /// <summary>Every invitation record (Guest List entries and exclusion records alike) for one
    /// Activity, in ascending invitee order.</summary>
    public static IEnumerable<ActivityInvitation> ForActivity(WorldState state, RuntimeId<Activity> activityId) =>
        state.ActivityInvitations.InAscendingOrder()
            .Select(entry => entry.Value)
            .Where(invitation => invitation.ActivityId == activityId);

    /// <summary>Only the invitations actually on the Guest List (not §4.2 exclusion records).</summary>
    public static IEnumerable<ActivityInvitation> GuestList(WorldState state, RuntimeId<Activity> activityId) =>
        ForActivity(state, activityId).Where(invitation => invitation.RsvpStatus != ActivityRsvpStatus.NotInvited);

    public static ActivityInvitation? Get(WorldState state, RuntimeId<Activity> activityId, RuntimeId<Character> inviteeId) =>
        state.ActivityInvitations.TryGet(new ActivityInvitationKey(activityId, inviteeId), out var invitation) ? invitation : null;

    public static void Replace(WorldState state, ActivityInvitation invitation)
    {
        state.ActivityInvitations.Remove(invitation.Key);
        state.ActivityInvitations.Add(invitation.Key, invitation);
    }
}
