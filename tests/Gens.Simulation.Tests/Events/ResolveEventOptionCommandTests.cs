using Gens.Simulation.Commands;
using Gens.Simulation.Events;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Events;

public sealed class ResolveEventOptionCommandTests
{
    private static readonly DefinitionId<EventDefinition> SingleStageDefId = new("resolve-test-single");
    private static readonly DefinitionId<EventDefinition> TwoStageDefId = new("resolve-test-two-stage");
    private static readonly DefinitionId<EventOptionDefinition> PickMeOption = new("pick-me");
    private static readonly DefinitionId<EventOptionDefinition> IneligibleOption = new("ineligible");
    private static readonly DefinitionId<EventOptionDefinition> Stage2Option = new("stage-two-option");

    [Test]
    public void ResolvingASingleStageDefinitionMarksTheInstanceResolved()
    {
        var (state, catalog, instanceId) = FireSingleStageInstance();
        var pipeline = ResolveEventOptionCommands.BuildPipeline(catalog);

        var result = pipeline.Execute(state, new ResolveEventOptionCommand(
            state.CommandIds.Issue(), "char_0000001", new GameDate(0), null, instanceId, PickMeOption));

        Assert.That(result.Accepted, Is.True);
        var announcement = result.Events.OfType<EventOptionResolvedEvent>().Single();
        Assert.That(announcement.OptionId, Is.EqualTo(PickMeOption));

        state.EventInstances.TryGet(instanceId, out var instance);
        Assert.That(instance!.Status, Is.EqualTo(EventInstanceStatus.Resolved));
        Assert.That(instance.ResolvedOptionId, Is.EqualTo(PickMeOption));
        Assert.That(instance.ResolvingEventId, Is.EqualTo(announcement.EventId.ToTaggedString()));
    }

    [Test]
    public void ResolvingAnEarlierStageOfAMultiStageDefinitionQueuesTheNextStage()
    {
        var state = new WorldState(new GameDate(0));
        var catalog = new EventCatalog(new[] { MakeTwoStageDefinition() });
        var firePipeline = FireEventCommands.BuildPipeline(catalog);
        var fireResult = firePipeline.Execute(state, new FireEventCommand(
            state.CommandIds.Issue(), "system", new GameDate(0), null, TwoStageDefId, new[] { "char_0000001" }, "char_0000001"));
        var instanceId = ((EventFiredEvent)fireResult.Events.Single()).InstanceId;

        var resolvePipeline = ResolveEventOptionCommands.BuildPipeline(catalog);
        var result = resolvePipeline.Execute(state, new ResolveEventOptionCommand(
            state.CommandIds.Issue(), "char_0000001", new GameDate(0), null, instanceId, PickMeOption));

        Assert.That(result.Accepted, Is.True);
        state.EventInstances.TryGet(instanceId, out var instance);
        Assert.That(instance!.Status, Is.EqualTo(EventInstanceStatus.AwaitingNextStage));
        Assert.That(instance.CurrentStageIndex, Is.EqualTo(0));
        Assert.That(instance.ExpiresDate, Is.EqualTo(new GameDate(2)));
    }

