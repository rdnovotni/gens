using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Actors;
using Gens.Simulation.Diplomacy;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;

namespace Gens.Simulation.Queries;

/// <summary>One Foreign People a household has been in contact with, as surfaced by <see
/// cref="FrontierDiplomacyQuery"/>.</summary>
public readonly record struct FrontierDiplomacyEntry(
    RuntimeId<Actor> ForeignPeopleActorId,
    string Name,
    LivingWorldActorTier Tier,
    HouseStandingLevel Standing,
    int Goodwill,
    IReadOnlyList<FrontierTreaty> ActiveTreaties);

public readonly record struct FrontierDiplomacyProjection(IReadOnlyList<FrontierDiplomacyEntry> ContactedPeoples);

/// <summary>
/// Every Foreign People <paramref name="observerId"/>'s own household has actually had contact with
/// (Phase 16 item 5 slice 1) — a household's own standing and treaties are read live, since a full <see
/// cref="KnowledgeState"/> partition (ADR 0008) does not exist in code yet; this follows the same
/// live-read-approximation precedent <see cref="SpyPlacementRosterQuery"/> and <see
/// cref="Actors.RivalDossier"/> already established rather than building real knowledge propagation for
/// this slice. Never surfaces another household's own standing or treaties with the same people.
/// </summary>
public sealed class FrontierDiplomacyQuery : IWorldQuery<FrontierDiplomacyProjection>
{
    /// <param name="observerId">A tagged <see cref="RuntimeId{T}"/> string for a <see
    /// cref="Household"/> — this query answers "what has this household's own diplomacy actually
    /// touched."</param>
    public FrontierDiplomacyProjection Execute(WorldState state, string observerId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (observerId is null)
            throw new ArgumentNullException(nameof(observerId));

        var householdId = RuntimeId<Household>.Parse(observerId);

        var entries = new List<FrontierDiplomacyEntry>();
        foreach (var entry in state.Actors.InAscendingOrder())
        {
            if (entry.Value.ActorType != LivingWorldActorType.ForeignPeople)
                continue;

            var actorId = entry.Key;
            var key = new PerPeopleStandingKey(householdId, actorId);
            if (!state.PerPeopleStandings.TryGet(key, out var standing))
                continue;

            var treaties = state.FrontierTreaties.InAscendingOrder()
                .Where(t => t.Value.HouseholdId == householdId && t.Value.ForeignPeopleActorId == actorId &&
                    t.Value.Status == FrontierTreatyStatus.Active)
                .Select(t => t.Value)
                .ToArray();

            entries.Add(new FrontierDiplomacyEntry(
                actorId, entry.Value.Name, entry.Value.Tier, standing.Standing, standing.Goodwill, treaties));
        }

        return new FrontierDiplomacyProjection(entries);
    }
}
