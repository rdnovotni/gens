using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;

namespace Gens.Simulation.Actors;

/// <summary>
/// Seeds a <see cref="LivingWorldActorType.BanditConfederation"/> at campaign start (Phase 16 item 2;
/// <c>gens-piracy-banditry-design.md</c> §2: "reusing Rival Houses' framework directly rather than
/// inventing a parallel one"). Mirrors <see cref="RivalHouseCreationService.CreateAncientSeed"/>'s
/// exact shape rather than duplicating a second house-creation pattern; a Confederation carries no
/// Identity tags (§3.3's Economic/Faction tags describe a citizen household, not a raiding band) and
/// always starts <see cref="LivingWorldActorTier.Background"/> with no head Character, matching
/// <c>LivingWorldActor.HeadCharacterId</c>'s lazy-instantiation convention (§3.2) — a Confederation only
/// gets a real leader the moment the player actually faces or deals with one (§2: "a leader generated
/// as a full Character the moment the player actually faces or deals with them").
///
/// §2's own land-vs-sea distinction ("Banditry is land-based... Piracy is sea-based... both are the
/// same underlying actor type at different terrain and Combatant profiles") is deliberately not modeled
/// as a data field here: no terrain/Combatant-profile concept exists anywhere in this codebase yet (see
/// <see cref="LivingWorldActorMilitaryStrength"/>'s own "no Force/Squad record exists" admission), so
/// this slice treats every <see cref="LivingWorldActorType.BanditConfederation"/> uniformly regardless
/// of land or sea domain — a caller wanting a specifically maritime Confederation simply seeds its
/// <see cref="LivingWorldActor.HomeSettlementId"/> as a coastal settlement, with the terrain/Fleet
/// distinction itself left to whatever future pass finally builds Military &amp; Combat's Fleet type.
/// </summary>
public static class BanditConfederationCreationService
{
    /// <summary>Seeds one Background-tier <see cref="LivingWorldActorType.BanditConfederation"/>.</summary>
    public static LivingWorldActor CreateAncientSeed(
        WorldState state,
        string name,
        LivingWorldActorStandingTrend standingTrend,
        LivingWorldActorMilitaryStrength militaryStrength,
        RuntimeId<Region> regionId,
        RuntimeId<Settlement> homeSettlementId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var actor = LivingWorldActor.Create(
            state.ActorIds.Issue(), LivingWorldActorType.BanditConfederation, name, LivingWorldActorTier.Background,
            standingTrend, LivingWorldActorOrigin.Ancient, parentActorId: null, LivingWorldActorIdentity.None,
            dignitas: 0, new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null), militaryStrength,
            regionId, homeSettlementId);
        state.Actors.Add(actor.ActorId, actor);
        return actor;
    }
}
