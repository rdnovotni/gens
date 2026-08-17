using Gens.Simulation.Commands;
using Gens.Simulation.Events;
using Gens.Simulation.Identity;
using Gens.Simulation.Queries;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Queries;

public sealed class EventInstanceDetailQueryTests
{
    private static readonly DefinitionId<EventDefinition> DefId = new("detail-test-def");
    private static readonly DefinitionId<EventOptionDefinition> PickedOption = new("picked-option");
    private static readonly DefinitionId<EventOptionDefinition> OtherOption = new("other-option");

    [Test]
    public void ProjectsThePendingInstancesOfferedOptionsWithNoneMarkedResolved()
    {
        var (state, catalog, instanceId) = FireInstance();

        var projection = new EventInstanceDetailQuery(instanceId, catalog).Execute(state, "player");

        Assert.That(projection.Status, Is.EqualTo(EventInstanceStatus.Pending));
        Assert.That(projection.Options.Select(o => o.OptionId), Is.EquivalentTo(new[] { "picked-option", "other-option" }));
        Assert.That(projection.Options.All(o => !o.IsResolvedOption), Is.True);
        Assert.That(projection.ResolvedOptionId, Is.Null);
    }

    [Test]
    public void ProjectsWhichOptionResolvedTheInstanceAfterItIsPicked()
    {
        var (state, catalog, instanceId) = FireInstance();
        var resolvePipeline = ResolveEventOptionCommands.BuildPipeline(catalog);
        resolvePipeline.Execute(state, new ResolveEventOptionCommand(
            state.CommandIds.Issue(), "char_0000001", new GameDate(0), null, instanceId, PickedOption));

        var projection = new EventInstanceDetailQuery(instanceId, catalog).Execute(state, "player");

        Assert.That(projection.Status, Is.EqualTo(EventInstanceStatus.Resolved));
        Assert.That(projection.ResolvedOptionId, Is.EqualTo("picked-option"));
        var picked = projection.Options.Single(o => o.OptionId == "picked-option");
        Assert.That(picked.IsResolvedOption, Is.True);
        var other = projection.Options.Single(o => o.OptionId == "other-option");
        Assert.That(other.IsResolvedOption, Is.False);
        Assert.That(projection.ResolvingEventId, Is.Not.Null);
    }

    [Test]
    public void UnknownInstanceThrows()
    {
        var state = new WorldState(new GameDate(0));
        var catalog = new EventCatalog(new[] { MakeDefinition() });

        Assert.Throws<KeyNotFoundException>(
            () => new EventInstanceDetailQuery(state.EventInstanceIds.Issue(), catalog).Execute(state, "player"));
    }

    private static (WorldState State, EventCatalog Catalog, RuntimeId<EventInstance> InstanceId) FireInstance()
    {
        var state = new WorldState(new GameDate(0));
        var catalog = new EventCatalog(new[] { MakeDefinition() });
        var firePipeline = FireEventCommands.BuildPipeline(catalog);
        var result = firePipeline.Execute(state, new FireEventCommand(
            state.CommandIds.Issue(), "system", new GameDate(0), null, DefId, new[] { "char_0000001" }, "char_0000001"));
        var instanceId = ((EventFiredEvent)result.Events.Single()).InstanceId;
        return (state, catalog, instanceId);
    }

    private static EventDefinition MakeDefinition()
    {
        var picked = new EventOptionDefinition(
            PickedOption, "n", "d",
            eligibility: static (_, _) => null,
            scoreForAi: static (_, _) => 1.0,
            resolve: static (_, _, _, _) => Array.Empty<IDomainEvent>());
        var other = new EventOptionDefinition(
            OtherOption, "n", "d",
            eligibility: static (_, _) => null,
            scoreForAi: static (_, _) => 1.0,
            resolve: static (_, _, _, _) => Array.Empty<IDomainEvent>());
        var stage = new EventStageDefinition(0, "n", "d", new[] { picked, other }, expiryMonths: 3);
        return new EventDefinition(
            DefId, EventScope.Personal, "n", "d", EventTriggerKind.Random,
            eligibility: static (_, _) => true,
            stages: new[] { stage },
            weight: static (_, _) => 1);
    }
}
