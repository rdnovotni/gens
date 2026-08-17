using Gens.Simulation.Events;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Queries;

/// <summary>One option the projected <see cref="EventInstance"/>'s current (or, once terminal, its
/// final) stage offered.</summary>
public readonly record struct EventInstanceOptionProjection(string OptionId, string NameKey, string DescriptionKey, bool IsResolvedOption);

/// <summary>The Monthly Report's drill-down projection (Phase 9 item 4): the full detail behind one
/// report entry that links back to an <see cref="EventInstance"/> — its definition, scope, subjects,
/// current stage, status, and every option that stage offered, flagging which one (if any) was
/// actually picked. Built from <see cref="WorldState.EventInstances"/> plus the caller-supplied <see
/// cref="EventCatalog"/> content, matching <see cref="HouseholdPolicyModifiersQuery"/>'s identical
/// "no <see cref="State.KnowledgeState"/> filtering yet" precedent — every observer sees the same
/// projection.</summary>
public readonly record struct EventInstanceDetailProjection(
    string InstanceId,
    string DefinitionId,
    string NameKey,
    string DescriptionKey,
    EventScope Scope,
    IReadOnlyList<string> SubjectIds,
    string ActorId,
    int CurrentStageIndex,
    EventInstanceStatus Status,
    IReadOnlyList<EventInstanceOptionProjection> Options,
    string? ResolvedOptionId,
    string? ResolvingEventId);

/// <summary>Projects a single, caller-specified <see cref="EventInstance"/>'s full detail — the
/// catalog is supplied at construction time alongside the instance ID, the same "caller-specified
/// subject" shape <see cref="CharacterLifecycleQuery"/> establishes, extended with the content
/// dependency this query alone needs (a fired instance's own record carries only a <see
/// cref="Identity.DefinitionId{T}"/> reference, per rule 10 — resolving it to a name/description/option
/// list needs the catalog it was fired from).</summary>
public sealed class EventInstanceDetailQuery : IWorldQuery<EventInstanceDetailProjection>
{
    private readonly RuntimeId<EventInstance> _instanceId;
    private readonly EventCatalog _catalog;

    public EventInstanceDetailQuery(RuntimeId<EventInstance> instanceId, EventCatalog catalog)
    {
        _instanceId = instanceId;
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    public EventInstanceDetailProjection Execute(WorldState state, string observerId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (!state.EventInstances.TryGet(_instanceId, out var instance))
            throw new KeyNotFoundException($"No EventInstance with ID '{_instanceId}' exists.");

        var definition = _catalog.Get(instance!.DefinitionId);

        // The instance's own CurrentStageIndex describes the stage it is at once terminal too — a
        // Resolved instance's last-current stage is exactly the stage whose option was actually
        // picked, so no separate "final stage" lookup is needed.
        var hasStage = definition.TryGetStage(instance.CurrentStageIndex, out var stage);
        var options = !hasStage
            ? Array.Empty<EventInstanceOptionProjection>()
            : stage.Options
                .Select(option => new EventInstanceOptionProjection(
                    option.Id.Value,
                    option.NameKey,
                    option.DescriptionKey,
                    instance.ResolvedOptionId is { } resolvedId && resolvedId.Equals(option.Id)))
                .ToArray();

        return new EventInstanceDetailProjection(
            InstanceId: instance.InstanceId.ToTaggedString(),
            DefinitionId: definition.Id.Value,
            NameKey: definition.NameKey,
            DescriptionKey: definition.DescriptionKey,
            Scope: instance.Scope,
            SubjectIds: instance.SubjectIds,
            ActorId: instance.ActorId,
            CurrentStageIndex: instance.CurrentStageIndex,
            Status: instance.Status,
            Options: options,
            ResolvedOptionId: instance.ResolvedOptionId?.Value,
            ResolvingEventId: instance.ResolvingEventId);
    }
}
