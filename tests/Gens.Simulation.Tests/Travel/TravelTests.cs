using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Random;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Travel;

public sealed class TravelTests
{
    private static readonly GameDate StartDate = new(0);

    // ---- TravelLocation ------------------------------------------------------------------------

    [Test]
    public void HomeLocationCarriesOnlyItsSettlement()
    {
        var settlementId = new RuntimeIdCounter<Settlement>().Issue();
        var location = TravelLocation.Home(settlementId);

        Assert.Multiple(() =>
        {
            Assert.That(location.Kind, Is.EqualTo(LocationKind.Home));
            Assert.That(location.SettlementId, Is.EqualTo(settlementId));
            Assert.That(location.RegionId, Is.Null);
            Assert.That(location.ActorId, Is.Null);
        });
    }

    [Test]
    public void RomeCarriesNoSettlementOrRegion()
    {
        var location = TravelLocation.Rome();

        Assert.Multiple(() =>
        {
            Assert.That(location.Kind, Is.EqualTo(LocationKind.Rome));
            Assert.That(location.SettlementId, Is.Null);
            Assert.That(location.RegionId, Is.Null);
            Assert.That(location.ActorId, Is.Null);
        });
    }

    [Test]
    public void RivalEstateCarriesActorSettlementAndRegion()
    {
        var actorId = new RuntimeIdCounter<Actor>().Issue();
        var settlementId = new RuntimeIdCounter<Settlement>().Issue();
        var location = TravelLocation.RivalEstate(actorId, settlementId, TravelTestFixtures.FarRegionId);

        Assert.Multiple(() =>
        {
            Assert.That(location.Kind, Is.EqualTo(LocationKind.RivalEstate));
            Assert.That(location.ActorId, Is.EqualTo(actorId));
            Assert.That(location.SettlementId, Is.EqualTo(settlementId));
            Assert.That(location.RegionId, Is.EqualTo(TravelTestFixtures.FarRegionId));
        });
    }

    [Test]
    public void FrontierRegionCarriesNoSettlement()
    {
        var location = TravelLocation.FrontierRegion(TravelTestFixtures.FarRegionId);

        Assert.Multiple(() =>
        {
            Assert.That(location.Kind, Is.EqualTo(LocationKind.FrontierRegion));
            Assert.That(location.SettlementId, Is.Null);
            Assert.That(location.RegionId, Is.EqualTo(TravelTestFixtures.FarRegionId));
        });
    }

    // ---- DistanceTierCatalog ---------------------------------------------------------------------

    [Test]
    public void ResolveReturnsNearForARegionAgainstItself()
    {
        var catalog = TravelTestFixtures.BuildDistanceTierCatalog();
        Assert.That(catalog.Resolve(TravelTestFixtures.HomeRegionId, TravelTestFixtures.HomeRegionId), Is.EqualTo(DistanceTier.Near));
    }

    [Test]
    public void ResolveReadsAnAuthoredPairInEitherDirection()
    {
        var catalog = TravelTestFixtures.BuildDistanceTierCatalog();

        Assert.Multiple(() =>
        {
            Assert.That(catalog.Resolve(TravelTestFixtures.HomeRegionId, TravelTestFixtures.FarRegionId), Is.EqualTo(DistanceTier.Far));
            Assert.That(catalog.Resolve(TravelTestFixtures.FarRegionId, TravelTestFixtures.HomeRegionId), Is.EqualTo(DistanceTier.Far));
        });
    }

    [Test]
    public void ResolveDefaultsToModerateForAnUnlistedPair()
    {
        var catalog = TravelTestFixtures.BuildDistanceTierCatalog();
        Assert.That(catalog.Resolve(TravelTestFixtures.HomeRegionId, TravelTestFixtures.UnlistedRegionId), Is.EqualTo(DistanceTier.Moderate));
    }

    [Test]
    public void ConstructorRejectsADuplicatePair()
    {
        Assert.Throws<ArgumentException>(() => new DistanceTierCatalog(new[]
        {
            new RegionDistanceTierEntry(TravelTestFixtures.HomeRegionId, TravelTestFixtures.FarRegionId, DistanceTier.Far),
            new RegionDistanceTierEntry(TravelTestFixtures.FarRegionId, TravelTestFixtures.HomeRegionId, DistanceTier.Near),
        }));
    }

