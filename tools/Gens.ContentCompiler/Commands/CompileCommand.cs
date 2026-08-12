using Gens.ContentCompiler.Content;
using Gens.Simulation.Saves;

namespace Gens.ContentCompiler.Commands;

/// <summary><c>compile &lt;contentRoot&gt; &lt;outputPath&gt;</c>: validates (failing hard on any
/// error, same as <see cref="ValidateCommand"/>) then writes the normalized, canonical, manifest-
/// bearing compiled pack.</summary>
public static class CompileCommand
{
    public static int Run(string contentRoot, string outputPath)
    {
        var pack = ContentPack.Load(contentRoot);
        var validator = new ContentValidator(Path.Combine(contentRoot, "schemas"));
        var errors = validator.Validate(pack);
        if (errors.Count > 0)
        {
            Console.Error.WriteLine($"Refusing to compile: {errors.Count} validation error(s):");
            foreach (var error in errors)
                Console.Error.WriteLine($"  {error}");
            return 1;
        }

        var compiled = ContentManifestBuilder.Build(pack);
        var bytes = CanonicalJson.SerializeToCanonicalBytes(compiled);

        var outputDirectory = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrEmpty(outputDirectory))
            Directory.CreateDirectory(outputDirectory);
        File.WriteAllBytes(outputPath, bytes);

        Console.WriteLine($"Compiled {pack.AllDefinitions.Count()} definitions to '{outputPath}' " +
            $"(schemaVersion={compiled.Manifest.SchemaVersion}, dependencyOrder=[{string.Join(", ", compiled.Manifest.DependencyOrder)}]).");
        return 0;
    }
}
