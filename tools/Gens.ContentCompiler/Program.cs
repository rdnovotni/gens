using System.Text.Json;
using Json.Schema;

if (args is not [var schemaPath, var sourcePath, var outputPath])
{
    Console.Error.WriteLine("Usage: gens-content <schema.json> <source.json> <output.json>");
    return 2;
}

var schema = JsonSchema.FromText(await File.ReadAllTextAsync(schemaPath));
using var document = JsonDocument.Parse(await File.ReadAllTextAsync(sourcePath));
var result = schema.Evaluate(document.RootElement, new EvaluationOptions { OutputFormat = OutputFormat.List });
if (!result.IsValid)
{
    Console.Error.WriteLine(JsonSerializer.Serialize(result, JsonOptions.Indented));
    return 1;
}

var ids = new HashSet<string>(StringComparer.Ordinal);
foreach (var definition in document.RootElement.EnumerateArray())
{
    var id = definition.GetProperty("id").GetString()!;
    if (!ids.Add(id))
    {
        Console.Error.WriteLine($"Duplicate stable ID: {id}");
        return 1;
    }
}

Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
await using var output = File.Create(outputPath);
await JsonSerializer.SerializeAsync(output, document.RootElement, JsonOptions.Compact);
return 0;

internal static class JsonOptions
{
    public static readonly JsonSerializerOptions Indented = new() { WriteIndented = true };
    public static readonly JsonSerializerOptions Compact = new() { WriteIndented = false };
}
