using Gens.Simulation.Characters;
using Gens.Simulation.Health;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Tests.Characters;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Health;

public sealed class CharacterHealthConditionSystemTests
{
    private const string ProgressionStreamName = "test-health-progression";
    private static readonly DefinitionId<HealthConditionDefinition> TestFever = new("test-fever");

    [Test]
    public void AnUntreatedCaseDrainsHealthAndSeverityDriftsWhenNeitherRollResolvesIt()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, condition: new Condition(80, 0, 50, 20, 50)));
        var caseId = AddCondition(state, characterId, HealthConditionCategory.Chronic, hasCure: true, severity: 50);

        var drain = HealthConditionProgressionCalculator.MonthlyHealthDrain(HealthConditionCategory.Chronic, 50, treated: false);
        var newHealth = 80 - drain;
        var recoveryThreshold = Threshold(HealthConditionProgressionCalculator.MonthlyRecoveryProbability(
            HealthConditionCategory.Chronic, hasCure: true, treated: false));
        var fatalityThreshold = Threshold(HealthConditionProgressionCalculator.MonthlyFatalityProbability(
            HealthConditionCategory.Chronic, 50, newHealth, treated: false));
        var streams = StreamsWithDraws(v => v >= recoveryThreshold, v => v >= fatalityThreshold);

        var system = new CharacterHealthConditionSystem(ProgressionStreamName);
        var events = system.Tick(state, new MonthlyTickContext(new GameDate(10), streams));

        state.Characters.TryGet(characterId, out var updatedCharacter);
        state.CharacterHealthConditions.TryGet(caseId, out var updatedCase);

        Assert.That(updatedCharacter.Condition.Health, Is.EqualTo(newHealth));
        Assert.That(updatedCase.Status, Is.EqualTo(CharacterHealthConditionStatus.Active));
        Assert.That(updatedCase.Severity, Is.EqualTo(51));
        Assert.That(updatedCase.TreatedByPhysician, Is.False);
        Assert.That(events.OfType<CharacterHealthConditionRecoveredEvent>(), Is.Empty);
        Assert.That(events.OfType<CharacterDiedEvent>(), Is.Empty);
    }

    [Test]
    public void RecoveryGrantsImmunityForAnAcuteCase()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, condition: new Condition(80, 0, 50, 20, 50)));
        var caseId = AddCondition(state, characterId, HealthConditionCategory.Acute, hasCure: true, severity: 30);

        var recoveryThreshold = Threshold(HealthConditionProgressionCalculator.MonthlyRecoveryProbability(
            HealthConditionCategory.Acute, hasCure: true, treated: false));
        var streams = StreamsWithDraws(v => v < recoveryThreshold);

        var system = new CharacterHealthConditionSystem(ProgressionStreamName);
        var events = system.Tick(state, new MonthlyTickContext(new GameDate(10), streams));

        state.CharacterHealthConditions.TryGet(caseId, out var updatedCase);
        Assert.That(updatedCase.Status, Is.EqualTo(CharacterHealthConditionStatus.Recovered));
        Assert.That(updatedCase.GrantedImmunity, Is.True);

        var recovered = events.OfType<CharacterHealthConditionRecoveredEvent>().Single();
        Assert.That(recovered.CharacterId, Is.EqualTo(characterId));
        Assert.That(recovered.GrantedImmunity, Is.True);
    }

    [Test]
    public void RecoveryDoesNotGrantImmunityForAChronicCase()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, condition: new Condition(80, 0, 50, 20, 50)));
        var caseId = AddCondition(state, characterId, HealthConditionCategory.Chronic, hasCure: true, severity: 10);

        var recoveryThreshold = Threshold(HealthConditionProgressionCalculator.MonthlyRecoveryProbability(
            HealthConditionCategory.Chronic, hasCure: true, treated: false));
        var streams = StreamsWithDraws(v => v < recoveryThreshold);

        var system = new CharacterHealthConditionSystem(ProgressionStreamName);
        system.Tick(state, new MonthlyTickContext(new GameDate(10), streams));

        state.CharacterHealthConditions.TryGet(caseId, out var updatedCase);
        Assert.That(updatedCase.Status, Is.EqualTo(CharacterHealthConditionStatus.Recovered));
        Assert.That(updatedCase.GrantedImmunity, Is.False);
    }

    [Test]
    public void AFatalRollKillsTheCharacterAndAttributesTheDeathToTheCondition()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, condition: new Condition(5, 0, 50, 20, 50)));
        AddCondition(state, characterId, HealthConditionCategory.Acute, hasCure: false, severity: 100);

        var drain = HealthConditionProgressionCalculator.MonthlyHealthDrain(HealthConditionCategory.Acute, 100, treated: false);
        var newHealth = Math.Max(1, 5 - drain);
        var recoveryThreshold = Threshold(HealthConditionProgressionCalculator.MonthlyRecoveryProbability(
            HealthConditionCategory.Acute, hasCure: false, treated: false));
        var fatalityThreshold = Threshold(HealthConditionProgressionCalculator.MonthlyFatalityProbability(
            HealthConditionCategory.Acute, 100, newHealth, treated: false));
        var streams = StreamsWithDraws(v => v >= recoveryThreshold, v => v < fatalityThreshold);

        var system = new CharacterHealthConditionSystem(ProgressionStreamName);
        var events = system.Tick(state, new MonthlyTickContext(new GameDate(10), streams));

        state.Characters.TryGet(characterId, out var deceased);
        Assert.That(deceased.IsAlive, Is.False);
        Assert.That(deceased.DeathRecord!.Value.Cause, Is.EqualTo(DeathCause.Disease));
        Assert.That(deceased.DeathRecord.Value.ConditionId, Is.EqualTo(TestFever));

        var died = events.OfType<CharacterDiedEvent>().Single();
        Assert.That(died.CharacterId, Is.EqualTo(characterId));
    }

    [Test]
    public void ASecondConditionOnTheSameCharacterIsResolvedRatherThanLeftActiveWhenTheFirstKillsThem()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, condition: new Condition(5, 0, 50, 20, 50)));
        // Issued first, so processed first (ascending onset order) — this is the one whose fatal roll
        // actually kills the Character.
        var fatalCaseId = AddCondition(state, characterId, HealthConditionCategory.Acute, hasCure: false, severity: 100);
        // Issued second, so processed after the Character is already dead — this is the orphaned case
        // the fix resolves instead of leaving Active forever.
        var orphanedCaseId = AddCondition(state, characterId, HealthConditionCategory.Chronic, hasCure: true, severity: 10);

        var drain = HealthConditionProgressionCalculator.MonthlyHealthDrain(HealthConditionCategory.Acute, 100, treated: false);
        var newHealth = Math.Max(1, 5 - drain);
        var recoveryThreshold = Threshold(HealthConditionProgressionCalculator.MonthlyRecoveryProbability(
            HealthConditionCategory.Acute, hasCure: false, treated: false));
        var fatalityThreshold = Threshold(HealthConditionProgressionCalculator.MonthlyFatalityProbability(
            HealthConditionCategory.Acute, 100, newHealth, treated: false));
        var streams = StreamsWithDraws(v => v >= recoveryThreshold, v => v < fatalityThreshold);

        var system = new CharacterHealthConditionSystem(ProgressionStreamName);
        system.Tick(state, new MonthlyTickContext(new GameDate(10), streams));

        state.CharacterHealthConditions.TryGet(fatalCaseId, out var fatalCase);
        state.CharacterHealthConditions.TryGet(orphanedCaseId, out var orphanedCase);

        Assert.That(fatalCase.Status, Is.EqualTo(CharacterHealthConditionStatus.Fatal));
        Assert.That(orphanedCase.Status, Is.Not.EqualTo(CharacterHealthConditionStatus.Active));
        Assert.That(orphanedCase.ResolvedDate, Is.EqualTo(new GameDate(10)));
        Assert.That(HealthQueries.HasActiveCondition(state, characterId, orphanedCase.ConditionId), Is.False);
    }

    [Test]
    public void TreatmentGoesToTheEarliestOnsetCaseWhenHouseholdCapacityIsLimited()
    {
        var state = new WorldState(new GameDate(10));
        var householdId = state.HouseholdIds.Issue();

        var physicianId = state.CharacterIds.Issue();
        state.Characters.Add(physicianId, CharacterTestFixtures.Minimal(
            physicianId, household: householdId,
            skills: new LaborSkills(0, 0, 0, 0, 10),
            duty: new DutyAssignment(householdId, DutySlot.Physician, new GameDate(0))));

        var firstPatientId = state.CharacterIds.Issue();
        state.Characters.Add(firstPatientId, CharacterTestFixtures.Minimal(
            firstPatientId, household: householdId, condition: new Condition(80, 0, 50, 20, 50)));
        var firstCaseId = AddCondition(state, firstPatientId, HealthConditionCategory.Chronic, hasCure: true, severity: 10);

        var secondPatientId = state.CharacterIds.Issue();
        state.Characters.Add(secondPatientId, CharacterTestFixtures.Minimal(
            secondPatientId, household: householdId, condition: new Condition(80, 0, 50, 20, 50)));
        var secondCaseId = AddCondition(state, secondPatientId, HealthConditionCategory.Chronic, hasCure: true, severity: 10);

        Assert.That(CareCapacityCalculator.MonthlyCareCapacity(10), Is.EqualTo(1));

        var streams = new RandomStreamSet();
        streams.Add(ProgressionStreamName, 12345, 1);

        var system = new CharacterHealthConditionSystem(ProgressionStreamName);
        system.Tick(state, new MonthlyTickContext(new GameDate(10), streams));

        state.CharacterHealthConditions.TryGet(firstCaseId, out var firstCase);
        state.CharacterHealthConditions.TryGet(secondCaseId, out var secondCase);

        Assert.That(firstCase.TreatedByPhysician, Is.True);
        Assert.That(secondCase.TreatedByPhysician, Is.False);
    }

    private static RuntimeId<CharacterHealthCondition> AddCondition(
        WorldState state, RuntimeId<Character> characterId, HealthConditionCategory category, bool hasCure, int severity)
    {
        var id = state.CharacterHealthConditionIds.Issue();
        state.CharacterHealthConditions.Add(id, CharacterHealthCondition.Create(
            id, characterId, TestFever, category, hasCure, severity, new GameDate(1)));
        return id;
    }

    private static uint Threshold(double probability) =>
        (uint)Math.Clamp(probability * 1_000_000, 0, 1_000_000);

    /// <summary>Deterministically finds a seed whose sequential draws each satisfy the corresponding
    /// predicate, the same "search rather than hand-pick a magic seed" idiom
    /// <c>CharacterLifecycleSystemTests</c> already established.</summary>
    private static RandomStreamSet StreamsWithDraws(params Predicate<uint>[] matchesDraw)
    {
        for (ulong seed = 0; seed < 200_000; seed++)
        {
            var probe = new RandomStreamSet();
            probe.Add(ProgressionStreamName, seed, 1);
            var matched = true;
            foreach (var predicate in matchesDraw)
            {
                if (!predicate(probe.NextUInt(ProgressionStreamName, 1_000_000)))
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                var streams = new RandomStreamSet();
                streams.Add(ProgressionStreamName, seed, 1);
                return streams;
            }
        }

        throw new InvalidOperationException("No seed found matching the requested draw sequence.");
    }
}
