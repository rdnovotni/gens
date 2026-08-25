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

namespace Gens.Simulation.Tests.Stewardship;

/// <summary>Phase 10 item 2/10/11 coverage for the steward autonomous decision loop.</summary>
public sealed class StewardAutonomousDecisionSystemTests
{
    private const string CompetenceStream = "stewardship.competence";
    private const string LoyaltyStream = "stewardship.loyalty";

    private static StewardAutonomousDecisionSystem MakeSystem(RuntimeId<Settlement> settlementId) =>
        new(PolicyActionDefinitions.BuildCatalog(), _ => settlementId, CompetenceStream, LoyaltyStream);

    private static RandomStreamSet Streams(ulong seed)
    {
        var streams = new RandomStreamSet();
        streams.Add(CompetenceStream, seed, 1);
        streams.Add(LoyaltyStream, seed, 2);
        return streams;
    }

    /// <summary>Registers a steward Character with maximal Stewardship/Learning/Loyalty — guarantees a
    /// 100% competence-execution chance and a 0% Loyalty-incident chance, so tests can assert exactly
    /// which action fired without also having to control two independent RNG rolls.</summary>
    private static RuntimeId<Character> RegisterReliableSteward(WorldState state)
    {
        var stewardId = state.CharacterIds.Issue();
        var steward = Character.Create(
            id: stewardId, praenomen: "Marcus", nomen: "Aurelius", cognomen: null, sex: Sex.Male,
            birthDate: new GameDate(0),
            visualProfile: Gens.Simulation.Tests.Characters.CharacterTestFixtures.MinimalVisualProfile,
            status: LegalStatus.RomanCitizen, socialClass: SocialClass.Plebeian, culture: new DefinitionId<Culture>("roman"),
            location: default, household: null, attributes: new CoreAttributes(10, 10, 100, 10, 100),
            skills: new LaborSkills(10, 10, 10, 10, 10), condition: new Condition(80, 0, 100, 20, 50),
            source: CharacterSource.Familia, instantiatedAtMonth: 0);
        state.Characters.Add(stewardId, steward);
        return stewardId;
    }

    private static RuntimeId<StewardshipAssignment> Appoint(
        WorldState state, RuntimeId<Household> householdId, RuntimeId<Character> stewardId, StewardAutonomyLevel level)
    {
        var appointResult = StewardshipCommands.AppointPipeline.Execute(
            state,
            new AppointStewardshipCommand(
                state.CommandIds.Issue(), householdId.ToTaggedString(), state.Date, null, householdId,
                StewardshipContext.Travel, StewardshipMode.SingleSteward, stewardId, null, null, level));
        return ((StewardshipAssignedEvent)appointResult.Events[0]).AssignmentId;
    }

    [Test]
    public void DeclaresTheRelationshipsActorsPhase()
    {
        var system = MakeSystem(default);
        Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
    }

    [Test]
    public void AStewardWithNoRegisteredCharacterAlwaysHolds()
    {
        var state = new WorldState(new GameDate(10));
        var householdId = state.HouseholdIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        // Deliberately not registering a Character for this ID — TryGet will fail.
        var assignmentId = Appoint(state, householdId, state.CharacterIds.Issue(), StewardAutonomyLevel.FullAutonomy);

        var system = MakeSystem(settlementId);
        var events = system.Tick(state, new MonthlyTickContext(state.Date, Streams(1)));

        Assert.Multiple(() =>
        {
            Assert.That(events, Is.Empty);
            var logs = state.AutonomousDecisionLogs.InAscendingOrder().Select(e => e.Value).Where(l => l.AssignmentId == assignmentId).ToArray();
            Assert.That(logs, Has.Length.EqualTo(1));
            Assert.That(logs[0].DecisionType, Is.EqualTo("none"));
        });
    }

