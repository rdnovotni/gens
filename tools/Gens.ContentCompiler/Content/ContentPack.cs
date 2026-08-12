using System.Text.Json;

namespace Gens.ContentCompiler.Content;

/// <summary>Every definition loaded from <c>content/source/&lt;family&gt;/*.json</c>, grouped by
/// family, plus the raw source file bytes each definition came from (for manifest source hashes).</summary>
public sealed class ContentPack
{
    private readonly Dictionary<string, List<ContentDefinition>> _definitionsByFamily;

    private ContentPack(
        Dictionary<string, List<ContentDefinition>> definitionsByFamily,
        IReadOnlyDictionary<string, byte[]> sourceFileBytes,
        IReadOnlyDictionary<string, string> localization)
    {
        _definitionsByFamily = definitionsByFamily;
        SourceFileBytes = sourceFileBytes;
        Localization = localization;
    }

    /// <summary>Every loaded source file's raw bytes, keyed by its path relative to the content root.</summary>
    public IReadOnlyDictionary<string, byte[]> SourceFileBytes { get; }

    public IReadOnlyDictionary<string, string> Localization { get; }

    public IReadOnlyList<ContentDefinition> DefinitionsIn(string family) =>
        _definitionsByFamily.TryGetValue(family, out var list) ? list : Array.Empty<ContentDefinition>();

    public IEnumerable<ContentDefinition> AllDefinitions => _definitionsByFamily.Values.SelectMany(static list => list);

    /// <summary>Loads every family's source files from <c>&lt;contentRoot&gt;/source/&lt;family&gt;/*.json</c>
    /// and the localization map from <c>&lt;contentRoot&gt;/source/localization/en.json</c>.</summary>
    public static ContentPack Load(string contentRoot)
    {
        var sourceRoot = Path.Combine(contentRoot, "source");
        var definitionsByFamily = new Dictionary<string, List<ContentDefinition>>(StringComparer.Ordinal);
        var sourceFileBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal);

        foreach (var family in ContentFamilies.All)
        {
            var familyDirectory = Path.Combine(sourceRoot, family.Name);
            var definitions = new List<ContentDefinition>();
            if (Directory.Exists(familyDirectory))
            {
                foreach (var file in Directory.EnumerateFiles(familyDirectory, "*.json").OrderBy(static f => f, StringComparer.Ordinal))
                {
                    var bytes = File.ReadAllBytes(file);
                    var relativePath = Path.GetRelativePath(contentRoot, file).Replace('\\', '/');
                    sourceFileBytes[relativePath] = bytes;

                    using var document = JsonDocument.Parse(bytes);
                    foreach (var element in document.RootElement.EnumerateArray())
                    {
                        var id = element.GetProperty("id").GetString()!;
                        definitions.Add(new ContentDefinition(family.Name, id, relativePath, element.Clone()));
                    }
                }
            }

            definitionsByFamily[family.Name] = definitions;
        }

        var localizationPath = Path.Combine(sourceRoot, "localization", "en.json");
        var localization = File.Exists(localizationPath)
            ? JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllBytes(localizationPath))
                ?? new Dictionary<string, string>()
            : new Dictionary<string, string>();
        if (File.Exists(localizationPath))
            sourceFileBytes[Path.GetRelativePath(contentRoot, localizationPath).Replace('\\', '/')] = File.ReadAllBytes(localizationPath);

        return new ContentPack(definitionsByFamily, sourceFileBytes, localization);
    }
}
