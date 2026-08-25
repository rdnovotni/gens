using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;

namespace Gens.Simulation.Actors;

/// <summary>
/// House creation (Phase 10 item 4; <c>gens-rival-houses-design.md</c> §2.2) along the three paths a
/// <see cref="LivingWorldActor"/> comes into being: an <see cref="LivingWorldActorOrigin.Ancient"/>
/// house seeded at campaign start, a <see cref="LivingWorldActorOrigin.NovusHomo"/> promotion (a
/// wealthy/distinguished family rising into the roster), and a <see
/// cref="LivingWorldActorOrigin.CadetBranch"/> split off an existing house. Every path creates a
/// <see cref="LivingWorldActorTier.Background"/> actor with no head Character yet — see <see
/// cref="LivingWorldActorHeadGenerator"/> for the separate lazy-instantiation step that generates one
/// only once actually needed (§3.2). No content-authored seed data is read here: this package's
/// research found no compiled-content-to-runtime-catalog loader exists yet for any content family
/// (not even the established <c>names</c>/<c>cultures</c>/<c>regions</c> families), so building one
/// solely for a short starting-house list would be new infrastructure, not reuse — out of this
/// package's scope. Every "ancient" house is instead seeded by whatever calls <see
/// cref="CreateAncientSeed"/> (a future campaign bootstrap step) with an already-chosen name and
/// figures, matching <c>CampaignBootstrapper</c>'s own existing "just issue the ID, no catalog"
/// pattern for the player's starting household.
/// </summary>
public static class RivalHouseCreationService
{
    /// <summary>Seeds an <see cref="LivingWorldActorOrigin.Ancient"/> house at campaign start.</summary>
    public static LivingWorldActor CreateAncientSeed(
        WorldState state,
        string nomen,
        LivingWorldActorStandingTrend standingTrend,
        LivingWorldActorIdentity identityTags,
        int dignitas,
        LivingWorldActorNetWorth netWorth,
        LivingWorldActorMilitaryStrength militaryStrength,
        RuntimeId<Region> regionId,
        RuntimeId<Settlement> homeSettlementId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var actor = LivingWorldActor.Create(
            state.ActorIds.Issue(), LivingWorldActorType.Gens, nomen, LivingWorldActorTier.Background,
            standingTrend, LivingWorldActorOrigin.Ancient, parentActorId: null, identityTags, dignitas,
            netWorth, militaryStrength, regionId, homeSettlementId);
        state.Actors.Add(actor.ActorId, actor);
        return actor;
    }

    /// <summary>Promotes a wealthy/distinguished family into a new Background house (§2.2's <i>novus
    /// homo</i> path). Always starts <see cref="LivingWorldActorStandingTrend.Rising"/> with negligible
    /// military strength — a family just arriving at this tier has not yet built the standing a
    /// long-established house has.</summary>
    public static LivingWorldActor CreateNovusHomo(
        WorldState state,
        string nomen,
        LivingWorldActorIdentity identityTags,
        int dignitas,
        LivingWorldActorNetWorth netWorth,
        RuntimeId<Region> regionId,
        RuntimeId<Settlement> homeSettlementId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var actor = LivingWorldActor.Create(
            state.ActorIds.Issue(), LivingWorldActorType.Gens, nomen, LivingWorldActorTier.Background,
            LivingWorldActorStandingTrend.Rising, LivingWorldActorOrigin.NovusHomo, parentActorId: null,
            identityTags, dignitas, netWorth, new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Negligible),
            regionId, homeSettlementId);
        state.Actors.Add(actor.ActorId, actor);
        return actor;
    }

    /// <summary>Splits a new house off an existing one (§2.2's cadet-branch path: "a younger son takes
    /// the cognomen and a share of the Holdings and founds a new, separately-tracked
    /// <c>LivingWorldActor</c>"). The cadet inherits its parent's region, home settlement, and Identity
    /// tags, and — per §2.2's "starts with soft positive relationship baseline to parent house" — this
    /// also records an initial <see cref="HouseStandingLevel.Allied"/> <see cref="HouseStanding"/>
    /// between parent and cadet, unless that pair is somehow already tracked.</summary>
    public static LivingWorldActor CreateCadetBranch(WorldState state, LivingWorldActor parent, string cadetName)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (parent is null)
            throw new ArgumentNullException(nameof(parent));

        var cadet = LivingWorldActor.Create(
            state.ActorIds.Issue(), parent.ActorType, cadetName, LivingWorldActorTier.Background,
            LivingWorldActorStandingTrend.Rising, LivingWorldActorOrigin.CadetBranch, parent.ActorId,
            parent.IdentityTags, dignitas: 0,
            new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Negligible),
            parent.RegionId, parent.HomeSettlementId);
        state.Actors.Add(cadet.ActorId, cadet);

        var standingKey = HouseStandingKey.Between(parent.ActorId, cadet.ActorId);
        if (!state.HouseStandings.TryGet(standingKey, out _))
            state.HouseStandings.Add(standingKey, new HouseStanding(HouseStandingLevel.Allied));

        return cadet;
    }
}
