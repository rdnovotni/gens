using Gens.Simulation.Characters;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class InitiatePursuitCommandTests
{
    private static readonly GameDate SubmittedDate = new(300);

    [Test]
    public void ValidInitiationSetsAnActivePursuit()
    {
        var state = new WorldState(SubmittedDate);
        var householdId = state.HouseholdIds.Issue();
        var characterId = state.CharacterIds.Issue();
        var pursuerId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, status: LegalStatus.Enslaved,
            flight: new FledRecord(new GameDate(299), householdId, null)));

        var command = new InitiatePursuitCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, pursuerId);
        var result = InitiatePursuitCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        Assert.That(result.Events.Single(), Is.InstanceOf<PursuitInitiatedEvent>());

        state.Characters.TryGet(characterId, out var character);
        Assert.That(character.Pursuit, Is.EqualTo(new PursuitRecord(LaborFlightSystem.PursuitDurationMonths, pursuerId)));
    }

    [Test]
    public void ValidationRejectsACharacterWhoNeverFled()
    {
        var state = new WorldState(SubmittedDate);
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, status: LegalStatus.Enslaved));

        var command = new InitiatePursuitCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId);
        var result = InitiatePursuitCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(InitiatePursuitCommands.NotFled));
    }

    [Test]
    public void ValidationRejectsAPursuitAlreadyUnderway()
    {
        var state = new WorldState(SubmittedDate);
        var householdId = state.HouseholdIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, status: LegalStatus.Enslaved,
            flight: new FledRecord(new GameDate(299), householdId, null),
            pursuit: new PursuitRecord(2, null)));

        var command = new InitiatePursuitCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId);
        var result = InitiatePursuitCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(InitiatePursuitCommands.AlreadyPursuing));
    }

    [Test]
    public void ValidationRejectsInitiationAfterTheTrailHasGoneCold()
    {
        var state = new WorldState(SubmittedDate);
        var householdId = state.HouseholdIds.Issue();
        var characterId = state.CharacterIds.Issue();
        var longAgo = new GameDate(SubmittedDate.TotalMonths - InitiatePursuitCommands.PursuitWindowMonths - 1);
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, status: LegalStatus.Enslaved,
            flight: new FledRecord(longAgo, householdId, null)));

        var command = new InitiatePursuitCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId);
        var result = InitiatePursuitCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(InitiatePursuitCommands.TrailHasGoneCold));
    }
}
