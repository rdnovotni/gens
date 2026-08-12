using Gens.ContentCompiler.Content;

namespace Gens.ContentCompiler.Commands;

/// <summary><c>validate &lt;contentRoot&gt;</c>: schema + duplicate-ID + reference + localization
/// validation only, no compiled output. Exit code 0 on a clean pack, 1 otherwise — the shape Phase 3's
/// exit gate needs ("invalid references and schema violations fail before Unity starts").</summary>
public static class ValidateCommand
{
    public static int Run(string contentRoot)
    {
        var pack = ContentPack.Load(contentRoot);
        var validator = new ContentValidator(Path.Combine(contentRoot, "schemas"));
        var errors = validator.Validate(pack);

        if (errors.Count == 0)
        {
            Console.WriteLine($"Content pack at '{contentRoot}' is valid ({pack.AllDefinitions.Count()} definitions across {ContentFamilies.All.Count} families).");
            return 0;
        }

        Console.Error.WriteLine($"Content pack at '{contentRoot}' failed validation with {errors.Count} error(s):");
        foreach (var error in errors)
            Console.Error.WriteLine($"  {error}");
        return 1;
    }
}
