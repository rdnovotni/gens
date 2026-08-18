using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Events;

/// <summary>Emitted by <see cref="EventPoolSystem"/> when a <see cref="EventInstanceStatus.Pending"/>
/// instance's current stage passes its <see cref="EventInstance.ExpiresDate"/> without an option being
/// picked (§7: "an unresolved event/option can expire"). Not command-sourced — like every other
/// bookkeeping-only transition a monthly system performs directly on <c>WorldState</c> (e.g. <see
/// cref="Characters.CharacterLifecycleSystem"/>'s own direct mutations), rather than through <see
/// cref="CommandPipeline{TState,TCommand}"/>, since no submitted command caused it.</summary>
public sealed record EventExpiredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<EventInstance> InstanceId,
    DefinitionId<EventDefinition> DefinitionId,
    EventScope Scope,
    IReadOnlyList<string> SubjectIds,
    string? CausationId) : IDomainEvent, IScopedEvent, IEventInstanceLinkedEvent
{
    public string Type => "events.expired";
    public int SchemaVersion => 1;
    public Visibility Visibility => EventVisibilityRules.For(Scope, SubjectIds);
}

/// <summary>Emitted by <see cref="EventPoolSystem"/> when an <see
/// cref="EventInstanceStatus.AwaitingNextStage"/> instance's queued next stage (§8) becomes current.
/// Distinct from <see cref="EventFiredEvent"/>: the instance already existed, only its stage
/// changed.</summary>
public sealed record EventStageAdvancedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<EventInstance> InstanceId,
    DefinitionId<EventDefinition> DefinitionId,
    EventScope Scope,
    int NewStageIndex,
    IReadOnlyList<string> SubjectIds,
    string? CausationId) : IDomainEvent, IScopedEvent, IEventInstanceLinkedEvent
{
    public string Type => "events.stageAdvanced";
    public int SchemaVersion => 1;
    public Visibility Visibility => EventVisibilityRules.For(Scope, SubjectIds);
}
