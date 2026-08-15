using Gens.Simulation.Characters;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class SetIndividualRegimenCommandTests
{
    private static readonly GameDate SubmittedDate = new(300);
    private static readonly RegimenSettings Comfortable = new(
        DietTier.Generous, AccommodationTier.Comfortable, FreedomsTier.FreeMovement, DisciplineTier.Lenient);

    [Test]
    public void ValidOverrideSetsRegimenAndEmitsAnEvent()
    {
        var state = new WorldState(SubmittedDate);
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, status: LegalStatus.Enslaved));

        var command = new SetIndividualRegimenCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, Comfortable);
        var result = SetIndividualRegimenCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        Assert.That(result.Events.Single(), Is.InstanceOf<IndividualRegimenSetEvent>());

        state.Characters.TryGet(characterId, out var character);
        Assert.That(character.Regimen, Is.EqualTo(Comfortable));
    }

    [Test]
    public void ANullRegimenClearsTheOverride()
    {
        var state = new WorldState(SubmittedDate);
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, status: LegalStatus.Enslaved, regimen: Comfortable));

        var command = new SetIndividualRegimenCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, null);
        var result = SetIndividualRegimenCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        state.Characters.TryGet(characterId, out var character);
        Assert.That(character.Regimen, Is.Null);
    }

    [Test]
    public void ValidationRejectsANonEnslavedCharacter()
    {
        var state = new WorldState(SubmittedDate);
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));

        var command = new SetIndividualRegimenCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, Comfortable);
        var result = SetIndividualRegimenCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(SetIndividualRegimenCommands.NotEnslaved));
    }

    [Test]
    public void ValidationRejectsADeceasedCharacter()
    {
        var state = new WorldState(SubmittedDate);
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, status: LegalStatus.Enslaved,
            deathRecord: new DeathRecord(new GameDate(280), DeathCause.Disease, 23)));

        var command = new SetIndividualRegimenCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, Comfortable);
        var result = SetIndividualRegimenCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(SetIndividualRegimenCommands.CharacterDeceased));
    }
}