    [Test]
    public void UnknownInstanceIsRejected()
    {
        var state = new WorldState(new GameDate(0));
        var catalog = new EventCatalog(new[] { MakeSingleStageDefinition() });
        var pipeline = ResolveEventOptionCommands.BuildPipeline(catalog);

        var result = pipeline.Execute(state, new ResolveEventOptionCommand(
            state.CommandIds.Issue(), "char_0000001", new GameDate(0), null, state.EventInstanceIds.Issue(), PickMeOption));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ResolveEventOptionCommands.InstanceNotFound));
    }

    [Test]
    public void AnExpiredInstanceIsRejected()
    {
        var (state, catalog, instanceId) = FireSingleStageInstance(expiryMonths: 1);
        var pipeline = ResolveEventOptionCommands.BuildPipeline(catalog);

        var result = pipeline.Execute(state, new ResolveEventOptionCommand(
            state.CommandIds.Issue(), "char_0000001", new GameDate(5), null, instanceId, PickMeOption));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ResolveEventOptionCommands.InstanceExpired));
    }

    [Test]
    public void AnUnknownOptionIsRejected()
    {
        var (state, catalog, instanceId) = FireSingleStageInstance();
        var pipeline = ResolveEventOptionCommands.BuildPipeline(catalog);

        var result = pipeline.Execute(state, new ResolveEventOptionCommand(
            state.CommandIds.Issue(), "char_0000001", new GameDate(0), null, instanceId, new DefinitionId<EventOptionDefinition>("no-such-option")));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ResolveEventOptionCommands.OptionNotFound));
    }

    [Test]
    public void AnIneligibleOptionIsRejectedWithItsOwnErrorCode()
    {
        var ineligibleError = new ValidationErrorCode("test.optionIneligible");
        var eligibleOption = new EventOptionDefinition(
            PickMeOption, "n", "d",
            eligibility: static (_, _) => null,
            scoreForAi: static (_, _) => 1.0,
            resolve: static (_, _, _, _) => Array.Empty<IDomainEvent>());
        var ineligibleOption = new EventOptionDefinition(
            IneligibleOption, "n", "d",
            eligibility: (_, _) => ineligibleError,
            scoreForAi: static (_, _) => 1.0,
            resolve: static (_, _, _, _) => Array.Empty<IDomainEvent>());
        var stage = new EventStageDefinition(0, "n", "d", new[] { eligibleOption, ineligibleOption }, expiryMonths: 3);
        var definition = new EventDefinition(
            SingleStageDefId, EventScope.Personal, "n", "d", EventTriggerKind.Random,
            eligibility: static (_, _) => true,
            stages: new[] { stage },
            weight: static (_, _) => 1);

        var state = new WorldState(new GameDate(0));
        var catalog = new EventCatalog(new[] { definition });
        var firePipeline = FireEventCommands.BuildPipeline(catalog);
        var fireResult = firePipeline.Execute(state, new FireEventCommand(
            state.CommandIds.Issue(), "system", new GameDate(0), null, SingleStageDefId, new[] { "char_0000001" }, "char_0000001"));
        var instanceId = ((EventFiredEvent)fireResult.Events.Single()).InstanceId;

        var resolvePipeline = ResolveEventOptionCommands.BuildPipeline(catalog);
        var result = resolvePipeline.Execute(state, new ResolveEventOptionCommand(
            state.CommandIds.Issue(), "char_0000001", new GameDate(0), null, instanceId, IneligibleOption));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ineligibleError));
    }

    private static (WorldState State, EventCatalog Catalog, RuntimeId<EventInstance> InstanceId) FireSingleStageInstance(int expiryMonths = 3)
    {
        var state = new WorldState(new GameDate(0));
        var catalog = new EventCatalog(new[] { MakeSingleStageDefinition(expiryMonths) });
        var firePipeline = FireEventCommands.BuildPipeline(catalog);
        var fireResult = firePipeline.Execute(state, new FireEventCommand(
            state.CommandIds.Issue(), "system", new GameDate(0), null, SingleStageDefId, new[] { "char_0000001" }, "char_0000001"));
        var instanceId = ((EventFiredEvent)fireResult.Events.Single()).InstanceId;
        return (state, catalog, instanceId);
    }

    private static EventDefinition MakeSingleStageDefinition(int expiryMonths = 3)
    {
        var option = new EventOptionDefinition(
            PickMeOption, "n", "d",
            eligibility: static (_, _) => null,
            scoreForAi: static (_, _) => 1.0,
            resolve: static (_, _, _, _) => Array.Empty<IDomainEvent>());
        var stage = new EventStageDefinition(0, "n", "d", new[] { option }, expiryMonths);
        return new EventDefinition(
            SingleStageDefId, EventScope.Personal, "n", "d", EventTriggerKind.Random,
            eligibility: static (_, _) => true,
            stages: new[] { stage },
            weight: static (_, _) => 1);
    }

    private static EventDefinition MakeTwoStageDefinition()
    {
        var stage0Option = new EventOptionDefinition(
            PickMeOption, "n", "d",
            eligibility: static (_, _) => null,
            scoreForAi: static (_, _) => 1.0,
            resolve: static (_, _, _, _) => Array.Empty<IDomainEvent>());
        var stage1Option = new EventOptionDefinition(
            Stage2Option, "n", "d",
            eligibility: static (_, _) => null,
            scoreForAi: static (_, _) => 1.0,
            resolve: static (_, _, _, _) => Array.Empty<IDomainEvent>());
        var stage0 = new EventStageDefinition(0, "n", "d", new[] { stage0Option }, expiryMonths: 3, nextStageDelayMonths: 2);
        var stage1 = new EventStageDefinition(1, "n", "d", new[] { stage1Option }, expiryMonths: 3);
        return new EventDefinition(
            TwoStageDefId, EventScope.Household, "n", "d", EventTriggerKind.Random,
            eligibility: static (_, _) => true,
            stages: new[] { stage0, stage1 },
            weight: static (_, _) => 1);
    }
}
