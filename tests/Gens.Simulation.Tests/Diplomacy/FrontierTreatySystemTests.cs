using Gens.Simulation.Actors;
using Gens.Simulation.Cultures;
using Gens.Simulation.Diplomacy;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Diplomacy;

public sealed class FrontierTreatySystemTests
{
    private static readonly GameDate StartDate = new(0);

    private static RandomStreamSet EmptyStreams() => new();

    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Actor> PeopleActorId) SetupWithAPeople()
    {
        var state = new WorldState(StartDate);
        var householdId = state.HouseholdIds.Issue();
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var actor = ForeignPeopleCreationService.CreateAncientSeed(
            state, KnownWorldCultures.BuildCatalog(), "Suebi", KnownWorldCultures.Germanic,
            LivingWorldActorStandingTrend.Established, new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest),
            regionId, settlementId, StartDate);
        return (state, householdId, actor.ActorId);
    }

    [Test]
    public void DeclaresTheRelationshipsActorsPhaseAndReadWriteSet()
    {
        var system = new FrontierTreatySystem();

        Assert.Multiple(() =>
        {
            Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
            Assert.That(system.Reads, Is.EquivalentTo(new[] { "frontierTreaties", "perPeopleStandings", "ledgerAccounts" }));
            Assert.That(system.Writes, Is.EquivalentTo(new[]
            {
                "frontierTreaties", "perPeopleStandings", "eventIds", "ledgerAccounts", "ledgerTransactions", "ledgerTransactionIds",
            }));
        });
    }

    [Test]
    public void PostsMonthlyTributeFromHouseholdToPeopleWhenFundsAreSufficient()
    {
        var (state, householdId, actorId) = SetupWithAPeople();
        state.LedgerAccounts.Add(
            LedgerAccountKey.ForHousehold(householdId), new LedgerAccount(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(1_000)));

        var treatyId = state.FrontierTreatyIds.Issue();
        state.FrontierTreaties.Add(treatyId, FrontierTreaty.Create(
            treatyId, householdId, actorId, FrontierTreatyType.Tribute, TributeDirection.HouseholdPaysPeople,
            Money.FromDenarii(100), StartDate, new GameDate(60)));

        new FrontierTreatySystem().Tick(state, new MonthlyTickContext(new GameDate(1), EmptyStreams()));

        var householdBalance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var h) ? h!.Balance : Money.Zero;
        var peopleBalance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForActor(actorId), out var a) ? a!.Balance : Money.Zero;
        Assert.Multiple(() =>
        {
            Assert.That(householdBalance, Is.EqualTo(Money.FromDenarii(900)));
            Assert.That(peopleBalance, Is.EqualTo(Money.FromDenarii(100)));
        });
    }

    [Test]
    public void FallsIntoArrearsAndAppliesAGoodwillPenaltyWhenFundsAreInsufficient()
    {
        var (state, householdId, actorId) = SetupWithAPeople();
        state.LedgerAccounts.Add(
            LedgerAccountKey.ForHousehold(householdId), new LedgerAccount(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(10)));

        var treatyId = state.FrontierTreatyIds.Issue();
        state.FrontierTreaties.Add(treatyId, FrontierTreaty.Create(
            treatyId, householdId, actorId, FrontierTreatyType.Tribute, TributeDirection.HouseholdPaysPeople,
            Money.FromDenarii(100), StartDate, new GameDate(60)));

        new FrontierTreatySystem().Tick(state, new MonthlyTickContext(new GameDate(1), EmptyStreams()));

        var householdBalance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var h) ? h!.Balance : Money.Zero;
        Assert.Multiple(() =>
        {
            Assert.That(householdBalance, Is.EqualTo(Money.FromDenarii(10)));
            Assert.That(PerPeopleStandingResolver.GetEffective(state, householdId, actorId).Goodwill, Is.LessThan(0));
        });
    }

    [Test]
    public void ExpiresATreatyOnceItsTermHasRunOutAndEmitsAnEndedEvent()
    {
        var (state, householdId, actorId) = SetupWithAPeople();
        var treatyId = state.FrontierTreatyIds.Issue();
        state.FrontierTreaties.Add(treatyId, FrontierTreaty.Create(
            treatyId, householdId, actorId, FrontierTreatyType.NonAggression, TributeDirection.None, Money.Zero,
            StartDate, new GameDate(3)));

        var events = new FrontierTreatySystem().Tick(state, new MonthlyTickContext(new GameDate(3), EmptyStreams()));

        state.FrontierTreaties.TryGet(treatyId, out var treaty);
        Assert.Multiple(() =>
        {
            Assert.That(treaty!.Status, Is.EqualTo(FrontierTreatyStatus.Expired));
            Assert.That(events, Has.Some.InstanceOf<FrontierTreatyEndedEvent>());
        });
    }

    [Test]
    public void ASecondTickAfterExpiryIsANoOp()
    {
        var (state, householdId, actorId) = SetupWithAPeople();
        var treatyId = state.FrontierTreatyIds.Issue();
        state.FrontierTreaties.Add(treatyId, FrontierTreaty.Create(
            treatyId, householdId, actorId, FrontierTreatyType.NonAggression, TributeDirection.None, Money.Zero,
            StartDate, new GameDate(3)));

        new FrontierTreatySystem().Tick(state, new MonthlyTickContext(new GameDate(3), EmptyStreams()));
        var events = new FrontierTreatySystem().Tick(state, new MonthlyTickContext(new GameDate(4), EmptyStreams()));

        Assert.That(events, Is.Empty);
    }
}
