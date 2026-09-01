using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Wanderers;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Wanderers;

public sealed class WandererCompetitionTests
{
    private const string StreamName = "test-wanderer-generation";
    private static readonly GameDate Now = new(18);

    private static RegisterWandererInterestCommand Interest(
        WorldState state, RuntimeId<Wanderer> wandererId, RuntimeId<Household> householdId) =>
        new(state.CommandIds.Issue(), "player", Now, null, wandererId, householdId);

    private static HostWandererCommand Host(
        WorldState state, RuntimeId<Wanderer> wandererId, RuntimeId<Household> householdId) =>
        new(state.CommandIds.Issue(), "player", Now, null, wandererId, householdId);

    [Test]
    public void TwoRivalHouseholdsCanBothRegisterInterestInAFamousWanderer()
    {
        var state = new WorldState(Now);
        var player = state.HouseholdIds.Issue();
        var rival = state.HouseholdIds.Issue();
        var wanderer = WandererTestFixtures.AddWanderer(state, fame: 70);

        Assert.That(RegisterWandererInterestCommands.Pipeline.Execute(state, Interest(state, wanderer.Id, player)).Accepted, Is.True);
        var second = RegisterWandererInterestCommands.Pipeline.Execute(state, Interest(state, wanderer.Id, rival));

        state.Wanderers.TryGet(wanderer.Id, out var contested);
        Assert.Multiple(() =>
        {
            Assert.That(second.Accepted, Is.True);
            Assert.That(contested!.InterestedHouseholdIds, Is.EqualTo(new[] { player, rival }));
            Assert.That(second.Events.OfType<WandererInterestRegisteredEvent>().Single().CompetingHouseholdCount, Is.EqualTo(2));
        });
    }

