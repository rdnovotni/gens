using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Interactions;

/// <summary>Phase 16 item 1 coverage for <see cref="PlaceSpyCommand"/>.</summary>
public sealed class PlaceSpyCommandTests
{
    private static RuntimeId<Actor> AddTargetActor(WorldState state)
    {
        var netWorth = new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null);
        var military = new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest);
        var actor = RivalHouseCreationService.CreateAncientSeed(
            state, "Cornelia", LivingWorldActorStandingTrend.Established, LivingWorldActorIdentity.None,
            0, netWorth, military, state.RegionIds.Issue(), state.SettlementIds.Issue());
        return actor.ActorId;
    }

    private static WorldState StateWithSpySponsorAndTarget(
        out RuntimeId<Character> spyId, out RuntimeId<Character> sponsorId, out RuntimeId<Actor> targetActorId)
    {
        var state = new WorldState(new GameDate(0));
        spyId = state.CharacterIds.Issue();
        state.Characters.Add(spyId, CharacterTestFixtures.Minimal(spyId));
        sponsorId = state.CharacterIds.Issue();
        state.Characters.Add(sponsorId, CharacterTestFixtures.Minimal(sponsorId));
        targetActorId = AddTargetActor(state);
        return state;
    }

    [Test]
    public void AcceptsAFreshQuickOpPlacement()
    {
        var state = StateWithSpySponsorAndTarget(out var spyId, out var sponsorId, out var targetActorId);
        var command = new PlaceSpyCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), new GameDate(0), CausationId: null,
            spyId, sponsorId, targetActorId, SpyPlacementType.QuickOp);

        var result = PlaceSpyCommands.Pipeline.Execute(state, command);

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(result.Events, Has.Count.EqualTo(1));
            Assert.That(result.Events[0], Is.InstanceOf<SpyPlacedEvent>());
            Assert.That(state.SpyPlacements.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public void AcceptsAFreshPersistentNetworkPlacement()
    {
        var state = StateWithSpySponsorAndTarget(out var spyId, out var sponsorId, out var targetActorId);
        var command = new PlaceSpyCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), new GameDate(0), CausationId: null,
            spyId, sponsorId, targetActorId, SpyPlacementType.PersistentNetwork);

        var result = PlaceSpyCommands.Pipeline.Execute(state, command);

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(state.SpyPlacements.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public void RejectsWhenTheSpyIsUnknown()
    {
        var state = StateWithSpySponsorAndTarget(out _, out var sponsorId, out var targetActorId);
        var unknownSpyId = state.CharacterIds.Issue();
        var command = new PlaceSpyCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), new GameDate(0), CausationId: null,
            unknownSpyId, sponsorId, targetActorId, SpyPlacementType.QuickOp);

        var result = PlaceSpyCommands.Pipeline.Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(PlaceSpyCommands.SpyNotFound));
    }

    [Test]
    public void RejectsWhenTheTargetActorIsUnknown()
    {
        var state = StateWithSpySponsorAndTarget(out var spyId, out var sponsorId, out _);
        var unknownActorId = state.ActorIds.Issue();
        var command = new PlaceSpyCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), new GameDate(0), CausationId: null,
            spyId, sponsorId, unknownActorId, SpyPlacementType.QuickOp);

        var result = PlaceSpyCommands.Pipeline.Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(PlaceSpyCommands.TargetActorNotFound));
    }

    [Test]
    public void RejectsWhenTheSpyIsDeceased()
    {
        var state = new WorldState(new GameDate(0));
        var spyId = state.CharacterIds.Issue();
        state.Characters.Add(spyId, CharacterTestFixtures.Minimal(
            spyId, deathRecord: new DeathRecord(new GameDate(0), DeathCause.OldAge, 70)));
        var sponsorId = state.CharacterIds.Issue();
        state.Characters.Add(sponsorId, CharacterTestFixtures.Minimal(sponsorId));
        var targetActorId = AddTargetActor(state);

        var command = new PlaceSpyCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), new GameDate(0), CausationId: null,
            spyId, sponsorId, targetActorId, SpyPlacementType.QuickOp);

        var result = PlaceSpyCommands.Pipeline.Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(PlaceSpyCommands.SpyDeceased));
    }

    [Test]
    public void RejectsASecondConcurrentPlacementForTheSameSpy()
    {
        var state = StateWithSpySponsorAndTarget(out var spyId, out var sponsorId, out var targetActorId);
        var otherTargetActorId = AddTargetActor(state);

        var first = new PlaceSpyCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), new GameDate(0), CausationId: null,
            spyId, sponsorId, targetActorId, SpyPlacementType.QuickOp);
        PlaceSpyCommands.Pipeline.Execute(state, first);

        var second = new PlaceSpyCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), new GameDate(0), CausationId: null,
            spyId, sponsorId, otherTargetActorId, SpyPlacementType.QuickOp);
        var result = PlaceSpyCommands.Pipeline.Execute(state, second);

        Assert.That(result.Error, Is.EqualTo(PlaceSpyCommands.SpyAlreadyPlaced));
    }

    [Test]
    public void RejectsAPlacementOnceTheSponsorsSpymasterCapacityIsReached()
    {
        var state = StateWithSpySponsorAndTarget(out _, out var sponsorId, out var targetActorId);

        for (var i = 0; i < SpyPlacementCatalog.SpymasterCapacityCap; i++)
        {
            var spyId = state.CharacterIds.Issue();
            state.Characters.Add(spyId, CharacterTestFixtures.Minimal(spyId));
            var command = new PlaceSpyCommand(
                state.CommandIds.Issue(), sponsorId.ToTaggedString(), new GameDate(0), CausationId: null,
                spyId, sponsorId, targetActorId, SpyPlacementType.QuickOp);
            var accepted = PlaceSpyCommands.Pipeline.Execute(state, command);
            Assert.That(accepted.Accepted, Is.True);
        }

        var extraSpyId = state.CharacterIds.Issue();
        state.Characters.Add(extraSpyId, CharacterTestFixtures.Minimal(extraSpyId));
        var extra = new PlaceSpyCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), new GameDate(0), CausationId: null,
            extraSpyId, sponsorId, targetActorId, SpyPlacementType.QuickOp);
        var result = PlaceSpyCommands.Pipeline.Execute(state, extra);

        Assert.That(result.Error, Is.EqualTo(PlaceSpyCommands.SpymasterCapacityExceeded));
    }
}
