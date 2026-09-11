using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Actors;
using Gens.Simulation.Cultures;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Diplomacy;

/// <summary>
/// Seeds a <see cref="LivingWorldActorType.ForeignPeople"/> at campaign start (Phase 16 item 5 slice 1;
/// <c>gens-diplomacy-non-roman-peoples-design.md</c> §2: "reuses Rival Houses' own Background/Note
/// tiering wholesale"). Mirrors <see cref="BanditConfederationCreationService.CreateAncientSeed"/>'s
/// exact shape: Background tier, <see cref="LivingWorldActorOrigin.Ancient"/>, no head Character yet
/// (lazily generated the moment the player actually engages this people, matching §2's own "sustained
/// contact promotes them to a real, lazily-instantiated leader Character"), and no Identity tags (§3.3's
/// Economic/Faction tags describe a Roman household, not a foreign people).
///
/// Only a <see cref="CultureCategory.Frontier"/> culture (as of the seeding date) may seed a Foreign
/// People here — Great Power, Contested Buffer, and Trade-Contact-Only peoples are explicitly out of
/// this slice's scope (see the build roadmap's Phase 16 item 5 progress note).
/// </summary>
public static class ForeignPeopleCreationService
{
    /// <summary>Seeds one Background-tier <see cref="LivingWorldActorType.ForeignPeople"/> for
    /// <paramref name="cultureId"/>. Throws if that culture is not <see
    /// cref="CultureCategory.Frontier"/> as of <paramref name="asOf"/> — this slice never seeds a
    /// Great Power, Contested Buffer, or Trade-Contact-Only people.</summary>
    public static LivingWorldActor CreateAncientSeed(
        WorldState state,
        CultureCatalog cultures,
        string name,
        DefinitionId<Culture> cultureId,
        LivingWorldActorStandingTrend standingTrend,
        LivingWorldActorMilitaryStrength militaryStrength,
        RuntimeId<Region> regionId,
        RuntimeId<Settlement> homeSettlementId,
        GameDate asOf)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (cultures is null)
            throw new ArgumentNullException(nameof(cultures));

        var culture = cultures.Get(cultureId);
        if (culture.CategoryAsOf(asOf) != CultureCategory.Frontier)
        {
            throw new ArgumentException(
                $"Culture '{cultureId.Value}' is not a Frontier culture as of {asOf}; only Frontier " +
                "peoples may be seeded by this slice.",
                nameof(cultureId));
        }

        var actor = LivingWorldActor.Create(
            state.ActorIds.Issue(), LivingWorldActorType.ForeignPeople, name, LivingWorldActorTier.Background,
            standingTrend, LivingWorldActorOrigin.Ancient, parentActorId: null, LivingWorldActorIdentity.None,
            dignitas: 0, new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null), militaryStrength,
            regionId, homeSettlementId);
        state.Actors.Add(actor.ActorId, actor);
        state.ForeignPeopleDetails.Add(actor.ActorId, new ForeignPeopleDetails(actor.ActorId, cultureId));
        return actor;
    }
}
