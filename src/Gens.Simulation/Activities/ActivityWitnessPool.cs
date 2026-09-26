using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Activities;

/// <summary>
/// §7's consolidation: "an Activity's own Guest List <i>is</i> its Witness Pool." Several systems
/// already lean on an informal "how many people are aware" input; this is the one shared, named
/// population they check against whenever the triggering moment lands during a hosted Activity. It
/// rebuilds none of their discovery formulas — it only answers who was there, and how public it was.
/// <see cref="Interactions.SchemeProgressSystem"/> is the first consumer: a Scheme whose initiator and
/// target both attend the same Activity held this month accrues extra discovery risk.
/// </summary>
public static class ActivityWitnessPool
{
    /// <summary>Everyone present: the host plus every invitee who accepted, is still alive, and has not
    /// since left on a trip (<see cref="ActivityAvailability.CanAttend"/>), in ascending ID order.</summary>
    public static IReadOnlyList<RuntimeId<Character>> Of(WorldState state, HostedActivity activity)
    {
        var present = new SortedSet<RuntimeId<Character>>();
        if (IsAlive(state, activity.HostCharacterId))
            present.Add(activity.HostCharacterId);

        foreach (var invitation in ActivityInvitationResolver.ForActivity(state, activity.Id))
            if (IsPresent(state, activity, invitation.InviteeId))
                present.Add(invitation.InviteeId);

        return present.ToArray();
    }

    /// <summary>Whether <paramref name="characterId"/> is present at <paramref name="activity"/>.</summary>
    public static bool IsPresent(WorldState state, HostedActivity activity, RuntimeId<Character> characterId)
    {
        if (!IsAlive(state, characterId))
            return false;
        if (activity.HostCharacterId == characterId)
            return true;

        return ActivityInvitationResolver.Get(state, activity.Id, characterId) is { IsAttending: true }
               && ActivityAvailability.CanAttend(state, characterId, activity.Venue.SettlementId);
    }

    /// <summary>The largest-Scale Activity held in <paramref name="month"/> that both Characters are
    /// present at, if any — the "this happened in front of witnesses" check. "Held" is either still
    /// in progress or concluded this very month: a Quick Activity begins, runs every Phase, and
    /// concludes inside one <see cref="ActivityProgressSystem"/> tick, so a consumer reading only
    /// in-progress Activities would never see one.</summary>
    public static HostedActivity? SharedActivityHeldIn(
        WorldState state, RuntimeId<Character> firstId, RuntimeId<Character> secondId, GameDate month)
    {
        HostedActivity? best = null;
        foreach (var activity in ActivityResolver.HeldIn(state, month))
        {
            if (!IsPresent(state, activity, firstId) || !IsPresent(state, activity, secondId))
                continue;
            if (best is null || activity.Scale > best.Scale)
                best = activity;
        }

        return best;
    }

    /// <summary>§7 applied to a Scheme: the extra monthly discovery risk from initiator and target both
    /// being present at the same Activity held this month (zero when they are not).</summary>
    public static int SchemeDiscoveryRiskBonus(
        WorldState state, RuntimeId<Character> initiatorId, RuntimeId<Character> targetId, GameDate month) =>
        SharedActivityHeldIn(state, initiatorId, targetId, month) is { } shared
            ? ActivityCatalog.SharedActivityDiscoveryRiskBonus(shared.Scale)
            : 0;

    private static bool IsAlive(WorldState state, RuntimeId<Character> characterId) =>
        state.Characters.TryGet(characterId, out var character) && character.IsAlive;
}
