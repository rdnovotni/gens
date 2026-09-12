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
        new ContentFamilySpec("education", "education-tracks.schema.json", ExtractEducationTrackReferences),
        // Phase 17 item 2 — the fixed five-entry Institutions of Renown roster. Registered as
        // "institutions" (its content/source directory name, per ContentPack's convention) rather than
        // the more descriptive "institutionsOfRenown" the plan's own prose uses informally, matching
        // "education"'s identical naming precedent above. NoReferences, mirroring cultures/religions:
        // specialtyTrack/primeCulturalAssociation are informal cross-family pointers this item does not
        // validate, the same restraint those two families already show for their own similarly-informal
        // fields.
        new ContentFamilySpec("institutions", "institutions-of-renown.schema.json", ContentFamilySpec.NoReferences),
    };

    /// <summary>An Educational Track's <c>deliveringBuildings</c> each reference a Building (Phase 17
    /// item 2) — the same "cross-file reference to validate" shape as <see
    /// cref="ExtractBuildingReferences"/>'s own recipe-line references. The family's authoring directory
    /// is named <c>content/source/education/</c> per the ticket's own plan, so this family is registered
    /// as <c>"education"</c> (matching <see cref="ContentPack"/>'s directory-name convention) rather than
    /// the more descriptive <c>"educationTracks"</c> the plan's own prose uses informally.</summary>
    private static IEnumerable<ContentReference> ExtractEducationTrackReferences(JsonElement definition)
    {
        if (!definition.TryGetProperty("deliveringBuildings", out var buildings))
            yield break;

        foreach (var building in buildings.EnumerateArray())
            yield return new ContentReference("buildings", building.GetString()!);
    }

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
