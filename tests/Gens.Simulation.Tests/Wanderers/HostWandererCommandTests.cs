using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using Gens.Simulation.Wanderers;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Wanderers;

public sealed class HostWandererCommandTests
{
    private static readonly GameDate Now = new(24);

    private static HostWandererCommand Command(
        WorldState state,
        RuntimeId<Wanderer> wandererId,
        RuntimeId<Household> householdId,
        RuntimeId<Character>? beneficiary = null) =>
        new(state.CommandIds.Issue(), "player", Now, null, wandererId, householdId, beneficiary);

    [Test]
    public void HostingPaysAFeeRaisesDignitasRaisesTheWandererFameAndLogsTheEngagement()
    {
        var state = new WorldState(Now);
        var householdId = state.HouseholdIds.Issue();
        var wanderer = WandererTestFixtures.AddWanderer(state, WandererType.PhilosopherRhetorician, fame: 50);
        var profile = WandererTestFixtures.TypeCatalog.Get(WandererType.PhilosopherRhetorician);
        var pipeline = HostWandererCommands.CreatePipeline(WandererTestFixtures.TypeCatalog);

        var result = pipeline.Execute(state, Command(state, wanderer.Id, householdId));

        Assert.That(result.Accepted, Is.True);
        state.Wanderers.TryGet(wanderer.Id, out var hosted);
        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account);
        var engagement = state.WandererEngagements.InAscendingOrder().Single().Value;

