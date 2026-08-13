using Gens.Simulation.Characters;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class ReleaseDutyCommandTests
{
    [Test]
    public void ValidReleaseClearsTheDutyAndEmitsAnEvent()
    {
        var state = new WorldState(new GameDate(10));
        var householdId = state.HouseholdIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, household: householdId,
            duty: new DutyAssignment(householdId, DutySlot.FieldHand, new GameDate(5))));

        var command = new ReleaseDutyCommand(state.CommandIds.Issue(), "player", new GameDate(10), null, characterId);
        var result = ReleaseDutyCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        Assert.That(result.Events.Single(), Is.InstanceOf<DutyReleasedEvent>());

        state.Characters.TryGet(characterId, out var character);
        Assert.That(character.Duty, Is.Null);
    }

    [Test]
    public void ValidationRejectsACharacterWithNoActiveDuty()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));

        var command = new ReleaseDutyCommand(state.CommandIds.Issue(), "player", new GameDate(10), null, characterId);
        var result = ReleaseDutyCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ReleaseDutyCommands.NoActiveDuty));
    }
}
