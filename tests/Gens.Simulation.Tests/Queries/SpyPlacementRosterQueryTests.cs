using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.Queries;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;
using CharacterTestFixtures = Gens.Simulation.Tests.Characters.CharacterTestFixtures;

namespace Gens.Simulation.Tests.Queries;

/// <summary>Phase 16 item 1 coverage for <see cref="SpyPlacementRosterQuery"/>.</summary>
public sealed class SpyPlacementRosterQueryTests
{
    private static RuntimeId<Actor> AddTargetActor(WorldState state, RuntimeId<Character>? headCharacterId = null)
    {
        var netWorth = new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null);
        var military = new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest);
        var actor = RivalHouseCreationService.CreateAncientSeed(
            state, "Cornelia", LivingWorldActorStandingTrend.Established, LivingWorldActorIdentity.None,
            0, netWorth, military, state.RegionIds.Issue(), state.SettlementIds.Issue());

        if (headCharacterId is { } headId)
        {
            state.Actors.Remove(actor.ActorId);
            state.Actors.Add(actor.ActorId, actor with { HeadCharacterId = headId });
        }

        return actor.ActorId;
    }

    [Test]
    public void ASponsorSeesTheirOwnPlacementRegardlessOfStatus()
    {
        var state = new WorldState(new GameDate(0));
        var spyId = state.CharacterIds.Issue();
        state.Characters.Add(spyId, CharacterTestFixtures.Minimal(spyId));
        var sponsorId = state.CharacterIds.Issue();
        state.Characters.Add(sponsorId, CharacterTestFixtures.Minimal(sponsorId));
        var targetActorId = AddTargetActor(state);
        var placementId = state.SpyPlacementIds.Issue();
        state.SpyPlacements.Add(placementId, SpyPlacement.Create(
            placementId, spyId, sponsorId, targetActorId, SpyPlacementType.QuickOp, concealmentQuality: 50, new GameDate(0)));

        var projection = new SpyPlacementRosterQuery().Execute(state, sponsorId.ToTaggedString());

        Assert.That(projection.Placements, Has.Count.EqualTo(1));
        Assert.That(projection.Placements[0].IsOwnPlacement, Is.True);
    }

    [Test]
    public void AnUninvolvedObserverSeesNoUndiscoveredPlacements()
    {
        var state = new WorldState(new GameDate(0));
        var spyId = state.CharacterIds.Issue();
        state.Characters.Add(spyId, CharacterTestFixtures.Minimal(spyId));
        var sponsorId = state.CharacterIds.Issue();
        state.Characters.Add(sponsorId, CharacterTestFixtures.Minimal(sponsorId));
        var observerId = state.CharacterIds.Issue();
        state.Characters.Add(observerId, CharacterTestFixtures.Minimal(observerId));
        var targetActorId = AddTargetActor(state);
        var placementId = state.SpyPlacementIds.Issue();
        state.SpyPlacements.Add(placementId, SpyPlacement.Create(
            placementId, spyId, sponsorId, targetActorId, SpyPlacementType.QuickOp, concealmentQuality: 50, new GameDate(0)));

        var projection = new SpyPlacementRosterQuery().Execute(state, observerId.ToTaggedString());

        Assert.That(projection.Placements, Is.Empty);
    }

    [Test]
    public void ATargetsHeadSeesAPlacementOnlyOnceItResolvesDiscoveredAndTraced()
    {
        var state = new WorldState(new GameDate(0));
        var spyId = state.CharacterIds.Issue();
        state.Characters.Add(spyId, CharacterTestFixtures.Minimal(spyId));
        var sponsorId = state.CharacterIds.Issue();
        state.Characters.Add(sponsorId, CharacterTestFixtures.Minimal(sponsorId));
        var targetHeadId = state.CharacterIds.Issue();
        state.Characters.Add(targetHeadId, CharacterTestFixtures.Minimal(targetHeadId));
        var targetActorId = AddTargetActor(state, targetHeadId);
        var placementId = state.SpyPlacementIds.Issue();
        var placement = SpyPlacement.Create(
            placementId, spyId, sponsorId, targetActorId, SpyPlacementType.QuickOp, concealmentQuality: 50, new GameDate(0));
        state.SpyPlacements.Add(placementId, placement);

        var beforeDiscovery = new SpyPlacementRosterQuery().Execute(state, targetHeadId.ToTaggedString());
        Assert.That(beforeDiscovery.Placements, Is.Empty);

        state.SpyPlacements.Remove(placementId);
        state.SpyPlacements.Add(placementId, placement with { Status = SpyPlacementStatus.DiscoveredAndTraced });

        var afterDiscovery = new SpyPlacementRosterQuery().Execute(state, targetHeadId.ToTaggedString());
        Assert.Multiple(() =>
        {
            Assert.That(afterDiscovery.Placements, Has.Count.EqualTo(1));
            Assert.That(afterDiscovery.Placements[0].IsOwnPlacement, Is.False);
        });
    }
}
