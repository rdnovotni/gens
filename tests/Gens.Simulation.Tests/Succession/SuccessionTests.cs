using Gens.Simulation.Characters;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Succession;

/// <summary>Phase 11 item 1's succession fixtures: ordinary inheritance, contested inheritance,
/// adoption, debt inheritance, absent heirs (spouse-in-trust), and extinction (item 6).</summary>
public sealed class SuccessionTests
{
    private static RandomStreamSet Streams(ulong seed)
    {
        var streams = new RandomStreamSet();
        streams.Add(SuccessionHandoffSystem.DisputeTriggerStreamName, seed, 1);
        streams.Add(SuccessionDisputeResolutionSystem.ScoringStreamName, seed, 1);
        streams.Add(SuccessionDisputeResolutionSystem.SplinterStreamName, seed, 1);
        return streams;
    }

    private static RuntimeId<Household> Establish(WorldState state, RuntimeId<Character> headId, GameDate since)
    {
        var householdId = state.HouseholdIds.Issue();
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, since));
        return householdId;
    }

    [Test]
    public void HandoffSystemDeclaresTheRelationshipsActorsPhaseAndReadWriteSet()
    {
        var system = new SuccessionHandoffSystem();
        Assert.Multiple(() =>
        {
            Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
            Assert.That(system.Reads, Is.EquivalentTo(new[] { "householdHeadships", "heirDesignations", "characters", "successionDisputes" }));
            Assert.That(system.Writes, Is.EquivalentTo(new[] { "householdHeadships", "successionDisputes", "eventIds" }));
        });
    }

    [Test]
    public void DisputeResolutionSystemDeclaresTheRelationshipsActorsPhaseAndReadWriteSet()
    {
        var system = new SuccessionDisputeResolutionSystem();
        Assert.Multiple(() =>
        {
            Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
            Assert.That(system.Prerequisites, Is.EquivalentTo(new[] { "succession.handoff" }));
        });
    }

    [Test]
    public void OrdinaryInheritancePassesTheHouseholdToTheSoleLegitimateChild()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(
            headId, deathRecord: new DeathRecord(new GameDate(1), DeathCause.OldAge, 70)));
        var childId = state.CharacterIds.Issue();
        state.Characters.Add(childId, CharacterTestFixtures.Minimal(childId, fatherId: headId, birthDate: new GameDate(-240)));
        var householdId = Establish(state, headId, new GameDate(0));

        var events = new SuccessionHandoffSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        Assert.Multiple(() =>
        {
            Assert.That(events, Has.Count.EqualTo(1));
            Assert.That(events[0], Is.InstanceOf<HouseholdHeadTransferredEvent>());
            state.HouseholdHeadships.TryGet(householdId, out var headship);
            Assert.That(headship.HeadCharacterId, Is.EqualTo(childId));
            Assert.That(headship.RegentCharacterId, Is.Null);
        });
    }

    [Test]
    public void ADeclaredHeirIsChosenOverTheDefaultAgnaticOrder()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(
            headId, deathRecord: new DeathRecord(new GameDate(1), DeathCause.OldAge, 70)));
        var elderSonId = state.CharacterIds.Issue();
        state.Characters.Add(elderSonId, CharacterTestFixtures.Minimal(elderSonId, fatherId: headId, birthDate: new GameDate(-240)));
        var youngerDaughterId = state.CharacterIds.Issue();
        state.Characters.Add(youngerDaughterId, CharacterTestFixtures.Minimal(
            youngerDaughterId, fatherId: headId, birthDate: new GameDate(-200)));
        var householdId = Establish(state, headId, new GameDate(0));

        var declare = new DeclareHeirCommand(state.CommandIds.Issue(), headId.ToTaggedString(), new GameDate(0), null, householdId, youngerDaughterId);
        var declareResult = DeclareHeirCommands.Pipeline.Execute(state, declare);
        Assert.That(declareResult.Accepted, Is.True);

        var events = new SuccessionHandoffSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        Assert.Multiple(() =>
        {
            Assert.That(events, Has.Count.EqualTo(1));
            state.HouseholdHeadships.TryGet(householdId, out var headship);
            Assert.That(headship.HeadCharacterId, Is.EqualTo(youngerDaughterId));
        });
    }

    [Test]
    public void ContestedSuccessionOpensADisputeAndLaterResolvesToAWinner()
    {
        SuccessionDispute? dispute = null;
        RuntimeId<Household> householdId = default;
        RuntimeId<Character> firstClaimantId = default;
        RuntimeId<Character> secondClaimantId = default;
        WorldState? resolvedState = null;

        for (var seed = 1UL; seed < 200 && dispute is null; seed++)
        {
            var state = new WorldState(new GameDate(0));
            var headId = state.CharacterIds.Issue();
            state.Characters.Add(headId, CharacterTestFixtures.Minimal(
                headId, deathRecord: new DeathRecord(new GameDate(1), DeathCause.OldAge, 70)));
            firstClaimantId = state.CharacterIds.Issue();
            state.Characters.Add(firstClaimantId, CharacterTestFixtures.Minimal(firstClaimantId, fatherId: headId, birthDate: new GameDate(-240)));
            secondClaimantId = state.CharacterIds.Issue();
            state.Characters.Add(secondClaimantId, CharacterTestFixtures.Minimal(secondClaimantId, fatherId: headId, birthDate: new GameDate(-200)));
            householdId = Establish(state, headId, new GameDate(0));

            new SuccessionHandoffSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(seed)));

            foreach (var entry in state.SuccessionDisputes.InAscendingOrder())
                dispute = entry.Value;
            resolvedState = state;
        }

        Assert.That(dispute, Is.Not.Null, "Expected at least one seed to trigger a succession dispute within the attempt budget.");
        var confirmedDispute = dispute!;
        var finalState = resolvedState!;

        // Headship stays vacant (still pointing at the dead head) while the dispute is pending.
        finalState.HouseholdHeadships.TryGet(householdId, out var pendingHeadship);
        Assert.That(pendingHeadship.HeadCharacterId, Is.Not.EqualTo(firstClaimantId).And.Not.EqualTo(secondClaimantId));

        var resolutionDate = confirmedDispute.ResolutionDueDate;
        var resolveEvents = new SuccessionDisputeResolutionSystem().Tick(finalState, new MonthlyTickContext(resolutionDate, Streams(1)));

        Assert.Multiple(() =>
        {
            Assert.That(resolveEvents, Has.Count.GreaterThanOrEqualTo(1));
            Assert.That(resolveEvents.Any(e => e is SuccessionDisputeResolvedEvent), Is.True);

            finalState.SuccessionDisputes.TryGet(confirmedDispute.DisputeId, out var resolved);
            Assert.That(resolved.Status, Is.Not.EqualTo(SuccessionDisputeStatus.Pending));
            Assert.That(resolved.WinnerCharacterId, Is.EqualTo(firstClaimantId).Or.EqualTo(secondClaimantId));

            finalState.HouseholdHeadships.TryGet(householdId, out var finalHeadship);
            Assert.That(finalHeadship.HeadCharacterId, Is.EqualTo(resolved.WinnerCharacterId));
        });
    }

    [Test]
    public void AdoptedChildStandsIdenticallyToABirthChildInTheEligiblePool()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId));
        var householdId = Establish(state, headId, new GameDate(0));

        var candidateId = state.CharacterIds.Issue();
        state.Characters.Add(candidateId, CharacterTestFixtures.Minimal(candidateId, birthDate: new GameDate(-240)));

        var adopt = new AdoptChildCommand(state.CommandIds.Issue(), headId.ToTaggedString(), new GameDate(0), null, householdId, headId, candidateId);
        var adoptResult = AdoptChildCommands.Pipeline.Execute(state, adopt);
        Assert.That(adoptResult.Accepted, Is.True);

        state.Characters.TryGet(candidateId, out var candidate);
        Assert.That(candidate.Household, Is.EqualTo(householdId));

        state.HeirDesignations.TryGet(householdId, out var designation);
        var pool = HeirEligibilityService.EligibleHeirs(state, headId, designation);
        Assert.That(pool, Does.Contain(candidateId));

        // The head then dies with only the adopted child eligible — the household passes to them.
        state.Characters.TryGet(headId, out var head);
        var updatedHead = head with { DeathRecord = new DeathRecord(new GameDate(1), DeathCause.OldAge, 70) };
        state.Characters.Remove(headId);
        state.Characters.Add(headId, updatedHead);

        new SuccessionHandoffSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        state.HouseholdHeadships.TryGet(householdId, out var headship);
        Assert.That(headship.HeadCharacterId, Is.EqualTo(candidateId));
    }

    [Test]
    public void DebtObligationsRemainWithTheHouseholdAcrossAnOrdinaryHandoff()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(
            headId, deathRecord: new DeathRecord(new GameDate(1), DeathCause.OldAge, 70)));
        var childId = state.CharacterIds.Issue();
        state.Characters.Add(childId, CharacterTestFixtures.Minimal(childId, fatherId: headId, birthDate: new GameDate(-240)));
        var householdId = Establish(state, headId, new GameDate(0));

        var debt = DebtService.IssueLoan(state, new GameDate(0), settlementId, householdId, Money.FromDenarii(500));

        new SuccessionHandoffSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        state.HouseholdHeadships.TryGet(householdId, out var headship);
        Assert.Multiple(() =>
        {
            Assert.That(headship.HeadCharacterId, Is.EqualTo(childId));
            state.DebtRecords.TryGet(debt.Id, out var stillOwed);
            Assert.That(stillOwed.DebtorHouseholdId, Is.EqualTo(householdId));
            Assert.That(stillOwed.Principal, Is.EqualTo(Money.FromDenarii(500)));
        });
    }

    [Test]
    public void AbsentHeirsLeaveTheEstateWithASurvivingSpouseInTrust()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        var spouseId = state.CharacterIds.Issue();

        var head = CharacterTestFixtures.Minimal(
            headId,
            maritalHistory: new[] { new MarriageRecord(spouseId, new GameDate(-100), new GameDate(1), MarriageEndReason.Death) },
            deathRecord: new DeathRecord(new GameDate(1), DeathCause.OldAge, 70));
        state.Characters.Add(headId, head);
        state.Characters.Add(spouseId, CharacterTestFixtures.Minimal(
            spouseId,
            maritalHistory: new[] { new MarriageRecord(headId, new GameDate(-100), new GameDate(1), MarriageEndReason.Death) }));

        var householdId = Establish(state, headId, new GameDate(0));

        var events = new SuccessionHandoffSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        Assert.Multiple(() =>
        {
            Assert.That(events, Has.Count.EqualTo(1));
            Assert.That(events[0], Is.InstanceOf<HouseholdHeadTransferredEvent>());
            state.HouseholdHeadships.TryGet(householdId, out var headship);
            Assert.That(headship.HeadCharacterId, Is.EqualTo(spouseId));
        });
    }

    [Test]
    public void ExtinguishesAHouseholdWithNoEligibleHeirAndNoSurvivingSpouse()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(
            headId, deathRecord: new DeathRecord(new GameDate(1), DeathCause.OldAge, 70)));
        var householdId = Establish(state, headId, new GameDate(0));

        var events = new SuccessionHandoffSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        Assert.Multiple(() =>
        {
            Assert.That(events, Has.Count.EqualTo(1));
            Assert.That(events[0], Is.InstanceOf<HouseholdExtinguishedEvent>());
            Assert.That(state.HouseholdHeadships.TryGet(householdId, out _), Is.False);
        });
    }

    [Test]
    public void DisowningTheOnlyEligibleHeirExtinguishesTheHouseholdOnceTheHeadDies()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId));
        var childId = state.CharacterIds.Issue();
        state.Characters.Add(childId, CharacterTestFixtures.Minimal(childId, fatherId: headId, birthDate: new GameDate(-240)));
        var householdId = Establish(state, headId, new GameDate(0));

        var disown = new DisownHeirCommand(state.CommandIds.Issue(), headId.ToTaggedString(), new GameDate(0), null, householdId, childId);
        var disownResult = DisownHeirCommands.Pipeline.Execute(state, disown);
        Assert.That(disownResult.Accepted, Is.True);

        state.Characters.TryGet(headId, out var head);
        var updatedHead = head with { DeathRecord = new DeathRecord(new GameDate(1), DeathCause.OldAge, 70) };
        state.Characters.Remove(headId);
        state.Characters.Add(headId, updatedHead);

        var events = new SuccessionHandoffSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        Assert.Multiple(() =>
        {
            Assert.That(events[0], Is.InstanceOf<HouseholdExtinguishedEvent>());
            state.Characters.TryGet(childId, out var disowned);
            Assert.That(disowned.Condition.Loyalty, Is.EqualTo(50 - SuccessionCatalog.DisownedLoyaltyPenalty));
        });
    }

    [Test]
    public void AcknowledgedIllegitimateChildBecomesEligible()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId));
        var childId = state.CharacterIds.Issue();
        state.Characters.Add(childId, CharacterTestFixtures.Minimal(
            childId, fatherId: headId, legitimacy: Legitimacy.Illegitimate, birthDate: new GameDate(-240)));
        var householdId = Establish(state, headId, new GameDate(0));

        state.HeirDesignations.TryGet(householdId, out var beforeDesignation);
        var poolBefore = HeirEligibilityService.EligibleHeirs(state, headId, beforeDesignation);
        Assert.That(poolBefore, Does.Not.Contain(childId));

        var acknowledge = new AcknowledgeIllegitimateChildCommand(
            state.CommandIds.Issue(), headId.ToTaggedString(), new GameDate(0), null, householdId, headId, childId);
        var result = AcknowledgeIllegitimateChildCommands.Pipeline.Execute(state, acknowledge);
        Assert.That(result.Accepted, Is.True);

        state.HeirDesignations.TryGet(householdId, out var afterDesignation);
        var poolAfter = HeirEligibilityService.EligibleHeirs(state, headId, afterDesignation);
        Assert.That(poolAfter, Does.Contain(childId));
    }
}
