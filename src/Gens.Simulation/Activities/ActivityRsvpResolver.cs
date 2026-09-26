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

/// <summary>Emitted when §4.2's exclusion check finds someone who reasonably expected an invitation
/// and did not get one. Public for a Grand or Lavish Activity — the whole town knows who was not at
/// the great feast, discoverable without being present (§8.2) — otherwise private to the snubbed
/// Character and the host.</summary>
public sealed record ActivityExclusionSnubEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Activity> ActivityId,
    RuntimeId<Character> ExcludedCharacterId,
    RuntimeId<Character> HostCharacterId,
    ActivityScaleTier Scale,
    int OpinionDelta) : IDomainEvent
{
    public string Type => "activities.exclusionSnub";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ExcludedCharacterId.ToTaggedString(), HostCharacterId.ToTaggedString() };
    public string? CausationId => null;

    public Visibility Visibility => Scale >= ActivityScaleTier.Grand
        ? Visibility.Public
        : Visibility.Private(ExcludedCharacterId.ToTaggedString(), HostCharacterId.ToTaggedString());
}

/// <summary>
/// §4's Guest List reads, applied when an Activity begins. Deterministic — no random draw — so the
/// same relationship web always yields the same guest list outcome.
/// </summary>
public static class ActivityRsvpResolver
{
    private const BondTag KinBonds = BondTag.Parent | BondTag.Child | BondTag.Sibling;
    private const BondTag PatronageBonds = BondTag.Patron | BondTag.Client;

    /// <summary>
    /// §4.1: an unanswered invitee's RSVP "reads their existing opinion, Faction, and standing". Their
    /// own opinion of the host, lifted by kinship/marriage/friendship/patronage and hurt by rivalry;
    /// sharing (or not) the host's Culture (the "properly Roman vs. heavily Hellenized" read); and the
    /// draw of a bigger gathering. A Nemesis always accepts — "specifically to cause trouble once
    /// there", §4.1's sharper case. A dead invitee, or one physically away (<see
    /// cref="ActivityAvailability.CanAttend"/>), cannot attend at all.
    /// </summary>
    public static (ActivityRsvpStatus Status, bool AttendsToCauseTrouble) Resolve(
        WorldState state, HostedActivity activity, RuntimeId<Character> inviteeId)
    {
        if (!state.Characters.TryGet(inviteeId, out var invitee) || !invitee.IsAlive ||
            !ActivityAvailability.CanAttend(state, inviteeId, activity.Venue.SettlementId))
            return (ActivityRsvpStatus.Declined, false);

        var hasTie = state.Relationships.TryGet(new RelationshipKey(inviteeId, activity.HostCharacterId), out var tie);
        if (hasTie && tie.HasBond(BondTag.Nemesis))
            return (ActivityRsvpStatus.Accepted, true);

        var score = hasTie ? tie.Opinion : 0;
        if (hasTie)
        {
            if ((tie.Bonds & KinBonds) != BondTag.None)
                score += ActivityCatalog.RsvpKinBonus;
            if (tie.HasBond(BondTag.Spouse))
                score += ActivityCatalog.RsvpSpouseBonus;
            if (tie.HasBond(BondTag.Friend))
                score += ActivityCatalog.RsvpFriendBonus;
            if ((tie.Bonds & PatronageBonds) != BondTag.None)
                score += ActivityCatalog.RsvpPatronageBonus;
            if (tie.HasBond(BondTag.Rival))
                score -= ActivityCatalog.RsvpRivalPenalty;
        }

        if (state.Characters.TryGet(activity.HostCharacterId, out var host))
            score += host.Culture == invitee.Culture ? ActivityCatalog.RsvpCultureAffinity : -ActivityCatalog.RsvpCultureAffinity;

        score += ActivityCatalog.RsvpScalePrestigeBonus(activity.Scale);

        return (score >= ActivityCatalog.RsvpAcceptThreshold ? ActivityRsvpStatus.Accepted : ActivityRsvpStatus.Declined, false);
    }

    /// <summary>
    /// §4.2: who would "reasonably expect an invitation" and was left off. The host's own household's
    /// living Adults, and anyone the host is tied to by marriage, kinship, friendship, or patronage
    /// (the host's own outgoing <see cref="Relationship"/> bonds) — minus everyone already invited. An
    /// Intimate gathering expects no one (<see cref="ActivityCatalog.ExclusionOpinionPenalty"/> is zero).
    /// Ascending ID order.
    /// </summary>
    public static IReadOnlyList<RuntimeId<Character>> ExpectedButExcluded(WorldState state, HostedActivity activity, GameDate date)
    {
        if (ActivityCatalog.ExclusionOpinionPenalty(activity.Scale) == 0)
            return Array.Empty<RuntimeId<Character>>();

        var expected = new SortedSet<RuntimeId<Character>>();
        if (activity.HostHouseholdId is { } householdId)
        {
            foreach (var entry in state.Characters.InAscendingOrder())
                if (entry.Value.Household == householdId)
                    expected.Add(entry.Key);
        }

        const BondTag expectingBonds = KinBonds | PatronageBonds | BondTag.Spouse | BondTag.Friend;
        foreach (var entry in state.Relationships.InAscendingOrder())
            if (entry.Key.From == activity.HostCharacterId && (entry.Value.Bonds & expectingBonds) != BondTag.None)
                expected.Add(entry.Key.To);

        return expected
            .Where(id => id != activity.HostCharacterId)
            .Where(id => ActivityInvitationResolver.Get(state, activity.Id, id) is null)
            .Where(id => state.Characters.TryGet(id, out var character)
                         && character.IsAlive
                         && character.GetLifecycleStage(date) >= LifecycleStage.Adult)
            .ToArray();
    }
}
