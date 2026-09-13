using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;

namespace Gens.Simulation.Education;

/// <summary>Rejects duplicate track IDs at construction — mirrors <see
/// cref="Cultures.CultureCatalog"/>'s identical shape.</summary>
public sealed class EducationTrackCatalog
{
    private readonly Dictionary<string, EducationTrackDefinition> _entries;

    public EducationTrackCatalog(IEnumerable<EducationTrackDefinition> definitions)
    {
        if (definitions is null)
            throw new ArgumentNullException(nameof(definitions));

        var map = new Dictionary<string, EducationTrackDefinition>(StringComparer.Ordinal);
        foreach (var definition in definitions)
        {
            if (!map.TryAdd(definition.Id.Value, definition))
                throw new ArgumentException($"Duplicate education track ID '{definition.Id.Value}'.", nameof(definitions));
        }

        _entries = map;
    }

    public int Count => _entries.Count;

    public bool TryGet(Identity.DefinitionId<EducationTrack> id, out EducationTrackDefinition definition) =>
        _entries.TryGetValue(id.Value, out definition!);

    public EducationTrackDefinition Get(Identity.DefinitionId<EducationTrack> id) =>
        TryGet(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"No education track is registered for ID '{id.Value}'.");

    public IEnumerable<EducationTrackDefinition> All() => _entries.Values;
}
