using Gens.Simulation.Characters;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class ApplyPermanentInjuryCommandTests
{
    [Test]
    public void ValidInjuryIsAppendedAndEmitsAnEvent()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));

        var command = new ApplyPermanentInjuryCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null, characterId,
            PermanentInjuryTarget.Martial, 15, "battlefield wound");
        var result = ApplyPermanentInjuryCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        var applied = (PermanentInjuryAppliedEvent)result.Events.Single();
        Assert.That(applied.Magnitude, Is.EqualTo(15));

        state.Characters.TryGet(characterId, out var character);
        var injury = character.PermanentInjuries.Single();
        Assert.That(injury.Target, Is.EqualTo(PermanentInjuryTarget.Martial));
        Assert.That(injury.Cause, Is.EqualTo("battlefield wound"));
    }

    [Test]
    public void ValidationRejectsAMissingCharacter()
    {
        var state = new WorldState(new GameDate(10));

        var command = new ApplyPermanentInjuryCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null,
            new Gens.Simulation.Identity.RuntimeIdCounter<Character>().Issue(),
            PermanentInjuryTarget.Martial, 15, "battlefield wound");
        var result = ApplyPermanentInjuryCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ApplyPermanentInjuryCommands.CharacterNotFound));
    }

    [Test]
    public void ValidationRejectsADeceasedCharacter()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, deathRecord: new DeathRecord(new GameDate(5), DeathCause.Disease, 20)));

        var command = new ApplyPermanentInjuryCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null, characterId,
            PermanentInjuryTarget.Martial, 15, "battlefield wound");
        var result = ApplyPermanentInjuryCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ApplyPermanentInjuryCommands.CharacterDeceased));
    }

    [TestCase(0)]
    [TestCase(101)]
    public void ValidationRejectsAnOutOfRangeMagnitude(int magnitude)
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));

        var command = new ApplyPermanentInjuryCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null, characterId,
            PermanentInjuryTarget.Martial, magnitude, "battlefield wound");
        var result = ApplyPermanentInjuryCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ApplyPermanentInjuryCommands.InvalidMagnitude));
    }

    [Test]
    public void ValidationRejectsAnEmptyCause()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));

        var command = new ApplyPermanentInjuryCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null, characterId,
            PermanentInjuryTarget.Martial, 15, "   ");
        var result = ApplyPermanentInjuryCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ApplyPermanentInjuryCommands.EmptyCause));
    }
}
