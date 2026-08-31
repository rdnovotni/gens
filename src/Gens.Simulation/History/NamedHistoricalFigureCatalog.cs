using Gens.Simulation.Identity;

namespace Gens.Simulation.History;

/// <summary>The in-memory lookup over every registered <see cref="NamedHistoricalFigureDefinition"/> —
/// mirrors <see cref="Gens.Simulation.Events.EventCatalog"/>'s identical shape.</summary>
public sealed class NamedHistoricalFigureCatalog
{
    private readonly Dictionary<DefinitionId<NamedHistoricalFigureDefinition>, NamedHistoricalFigureDefinition> _byId;

    public NamedHistoricalFigureCatalog(IEnumerable<NamedHistoricalFigureDefinition> definitions)
    {
        if (definitions is null)
            throw new ArgumentNullException(nameof(definitions));

        var byId = new Dictionary<DefinitionId<NamedHistoricalFigureDefinition>, NamedHistoricalFigureDefinition>();
        foreach (var definition in definitions)
        {
            if (!byId.TryAdd(definition.Id, definition))
                throw new ArgumentException($"Duplicate named historical figure ID '{definition.Id}' in catalog.", nameof(definitions));
        }

        _byId = byId;
    }

    public bool TryGet(DefinitionId<NamedHistoricalFigureDefinition> id, out NamedHistoricalFigureDefinition definition) =>
        _byId.TryGetValue(id, out definition!);

    public NamedHistoricalFigureDefinition Get(DefinitionId<NamedHistoricalFigureDefinition> id) =>
        TryGet(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"No named historical figure '{id}' is registered in this catalog.");

    public IEnumerable<NamedHistoricalFigureDefinition> All() => _byId.Values;
}