        Assert.Multiple(() =>
        {
            Assert.That(account!.Balance, Is.EqualTo(Money.Zero - profile.HostFee));
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(profile.HostDignitasGain));
            Assert.That(hosted!.Fame, Is.EqualTo(50 + profile.EngagementFameGain));
            Assert.That(hosted.FameTrend, Is.EqualTo(WandererFameTrend.Rising));
            Assert.That(hosted.MonthsSinceLastEngagement, Is.Zero);
            Assert.That(hosted.Status, Is.EqualTo(WandererStatus.Wandering), "a Host never recruits.");
            Assert.That(hosted.IsActivelyTracked, Is.True, "a hosted Wanderer moves on afterward.");
            Assert.That(engagement.EngagementType, Is.EqualTo(WandererEngagementType.Host));
            Assert.That(engagement.FeePaid, Is.EqualTo(profile.HostFee));
            Assert.That(engagement.ResultingCharacterId, Is.Null);
            Assert.That(result.Events.OfType<WandererHostedEvent>().Single().DignitasGained,
                Is.EqualTo(profile.HostDignitasGain));
        });
    }

    [Test]
    public void HostingAPhysicianRestoresRealHealthToANamedHouseholdMember()
    {
        var state = new WorldState(Now);
        var householdId = state.HouseholdIds.Issue();
        var patientId = state.CharacterIds.Issue();
        state.Characters.Add(patientId, CharacterTestFixtures.Minimal(
            patientId, household: householdId, condition: new Condition(40, 10, 50, 50, 50)));
        var wanderer = WandererTestFixtures.AddWanderer(state, WandererType.Physician);
        var pipeline = HostWandererCommands.CreatePipeline(WandererTestFixtures.TypeCatalog);

        var result = pipeline.Execute(state, Command(state, wanderer.Id, householdId, patientId));

        Assert.That(result.Accepted, Is.True);
        state.Characters.TryGet(patientId, out var patient);
        var engagement = state.WandererEngagements.InAscendingOrder().Single().Value;

        Assert.Multiple(() =>
        {
            Assert.That(patient!.Condition.Health, Is.EqualTo(40 + HostWandererCommands.PhysicianHealthRecovery));
            Assert.That(patient.Condition.Fatigue, Is.EqualTo(10), "only Health moves.");
            Assert.That(engagement.HealthRestored, Is.EqualTo(HostWandererCommands.PhysicianHealthRecovery));
            Assert.That(engagement.BeneficiaryCharacterId, Is.EqualTo(patientId));
        });
    }

    [Test]
    public void APhysicianTreatmentIsClampedAtTheHealthCeiling()
    {
        var state = new WorldState(Now);
        var householdId = state.HouseholdIds.Issue();
        var patientId = state.CharacterIds.Issue();
        state.Characters.Add(patientId, CharacterTestFixtures.Minimal(
            patientId, household: householdId, condition: new Condition(95, 10, 50, 50, 50)));
        var wanderer = WandererTestFixtures.AddWanderer(state, WandererType.Physician);
        var pipeline = HostWandererCommands.CreatePipeline(WandererTestFixtures.TypeCatalog);

        pipeline.Execute(state, Command(state, wanderer.Id, householdId, patientId));

        state.Characters.TryGet(patientId, out var patient);
        Assert.Multiple(() =>
        {
            Assert.That(patient!.Condition.Health, Is.EqualTo(100));
            Assert.That(state.WandererEngagements.InAscendingOrder().Single().Value.HealthRestored, Is.EqualTo(5));
        });
    }

    [Test]
    public void OnlyAPhysicianAcceptsAPatient()
    {
        var state = new WorldState(Now);
        var householdId = state.HouseholdIds.Issue();
        var patientId = state.CharacterIds.Issue();
        state.Characters.Add(patientId, CharacterTestFixtures.Minimal(patientId, household: householdId));
        var wanderer = WandererTestFixtures.AddWanderer(state, WandererType.Entertainer);
        var pipeline = HostWandererCommands.CreatePipeline(WandererTestFixtures.TypeCatalog);

        var result = pipeline.Execute(state, Command(state, wanderer.Id, householdId, patientId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(HostWandererCommands.BeneficiaryNotTreatable));
        });
    }

    [Test]
    public void ValidationRejectsAPatientWhoIsMissingDeceasedOrOutsideTheHousehold()
    {
        var state = new WorldState(Now);
        var householdId = state.HouseholdIds.Issue();
        var otherHouseholdId = state.HouseholdIds.Issue();
        var deadId = state.CharacterIds.Issue();
        state.Characters.Add(deadId, CharacterTestFixtures.Minimal(
            deadId, household: householdId, deathRecord: new DeathRecord(new GameDate(20), DeathCause.OldAge, 70)));
        var outsiderId = state.CharacterIds.Issue();
        state.Characters.Add(outsiderId, CharacterTestFixtures.Minimal(outsiderId, household: otherHouseholdId));
        var wanderer = WandererTestFixtures.AddWanderer(state, WandererType.Physician);
        var pipeline = HostWandererCommands.CreatePipeline(WandererTestFixtures.TypeCatalog);

        Assert.Multiple(() =>
        {
            Assert.That(
                pipeline.Execute(state, Command(state, wanderer.Id, householdId, state.CharacterIds.Issue())).Error,
                Is.EqualTo(HostWandererCommands.BeneficiaryNotFound));
            Assert.That(
                pipeline.Execute(state, Command(state, wanderer.Id, householdId, deadId)).Error,
                Is.EqualTo(HostWandererCommands.BeneficiaryDeceased));
            Assert.That(
                pipeline.Execute(state, Command(state, wanderer.Id, householdId, outsiderId)).Error,
                Is.EqualTo(HostWandererCommands.BeneficiaryNotHouseholdMember));
        });
    }

    [Test]
    public void ValidationRejectsAMissingWandererAndAnAlreadyRecruitedOne()
    {
        var state = new WorldState(Now);
        var householdId = state.HouseholdIds.Issue();
        var wanderer = WandererTestFixtures.AddWanderer(state);
        state.Wanderers.Remove(wanderer.Id);
        state.Wanderers.Add(wanderer.Id, wanderer with
        {
            Status = WandererStatus.Recruited,
            IsActivelyTracked = false,
        });
        var pipeline = HostWandererCommands.CreatePipeline(WandererTestFixtures.TypeCatalog);

        Assert.Multiple(() =>
        {
            Assert.That(
                pipeline.Execute(state, Command(state, state.WandererIds.Issue(), householdId)).Error,
                Is.EqualTo(HostWandererCommands.WandererNotFound));
            Assert.That(
                pipeline.Execute(state, Command(state, wanderer.Id, householdId)).Error,
                Is.EqualTo(HostWandererCommands.WandererUnavailable));
        });
    }

    [Test]
    public void AHigherFameWandererDeliversAMoreValuableHostBenefit()
    {
        var lowState = new WorldState(Now);
        var lowHouseholdId = lowState.HouseholdIds.Issue();
        var lowWanderer = WandererTestFixtures.AddWanderer(lowState, WandererType.PhilosopherRhetorician, fame: 10);
        HostWandererCommands.CreatePipeline(WandererTestFixtures.TypeCatalog)
            .Execute(lowState, Command(lowState, lowWanderer.Id, lowHouseholdId));

        var highState = new WorldState(Now);
        var highHouseholdId = highState.HouseholdIds.Issue();
        var highWanderer = WandererTestFixtures.AddWanderer(highState, WandererType.PhilosopherRhetorician, fame: 90);
        HostWandererCommands.CreatePipeline(WandererTestFixtures.TypeCatalog)
            .Execute(highState, Command(highState, highWanderer.Id, highHouseholdId));

        Assert.That(
            DignitasResolver.Current(lowState, lowHouseholdId),
            Is.LessThan(DignitasResolver.Current(highState, highHouseholdId)),
            "§4: a high-Fame Wanderer must be a genuinely more valuable Host target than an obscure one.");
    }

    [Test]
    public void AHigherFameWandererPhysicianRestoresMoreHealth()
    {
        var lowState = new WorldState(Now);
        var lowHouseholdId = lowState.HouseholdIds.Issue();
        var lowPatientId = lowState.CharacterIds.Issue();
        lowState.Characters.Add(lowPatientId, CharacterTestFixtures.Minimal(
            lowPatientId, household: lowHouseholdId, condition: new Condition(40, 10, 50, 50, 50)));
        var lowWanderer = WandererTestFixtures.AddWanderer(lowState, WandererType.Physician, fame: 10);
        HostWandererCommands.CreatePipeline(WandererTestFixtures.TypeCatalog)
            .Execute(lowState, Command(lowState, lowWanderer.Id, lowHouseholdId, lowPatientId));

        var highState = new WorldState(Now);
        var highHouseholdId = highState.HouseholdIds.Issue();
        var highPatientId = highState.CharacterIds.Issue();
        highState.Characters.Add(highPatientId, CharacterTestFixtures.Minimal(
            highPatientId, household: highHouseholdId, condition: new Condition(40, 10, 50, 50, 50)));
        var highWanderer = WandererTestFixtures.AddWanderer(highState, WandererType.Physician, fame: 90);
        HostWandererCommands.CreatePipeline(WandererTestFixtures.TypeCatalog)
            .Execute(highState, Command(highState, highWanderer.Id, highHouseholdId, highPatientId));

        lowState.Characters.TryGet(lowPatientId, out var lowPatient);
        highState.Characters.TryGet(highPatientId, out var highPatient);
        Assert.That(lowPatient!.Condition.Health, Is.LessThan(highPatient!.Condition.Health));
    }

    [Test]
    public void TheCommittingHouseholdMayHostTheSameWandererAgain()
    {
        var state = new WorldState(Now);
        var householdId = state.HouseholdIds.Issue();
        var wanderer = WandererTestFixtures.AddWanderer(state, fame: 50);
        var pipeline = HostWandererCommands.CreatePipeline(WandererTestFixtures.TypeCatalog);

        Assert.That(pipeline.Execute(state, Command(state, wanderer.Id, householdId)).Accepted, Is.True);
        Assert.That(pipeline.Execute(state, Command(state, wanderer.Id, householdId)).Accepted, Is.True);

        Assert.That(WandererQueries.EngagementsFor(state, wanderer.Id).Count(), Is.EqualTo(2));
    }
}
