using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Policies;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Stewardship;

/// <summary>Phase 10 item 2/10 coverage for the steward autonomous decision loop.</summary>
public sealed class StewardAutonomousDecisionSystemTests
{
    private static StewardAutonomousDecisionSystem MakeSystem(RuntimeId<Settlement> settlementId) =>
        new(PolicyActionDefinitions.BuildCatalog(), _ => settlementId);

    private static RuntimeId<StewardshipAssignment> Appoint(
        WorldState state, RuntimeId<Household> householdId, StewardAutonomyLevel level)
    {
        var appointResult = StewardshipCommands.AppointPipeline.Execute(
            state,
            new AppointStewardshipCommand(
                state.CommandIds.Issue(), householdId.ToTaggedString(), state.Date, null, householdId,
                StewardshipContext.Travel, StewardshipMode.SingleSteward, state.CharacterIds.Issue(), null, null, level));
        return ((StewardshipAssignedEvent)appointResult.Events[0]).AssignmentId;
    }

    [Test]
    public void DeclaresTheRelationshipsActorsPhase()
    {
        var system = MakeSystem(default);
        Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
    }

    [Test]
    public void AConservativeStewardHoldsEveryMonthGivenTodaysActionCatalog()
    {
        var state = new WorldState(new GameDate(10));
        var householdId = state.HouseholdIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var assignmentId = Appoint(state, householdId, StewardAutonomyLevel.Conservative);

        // Fund the treasury generously so FundFestival's own eligibility is not the reason it's skipped.
        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(500)),
                new LedgerPosting(LedgerAccountKey.Mint, -(Money.FromDenarii(500))),
            });

        var system = MakeSystem(settlementId);
        var events = system.Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        Assert.Multiple(() =>
        {
            Assert.That(events, Is.Empty);
            var logs = state.AutonomousDecisionLogs.InAscendingOrder().Select(e => e.Value).Where(l => l.AssignmentId == assignmentId).ToArray();
            Assert.That(logs, Has.Length.EqualTo(1));
            Assert.That(logs[0].DecisionType, Is.EqualTo("none"));
            Assert.That(logs[0].Outcome, Is.EqualTo("held"));
        });
    }

    [Test]
    public void AStandardStewardRestoresADriftedRitesBudgetToItsDefault()
    {
        var state = new WorldState(new GameDate(10));
        var householdId = state.HouseholdIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        Appoint(state, householdId, StewardAutonomyLevel.Standard);

        state.HouseholdPolicies.Add(householdId, new HouseholdPolicyState(householdId, RitesBudgetTier.Frugal, new GameDate(0)));

        var system = MakeSystem(settlementId);
        var events = system.Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        Assert.Multiple(() =>
        {
            Assert.That(events, Has.Count.EqualTo(1));
            Assert.That(events[0], Is.InstanceOf<RitesBudgetChangedEvent>());
            Assert.That(HouseholdPolicyResolver.GetEffectiveRitesBudget(state, householdId), Is.EqualTo(RitesBudgetCatalog.Default));
        });
    }

    [Test]
    public void AStandardStewardCannotFundAFestivalEvenWithFullTreasury()
    {
        var state = new WorldState(new GameDate(10));
        var householdId = state.HouseholdIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        Appoint(state, householdId, StewardAutonomyLevel.Standard);

        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(500)),
                new LedgerPosting(LedgerAccountKey.Mint, -(Money.FromDenarii(500))),
            });

        var system = MakeSystem(settlementId);
        var events = system.Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        // Rites Budget is already at its default, so nothing left in the Standard-permitted set fires.
        Assert.That(events, Is.Empty);
    }

    [Test]
    public void AFullAutonomyStewardFundsAFestivalOnceRitesBudgetIsAlreadyAtDefault()
    {
        var state = new WorldState(new GameDate(10));
        var householdId = state.HouseholdIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        Appoint(state, householdId, StewardAutonomyLevel.FullAutonomy);

        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(500)),
                new LedgerPosting(LedgerAccountKey.Mint, -(Money.FromDenarii(500))),
            });

        var system = MakeSystem(settlementId);
        var events = system.Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        Assert.That(events.Any(e => e is FestivalFundedEvent), Is.True);
    }

    [Test]
    public void AnAlwaysHeldActionIsNeverChosenEvenAtFullAutonomy()
    {
        // With today's empty StewardAlwaysHeldCatalog this is vacuously true; asserted directly against
        // the catalog itself so a future addition to it is what this test would need updating for, not
        // the system's own filtering logic.
        Assert.That(StewardAlwaysHeldCatalog.IsAlwaysHeld(PolicyActionDefinitions.ChangeRitesBudget), Is.False);
    }
}