    [Test]
    public void AnObscureWandererIsNobodyRace()
    {
        var state = new WorldState(Now);
        var householdId = state.HouseholdIds.Issue();
        var wanderer = WandererTestFixtures.AddWanderer(
            state, fame: WandererFameCalculator.CompetitionVisibilityThreshold - 1);

        var result = RegisterWandererInterestCommands.Pipeline.Execute(state, Interest(state, wanderer.Id, householdId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(RegisterWandererInterestCommands.InsufficientFame));
        });
    }

    [Test]
    public void TheSameHouseholdCannotRegisterInterestTwice()
    {
        var state = new WorldState(Now);
        var householdId = state.HouseholdIds.Issue();
        var wanderer = WandererTestFixtures.AddWanderer(state, fame: 70);

        RegisterWandererInterestCommands.Pipeline.Execute(state, Interest(state, wanderer.Id, householdId));
        var second = RegisterWandererInterestCommands.Pipeline.Execute(state, Interest(state, wanderer.Id, householdId));

        Assert.That(second.Error, Is.EqualTo(RegisterWandererInterestCommands.AlreadyInterested));
    }

    [Test]
    public void TheFirstHouseholdToHostWinsTheRaceAndEveryRivalIsShutOut()
    {
        var state = new WorldState(Now);
        var player = state.HouseholdIds.Issue();
        var rival = state.HouseholdIds.Issue();
        var wanderer = WandererTestFixtures.AddWanderer(state, fame: 70);
        RegisterWandererInterestCommands.Pipeline.Execute(state, Interest(state, wanderer.Id, player));
        RegisterWandererInterestCommands.Pipeline.Execute(state, Interest(state, wanderer.Id, rival));
        var hostPipeline = HostWandererCommands.CreatePipeline(WandererTestFixtures.TypeCatalog);

        var rivalCommits = hostPipeline.Execute(state, Host(state, wanderer.Id, rival));
        var playerTooLate = hostPipeline.Execute(state, Host(state, wanderer.Id, player));

        state.Wanderers.TryGet(wanderer.Id, out var resolved);
        Assert.Multiple(() =>
        {
            Assert.That(rivalCommits.Accepted, Is.True);
            Assert.That(playerTooLate.Accepted, Is.False);
            Assert.That(playerTooLate.Error, Is.EqualTo(HostWandererCommands.CommittedElsewhere));
            Assert.That(resolved!.CommittedHouseholdId, Is.EqualTo(rival));
            Assert.That(resolved.InterestedHouseholdIds, Is.Empty, "the race is resolved, not held open.");
        });
    }

    [Test]
    public void ARecruitAlsoResolvesTheRaceAndLocksOutTheLoser()
    {
        var state = new WorldState(Now);
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var player = state.HouseholdIds.Issue();
        var rival = state.HouseholdIds.Issue();
        var wanderer = WandererTestFixtures.AddWanderer(state, fame: 70);
        RegisterWandererInterestCommands.Pipeline.Execute(state, Interest(state, wanderer.Id, player));

        var streams = new RandomStreamSet();
        streams.AddDerived(StreamName, 5);
        var recruit = RecruitWandererCommands.CreatePipeline(streams, WandererTestFixtures.TypeCatalog).Execute(
            state,
            new RecruitWandererCommand(
                state.CommandIds.Issue(), "player", Now, null, wanderer.Id, rival, settlementId, StreamName));

        var playerTooLate = HostWandererCommands.CreatePipeline(WandererTestFixtures.TypeCatalog)
            .Execute(state, Host(state, wanderer.Id, player));

        state.Wanderers.TryGet(wanderer.Id, out var resolved);
        Assert.Multiple(() =>
        {
            Assert.That(recruit.Accepted, Is.True);
            Assert.That(resolved!.CommittedHouseholdId, Is.EqualTo(rival));
            Assert.That(resolved.InterestedHouseholdIds, Is.Empty);
            Assert.That(playerTooLate.Error, Is.EqualTo(HostWandererCommands.WandererUnavailable));
        });
    }

    [Test]
    public void InterestCannotBeRegisteredOnceTheRaceIsAlreadyResolved()
    {
        var state = new WorldState(Now);
        var player = state.HouseholdIds.Issue();
        var latecomer = state.HouseholdIds.Issue();
        var wanderer = WandererTestFixtures.AddWanderer(state, fame: 70);
        HostWandererCommands.CreatePipeline(WandererTestFixtures.TypeCatalog)
            .Execute(state, Host(state, wanderer.Id, player));

        var result = RegisterWandererInterestCommands.Pipeline.Execute(state, Interest(state, wanderer.Id, latecomer));

        Assert.That(result.Error, Is.EqualTo(RegisterWandererInterestCommands.AlreadyResolved));
    }

    [Test]
    public void ValidationRejectsAMissingWandererAndAnUntrackedOne()
    {
        var state = new WorldState(Now);
        var householdId = state.HouseholdIds.Issue();
        var wanderer = WandererTestFixtures.AddWanderer(state, fame: 70);
        state.Wanderers.Remove(wanderer.Id);
        state.Wanderers.Add(wanderer.Id, wanderer with { IsActivelyTracked = false });

        Assert.Multiple(() =>
        {
            Assert.That(
                RegisterWandererInterestCommands.Pipeline.Execute(
                    state, Interest(state, state.WandererIds.Issue(), householdId)).Error,
                Is.EqualTo(RegisterWandererInterestCommands.WandererNotFound));
            Assert.That(
                RegisterWandererInterestCommands.Pipeline.Execute(state, Interest(state, wanderer.Id, householdId)).Error,
                Is.EqualTo(RegisterWandererInterestCommands.WandererUnavailable));
        });
    }

    [Test]
    public void AvailabilityReadsTheSameWayTheEngagementCommandsDo()
    {
        var state = new WorldState(Now);
        var player = state.HouseholdIds.Issue();
        var rival = state.HouseholdIds.Issue();
        var wanderer = WandererTestFixtures.AddWanderer(state, fame: 70);

        Assert.That(WandererQueries.IsAvailableTo(state, wanderer.Id, player), Is.True);

        HostWandererCommands.CreatePipeline(WandererTestFixtures.TypeCatalog)
            .Execute(state, Host(state, wanderer.Id, player));

        Assert.Multiple(() =>
        {
            Assert.That(WandererQueries.IsAvailableTo(state, wanderer.Id, player), Is.True);
            Assert.That(WandererQueries.IsAvailableTo(state, wanderer.Id, rival), Is.False);
        });
    }
}
