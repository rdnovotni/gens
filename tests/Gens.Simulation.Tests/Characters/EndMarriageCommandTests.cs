using Gens.Simulation.Characters;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class EndMarriageCommandTests
{
    [Test]
    public void ValidDivorceClosesBothSidesAndEmitsAnEvent()
    {
        var state = new WorldState(new GameDate(20));
        var characterId = state.CharacterIds.Issue();
        var spouseId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, maritalHistory: new[] { new MarriageRecord(spouseId, new GameDate(0), null, null) }));
        state.Characters.Add(spouseId, CharacterTestFixtures.Minimal(
            spouseId, maritalHistory: new[] { new MarriageRecord(characterId, new GameDate(0), null, null) }));

        var command = new EndMarriageCommand(
            state.CommandIds.Issue(), "player", new GameDate(20), null, characterId, MarriageEndReason.Divorce);
        var result = EndMarriageCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        var ended = (MarriageEndedEvent)result.Events.Single();
        Assert.That(ended.Reason, Is.EqualTo(MarriageEndReason.Divorce));

        state.Characters.TryGet(characterId, out var character);
        state.Characters.TryGet(spouseId, out var spouse);
        Assert.That(character.CurrentSpouseId, Is.Null);
        Assert.That(spouse.CurrentSpouseId, Is.Null);
        Assert.That(character.MaritalHistory.Single().EndReason, Is.EqualTo(MarriageEndReason.Divorce));
        Assert.That(spouse.MaritalHistory.Single().EndReason, Is.EqualTo(MarriageEndReason.Divorce));
    }

    [Test]
    public void ValidationRejectsDeathAsAReason()
    {
        var state = new WorldState(new GameDate(20));
        var characterId = state.CharacterIds.Issue();
        var spouseId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, maritalHistory: new[] { new MarriageRecord(spouseId, new GameDate(0), null, null) }));

        var command = new EndMarriageCommand(
            state.CommandIds.Issue(), "player", new GameDate(20), null, characterId, MarriageEndReason.Death);
        var result = EndMarriageCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(EndMarriageCommands.DeathNotAllowed));
    }

    [Test]
    public void ValidationRejectsAnUnmarriedCharacter()
    {
        var state = new WorldState(new GameDate(20));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));

        var command = new EndMarriageCommand(
            state.CommandIds.Issue(), "player", new GameDate(20), null, characterId, MarriageEndReason.Divorce);
        var result = EndMarriageCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(EndMarriageCommands.NotMarried));
    }
}
