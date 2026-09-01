using Gens.Simulation.Identity;

namespace Gens.Simulation.Health;

/// <summary>Content catalog of every authored <see cref="HealthConditionDefinition"/>, mirroring
/// <c>Cultures.CultureCatalog</c>'s identical "reject duplicate IDs at construction" shape. Empty by
/// default in this item: no real disease content is authored yet (Phase 14 item 2's job) — this only
/// establishes the container the real seven-endemic/four-epidemic roster will eventually populate.
/// Not part of <c>WorldState</c>: like <c>Cultures.CultureCatalog</c>, this is content a caller loads
/// once and consults by reference (<see cref="DefinitionId{T}"/>), not campaign state.</summary>
public sealed class HealthConditionCatalog
{
    private readonly Dictionary<string, HealthConditionDefinition> _entries;

    public HealthConditionCatalog(IEnumerable<HealthConditionDefinition> definitions)
    {
        if (definitions is null)
            throw new ArgumentNullException(nameof(definitions));

        var map = new Dictionary<string, HealthConditionDefinition>(StringComparer.Ordinal);
        foreach (var definition in definitions)
        {
            if (!map.TryAdd(definition.Id.Value, definition))
                throw new ArgumentException($"Duplicate health condition ID '{definition.Id.Value}'.", nameof(definitions));
        }

        _entries = map;
    }

    public int Count => _entries.Count;

    public bool TryGet(DefinitionId<HealthConditionDefinition> id, out HealthConditionDefinition definition) =>
        _entries.TryGetValue(id.Value, out definition!);

    public HealthConditionDefinition Get(DefinitionId<HealthConditionDefinition> id) =>
        TryGet(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"No health condition is registered for ID '{id.Value}'.");

    public IEnumerable<HealthConditionDefinition> All() => _entries.Values;
}
