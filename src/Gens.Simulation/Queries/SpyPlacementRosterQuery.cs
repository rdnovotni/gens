using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.State;

namespace Gens.Simulation.Queries;

/// <summary>One <see cref="Interactions.SpyPlacement"/> entry as surfaced by <see
/// cref="SpyPlacementRosterQuery"/> — a presentation-ready snapshot, not a live reference.</summary>
public readonly record struct SpyPlacementRosterEntry(
    RuntimeId<SpyPlacement> PlacementId,
    RuntimeId<Character> SpyCharacterId,
    RuntimeId<Actor> TargetActorId,
    SpyPlacementType Type,
    SpyPlacementStatus Status,
    bool IsOwnPlacement);

public readonly record struct SpyPlacementRosterProjection(IReadOnlyList<SpyPlacementRosterEntry> Placements);

/// <summary>
/// Every <see cref="Interactions.SpyPlacement"/> the requesting <paramref name="observerId"/>
/// legitimately knows about (Phase 16 item 1): placements they sponsor (an owner always knows their
/// own placements — no <see cref="State.KnowledgeState"/> indirection needed for a household's own
/// data, the same reasoning already applied elsewhere in this codebase), plus any placement resolved
/// <see cref="SpyPlacementStatus.DiscoveredAndTraced"/> where the observer is the targeted actor's own
/// head Character — the one outcome <see cref="Interactions.SpyPlacementProgressSystem"/> and <see
/// cref="Interactions.CounterEspionageSweepCommands"/> actually make knowable to the target.
///
/// This is a deliberate, narrower stopgap rather than a real <see cref="State.KnowledgeState"/>-backed
/// dossier: it filters directly off <see cref="Interactions.SpyPlacement.Status"/> and ownership rather
/// than reading through per-observer knowledge propagation, which does not exist yet for espionage
/// (Phase 16 item 1's own deferred scope). It does not read the raw <see
/// cref="State.WorldState.SpyPlacements"/> partition as a dossier for anyone not already entitled by
/// ownership or a resolution event, so it does not violate ADR 0008's "never read the truth partition
/// directly" rule — it is simply honest about being an interim surface pending real propagation.
/// </summary>
public sealed class SpyPlacementRosterQuery : IWorldQuery<SpyPlacementRosterProjection>
{
    /// <param name="observerId">A tagged <see cref="RuntimeId{T}"/> string for a <see
    /// cref="Character"/> — a sponsor or a targeted actor's own head. The reserved <c>"player"</c>
    /// observer sentinel (ADR 0008) is not accepted here: this query answers "what does this specific
    /// Character legitimately know," and the player's own household is always reached through its own
    /// characters, not a synthetic omniscient observer.</param>
    public SpyPlacementRosterProjection Execute(WorldState state, string observerId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (observerId is null)
            throw new ArgumentNullException(nameof(observerId));

        var observerCharacterId = RuntimeId<Character>.Parse(observerId);

        var entries = new List<SpyPlacementRosterEntry>();
        foreach (var entry in state.SpyPlacements.InAscendingOrder())
        {
            var placement = entry.Value;
            var isOwn = placement.SponsoringCharacterId == observerCharacterId;
            var isKnownToTarget = placement.Status == SpyPlacementStatus.DiscoveredAndTraced
                && state.Actors.TryGet(placement.TargetActorId, out var targetActor)
                && targetActor!.HeadCharacterId == observerCharacterId;

            if (isOwn || isKnownToTarget)
                entries.Add(new SpyPlacementRosterEntry(entry.Key, placement.SpyCharacterId, placement.TargetActorId, placement.Type, placement.Status, isOwn));
        }

        return new SpyPlacementRosterProjection(entries);
    }
}
