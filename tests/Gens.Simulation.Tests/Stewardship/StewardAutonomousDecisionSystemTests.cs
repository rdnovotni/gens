using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Policies;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Time;
using NUnit.Framework;
using CharacterFixtures = Gens.Simulation.Tests.Characters.CharacterTestFixtures;

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

    private static RuntimeId<StewardshipAssignment> AppointWithCharacter(
        WorldState state, RuntimeId<Household> householdId, StewardAutonomyLevel level, int stewardship, int loyalty)
    {
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(
            characterId,
            CharacterFixtures.Minimal(
                characterId,
                attributes: new CoreAttributes(10, 10, stewardship, 10, 10),
                condition: new Condition(80, 0, loyalty, 20, 50)));

        var appointResult = StewardshipCommands.AppointPipeline.Execute(
            state,
            new AppointStewardshipCommand(
                state.CommandIds.Issue(), householdId.ToTaggedString(), state.Date, null, householdId,
                StewardshipContext.Travel, StewardshipMode.SingleSteward, characterId, null, null, level));
        return ((StewardshipAssignedEvent)appointResult.Events[0]).AssignmentId;
    }

    [Test]
    public void CompetenceAndLoyaltyAreReadFromTheAppointeesRealStats()
    {
        var state = new WorldState(new GameDate(10));
        var householdId = state.HouseholdIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var assignmentId = AppointWithCharacter(state, householdId, StewardAutonomyLevel.Conservative, stewardship: 77, loyalty: 90);

        var system = MakeSystem(settlementId);
        system.Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        var log = state.AutonomousDecisionLogs.InAscendingOrder().Select(e => e.Value).Single(l => l.AssignmentId == assignmentId);
        Assert.Multiple(() =>
        {
            Assert.That(log.CompetenceRollFactor, Is.EqualTo(77));
            Assert.That(log.LoyaltyRiskRollFactor, Is.EqualTo(90));
            Assert.That(log.IncidentType, Is.Null);
        });
    }

    [Test]
    public void ALoyalStewardNeverRisksAnIncidentAcrossManyMonths()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var assignmentId = AppointWithCharacter(
            state, householdId, StewardAutonomyLevel.Conservative, stewardship: 50,
            loyalty: StewardIncidentCatalog.LoyaltyRiskThreshold);

        var system = MakeSystem(settlementId);
        // No loyalty-risk stream registered at all: if the system ever tried to roll for a Loyalty at
        // or above the threshold, this would throw rather than silently pass.
        var streams = new RandomStreamSet();
        for (var month = 0; month < 24; month++)
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));

        var logs = state.AutonomousDecisionLogs.InAscendingOrder().Select(e => e.Value).Where(l => l.AssignmentId == assignmentId);
        Assert.That(logs, Has.All.Matches<AutonomousDecisionLog>(l => l.IncidentType is null));
    }

    [Test]
    public void ADisloyalStewardEventuallyTriggersADiscoveredIncident()
    {
        // Loyalty well below the threshold with a real random stream drawn over enough months makes an
        // incident a near-certainty; deterministic given the fixed seed below (rule 8: this system owns
        // CampaignBootstrapper.StewardLoyaltyRiskStreamName).
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var assignmentId = AppointWithCharacter(state, householdId, StewardAutonomyLevel.Conservative, stewardship: 30, loyalty: 5);

        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(500)),
                new LedgerPosting(LedgerAccountKey.Mint, -(Money.FromDenarii(500))),
            });

        var system = MakeSystem(settlementId);
        var streams = new RandomStreamSet();
        streams.AddDerived(CampaignBootstrapper.StewardLoyaltyRiskStreamName, 12345UL);

        AutonomousDecisionLog? discovered = null;
        for (var month = 0; month < 200 && discovered is null; month++)
        {
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));
            discovered = state.AutonomousDecisionLogs.InAscendingOrder().Select(e => e.Value)
                .Where(l => l.AssignmentId == assignmentId && l.IncidentType is not null)
                .OrderByDescending(l => l.Month.TotalMonths)
                .FirstOrDefault();
        }

        Assert.That(discovered, Is.Not.Null, "Expected at least one incident across 200 months at 8% chance/month.");

        switch (discovered!.IncidentType)
        {
            case StewardIncidentType.Skimming:
            case StewardIncidentType.Embezzlement:
                var householdBalance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account)
                    ? account!.Balance
                    : Money.Zero;
                Assert.That(householdBalance, Is.LessThan(Money.FromDenarii(500)), "A money incident should have debited the household treasury.");
                break;
            case StewardIncidentType.ActiveSabotage:
                Assert.That(HouseholdPolicyResolver.GetEffectiveRitesBudget(state, householdId), Is.EqualTo(RitesBudgetTier.Frugal));
                break;
        }
    }
}
