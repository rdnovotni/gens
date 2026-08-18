using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Events;

/// <summary>Picks one option for a fired <see cref="EventInstance"/>'s current stage (Phase 9 item 3;
/// §3-4, §8). The one place an option's own consequences ever reach <see cref="WorldState"/> — <see
/// cref="EventOptionDefinition.Resolve"/> only ever runs from inside this command's own <see
/// cref="ResolveEventOptionCommands.BuildPipeline"/>-built <see
/// cref="CommandPipeline{TState,TCommand}"/> (ADR 0006), matching every other consequential mutation
/// in this codebase (rule 2). Submitted identically whether the picker is a human player (a future UI)
/// or <see cref="EventPoolSystem"/>'s own AI/NPC auto-resolution — there is no second, bespoke
/// AI-only mutation path.</summary>
public sealed record ResolveEventOptionCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<EventInstance> InstanceId,
    DefinitionId<EventOptionDefinition> OptionId) : ICommand;

/// <summary>Emitted whenever a <see cref="ResolveEventOptionCommand"/> is accepted, announcing which
/// option was picked — distinct from whatever domain event(s) <see
/// cref="EventOptionDefinition.Resolve"/> itself produced, which describe the option's own concrete
/// consequence.</summary>
public sealed record EventOptionResolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<EventInstance> InstanceId,
    DefinitionId<EventDefinition> DefinitionId,
    EventScope Scope,
    DefinitionId<EventOptionDefinition> OptionId,
    IReadOnlyList<string> SubjectIds,
    string? CausationId) : IDomainEvent, IScopedEvent, IEventInstanceLinkedEvent
{
    public string Type => "events.optionResolved";
    public int SchemaVersion => 1;
    public Visibility Visibility => EventVisibilityRules.For(Scope, SubjectIds);
}

/// <summary>The validate/mutate pipeline for <see cref="ResolveEventOptionCommand"/> (ADR 0006),
/// built per-<see cref="EventCatalog"/> like <see cref="FireEventCommands.BuildPipeline"/> — see that
/// method's own doc comment for why a static field shape does not fit here.</summary>
public static class ResolveEventOptionCommands
{
    public static readonly ValidationErrorCode InstanceNotFound = new("events.resolveOption.instanceNotFound");
    public static readonly ValidationErrorCode InstanceNotPending = new("events.resolveOption.instanceNotPending");
    public static readonly ValidationErrorCode InstanceExpired = new("events.resolveOption.instanceExpired");
    public static readonly ValidationErrorCode DefinitionNotFound = new("events.resolveOption.definitionNotFound");
    public static readonly ValidationErrorCode OptionNotFound = new("events.resolveOption.optionNotFound");

    public static CommandPipeline<WorldState, ResolveEventOptionCommand> BuildPipeline(EventCatalog catalog)
    {
        if (catalog is null)
            throw new ArgumentNullException(nameof(catalog));

        return new CommandPipeline<WorldState, ResolveEventOptionCommand>(
            validate: (state, command) => Validate(state, command, catalog),
            mutate: (state, command) => Mutate(state, command, catalog),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, ResolveEventOptionCommand command, EventCatalog catalog)
    {
        if (!TryResolveOption(state, command, catalog, out var instance, out var option, out var error))
            return error;

        var invocation = new EventInvocation(instance.ActorId, instance.SubjectIds, command.SubmittedDate);
        return option.Eligibility(state, invocation);
    }

    private static IDomainEvent[] Mutate(WorldState state, ResolveEventOptionCommand command, EventCatalog catalog)
    {
        if (!TryResolveOption(state, command, catalog, out var instance, out var option, out _))
            throw new InvalidOperationException("ResolveEventOptionCommand mutated after failing its own validation.");

        var definition = catalog.Get(instance.DefinitionId);
        var stage = definition.Stages[instance.CurrentStageIndex];

        var optionEvents = option.Resolve(state, instance, command.SubmittedDate, command.CommandId.ToTaggedString());

        var announcement = new EventOptionResolvedEvent(
            state.EventIds.Issue(),
            command.SubmittedDate,
            instance.InstanceId,
            instance.DefinitionId,
            instance.Scope,
            command.OptionId,
            instance.SubjectIds,
            command.CommandId.ToTaggedString());

        var hasNextStage = instance.CurrentStageIndex + 1 < definition.Stages.Count;
        state.EventInstances.Remove(instance.InstanceId);
        state.EventInstances.Add(instance.InstanceId, instance with
        {
            Status = hasNextStage ? EventInstanceStatus.AwaitingNextStage : EventInstanceStatus.Resolved,
            ResolvedOptionId = command.OptionId,
            ResolvedDate = command.SubmittedDate,
            ResolvingEventId = announcement.EventId.ToTaggedString(),
            ExpiresDate = hasNextStage
                ? new GameDate(checked(command.SubmittedDate.TotalMonths + stage.NextStageDelayMonths))
                : instance.ExpiresDate,
        });

        var events = new List<IDomainEvent>(optionEvents.Count + 1);
        events.AddRange(optionEvents);
        events.Add(announcement);
        return events.ToArray();
    }

    private static bool TryResolveOption(
        WorldState state,
        ResolveEventOptionCommand command,
        EventCatalog catalog,
        out EventInstance instance,
        out EventOptionDefinition option,
        out ValidationErrorCode? error)
    {
        instance = null!;
        option = null!;

        if (!state.EventInstances.TryGet(command.InstanceId, out var foundInstance))
        {
            error = InstanceNotFound;
            return false;
        }

        if (foundInstance!.Status != EventInstanceStatus.Pending)
        {
            error = InstanceNotPending;
            return false;
        }

        if (command.SubmittedDate.TotalMonths > foundInstance.ExpiresDate.TotalMonths)
        {
            error = InstanceExpired;
            return false;
        }

        if (!catalog.TryGet(foundInstance.DefinitionId, out var definition) ||
            !definition.TryGetStage(foundInstance.CurrentStageIndex, out var stage))
        {
            error = DefinitionNotFound;
            return false;
        }

        if (!stage.TryGetOption(command.OptionId, out var foundOption))
        {
            error = OptionNotFound;
            return false;
        }

        instance = foundInstance;
        option = foundOption;
        error = null;
        return true;
    }
}
