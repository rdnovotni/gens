using Gens.Simulation.Characters;
using Gens.Simulation.Health;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Tests.Characters;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Health;

public sealed class SetPersonalQuarantineCommandTests
{
    private static readonly DefinitionId<HealthConditionDefinition> TestFever = new("test-fever");

    [Test]
    public void QuarantiningAnActiveCaseSetsTheFlagAndEmitsAnEvent()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        var caseId = state.CharacterHealthConditionIds.Issue();
        state.CharacterHealthConditions.Add(caseId, CharacterHealthCondition.Create(
            caseId, characterId, TestFever, HealthConditionCategory.Acute, hasCure: false, severity: 40, new GameDate(9)));

        var command = new SetPersonalQuarantineCommand(state.CommandIds.Issue(), "player", new GameDate(10), null, caseId, true);
        var result = SetPersonalQuarantineCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        state.CharacterHealthConditions.TryGet(caseId, out var updated);
        Assert.That(updated.Quarantined, Is.True);
        var applied = (CharacterQuarantineChangedEvent)result.Events.Single();
        Assert.That(applied.Quarantined, Is.True);
        Assert.That(applied.CharacterId, Is.EqualTo(characterId));
    }

    [Test]
    public void LiftingQuarantineClearsTheFlag()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        var caseId = state.CharacterHealthConditionIds.Issue();
        state.CharacterHealthConditions.Add(caseId, CharacterHealthCondition.Create(
            caseId, characterId, TestFever, HealthConditionCategory.Acute, hasCure: false, severity: 40, new GameDate(9)) with
        { Quarantined = true });

        SetPersonalQuarantineCommands.Pipeline.Execute(
            state, new SetPersonalQuarantineCommand(state.CommandIds.Issue(), "player", new GameDate(10), null, caseId, false));

        state.CharacterHealthConditions.TryGet(caseId, out var updated);
        Assert.That(updated.Quarantined, Is.False);
    }

    [Test]
    public void ValidationRejectsAMissingCase()
    {
        var state = new WorldState(new GameDate(10));
        var command = new SetPersonalQuarantineCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null,
            new RuntimeIdCounter<CharacterHealthCondition>().Issue(), true);
        var result = SetPersonalQuarantineCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(SetPersonalQuarantineCommands.CaseNotFound));
    }

    [Test]
    public void ValidationRejectsAResolvedCase()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        var caseId = state.CharacterHealthConditionIds.Issue();
        var resolved = CharacterHealthCondition.Create(
            caseId, characterId, TestFever, HealthConditionCategory.Acute, hasCure: false, severity: 40, new GameDate(9))
            with
        { Status = CharacterHealthConditionStatus.Recovered, ResolvedDate = new GameDate(10) };
        state.CharacterHealthConditions.Add(caseId, resolved);

        var command = new SetPersonalQuarantineCommand(state.CommandIds.Issue(), "player", new GameDate(10), null, caseId, true);
        var result = SetPersonalQuarantineCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(SetPersonalQuarantineCommands.CaseNotActive));
    }
}
