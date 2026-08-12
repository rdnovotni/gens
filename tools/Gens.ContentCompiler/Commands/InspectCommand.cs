using Gens.ContentCompiler.Content;
using Gens.Simulation.Saves;

namespace Gens.ContentCompiler.Commands;

/// <summary><c>inspect &lt;contentRoot&gt; &lt;definitionId&gt;</c>: prints one definition's
/// normalized form (searching every family), for debugging content authoring without reading raw
/// source files by hand.</summary>
public static class InspectCommand
{
    public static int Run(string contentRoot, string definitionId)
    {
        var pack = ContentPack.Load(contentRoot);
        var matches = pack.AllDefinitions.Where(d => d.Id == definitionId).ToList();

        if (matches.Count == 0)
        {
            Console.Error.WriteLine($"No definition with ID '{definitionId}' was found in any family.");
            return 1;
        }

        foreach (var definition in matches)
        {
            var normalized = JsonNormalizer.Normalize(definition.Element);
            Console.WriteLine($"[{definition.Family}] {definition.Id} (from {definition.SourceFile}):");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(normalized, CanonicalJson.IndentedOptions));
        }

        return 0;
    }
}
