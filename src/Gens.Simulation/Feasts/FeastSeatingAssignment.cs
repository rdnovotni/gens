using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Activities;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Feasts;

/// <summary>§4's three couches of a Triclinium, ranked lowest to highest status.</summary>
public enum FeastCouch
{
    Imus,
    Medius,
    Summus,
}

/// <summary>§4's three reclining positions on one couch, ranked lowest to highest status.</summary>
public enum FeastCouchPosition
{
    Lowest,
    Middle,
    Highest,
}

/// <summary>§4's seating verdict, resolved once by <see cref="FeastSeatingResolutionSystem"/> when a
/// Feast's Arrival &amp; Seating Phase occurs. Null on a <see cref="FeastSeatingAssignment"/> until
/// then, matching <see cref="HostedActivity.Outcome"/>'s own "nullable until resolved" convention.</summary>
public enum FeastSeatingJudgment
{
    UnderSeated,
    AppropriatelySeated,
    OverSeated,
}

/// <summary>The <see cref="WorldState.FeastSeatingAssignments"/> ordering key: Activity first, guest
/// second, mirroring <see cref="ActivityInvitationKey"/>'s identical two-field shape so "every seat at
/// one Feast, in guest order" is a contiguous ascending scan (ADR 0004).</summary>
public readonly record struct FeastSeatingKey(RuntimeId<Activity> ActivityId, RuntimeId<Character> GuestId)
    : IComparable<FeastSeatingKey>
{
    public int CompareTo(FeastSeatingKey other)
    {
        var activityComparison = ActivityId.CompareTo(other.ActivityId);
        return activityComparison != 0 ? activityComparison : GuestId.CompareTo(other.GuestId);
    }

    public static bool operator <(FeastSeatingKey left, FeastSeatingKey right) => left.CompareTo(right) < 0;
    public static bool operator >(FeastSeatingKey left, FeastSeatingKey right) => left.CompareTo(right) > 0;
    public static bool operator <=(FeastSeatingKey left, FeastSeatingKey right) => left.CompareTo(right) <= 0;
    public static bool operator >=(FeastSeatingKey left, FeastSeatingKey right) => left.CompareTo(right) >= 0;
}

/// <summary>
/// One guest's seat at a Feast (Phase 17 item 5; <c>gens-feasts-design.md</c> §4, §10's
/// <c>seatingAssignments</c>) — the design doc's own "single most concrete new texture" this item adds.
/// Assigned by <see cref="AssignFeastSeatingCommand"/> while the Feast is still <see
/// cref="ActivityStatus.Planned"/>; <see cref="Judgment"/> is filled in once, by <see
/// cref="FeastSeatingResolutionSystem"/>, when the Feast's Arrival &amp; Seating Phase actually occurs.
/// </summary>
public sealed record FeastSeatingAssignment(
    RuntimeId<Activity> ActivityId,
    RuntimeId<Character> GuestId,
    FeastCouch Couch,
    FeastCouchPosition Position,
    FeastSeatingJudgment? Judgment = null)
{
    public FeastSeatingKey Key => new(ActivityId, GuestId);

    /// <summary>§4's seat of honor — the middle couch's highest position. Derived, not stored: the one
    /// <c>(Medius, Highest)</c> pair is already unique per Feast because <see
    /// cref="AssignFeastSeatingCommands"/> rejects two guests claiming the same seat, so no separate "at
    /// most one locus consularis" check is needed anywhere.</summary>
    public bool IsLocusConsularis => (Couch, Position) == FeastCatalog.LocusConsularis;
}

/// <summary>Read-side helpers over <see cref="WorldState.FeastSeatingAssignments"/>.</summary>
public static class FeastSeatingResolver
{
    public static IEnumerable<FeastSeatingAssignment> ForActivity(WorldState state, RuntimeId<Activity> activityId) =>
        state.FeastSeatingAssignments.InAscendingOrder()
            .Select(entry => entry.Value)
            .Where(assignment => assignment.ActivityId == activityId);

    public static FeastSeatingAssignment? Get(WorldState state, RuntimeId<Activity> activityId, RuntimeId<Character> guestId) =>
        state.FeastSeatingAssignments.TryGet(new FeastSeatingKey(activityId, guestId), out var assignment) ? assignment : null;

    public static void Replace(WorldState state, FeastSeatingAssignment assignment)
    {
        state.FeastSeatingAssignments.Remove(assignment.Key);
        state.FeastSeatingAssignments.Add(assignment.Key, assignment);
    }
}
