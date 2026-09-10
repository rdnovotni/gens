using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Interactions;

/// <summary>Phase 16 item 1 coverage for <see cref="CounterEspionageSweepCommand"/>.</summary>
public sealed class CounterEspionageSweepCommandTests
{
    private static RandomStreamSet Streams(ulong seed)
    {
        var streams = new RandomStreamSet();
        streams.Add(CounterEspionageSweepCommands.StreamName, seed, 1);
        return streams;
    }

    private static RuntimeId<Actor> AddSuspectedActor(WorldState state)
    {
        var netWorth = new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null);
        var military = new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest);
        var actor = RivalHouseCreationService.CreateAncientSeed(
            state, "Cornelia", LivingWorldActorStandingTrend.Established, LivingWorldActorIdentity.None,
            0, netWorth, military, state.RegionIds.Issue(), state.SettlementIds.Issue());
        return actor.ActorId;
    }

    private static WorldState StateWithFundedInvestigator(out RuntimeId<Character> investigatorId, out RuntimeId<Actor> suspectedActorId)
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        investigatorId = state.CharacterIds.Issue();
        state.Characters.Add(investigatorId, CharacterTestFixtures.Minimal(investigatorId, household: householdId));
        state.LedgerAccounts.Add(
            LedgerAccountKey.ForHousehold(householdId),
            new LedgerAccount(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(10_000)));
        suspectedActorId = AddSuspectedActor(state);
        return state;
    }

    [Test]
    public void RejectsWhenTheInvestigatorsHouseholdCannotAffordTheSweep()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var investigatorId = state.CharacterIds.Issue();
        state.Characters.Add(investigatorId, CharacterTestFixtures.Minimal(investigatorId, household: householdId));
        var suspectedActorId = AddSuspectedActor(state);

        var command = new CounterEspionageSweepCommand(
            state.CommandIds.Issue(), investigatorId.ToTaggedString(), new GameDate(0), CausationId: null,
            investigatorId, suspectedActorId);

        var result = CounterEspionageSweepCommands.CreatePipeline(Streams(1)).Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(CounterEspionageSweepCommands.InsufficientFunds));
    }

    [Test]
    public void RejectsWhenTheInvestigatorIsUnknown()
    {
        var state = StateWithFundedInvestigator(out _, out var suspectedActorId);
        var unknownId = state.CharacterIds.Issue();

        var command = new CounterEspionageSweepCommand(
            state.CommandIds.Issue(), unknownId.ToTaggedString(), new GameDate(0), CausationId: null,
            unknownId, suspectedActorId);

        var result = CounterEspionageSweepCommands.CreatePipeline(Streams(1)).Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(CounterEspionageSweepCommands.InvestigatorNotFound));
    }

    [Test]
    public void AcceptsAndFindsNothingWhenNoPlacementTargetsTheSuspectedActor()
    {
        var state = StateWithFundedInvestigator(out var investigatorId, out var suspectedActorId);
        var command = new CounterEspionageSweepCommand(
            state.CommandIds.Issue(), investigatorId.ToTaggedString(), new GameDate(0), CausationId: null,
            investigatorId, suspectedActorId);

        var result = CounterEspionageSweepCommands.CreatePipeline(Streams(1)).Execute(state, command);

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            var resolvedEvent = (CounterEspionageSweepResolvedEvent)Array.Find(
                result.Events.ToArray(), e => e is CounterEspionageSweepResolvedEvent)!;
            Assert.That(resolvedEvent.SpyIdentified, Is.False);
            Assert.That(resolvedEvent.FoundPlacementId, Is.Null);
        });
    }

    [Test]
    public void StillDeductsTheSweepCostWhenNothingIsFound()
    {
        var state = StateWithFundedInvestigator(out var investigatorId, out var suspectedActorId);
        state.Characters.TryGet(investigatorId, out var investigator);
        var account = LedgerAccountKey.ForHousehold(investigator!.Household!.Value);
        state.LedgerAccounts.TryGet(account, out var before);

        var command = new CounterEspionageSweepCommand(
            state.CommandIds.Issue(), investigatorId.ToTaggedString(), new GameDate(0), CausationId: null,
            investigatorId, suspectedActorId);
        CounterEspionageSweepCommands.CreatePipeline(Streams(1)).Execute(state, command);

        state.LedgerAccounts.TryGet(account, out var after);
        Assert.That(after!.Balance, Is.EqualTo(before!.Balance - Money.FromDenarii(SpyPlacementCatalog.SweepCostDenarii)));
    }

    [Test]
    public void RollsAgainstAnInProgressPlacementTargetingTheSuspectedActor()
    {
        var state = StateWithFundedInvestigator(out var investigatorId, out var suspectedActorId);
        var spyId = state.CharacterIds.Issue();
        state.Characters.Add(spyId, CharacterTestFixtures.Minimal(spyId));
        var sponsorId = state.CharacterIds.Issue();
        state.Characters.Add(sponsorId, CharacterTestFixtures.Minimal(sponsorId));
        var placementId = state.SpyPlacementIds.Issue();
        state.SpyPlacements.Add(placementId, SpyPlacement.Create(
            placementId, spyId, sponsorId, suspectedActorId, SpyPlacementType.PersistentNetwork, concealmentQuality: 0, new GameDate(0)));

        var command = new CounterEspionageSweepCommand(
            state.CommandIds.Issue(), investigatorId.ToTaggedString(), new GameDate(0), CausationId: null,
            investigatorId, suspectedActorId);
        var result = CounterEspionageSweepCommands.CreatePipeline(Streams(1)).Execute(state, command);

        state.SpyPlacements.TryGet(placementId, out var placement);
        var resolvedEvent = (CounterEspionageSweepResolvedEvent)Array.Find(
            result.Events.ToArray(), e => e is CounterEspionageSweepResolvedEvent)!;

        Assert.Multiple(() =>
        {
            if (resolvedEvent.SpyIdentified)
            {
                Assert.That(placement!.Status, Is.EqualTo(SpyPlacementStatus.DiscoveredAndTraced));
                Assert.That(state.RivalDossiers.TryGet(suspectedActorId, out _), Is.True);
                Assert.That(resolvedEvent.FoundPlacementId, Is.EqualTo(placementId));
            }
            else
            {
                Assert.That(placement!.Status, Is.EqualTo(SpyPlacementStatus.InProgress));
                Assert.That(resolvedEvent.FoundPlacementId, Is.Null);
            }
        });
    }
}
