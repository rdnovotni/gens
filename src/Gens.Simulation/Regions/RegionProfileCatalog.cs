using Gens.Simulation.Identity;

namespace Gens.Simulation.Regions;

/// <summary>The in-memory lookup over every registered <see cref="RegionProfileDefinition"/> — mirrors
/// <see cref="Gens.Simulation.Events.EventCatalog"/>'s identical shape: independent of <see
/// cref="Gens.Simulation.State.WorldState"/>, since a region profile's own metadata is content, not
/// runtime state.</summary>
public sealed class RegionProfileCatalog
{
    private readonly Dictionary<DefinitionId<RegionProfileDefinition>, RegionProfileDefinition> _byId;

    public RegionProfileCatalog(IEnumerable<RegionProfileDefinition> definitions)
    {
        if (definitions is null)
            throw new ArgumentNullException(nameof(definitions));

        var byId = new Dictionary<DefinitionId<RegionProfileDefinition>, RegionProfileDefinition>();
        foreach (var definition in definitions)
        {
            if (!byId.TryAdd(definition.Id, definition))
                throw new ArgumentException($"Duplicate region profile ID '{definition.Id}' in catalog.", nameof(definitions));
        }

        var capitalSeats = byId.Values
            .SelectMany(region => region.Gazetteer)
            .Where(entry => entry.Roles.Contains(GazetteerRole.Capital))
            .ToArray();
        if (capitalSeats.Length > 1)
        {
            throw new ArgumentException(
                "At most one gazetteer entry across the whole catalog may carry the Capital role (§8.3: Rome only).",
                nameof(definitions));
        }

        _byId = byId;
    }

    public bool TryGet(DefinitionId<RegionProfileDefinition> id, out RegionProfileDefinition definition) =>
        _byId.TryGetValue(id, out definition!);

    public RegionProfileDefinition Get(DefinitionId<RegionProfileDefinition> id) =>
        TryGet(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"No region profile '{id}' is registered in this catalog.");

    public IEnumerable<RegionProfileDefinition> All() => _byId.Values;

    public IEnumerable<RegionProfileDefinition> ForStatus(RegionStatus status) => _byId.Values.Where(region => region.Status == status);
}
