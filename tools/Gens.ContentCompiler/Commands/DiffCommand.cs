using System.Text.Json;
using Gens.ContentCompiler.Content;

namespace Gens.ContentCompiler.Commands;

/// <summary><c>diff &lt;compiledPackA&gt; &lt;compiledPackB&gt;</c>: compares two already-compiled,
/// normalized content packs (output of <see cref="CompileCommand"/>) definition by definition, per
/// family, so a content-authoring change's actual normalized effect is visible independent of source
/// file layout or key order.</summary>
public static class DiffCommand
{
    public static int Run(string packAPath, string packBPath)
    {
        using var docA = JsonDocument.Parse(File.ReadAllBytes(packAPath));
        using var docB = JsonDocument.Parse(File.ReadAllBytes(packBPath));

        var definitionsA = FlattenDefinitions(docA.RootElement);
        var definitionsB = FlattenDefinitions(docB.RootElement);

        var allKeys = definitionsA.Keys.Union(definitionsB.Keys, StringComparer.Ordinal).OrderBy(static k => k, StringComparer.Ordinal);
        var differenceCount = 0;

        foreach (var key in allKeys)
        {
            var hasA = definitionsA.TryGetValue(key, out var elementA);
            var hasB = definitionsB.TryGetValue(key, out var elementB);

            if (!hasA)
            {
                Console.WriteLine($"+ {key} (added in B)");
                differenceCount++;
            }
            else if (!hasB)
            {
                Console.WriteLine($"- {key} (removed in B)");
                differenceCount++;
            }
            else if (elementA.GetRawText() != elementB.GetRawText())
            {
                Console.WriteLine($"~ {key} (changed)");
                differenceCount++;
            }
        }

        if (differenceCount == 0)
            Console.WriteLine("No differences.");

        return 0;
    }

    private static Dictionary<string, JsonElement> FlattenDefinitions(JsonElement compiledPack)
    {
        var result = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var family in compiledPack.GetProperty("families").EnumerateArray())
        {
            var familyName = family.GetProperty("name").GetString()!;
            foreach (var definition in family.GetProperty("definitions").EnumerateArray())
            {
                var id = definition.GetProperty("id").GetString()!;
                result[$"{familyName}:{id}"] = definition;
            }
        }

        return result;
    }
}
