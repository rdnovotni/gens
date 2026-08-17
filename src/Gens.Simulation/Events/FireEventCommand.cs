using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Events;

/// <summary>Fires one <see cref="EventDefinition"/> against a set of subjects, creating its stage-0
/// <see cref="EventInstance"/> (Phase 9 item 3). Submitted by <see cref="EventPoolSystem"/> for both a
/// weighted-pool pick (§3) and a Scripted trigger (§4) — the one command path (rule 2) both firing
/// mechanisms share, matching <see cref="Campaign.ScheduledActionSystem"/>'s identical "dispatch
/// through the pipeline, don't mutate directly" shape for the actual firing moment. <see
/// cref="ActorId"/> is the reserved <c>"system"</c> submitter, distinct from <see
/// cref="DecidingActorId"/> — the in-fiction actor <see cref="EventInstance.ActorId"/> carries
/// forward, whose resolution options get scored (Phase 10's player/steward distinction, once it
/// exists, decides whether that deciding actor is the player or an NPC; until then <see
/// cref="EventPoolSystem"/>'s injected auto-resolve policy decides).</summary>
public sealed record FireEventCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    DefinitionId<EventDefinition> DefinitionId,
    IReadOnlyList<string> SubjectIds,
    string DecidingActorId) : ICommand;

/// <summary>Emitted whenever a <see cref="FireEventCommand"/> is accepted.</summary>
public sealed record EventFiredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<EventInstance> InstanceId,
    DefinitionId<EventDefinition> DefinitionId,
    EventScope Scope,
    IReadOnlyList<string> SubjectIds,
    string? CausationId) : IDomainEvent, IScopedEvent, IEventInstanceLinkedEvent
{
    public string Type => "events.fired";
    public int SchemaVersion => 1;

    /// <summary>Computed from <see cref="Scope"/>/<see cref="SubjectIds"/> (<see
    /// cref="EventVisibilityRules"/>), matching every other <see cref="IDomainEvent"/> in this
    /// codebase's own "Visibility is a fixed computed expression, not a caller-supplied field"
    /// convention.</summary>
    public Visibility Visibility => EventVisibilityRules.For(Scope, SubjectIds);
}

/// <summary>The validate/mutate pipeline for <see cref="FireEventCommand"/> (ADR 0006). Unlike <see
/// cref="Policies.ChangeRitesBudgetCommand"/>'s static <c>Pipeline</c> field, this pipeline is built
/// per-<see cref="EventCatalog"/> via <see cref="BuildPipeline"/>: the catalog is caller-loaded content
/// (mirroring <see cref="Actions.ActionCatalog"/>'s identical "not embedded in the save-state graph"
/// shape), not a fixed static table, so the pipeline's validate/mutate closures need one supplied.</summary>
public static class FireEventCommands
{
    public static readonly ValidationErrorCode DefinitionNotFound = new("events.fire.definitionNotFound");
    public static readonly ValidationErrorCode SubjectsRequired = new("events.fire.subjectsRequired");
    public static readonly ValidationErrorCode AlreadyActive = new("events.fire.alreadyActive");

    public static CommandPipeline<WorldState, FireEventCommand> BuildPipeline(EventCatalog catalog)
    {
        if (catalog is null)
            throw new ArgumentNullException(nameof(catalog));

        return new CommandPipeline<WorldState, FireEventCommand>(
            validate: (state, command) => Validate(state, command, catalog),
            mutate: (state, command) => Mutate(state, command, catalog),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, FireEventCommand command, EventCatalog catalog)
    {
        if (!catalog.TryGet(command.DefinitionId, out _))
            return DefinitionNotFound;
        if (command.SubjectIds is null || command.SubjectIds.Count == 0)
            return SubjectsRequired;

        var alreadyActive = state.EventInstances.InAscendingOrder().Any(entry =>
            entry.Value.DefinitionId.Equals(command.DefinitionId) &&
            entry.Value.Status is EventInstanceStatus.Pending or EventInstanceStatus.AwaitingNextStage &&
            entry.Value.SubjectIds.SequenceEqual(command.SubjectIds));
        return alreadyActive ? AlreadyActive : null;
    }

    private static IDomainEvent[] Mutate(WorldState state, FireEventCommand command, EventCatalog catalog)
    {
        var definition = catalog.Get(command.DefinitionId);
        var firstStage = definition.FirstStage;
        var instanceId = state.EventInstanceIds.Issue();
        var expiresDate = new GameDate(checked(command.SubmittedDate.TotalMonths + firstStage.ExpiryMonths));

        var instance = new EventInstance(
            instanceId,
            command.DefinitionId,
            definition.Scope,
            command.SubjectIds,
            command.DecidingActorId,
            CurrentStageIndex: 0,
            FiredDate: command.SubmittedDate,
            ExpiresDate: expiresDate,
            Status: EventInstanceStatus.Pending);
        state.EventInstances.Add(instanceId, instance);

        return new IDomainEvent[]
        {
            new EventFiredEvent(
                state.EventIds.Issue(),
                command.SubmittedDate,
                instanceId,
                command.DefinitionId,
                definition.Scope,
                command.SubjectIds,
                command.CommandId.ToTaggedString()),
        };
    }
}
