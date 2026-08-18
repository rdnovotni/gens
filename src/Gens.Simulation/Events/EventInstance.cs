using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Events;

/// <summary>The runtime record of one fired event (Phase 9 item 3): the <see cref="EventDefinition"/>
/// it came from, every subject it is about, the deciding actor, its current stage, and its
/// resolution/expiry bookkeeping. Immutable like every other <c>WorldState</c> record — <see
/// cref="EventPoolSystem"/> and <see cref="ResolveEventOptionCommands"/> replace an entry in <see
/// cref="Gens.Simulation.State.WorldState.EventInstances"/> rather than mutating one in place,
/// matching <see cref="Gens.Simulation.Policies.HouseholdPolicyState"/>'s identical
/// remove-then-re-add convention.</summary>
/// <param name="ExpiresDate">Carries a different meaning depending on <paramref name="Status"/>,
/// avoiding a second nullable date field: while <see cref="EventInstanceStatus.Pending"/>, it is the
/// deadline an option must be picked by before <see cref="EventPoolSystem"/>'s expiry pass marks the
/// instance <see cref="EventInstanceStatus.Expired"/> (§7); while <see
/// cref="EventInstanceStatus.AwaitingNextStage"/>, it is instead the due date <see
/// cref="EventPoolSystem"/>'s stage-advancement pass opens the next stage on (§8). Meaningless once
/// <paramref name="Status"/> reaches a terminal value.</param>
public sealed record EventInstance(
    RuntimeId<EventInstance> InstanceId,
    DefinitionId<EventDefinition> DefinitionId,
    EventScope Scope,
    IReadOnlyList<string> SubjectIds,
    string ActorId,
    int CurrentStageIndex,
    GameDate FiredDate,
    GameDate ExpiresDate,
    EventInstanceStatus Status,
    DefinitionId<EventOptionDefinition>? ResolvedOptionId = null,
    GameDate? ResolvedDate = null,
    string? ResolvingEventId = null);
