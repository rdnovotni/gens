using System.Reflection;
using Gens.Simulation.Saves;

namespace Gens.ContentCompiler.Content;

/// <summary>Builds the compiled manifest and normalized, dependency-ordered definition set for a
/// validated <see cref="ContentPack"/> (ADR 0012). Callers must validate first — this class computes
/// hashes and order, it does not re-check schema/reference/duplicate-ID correctness.</summary>
public static class ContentManifestBuilder
{
    public const int CurrentSchemaVersion = 1;

    public static CompiledContentPackDto Build(ContentPack pack)
    {
        var dependencyOrder = ComputeDependencyOrder(pack);

        var sourceHashes = pack.SourceFileBytes
            .OrderBy(static entry => entry.Key, StringComparer.Ordinal)
            .Select(static entry => new SourceFileHashDto { Path = entry.Key, Sha256 = CanonicalJson.Sha256Hex(entry.Value) })
            .ToArray();

        var manifest = new ContentManifestDto
        {
            SchemaVersion = CurrentSchemaVersion,
            CompilerVersion = typeof(ContentManifestBuilder).Assembly.GetName().Version?.ToString() ?? "0.0.0",
            SourceHashes = sourceHashes,
            DependencyOrder = dependencyOrder,
        };

        var families = dependencyOrder
            .Select(familyName => new CompiledFamilyDto
            {
                Name = familyName,
                Definitions = pack.DefinitionsIn(familyName)
                    .OrderBy(static d => d.Id, StringComparer.Ordinal)
                    .Select(static d => JsonNormalizer.Normalize(d.Element))
                    .ToArray(),
            })
            .ToArray();

        return new CompiledContentPackDto { Manifest = manifest, Families = families };
    }

    /// <summary>Kahn's algorithm over the family reference graph, computed empirically from the
    /// pack's actual definitions (self-references within one family, e.g. a Trait's <c>opposes</c>,
    /// are not graph edges — they don't constrain cross-family compile order). Ties break by
    /// <see cref="ContentFamilies.All"/>'s declared order for a deterministic result. A cycle is a
    /// hard failure: content families must have a well-defined compile order.</summary>
    private static List<string> ComputeDependencyOrder(ContentPack pack)
    {
        var familyNames = ContentFamilies.All.Select(static f => f.Name).ToList();
        var declaredIndex = familyNames.Select((name, index) => (name, index)).ToDictionary(static t => t.name, static t => t.index);

        var dependsOn = familyNames.ToDictionary(static name => name, static _ => new HashSet<string>(StringComparer.Ordinal));
        foreach (var family in ContentFamilies.All)
        {
            foreach (var definition in pack.DefinitionsIn(family.Name))
            {
                foreach (var reference in family.ExtractReferences(definition.Element))
                {
                    if (reference.TargetFamily != family.Name)
                        dependsOn[family.Name].Add(reference.TargetFamily);
                }
            }
        }

        var remaining = new HashSet<string>(familyNames, StringComparer.Ordinal);
        var ordered = new List<string>();
        while (remaining.Count > 0)
        {
            var ready = remaining
                .Where(name => dependsOn[name].All(dep => !remaining.Contains(dep)))
                .OrderBy(name => declaredIndex[name])
                .ToList();

            if (ready.Count == 0)
                throw new ContentValidationException(
                    $"Content families have a circular dependency among: {string.Join(", ", remaining.OrderBy(n => n, StringComparer.Ordinal))}.");

            foreach (var name in ready)
            {
                ordered.Add(name);
                remaining.Remove(name);
            }
        }

        return ordered;
    }
}

public sealed class ContentValidationException : Exception
{
    public ContentValidationException(string message)
        : base(message)
    {
    }
}
