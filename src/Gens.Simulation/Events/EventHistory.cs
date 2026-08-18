using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Events;

/// <summary>Read-only helpers over <see cref="WorldState.EventInstances"/> a definition's own <see
/// cref="EventDefinition.Eligibility"/>/<see cref="EventDefinition.ScriptedCondition"/> hooks can use
/// directly — most usefully <see cref="HasEverFired"/>, which a one-shot Scripted definition's own
/// Eligibility hook uses to stay firing exactly once per subject even though a Scripted trigger's own
/// firing guard (<see cref="FireEventCommands.AlreadyActive"/>) only blocks a definition still <see
/// cref="EventInstanceStatus.Pending"/> or <see cref="EventInstanceStatus.AwaitingNextStage"/>, not
/// one that has already fully <see cref="EventInstanceStatus.Resolved"/>.</summary>
public static class EventHistory
{
    public static bool HasEverFired(WorldState state, DefinitionId<EventDefinition> definitionId, IReadOnlyList<string> subjectIds) =>
        state.EventInstances.InAscendingOrder().Any(entry =>
            entry.Value.DefinitionId.Equals(definitionId) &&
            entry.Value.SubjectIds.SequenceEqual(subjectIds));
}