    [Test]
    public void RegionDistanceTierEntryRejectsARegionAgainstItself()
    {
        Assert.Throws<ArgumentException>(() =>
            new RegionDistanceTierEntry(TravelTestFixtures.HomeRegionId, TravelTestFixtures.HomeRegionId, DistanceTier.Near));
    }

    // ---- TravelRoute ------------------------------------------------------------------------------

    [Test]
    public void ResolveToHomeIsAlwaysNearAndSecure()
    {
        var settlementId = new RuntimeIdCounter<Settlement>().Issue();
        var origin = TravelLocation.Home(settlementId);
        var route = TravelRoute.Resolve(
            origin, origin, TravelTestFixtures.HomeRegionId,
            TravelTestFixtures.BuildRegionCatalog(), TravelTestFixtures.BuildDistanceTierCatalog());

        Assert.Multiple(() =>
        {
            Assert.That(route.DistanceTier, Is.EqualTo(DistanceTier.Near));
            Assert.That(route.RiskExposure, Is.EqualTo(RouteRiskLevel.Secure));
            Assert.That(route.TravelTimeMonths, Is.EqualTo(1));
        });
    }

    [Test]
    public void ResolveToFarRegionIsFarAndDangerous()
    {
        var settlementId = new RuntimeIdCounter<Settlement>().Issue();
        var origin = TravelLocation.Home(settlementId);
        var destination = TravelLocation.ProvincialCapital(TravelTestFixtures.FarRegionId, settlementId);
        var route = TravelRoute.Resolve(
            origin, destination, TravelTestFixtures.HomeRegionId,
            TravelTestFixtures.BuildRegionCatalog(), TravelTestFixtures.BuildDistanceTierCatalog());

        Assert.Multiple(() =>
        {
            Assert.That(route.DistanceTier, Is.EqualTo(DistanceTier.Far));
            Assert.That(route.RiskExposure, Is.EqualTo(RouteRiskLevel.Dangerous));
            Assert.That(route.TravelTimeMonths, Is.EqualTo(6));
        });
    }

    [Test]
    public void FrontierRegionIsAlwaysDangerousEvenAtNearTier()
    {
        var settlementId = new RuntimeIdCounter<Settlement>().Issue();
        var origin = TravelLocation.Home(settlementId);
        var destination = TravelLocation.FrontierRegion(TravelTestFixtures.NearRegionId);
        var route = TravelRoute.Resolve(
            origin, destination, TravelTestFixtures.HomeRegionId,
            TravelTestFixtures.BuildRegionCatalog(), TravelTestFixtures.BuildDistanceTierCatalog());

        Assert.Multiple(() =>
        {
            Assert.That(route.DistanceTier, Is.EqualTo(DistanceTier.Near));
            Assert.That(route.RiskExposure, Is.EqualTo(RouteRiskLevel.Dangerous));
        });
    }

    [Test]
    public void RomeResolvesItsTierViaTheCapitalRegionsGazetteerEntry()
    {
        var settlementId = new RuntimeIdCounter<Settlement>().Issue();
        var origin = TravelLocation.Home(settlementId);
        var route = TravelRoute.Resolve(
            origin, TravelLocation.Rome(), TravelTestFixtures.HomeRegionId,
            TravelTestFixtures.BuildRegionCatalog(includeCapital: true), TravelTestFixtures.BuildDistanceTierCatalog());

        Assert.That(route.DistanceTier, Is.EqualTo(DistanceTier.Moderate));
    }

    [Test]
    public void ResolveToRomeThrowsWhenNoRegionSeatsTheCapitalRole()
    {
        var settlementId = new RuntimeIdCounter<Settlement>().Issue();
        var origin = TravelLocation.Home(settlementId);

        Assert.Throws<InvalidOperationException>(() => TravelRoute.Resolve(
            origin, TravelLocation.Rome(), TravelTestFixtures.HomeRegionId,
            TravelTestFixtures.BuildRegionCatalog(includeCapital: false), TravelTestFixtures.BuildDistanceTierCatalog()));
    }

    // ---- TravelParty ------------------------------------------------------------------------------

    [Test]
    public void CreateRejectsTheTravelerAsTheirOwnRetinueMember()
    {
        var counter = new RuntimeIdCounter<Character>();
        var travelerId = counter.Issue();
        Assert.Throws<ArgumentException>(() => TravelParty.Create(travelerId, new[] { travelerId }));
    }

