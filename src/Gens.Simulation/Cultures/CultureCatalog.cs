using Gens.Simulation.Identity;

namespace Gens.Simulation.Cultures;

/// <summary>Rejects duplicate culture IDs at construction — mirrors <see
/// cref="Regions.RegionProfileCatalog"/>/<see cref="Events.EventCatalog"/>'s identical shape.</summary>
public sealed class CultureCatalog
{
    private readonly Dictionary<string, CultureDefinition> _entries;

    public CultureCatalog(IEnumerable<CultureDefinition> definitions)
    {
        if (definitions is null)
            throw new ArgumentNullException(nameof(definitions));

        var map = new Dictionary<string, CultureDefinition>(StringComparer.Ordinal);
        foreach (var definition in definitions)
        {
            if (!map.TryAdd(definition.Id.Value, definition))
                throw new ArgumentException($"Duplicate culture ID '{definition.Id.Value}'.", nameof(definitions));
        }

        _entries = map;
    }

    public int Count => _entries.Count;

    public bool TryGet(DefinitionId<Identity.Culture> id, out CultureDefinition definition) =>
        _entries.TryGetValue(id.Value, out definition!);

    public CultureDefinition Get(DefinitionId<Identity.Culture> id) =>
        TryGet(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"No culture is registered for ID '{id.Value}'.");

    public IEnumerable<CultureDefinition> All() => _entries.Values;
}