    [Test]
    public void AConservativeStewardHoldsEveryMonthGivenTodaysActionCatalog()
    {
        var state = new WorldState(new GameDate(10));
        var householdId = state.HouseholdIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var stewardId = RegisterReliableSteward(state);
        var assignmentId = Appoint(state, householdId, stewardId, StewardAutonomyLevel.Conservative);

        // Fund the treasury generously so FundFestival's own eligibility is not the reason it's skipped.
        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(500)),
                new LedgerPosting(LedgerAccountKey.Mint, -(Money.FromDenarii(500))),
            });

        var system = MakeSystem(settlementId);
        var events = system.Tick(state, new MonthlyTickContext(state.Date, Streams(1)));

        Assert.Multiple(() =>
        {
            Assert.That(events, Is.Empty);
            var logs = state.AutonomousDecisionLogs.InAscendingOrder().Select(e => e.Value).Where(l => l.AssignmentId == assignmentId).ToArray();
            Assert.That(logs, Has.Length.EqualTo(1));
            Assert.That(logs[0].DecisionType, Is.EqualTo("none"));
            Assert.That(logs[0].Outcome, Is.EqualTo("held"));
            Assert.That(logs[0].CompetenceRollFactor, Is.EqualTo(100));
            Assert.That(logs[0].LoyaltyRiskRollFactor, Is.EqualTo(0));
        });
    }

    [Test]
    public void AStandardStewardRestoresADriftedRitesBudgetToItsDefault()
    {
        var state = new WorldState(new GameDate(10));
        var householdId = state.HouseholdIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var stewardId = RegisterReliableSteward(state);
        Appoint(state, householdId, stewardId, StewardAutonomyLevel.Standard);

        state.HouseholdPolicies.Add(householdId, new HouseholdPolicyState(householdId, RitesBudgetTier.Frugal, new GameDate(0)));

        var system = MakeSystem(settlementId);
        var events = system.Tick(state, new MonthlyTickContext(state.Date, Streams(1)));

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
        var stewardId = RegisterReliableSteward(state);
        Appoint(state, householdId, stewardId, StewardAutonomyLevel.Standard);

        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(500)),
                new LedgerPosting(LedgerAccountKey.Mint, -(Money.FromDenarii(500))),
            });

        var system = MakeSystem(settlementId);
        var events = system.Tick(state, new MonthlyTickContext(state.Date, Streams(1)));

        // Rites Budget is already at its default, so nothing left in the Standard-permitted set fires.
        Assert.That(events, Is.Empty);
    }

    [Test]
    public void AFullAutonomyStewardFundsAFestivalOnceRitesBudgetIsAlreadyAtDefault()
    {
        var state = new WorldState(new GameDate(10));
        var householdId = state.HouseholdIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var stewardId = RegisterReliableSteward(state);
        var assignmentId = Appoint(state, householdId, stewardId, StewardAutonomyLevel.FullAutonomy);

        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(500)),
                new LedgerPosting(LedgerAccountKey.Mint, -(Money.FromDenarii(500))),
            });

        var system = MakeSystem(settlementId);
        var events = system.Tick(state, new MonthlyTickContext(state.Date, Streams(1)));

        Assert.Multiple(() =>
        {
            Assert.That(events.Any(e => e is FestivalFundedEvent), Is.True);
            var logs = state.AutonomousDecisionLogs.InAscendingOrder().Select(e => e.Value).Where(l => l.AssignmentId == assignmentId).ToArray();
            Assert.That(logs[0].DecisionType, Is.EqualTo(PolicyActionDefinitions.FundFestival.Value));
            Assert.That(logs[0].TreasuryImpact, Is.EqualTo(PolicyActionDefinitions.DefaultFestivalAmount));
        });
    }

    [Test]
    public void ALowLoyaltyStewardRollsTheExpectedMaximalIncidentChance()
    {
        var state = new WorldState(new GameDate(10));
        var householdId = state.HouseholdIds.Issue();
        var settlementId = state.SettlementIds.Issue();

        var stewardId = state.CharacterIds.Issue();
        var steward = Character.Create(
            id: stewardId, praenomen: "Gaius", nomen: "Fabius", cognomen: null, sex: Sex.Male,
            birthDate: new GameDate(0),
            visualProfile: Gens.Simulation.Tests.Characters.CharacterTestFixtures.MinimalVisualProfile,
            status: LegalStatus.RomanCitizen, socialClass: SocialClass.Plebeian, culture: new DefinitionId<Culture>("roman"),
            location: default, household: null, attributes: new CoreAttributes(10, 10, 0, 10, 0),
            skills: new LaborSkills(10, 10, 10, 10, 10), condition: new Condition(80, 0, 0, 20, 50),
            source: CharacterSource.Familia, instantiatedAtMonth: 0);
        state.Characters.Add(stewardId, steward);

        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(500)),
                new LedgerPosting(LedgerAccountKey.Mint, -(Money.FromDenarii(500))),
            });
        var assignmentId = Appoint(state, householdId, stewardId, StewardAutonomyLevel.Conservative);

        var system = MakeSystem(settlementId);
        system.Tick(state, new MonthlyTickContext(state.Date, Streams(0)));

        var logs = state.AutonomousDecisionLogs.InAscendingOrder().Select(e => e.Value).Where(l => l.AssignmentId == assignmentId).ToArray();
        Assert.That(logs[0].LoyaltyRiskRollFactor, Is.EqualTo(15));
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
