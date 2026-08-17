using Gens.Simulation.Identity;

namespace Gens.Simulation.Events;

/// <summary>The in-memory lookup over every registered <see cref="EventDefinition"/> — mirrors <see
/// cref="Gens.Simulation.Actions.ActionCatalog"/>'s identical shape: deliberately independent of <see
/// cref="Gens.Simulation.State.WorldState"/>, since a definition's own metadata is content, not
/// runtime state.</summary>
public sealed class EventCatalog
{
    private readonly Dictionary<DefinitionId<EventDefinition>, EventDefinition> _byId;

    public EventCatalog(IEnumerable<EventDefinition> definitions)
    {
        if (definitions is null)
            throw new ArgumentNullException(nameof(definitions));

        var byId = new Dictionary<DefinitionId<EventDefinition>, EventDefinition>();
        foreach (var definition in definitions)
        {
            if (!byId.TryAdd(definition.Id, definition))
                throw new ArgumentException($"Duplicate event definition ID '{definition.Id}' in catalog.", nameof(definitions));
        }

        _byId = byId;
    }

    public bool TryGet(DefinitionId<EventDefinition> id, out EventDefinition definition) => _byId.TryGetValue(id, out definition!);

    public EventDefinition Get(DefinitionId<EventDefinition> id) =>
        TryGet(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"No event definition '{id}' is registered in this catalog.");

    public IEnumerable<EventDefinition> All() => _byId.Values;

    public IEnumerable<EventDefinition> ForScope(EventScope scope) => _byId.Values.Where(definition => definition.Scope == scope);
}
