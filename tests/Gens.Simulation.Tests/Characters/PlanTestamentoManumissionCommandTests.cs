using Gens.Simulation.Characters;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class PlanTestamentoManumissionCommandTests
{
    private static readonly GameDate SubmittedDate = new(300);

    [Test]
    public void ValidPlanSetsAPendingTestamentoGrant()
    {
        var state = new WorldState(SubmittedDate);
        var grantorId = state.CharacterIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(grantorId, CharacterTestFixtures.Minimal(grantorId));
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, status: LegalStatus.Enslaved));

        var command = new PlanTestamentoManumissionCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, grantorId);
        var result = PlanTestamentoManumissionCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        state.Characters.TryGet(characterId, out var character);
        Assert.That(character.ManumissionPlan, Is.EqualTo(new ManumissionPlan(grantorId, ManumissionType.Testamento)));
        Assert.That(character.LegalStatus, Is.EqualTo(LegalStatus.Enslaved), "Testamento does not take effect immediately.");
    }

    [Test]
    public void ValidationRejectsAPlanAlreadyInPlace()
    {
        var state = new WorldState(SubmittedDate);
        var grantorId = state.CharacterIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(grantorId, CharacterTestFixtures.Minimal(grantorId));
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, status: LegalStatus.Enslaved,
            manumissionPlan: new ManumissionPlan(grantorId, ManumissionType.Testamento)));

        var command = new PlanTestamentoManumissionCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, grantorId);
        var result = PlanTestamentoManumissionCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(PlanTestamentoManumissionCommands.AlreadyPlanned));
    }

    [Test]
    public void ValidationRejectsADeceasedGrantor()
    {
        var state = new WorldState(SubmittedDate);
        var grantorId = state.CharacterIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(grantorId, CharacterTestFixtures.Minimal(
            grantorId, deathRecord: new DeathRecord(new GameDate(280), DeathCause.Disease, 40)));
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, status: LegalStatus.Enslaved));

        var command = new PlanTestamentoManumissionCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, characterId, grantorId);
        var result = PlanTestamentoManumissionCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(PlanTestamentoManumissionCommands.GrantorDeceased));
    }
}
