using Gens.Simulation.Characters;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class AssignDutyCommandTests
{
    private static readonly GameDate SubmittedDate = new(300);
    private static readonly GameDate AdultBirthDate = new(0);

    [Test]
    public void ValidAssignmentSetsDutyAndEmitsAnEvent()
    {
        var state = new WorldState(SubmittedDate);
        var householdId = state.HouseholdIds.Issue();
        var location = state.SettlementIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, birthDate: AdultBirthDate, household: householdId, location: location));

        var command = new AssignDutyCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, householdId, DutySlot.Cook);
        var result = AssignDutyCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        Assert.That(result.Events.Single(), Is.InstanceOf<DutyAssignedEvent>());

        state.Characters.TryGet(characterId, out var character);
        Assert.That(character.Duty, Is.EqualTo(new DutyAssignment(householdId, DutySlot.Cook, SubmittedDate)));
    }

    [Test]
    public void ValidationRejectsACharacterOutsideTheHousehold()
    {
        var state = new WorldState(SubmittedDate);
        var householdId = state.HouseholdIds.Issue();
        var otherHouseholdId = state.HouseholdIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, birthDate: AdultBirthDate, household: otherHouseholdId));

        var command = new AssignDutyCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, householdId, DutySlot.Cook);
        var result = AssignDutyCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(AssignDutyCommands.NotHouseholdMember));
    }

    [Test]
    public void ValidationRejectsATooYoungCharacter()
    {
        var state = new WorldState(SubmittedDate);
        var householdId = state.HouseholdIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, birthDate: new GameDate(295), household: householdId));

        var command = new AssignDutyCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, householdId, DutySlot.Cook);
        var result = AssignDutyCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(AssignDutyCommands.TooYoung));
    }

    [Test]
    public void ValidationRejectsACharacterAlreadyHoldingADuty()
    {
        var state = new WorldState(SubmittedDate);
        var householdId = state.HouseholdIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, birthDate: AdultBirthDate, household: householdId,
            duty: new DutyAssignment(householdId, DutySlot.FieldHand, SubmittedDate)));

        var command = new AssignDutyCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, householdId, DutySlot.Cook);
        var result = AssignDutyCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(AssignDutyCommands.AlreadyAssigned));
    }

    [Test]
    public void ValidationRejectsATooFatiguedCharacter()
    {
        var state = new WorldState(SubmittedDate);
        var householdId = state.HouseholdIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, birthDate: AdultBirthDate, household: householdId,
            condition: new Condition(80, 95, 50, 20, 50)));

        var command = new AssignDutyCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, householdId, DutySlot.Cook);
        var result = AssignDutyCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(AssignDutyCommands.Unavailable));
    }

    [Test]
    public void ValidationRejectsInsufficientCompetence()
    {
        var state = new WorldState(SubmittedDate);
        var householdId = state.HouseholdIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, birthDate: AdultBirthDate, household: householdId,
            skills: new LaborSkills(10, 10, 10, 0, 10)));

        var command = new AssignDutyCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, householdId, DutySlot.Cook);
        var result = AssignDutyCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(AssignDutyCommands.InsufficientCompetence));
    }

    [Test]
    public void ValidationRejectsALocationConflictWithOtherHouseholdMembers()
    {
        var state = new WorldState(SubmittedDate);
        var householdId = state.HouseholdIds.Issue();
        var homeLocation = state.SettlementIds.Issue();
        var elsewhere = state.SettlementIds.Issue();
        var residentId = state.CharacterIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(residentId, CharacterTestFixtures.Minimal(
            residentId, birthDate: AdultBirthDate, household: householdId, location: homeLocation));
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, birthDate: AdultBirthDate, household: householdId, location: elsewhere));

        var command = new AssignDutyCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, householdId, DutySlot.Cook);
        var result = AssignDutyCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(AssignDutyCommands.LocationConflict));
    }

    [Test]
    public void ValidationRejectsASlotAtCapacity()
    {
        var state = new WorldState(SubmittedDate);
        var householdId = state.HouseholdIds.Issue();
        var location = state.SettlementIds.Issue();
        var existingCookId = state.CharacterIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(existingCookId, CharacterTestFixtures.Minimal(
            existingCookId, birthDate: AdultBirthDate, household: householdId, location: location,
            duty: new DutyAssignment(householdId, DutySlot.Cook, SubmittedDate)));
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, birthDate: AdultBirthDate, household: householdId, location: location));

        var command = new AssignDutyCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, householdId, DutySlot.Cook);
        var result = AssignDutyCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(AssignDutyCommands.SlotAtCapacity));
    }

    [Test]
    public void ADeceasedSlotHolderDoesNotConsumeCapacityOrCauseALocationConflict()
    {
        var state = new WorldState(SubmittedDate);
        var householdId = state.HouseholdIds.Issue();
        var location = state.SettlementIds.Issue();
        var elsewhere = state.SettlementIds.Issue();
        var deceasedCookId = state.CharacterIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(deceasedCookId, CharacterTestFixtures.Minimal(
            deceasedCookId, birthDate: AdultBirthDate, household: householdId, location: elsewhere,
            duty: new DutyAssignment(householdId, DutySlot.Cook, SubmittedDate),
            deathRecord: new DeathRecord(new GameDate(280), DeathCause.Disease, 23)));
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, birthDate: AdultBirthDate, household: householdId, location: location));

        var command = new AssignDutyCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, householdId, DutySlot.Cook);
        var result = AssignDutyCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
    }
}
