using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Activities;

/// <summary>
/// §7's consolidation: "an Activity's own Guest List <i>is</i> its Witness Pool." Several systems
/// already lean on an informal "how many people are aware" input; this is the one shared, named
/// population they check against whenever the triggering moment lands during a hosted Activity. It
/// rebuilds none of their discovery formulas — it only answers who was there, and how public it was.
/// <see cref="Interactions.SchemeProgressSystem"/> is the first consumer: a Scheme whose initiator and
/// target are both attending the same in-progress Activity accrues extra discovery risk.
/// </summary>
public static class ActivityWitnessPool
{
    /// <summary>Everyone present: the host plus every invitee who accepted and is still alive, in
    /// ascending ID order.</summary>
    public static IReadOnlyList<RuntimeId<Character>> Of(WorldState state, HostedActivity activity)
    {
        var present = new SortedSet<RuntimeId<Character>>();
        if (IsAlive(state, activity.HostCharacterId))
            present.Add(activity.HostCharacterId);

        foreach (var invitation in ActivityInvitationResolver.ForActivity(state, activity.Id))
            if (invitation.IsAttending && IsAlive(state, invitation.InviteeId))
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

        return ActivityInvitationResolver.Get(state, activity.Id, characterId) is { IsAttending: true };
    }

    /// <summary>The largest-Scale in-progress Activity both Characters are currently present at, if
    /// any — the "this happened in front of witnesses" check.</summary>
    public static HostedActivity? SharedInProgressActivity(
        WorldState state, RuntimeId<Character> firstId, RuntimeId<Character> secondId)
    {
        HostedActivity? best = null;
        foreach (var activity in ActivityResolver.InProgress(state))
        {
            if (!IsPresent(state, activity, firstId) || !IsPresent(state, activity, secondId))
                continue;
            if (best is null || activity.Scale > best.Scale)
                best = activity;
        }

        return best;
    }

    /// <summary>§7 applied to a Scheme: the extra monthly discovery risk from initiator and target both
    /// being present at the same in-progress Activity (zero when they are not).</summary>
    public static int SchemeDiscoveryRiskBonus(WorldState state, RuntimeId<Character> initiatorId, RuntimeId<Character> targetId) =>
        SharedInProgressActivity(state, initiatorId, targetId) is { } shared
            ? ActivityCatalog.SharedActivityDiscoveryRiskBonus(shared.Scale)
            : 0;

    private static bool IsAlive(WorldState state, RuntimeId<Character> characterId) =>
        state.Characters.TryGet(characterId, out var character) && character.IsAlive;
}
