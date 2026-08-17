using Gens.Simulation.Commands;
using Gens.Simulation.Events;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Events;

public sealed class FireEventCommandTests
{
    private static readonly DefinitionId<EventDefinition> DefId = new("fire-test-def");
    private static readonly DefinitionId<EventOptionDefinition> OptionId = new("fire-test-option");

    [Test]
    public void AcceptedCommandCreatesAPendingInstanceAndEmitsEventFiredEvent()
    {
        var state = new WorldState(new GameDate(10));
        var catalog = new EventCatalog(new[] { MakeDefinition(EventScope.Personal, expiryMonths: 3) });
        var pipeline = FireEventCommands.BuildPipeline(catalog);

        var result = pipeline.Execute(state, new FireEventCommand(
            state.CommandIds.Issue(), "system", new GameDate(10), null, DefId, new[] { "char_0000001" }, "char_0000001"));

        Assert.That(result.Accepted, Is.True);
        var fired = (EventFiredEvent)result.Events.Single();
        Assert.That(fired.Scope, Is.EqualTo(EventScope.Personal));
        Assert.That(fired.Visibility.Channel, Is.EqualTo("witnessed"));

        Assert.That(state.EventInstances.TryGet(fired.InstanceId, out var instance), Is.True);
        Assert.That(instance!.Status, Is.EqualTo(EventInstanceStatus.Pending));
        Assert.That(instance.ExpiresDate, Is.EqualTo(new GameDate(13)));
        Assert.That(instance.CurrentStageIndex, Is.EqualTo(0));
    }

    [Test]
    public void UnknownDefinitionIsRejected()
    {
        var state = new WorldState(new GameDate(0));
        var catalog = new EventCatalog(Array.Empty<EventDefinition>());
        var pipeline = FireEventCommands.BuildPipeline(catalog);

        var result = pipeline.Execute(state, new FireEventCommand(
            state.CommandIds.Issue(), "system", new GameDate(0), null, DefId, new[] { "char_0000001" }, "char_0000001"));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(FireEventCommands.DefinitionNotFound));
    }

    [Test]
    public void EmptySubjectListIsRejected()
    {
        var state = new WorldState(new GameDate(0));
        var catalog = new EventCatalog(new[] { MakeDefinition(EventScope.Personal, expiryMonths: 3) });
        var pipeline = FireEventCommands.BuildPipeline(catalog);

        var result = pipeline.Execute(state, new FireEventCommand(
            state.CommandIds.Issue(), "system", new GameDate(0), null, DefId, Array.Empty<string>(), "char_0000001"));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(FireEventCommands.SubjectsRequired));
    }

    [Test]
    public void FiringTheSameDefinitionAgainForTheSameSubjectWhileStillPendingIsRejected()
    {
        var state = new WorldState(new GameDate(0));
        var catalog = new EventCatalog(new[] { MakeDefinition(EventScope.Personal, expiryMonths: 3) });
        var pipeline = FireEventCommands.BuildPipeline(catalog);
        var subjectIds = new[] { "char_0000001" };

        var first = pipeline.Execute(state, new FireEventCommand(
            state.CommandIds.Issue(), "system", new GameDate(0), null, DefId, subjectIds, "char_0000001"));
        Assert.That(first.Accepted, Is.True);

        var second = pipeline.Execute(state, new FireEventCommand(
            state.CommandIds.Issue(), "system", new GameDate(0), null, DefId, subjectIds, "char_0000001"));

        Assert.That(second.Accepted, Is.False);
        Assert.That(second.Error, Is.EqualTo(FireEventCommands.AlreadyActive));
    }

    private static EventDefinition MakeDefinition(EventScope scope, int expiryMonths)
    {
        var option = new EventOptionDefinition(
            OptionId, "n", "d",
            eligibility: static (_, _) => null,
            scoreForAi: static (_, _) => 1.0,
            resolve: static (_, _, _, _) => Array.Empty<IDomainEvent>());
        var stage = new EventStageDefinition(0, "n", "d", new[] { option }, expiryMonths);
        return new EventDefinition(
            DefId, scope, "n", "d", EventTriggerKind.Random,
            eligibility: static (_, _) => true,
            stages: new[] { stage },
            weight: static (_, _) => 1);
    }
}
