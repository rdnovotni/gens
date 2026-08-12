using System.Text.Json;

namespace Gens.ContentCompiler.Content;

/// <summary>
/// The fixed list of typed definition families (Phase 3, item 4 — "goods, buildings, traits,
/// policies, events, regions, cultures, religions, names, and presentation metadata"), in the exact
/// dependency order a content pack's families must compile in: a family only references families
/// listed before it here. <see cref="ContentManifestBuilder"/> derives its own topological order from
/// each family's actual <see cref="ContentFamilySpec.ExtractReferences"/> function rather than trusting
/// this ordering blindly — this list only needs to be *a* valid order, not verified here.
/// </summary>
public static class ContentFamilies
{
    public static readonly IReadOnlyList<ContentFamilySpec> All = new[]
    {
        new ContentFamilySpec("goods", "goods.schema.json", ContentFamilySpec.NoReferences),
        new ContentFamilySpec("cultures", "cultures.schema.json", ContentFamilySpec.NoReferences),
        new ContentFamilySpec("religions", "religions.schema.json", ContentFamilySpec.NoReferences),
        new ContentFamilySpec("names", "names.schema.json", ContentFamilySpec.NoReferences),
        new ContentFamilySpec("presentation", "presentation.schema.json", ContentFamilySpec.NoReferences),
        new ContentFamilySpec("policies", "policies.schema.json", ContentFamilySpec.NoReferences),
        new ContentFamilySpec("events", "events.schema.json", ContentFamilySpec.NoReferences),
        new ContentFamilySpec("traits", "traits.schema.json", ExtractTraitReferences),
        new ContentFamilySpec("buildings", "buildings.schema.json", ExtractBuildingReferences),
        new ContentFamilySpec("regions", "regions.schema.json", ExtractRegionReferences),
    };

    /// <summary>A Building's recipe inputs/outputs each reference a Good (ADR 0012's own example of a
    /// cross-file reference to validate).</summary>
    private static IEnumerable<ContentReference> ExtractBuildingReferences(JsonElement definition)
    {
        if (!definition.TryGetProperty("recipe", out var recipe))
            yield break;

        foreach (var lineGroup in new[] { "inputs", "outputs" })
        {
            if (!recipe.TryGetProperty(lineGroup, out var lines))
                continue;
            foreach (var line in lines.EnumerateArray())
                yield return new ContentReference("goods", line.GetProperty("good").GetString()!);
        }
    }

    /// <summary>A Region's <c>primaryCulture</c> references a Culture.</summary>
    private static IEnumerable<ContentReference> ExtractRegionReferences(JsonElement definition)
    {
        if (definition.TryGetProperty("primaryCulture", out var culture))
            yield return new ContentReference("cultures", culture.GetString()!);
    }

    /// <summary>A Trait's optional <c>opposes</c> field references another Trait in the same family.</summary>
    private static IEnumerable<ContentReference> ExtractTraitReferences(JsonElement definition)
    {
        if (definition.TryGetProperty("opposes", out var opposes))
            yield return new ContentReference("traits", opposes.GetString()!);
    }
}
