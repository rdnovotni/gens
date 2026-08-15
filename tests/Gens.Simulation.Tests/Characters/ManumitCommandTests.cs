using Gens.Simulation.Characters;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class ManumitCommandTests
{
    private static readonly GameDate SubmittedDate = new(300);

    [Test]
    public void VindictaGrantsFreedomAndThePatronsNomen()
    {
        var state = new WorldState(SubmittedDate);
        var householdId = state.HouseholdIds.Issue();
        var patronusId = state.CharacterIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(patronusId, CharacterTestFixtures.Minimal(patronusId, nomen: "Julius"));
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, status: LegalStatus.Enslaved,
            duty: new DutyAssignment(householdId, DutySlot.Cook, new GameDate(200)),
            regimen: new RegimenSettings(DietTier.Meager, AccommodationTier.Bare, FreedomsTier.Confined, DisciplineTier.Harsh)));

        var command = new ManumitCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, patronusId, ManumissionType.Vindicta);
        var result = ManumitCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        state.Characters.TryGet(characterId, out var freed);
        Assert.That(freed.LegalStatus, Is.EqualTo(LegalStatus.Freedman));
        Assert.That(freed.Nomen, Is.EqualTo("Julius"));
        Assert.That(freed.Regimen, Is.Null);
        // Labor continuity (§8): Duty is left untouched by manumission.
        Assert.That(freed.Duty, Is.Not.Null);
    }

    [Test]
    public void ValidationRejectsTestamentoAsAnImmediateMethod()
    {
        var state = new WorldState(SubmittedDate);
        var patronusId = state.CharacterIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(patronusId, CharacterTestFixtures.Minimal(patronusId));
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, status: LegalStatus.Enslaved));

        var command = new ManumitCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, patronusId, ManumissionType.Testamento);
        var result = ManumitCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ManumitCommands.InvalidMethod));
    }

    [Test]
    public void ValidationRejectsANonEnslavedCharacter()
    {
        var state = new WorldState(SubmittedDate);
        var patronusId = state.CharacterIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(patronusId, CharacterTestFixtures.Minimal(patronusId));
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));

        var command = new ManumitCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, patronusId, ManumissionType.Vindicta);
        var result = ManumitCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ManumitCommands.NotEnslaved));
    }

    [Test]
    public void ValidationRejectsADeceasedPatronus()
    {
        var state = new WorldState(SubmittedDate);
        var patronusId = state.CharacterIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(patronusId, CharacterTestFixtures.Minimal(
            patronusId, deathRecord: new DeathRecord(new GameDate(280), DeathCause.Disease, 40)));
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, status: LegalStatus.Enslaved));

        var command = new ManumitCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, patronusId, ManumissionType.Vindicta);
        var result = ManumitCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ManumitCommands.PatronusDeceased));
    }
}
