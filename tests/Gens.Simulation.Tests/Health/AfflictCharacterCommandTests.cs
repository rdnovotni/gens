using Gens.Simulation.Characters;
using Gens.Simulation.Health;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Tests.Characters;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Health;

public sealed class AfflictCharacterCommandTests
{
    private static readonly DefinitionId<HealthConditionDefinition> TestFever = new("test-fever");

    private static readonly HealthConditionCatalog Catalog = new(new[]
    {
        new HealthConditionDefinition(TestFever, "Test Fever", HealthConditionCategory.Acute, hasCure: false),
    });

    [Test]
    public void ValidAfflictionIsRecordedAndEmitsAnEvent()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));

        var command = new AfflictCharacterCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null, characterId, TestFever, 40);
        var result = AfflictCharacterCommands.BuildPipeline(Catalog).Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        var applied = (CharacterAfflictedEvent)result.Events.Single();
        Assert.That(applied.Severity, Is.EqualTo(40));
        Assert.That(applied.Category, Is.EqualTo(HealthConditionCategory.Acute));

        var condition = state.CharacterHealthConditions.InAscendingOrder().Single().Value;
        Assert.That(condition.CharacterId, Is.EqualTo(characterId));
        Assert.That(condition.ConditionId, Is.EqualTo(TestFever));
        Assert.That(condition.Category, Is.EqualTo(HealthConditionCategory.Acute));
        Assert.That(condition.HasCure, Is.False);
        Assert.That(condition.Status, Is.EqualTo(CharacterHealthConditionStatus.Active));
    }

    [Test]
    public void ValidationRejectsAnUnknownCondition()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));

        var command = new AfflictCharacterCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null, characterId,
            new DefinitionId<HealthConditionDefinition>("unregistered"), 40);
        var result = AfflictCharacterCommands.BuildPipeline(Catalog).Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(AfflictCharacterCommands.UnknownCondition));
    }

    [Test]
    public void ValidationRejectsAMissingCharacter()
    {
        var state = new WorldState(new GameDate(10));

        var command = new AfflictCharacterCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null,
            new RuntimeIdCounter<Character>().Issue(), TestFever, 40);
        var result = AfflictCharacterCommands.BuildPipeline(Catalog).Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(AfflictCharacterCommands.CharacterNotFound));
    }

    [Test]
    public void ValidationRejectsADeceasedCharacter()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, deathRecord: new DeathRecord(new GameDate(5), DeathCause.OldAge, 70)));

        var command = new AfflictCharacterCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null, characterId, TestFever, 40);
        var result = AfflictCharacterCommands.BuildPipeline(Catalog).Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(AfflictCharacterCommands.CharacterDeceased));
    }

    [TestCase(0)]
    [TestCase(101)]
    public void ValidationRejectsAnOutOfRangeSeverity(int severity)
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));

        var command = new AfflictCharacterCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null, characterId, TestFever, severity);
        var result = AfflictCharacterCommands.BuildPipeline(Catalog).Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(AfflictCharacterCommands.InvalidSeverity));
    }

    [Test]
    public void ValidationRejectsADuplicateActiveCondition()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        var pipeline = AfflictCharacterCommands.BuildPipeline(Catalog);
        pipeline.Execute(state, new AfflictCharacterCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null, characterId, TestFever, 40));

        var result = pipeline.Execute(state, new AfflictCharacterCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null, characterId, TestFever, 60));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(AfflictCharacterCommands.AlreadyActive));
    }

    [Test]
    public void ValidationRejectsAnImmuneCharacter()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        var priorCaseId = state.CharacterHealthConditionIds.Issue();
        state.CharacterHealthConditions.Add(priorCaseId, new CharacterHealthCondition
        {
            Id = priorCaseId,
            CharacterId = characterId,
            ConditionId = TestFever,
            Category = HealthConditionCategory.Acute,
            HasCure = false,
            Severity = 50,
            OnsetDate = new GameDate(1),
            Status = CharacterHealthConditionStatus.Recovered,
            GrantedImmunity = true,
            ResolvedDate = new GameDate(3),
        });

        var result = AfflictCharacterCommands.BuildPipeline(Catalog).Execute(state, new AfflictCharacterCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null, characterId, TestFever, 40));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(AfflictCharacterCommands.CharacterImmune));
    }
}
