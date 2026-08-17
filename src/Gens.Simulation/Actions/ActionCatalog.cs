using Gens.Simulation.Identity;

namespace Gens.Simulation.Actions;

/// <summary>The in-memory lookup over every registered <see cref="ActionDefinition"/> — deliberately
/// independent of <see cref="Gens.Simulation.State.WorldState"/>, matching <see
/// cref="Gens.Simulation.Characters.TraitCatalog"/>'s identical "the compiled catalog is resolved by
/// whichever caller has it loaded, not embedded in the save-state graph itself" shape. An action's own
/// metadata is content, not runtime state; only the concrete commands it wraps ever touch
/// <c>WorldState</c>.</summary>
public sealed class ActionCatalog
{
    private readonly Dictionary<DefinitionId<ActionDefinition>, ActionDefinition> _byId;

    public ActionCatalog(IEnumerable<ActionDefinition> definitions)
    {
        if (definitions is null)
            throw new ArgumentNullException(nameof(definitions));

        var byId = new Dictionary<DefinitionId<ActionDefinition>, ActionDefinition>();
        foreach (var definition in definitions)
        {
            if (!byId.TryAdd(definition.Id, definition))
                throw new ArgumentException($"Duplicate action definition ID '{definition.Id}' in catalog.", nameof(definitions));
        }

        _byId = byId;
    }

    public bool TryGet(DefinitionId<ActionDefinition> id, out ActionDefinition definition) => _byId.TryGetValue(id, out definition!);

    public ActionDefinition Get(DefinitionId<ActionDefinition> id) =>
        TryGet(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"No action definition '{id}' is registered in this catalog.");

    public IEnumerable<ActionDefinition> All() => _byId.Values;
}
