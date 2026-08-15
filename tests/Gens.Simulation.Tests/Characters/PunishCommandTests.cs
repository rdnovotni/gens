using Gens.Simulation.Characters;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class PunishCommandTests
{
    private static readonly GameDate SubmittedDate = new(300);

    [Test]
    public void MildPunishmentRaisesFatigueAndLowersLoyalty()
    {
        var state = new WorldState(SubmittedDate);
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, status: LegalStatus.Enslaved, condition: new Condition(80, 20, 50, 20, 50)));

        var command = new PunishCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, PunishmentTier.Mild);
        var result = PunishCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        state.Characters.TryGet(characterId, out var character);
        Assert.That(character.Condition.Fatigue, Is.EqualTo(30));
        Assert.That(character.Condition.Loyalty, Is.EqualTo(45));
        Assert.That(character.Condition.Health, Is.EqualTo(80));
    }

    [Test]
    public void ModeratePunishmentAlsoLowersHealth()
    {
        var state = new WorldState(SubmittedDate);
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, status: LegalStatus.Enslaved, condition: new Condition(80, 20, 50, 20, 50)));

        var command = new PunishCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, PunishmentTier.Moderate);
        PunishCommands.Pipeline.Execute(state, command);

        state.Characters.TryGet(characterId, out var character);
        Assert.That(character.Condition.Health, Is.EqualTo(70));
        Assert.That(character.Condition.Fatigue, Is.EqualTo(40));
        Assert.That(character.Condition.Loyalty, Is.EqualTo(35));
    }

    [Test]
    public void SeverePunishmentAppliesAPermanentInjuryToTheNamedTarget()
    {
        var state = new WorldState(SubmittedDate);
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, status: LegalStatus.Enslaved));

        var command = new PunishCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, PunishmentTier.Severe,
            PermanentInjuryTarget.Craft);
        PunishCommands.Pipeline.Execute(state, command);

        state.Characters.TryGet(characterId, out var character);
        var injury = character.PermanentInjuries.Single();
        Assert.That(injury.Target, Is.EqualTo(PermanentInjuryTarget.Craft));
        Assert.That(character.Condition.Health, Is.EqualTo(80), "Severe punishment must not touch Health directly (§3.1).");
    }

    [Test]
    public void LethalPunishmentKillsTheCharacter()
    {
        var state = new WorldState(SubmittedDate);
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, status: LegalStatus.Enslaved));

        var command = new PunishCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, PunishmentTier.Lethal);
        PunishCommands.Pipeline.Execute(state, command);

        state.Characters.TryGet(characterId, out var character);
        Assert.That(character.IsAlive, Is.False);
        Assert.That(character.DeathRecord!.Value.Cause, Is.EqualTo(DeathCause.Violence));
    }

    [Test]
    public void ValidationRejectsANonEnslavedCharacter()
    {
        var state = new WorldState(SubmittedDate);
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));

        var command = new PunishCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, PunishmentTier.Mild);
        var result = PunishCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(PunishCommands.NotEnslaved));
    }

    [Test]
    public void ValidationRejectsAnAlreadyDeceasedCharacter()
    {
        var state = new WorldState(SubmittedDate);
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, status: LegalStatus.Enslaved,
            deathRecord: new DeathRecord(new GameDate(280), DeathCause.Disease, 23)));

        var command = new PunishCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, PunishmentTier.Mild);
        var result = PunishCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(PunishCommands.CharacterDeceased));
    }
}
