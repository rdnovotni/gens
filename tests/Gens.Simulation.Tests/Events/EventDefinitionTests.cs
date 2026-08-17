using Gens.Simulation.Commands;
using Gens.Simulation.Events;
using Gens.Simulation.Identity;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Events;

public sealed class EventDefinitionTests
{
    private static readonly DefinitionId<EventDefinition> DefId = new("test-def");
    private static readonly DefinitionId<EventOptionDefinition> OptionId = new("test-option");

    [Test]
    public void RandomTriggerWithoutAWeightFunctionThrows()
    {
        Assert.Throws<ArgumentException>(() => new EventDefinition(
            DefId, EventScope.Personal, "n", "d", EventTriggerKind.Random,
            eligibility: static (_, _) => true,
            stages: new[] { OneOptionStage() },
            weight: null));
    }

    [Test]
    public void ScriptedTriggerWithoutAConditionThrows()
    {
        Assert.Throws<ArgumentException>(() => new EventDefinition(
            DefId, EventScope.Personal, "n", "d", EventTriggerKind.Scripted,
            eligibility: static (_, _) => true,
            stages: new[] { OneOptionStage() },
            scriptedCondition: null));
    }

    [Test]
    public void NonSequentialStageIndicesThrow()
    {
        var stage0 = OneOptionStage(stageIndex: 0);
        var stage2 = OneOptionStage(stageIndex: 2);

        Assert.Throws<ArgumentException>(() => new EventDefinition(
            DefId, EventScope.Personal, "n", "d", EventTriggerKind.Random,
            eligibility: static (_, _) => true,
            stages: new[] { stage0, stage2 },
            weight: static (_, _) => 1));
    }

    [Test]
    public void EmptyStagesListThrows()
    {
        Assert.Throws<ArgumentException>(() => new EventDefinition(
            DefId, EventScope.Personal, "n", "d", EventTriggerKind.Random,
            eligibility: static (_, _) => true,
            stages: Array.Empty<EventStageDefinition>(),
            weight: static (_, _) => 1));
    }

    [Test]
    public void ZeroCadenceMonthsThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new EventDefinition(
            DefId, EventScope.Personal, "n", "d", EventTriggerKind.Random,
            eligibility: static (_, _) => true,
            stages: new[] { OneOptionStage() },
            weight: static (_, _) => 1,
            cadenceMonths: 0));
    }

    [Test]
    public void TryGetStageReturnsFalseOutOfRange()
    {
        var definition = new EventDefinition(
            DefId, EventScope.Personal, "n", "d", EventTriggerKind.Random,
            eligibility: static (_, _) => true,
            stages: new[] { OneOptionStage() },
            weight: static (_, _) => 1);

        Assert.That(definition.TryGetStage(5, out _), Is.False);
        Assert.That(definition.TryGetStage(0, out _), Is.True);
    }

    [Test]
    public void StageWithDuplicateOptionIdsThrows()
    {
        var option = new EventOptionDefinition(
            OptionId, "n", "d",
            eligibility: static (_, _) => null,
            scoreForAi: static (_, _) => 1.0,
            resolve: static (_, _, _, _) => Array.Empty<IDomainEvent>());

        Assert.Throws<ArgumentException>(() => new EventStageDefinition(
            0, "n", "d", new[] { option, option }, expiryMonths: 1));
    }

    private static EventStageDefinition OneOptionStage(int stageIndex = 0)
    {
        var option = new EventOptionDefinition(
            OptionId, "n", "d",
            eligibility: static (_, _) => null,
            scoreForAi: static (_, _) => 1.0,
            resolve: static (_, _, _, _) => Array.Empty<IDomainEvent>());
        return new EventStageDefinition(stageIndex, "n", "d", new[] { option }, expiryMonths: 1);
    }
}
