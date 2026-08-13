using Gens.Simulation.Characters;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class RecordMarriageCommandTests
{
    [Test]
    public void ValidMarriageAddsAnOpenEntryToBothSidesAndEmitsAnEvent()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        var spouseId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        state.Characters.Add(spouseId, CharacterTestFixtures.Minimal(spouseId));

        var command = new RecordMarriageCommand(state.CommandIds.Issue(), "player", new GameDate(10), null, characterId, spouseId);
        var result = RecordMarriageCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        Assert.That(result.Events.Single(), Is.InstanceOf<CharactersMarriedEvent>());

        state.Characters.TryGet(characterId, out var character);
        state.Characters.TryGet(spouseId, out var spouse);
        Assert.That(character.CurrentSpouseId, Is.EqualTo(spouseId));
        Assert.That(spouse.CurrentSpouseId, Is.EqualTo(characterId));
    }

    [Test]
    public void ValidationRejectsMarryingOneself()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));

        var command = new RecordMarriageCommand(state.CommandIds.Issue(), "player", new GameDate(10), null, characterId, characterId);
        var result = RecordMarriageCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(RecordMarriageCommands.SelfMarriage));
    }

    [Test]
    public void ValidationRejectsAnAlreadyMarriedCharacter()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        var firstSpouseId = state.CharacterIds.Issue();
        var secondSpouseId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, maritalHistory: new[] { new MarriageRecord(firstSpouseId, new GameDate(0), null, null) }));
        state.Characters.Add(secondSpouseId, CharacterTestFixtures.Minimal(secondSpouseId));

        var command = new RecordMarriageCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null, characterId, secondSpouseId);
        var result = RecordMarriageCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(RecordMarriageCommands.AlreadyMarried));
    }

    [Test]
    public void ValidationRejectsADeceasedSpouse()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        var spouseId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        state.Characters.Add(spouseId, CharacterTestFixtures.Minimal(
            spouseId, deathRecord: new DeathRecord(new GameDate(5), DeathCause.Disease, 20)));

        var command = new RecordMarriageCommand(state.CommandIds.Issue(), "player", new GameDate(10), null, characterId, spouseId);
        var result = RecordMarriageCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(RecordMarriageCommands.SpouseDeceased));
    }
}