    [Test]
    public void CreateRejectsDuplicateRetinueMembers()
    {
        var counter = new RuntimeIdCounter<Character>();
        var travelerId = counter.Issue();
        var retinueId = counter.Issue();
        Assert.Throws<ArgumentException>(() => TravelParty.Create(travelerId, new[] { retinueId, retinueId }));
    }

    [Test]
    public void AllMembersPutsTheTravelerFirst()
    {
        var counter = new RuntimeIdCounter<Character>();
        var travelerId = counter.Issue();
        var retinueId = counter.Issue();
        var party = TravelParty.Create(travelerId, new[] { retinueId });

        Assert.That(party.AllMembers, Is.EqualTo(new[] { travelerId, retinueId }));
    }

    // ---- BeginTravelCommand -----------------------------------------------------------------------

    [Test]
    public void BeginTravelCreatesATripAndReservesTheParty()
    {
        var (state, travelerId, retinueId) = OneHouseholdWithARetinueMember();
        var pipeline = BeginTravelCommands.BuildPipeline(TravelTestFixtures.BuildRegionCatalog(), TravelTestFixtures.BuildDistanceTierCatalog());

        var command = new BeginTravelCommand(
            state.CommandIds.Issue(), "player", StartDate, null, travelerId, new[] { retinueId },
            TravelTestFixtures.HomeRegionId, TravelLocation.FrontierRegion(TravelTestFixtures.FarRegionId));
        var result = pipeline.Execute(state, command);

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(result.Events.Single(), Is.InstanceOf<TravelBegunEvent>());
            Assert.That(state.TravelTrips.Count, Is.EqualTo(1));
            Assert.That(TravelTripQueries.IsReserved(state, travelerId), Is.True);
            Assert.That(TravelTripQueries.IsReserved(state, retinueId), Is.True);
        });

        var trip = state.TravelTrips.InAscendingOrder().Single().Value;
        Assert.Multiple(() =>
        {
            Assert.That(trip.Status, Is.EqualTo(TravelTripStatus.Traveling));
            Assert.That(trip.Party.TravelerId, Is.EqualTo(travelerId));
            Assert.That(trip.Party.RetinueIds, Is.EqualTo(new[] { retinueId }));
            Assert.That(trip.DistanceTier, Is.EqualTo(DistanceTier.Far));
        });
    }

    [Test]
    public void BeginTravelRejectsASecondConcurrentTripForTheSameTraveler()
    {
        var (state, travelerId, _) = OneHouseholdWithARetinueMember();
        var pipeline = BeginTravelCommands.BuildPipeline(TravelTestFixtures.BuildRegionCatalog(), TravelTestFixtures.BuildDistanceTierCatalog());

        var first = new BeginTravelCommand(
            state.CommandIds.Issue(), "player", StartDate, null, travelerId, Array.Empty<RuntimeId<Character>>(),
            TravelTestFixtures.HomeRegionId, TravelLocation.FrontierRegion(TravelTestFixtures.FarRegionId));
        pipeline.Execute(state, first);

        var second = new BeginTravelCommand(
            state.CommandIds.Issue(), "player", StartDate, null, travelerId, Array.Empty<RuntimeId<Character>>(),
            TravelTestFixtures.HomeRegionId, TravelLocation.FrontierRegion(TravelTestFixtures.NearRegionId));
        var result = pipeline.Execute(state, second);

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(BeginTravelCommands.PartyMemberAlreadyTraveling));
        });
    }

    [Test]
    public void BeginTravelRejectsHomeAsADestination()
    {
        var (state, travelerId, _) = OneHouseholdWithARetinueMember();
        var pipeline = BeginTravelCommands.BuildPipeline(TravelTestFixtures.BuildRegionCatalog(), TravelTestFixtures.BuildDistanceTierCatalog());

        state.Characters.TryGet(travelerId, out var traveler);
        var command = new BeginTravelCommand(
            state.CommandIds.Issue(), "player", StartDate, null, travelerId, Array.Empty<RuntimeId<Character>>(),
            TravelTestFixtures.HomeRegionId, TravelLocation.Home(traveler.Location));
        var result = pipeline.Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(BeginTravelCommands.DestinationMustNotBeHome));
    }

    [Test]
    public void BeginTravelRejectsADeceasedTraveler()
    {
        var (state, travelerId, _) = OneHouseholdWithARetinueMember();
        state.Characters.TryGet(travelerId, out var traveler);
        state.Characters.Remove(travelerId);
        state.Characters.Add(travelerId, traveler with { DeathRecord = new DeathRecord(StartDate, DeathCause.OldAge, 40) });

        var pipeline = BeginTravelCommands.BuildPipeline(TravelTestFixtures.BuildRegionCatalog(), TravelTestFixtures.BuildDistanceTierCatalog());
        var command = new BeginTravelCommand(
            state.CommandIds.Issue(), "player", StartDate, null, travelerId, Array.Empty<RuntimeId<Character>>(),
            TravelTestFixtures.HomeRegionId, TravelLocation.FrontierRegion(TravelTestFixtures.FarRegionId));
        var result = pipeline.Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(BeginTravelCommands.TravelerDeceased));
    }

    // ---- TravelProgressSystem / arrival / return / recall ------------------------------------------

    [Test]
    public void TravelProgressSystemAdvancesToArrivedAndSetsTheTravelersLocation()
    {
        var (state, travelerId, _) = OneHouseholdWithARetinueMember();
        var pipeline = BeginTravelCommands.BuildPipeline(TravelTestFixtures.BuildRegionCatalog(), TravelTestFixtures.BuildDistanceTierCatalog());
        var destination = TravelLocation.ProvincialCapital(TravelTestFixtures.NearRegionId, new RuntimeIdCounter<Settlement>().Issue());
        var command = new BeginTravelCommand(
            state.CommandIds.Issue(), "player", StartDate, null, travelerId, Array.Empty<RuntimeId<Character>>(),
            TravelTestFixtures.HomeRegionId, destination);
        pipeline.Execute(state, command);

        var system = new TravelProgressSystem();
        var events = system.Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        var trip = state.TravelTrips.InAscendingOrder().Single().Value;
        state.Characters.TryGet(travelerId, out var traveler);

        Assert.Multiple(() =>
        {
            Assert.That(events.Single(), Is.InstanceOf<TravelArrivedEvent>());
            Assert.That(trip.Status, Is.EqualTo(TravelTripStatus.Arrived));
            Assert.That(traveler.CurrentTravelLocation, Is.EqualTo(destination));
        });
    }

    [Test]
    public void BeginReturnThenProgressCompletesTheTripAndClearsTheLocation()
    {
        var (state, travelerId, _) = OneHouseholdWithARetinueMember();
        var pipeline = BeginTravelCommands.BuildPipeline(TravelTestFixtures.BuildRegionCatalog(), TravelTestFixtures.BuildDistanceTierCatalog());
        var destination = TravelLocation.ProvincialCapital(TravelTestFixtures.NearRegionId, new RuntimeIdCounter<Settlement>().Issue());
        pipeline.Execute(state, new BeginTravelCommand(
            state.CommandIds.Issue(), "player", StartDate, null, travelerId, Array.Empty<RuntimeId<Character>>(),
            TravelTestFixtures.HomeRegionId, destination));

        var system = new TravelProgressSystem();
        system.Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        var tripId = state.TravelTrips.InAscendingOrder().Single().Key;
        var returnResult = BeginReturnCommands.Pipeline.Execute(
            state, new BeginReturnCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, tripId, EncounterCompleted: true));
        Assert.That(returnResult.Accepted, Is.True);

        var completionEvents = system.Tick(state, new MonthlyTickContext(new GameDate(2), new RandomStreamSet()));

        state.Characters.TryGet(travelerId, out var traveler);

        Assert.Multiple(() =>
        {
            Assert.That(completionEvents.Single(), Is.InstanceOf<TravelCompletedEvent>());
            Assert.That(state.TravelTrips.InAscendingOrder().Single().Value.Status, Is.EqualTo(TravelTripStatus.Completed));
            Assert.That(state.TravelTrips.InAscendingOrder().Single().Value.EncounterCompleted, Is.True);
            Assert.That(traveler.CurrentTravelLocation, Is.Null);
            Assert.That(TravelTripQueries.IsReserved(state, travelerId), Is.False);
        });
    }

    [Test]
    public void RecallForcesEncounterCompletedFalseAndSkipsStraightToReturning()
    {
        var (state, travelerId, _) = OneHouseholdWithARetinueMember();
        var pipeline = BeginTravelCommands.BuildPipeline(TravelTestFixtures.BuildRegionCatalog(), TravelTestFixtures.BuildDistanceTierCatalog());
        pipeline.Execute(state, new BeginTravelCommand(
            state.CommandIds.Issue(), "player", StartDate, null, travelerId, Array.Empty<RuntimeId<Character>>(),
            TravelTestFixtures.HomeRegionId, TravelLocation.FrontierRegion(TravelTestFixtures.FarRegionId)));

        var tripId = state.TravelTrips.InAscendingOrder().Single().Key;
        var recallResult = RecallTravelCommands.Pipeline.Execute(
            state, new RecallTravelCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, tripId));

        Assert.That(recallResult.Accepted, Is.True);
        state.TravelTrips.TryGet(tripId, out var trip);
        Assert.Multiple(() =>
        {
            Assert.That(trip.Status, Is.EqualTo(TravelTripStatus.Recalled));
            Assert.That(trip.EncounterCompleted, Is.False);
            Assert.That(trip.MonthsElapsed, Is.EqualTo(0));
        });
    }

    [Test]
    public void RecallIsRejectedOnceATripIsAlreadyReturning()
    {
        var (state, travelerId, _) = OneHouseholdWithARetinueMember();
        var pipeline = BeginTravelCommands.BuildPipeline(TravelTestFixtures.BuildRegionCatalog(), TravelTestFixtures.BuildDistanceTierCatalog());
        pipeline.Execute(state, new BeginTravelCommand(
            state.CommandIds.Issue(), "player", StartDate, null, travelerId, Array.Empty<RuntimeId<Character>>(),
            TravelTestFixtures.HomeRegionId, TravelLocation.FrontierRegion(TravelTestFixtures.NearRegionId)));

        new TravelProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));
        var tripId = state.TravelTrips.InAscendingOrder().Single().Key;
        BeginReturnCommands.Pipeline.Execute(
            state, new BeginReturnCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, tripId, EncounterCompleted: true));

        var recallResult = RecallTravelCommands.Pipeline.Execute(
            state, new RecallTravelCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, tripId));

        Assert.Multiple(() =>
        {
            Assert.That(recallResult.Accepted, Is.False);
            Assert.That(recallResult.Error, Is.EqualTo(RecallTravelCommands.NotRecallable));
        });
    }

    // ---- Save/load round trip ----------------------------------------------------------------------

    [Test]
    public void TravelStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, travelerId, retinueId) = OneHouseholdWithARetinueMember();
        var pipeline = BeginTravelCommands.BuildPipeline(TravelTestFixtures.BuildRegionCatalog(), TravelTestFixtures.BuildDistanceTierCatalog());
        pipeline.Execute(state, new BeginTravelCommand(
            state.CommandIds.Issue(), "player", StartDate, null, travelerId, new[] { retinueId },
            TravelTestFixtures.HomeRegionId, TravelLocation.FrontierRegion(TravelTestFixtures.FarRegionId)));

        new TravelProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.TravelTrips.Count, Is.EqualTo(state.TravelTrips.Count));
            restored.Characters.TryGet(travelerId, out var restoredTraveler);
            state.Characters.TryGet(travelerId, out var originalTraveler);
            Assert.That(restoredTraveler.CurrentTravelLocation, Is.EqualTo(originalTraveler.CurrentTravelLocation));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }

    // ---- Shared fixture -----------------------------------------------------------------------------

    private static (WorldState State, RuntimeId<Character> TravelerId, RuntimeId<Character> RetinueId) OneHouseholdWithARetinueMember()
    {
        var state = new WorldState(StartDate);
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var householdId = state.HouseholdIds.Issue();

        var travelerId = state.CharacterIds.Issue();
        state.Characters.Add(travelerId, CharacterTestFixtures.Minimal(travelerId, nomen: "Traveler", household: householdId, location: settlementId));

        var retinueId = state.CharacterIds.Issue();
        state.Characters.Add(retinueId, CharacterTestFixtures.Minimal(retinueId, nomen: "Retinue", household: householdId, location: settlementId));

        return (state, travelerId, retinueId);
    }
}
