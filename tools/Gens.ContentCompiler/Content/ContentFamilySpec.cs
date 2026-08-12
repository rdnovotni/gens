using System.Text.Json;

namespace Gens.ContentCompiler.Content;

/// <summary>
/// Declares one content family (ADR 0012): its authoring subdirectory under <c>content/source/</c>,
/// its JSON Schema file under <c>content/schemas/</c>, and how to extract this family's outbound
/// references to other families' <c>DefinitionId</c>s from one of its definitions, for cross-file
/// reference validation.
/// </summary>
public sealed record ContentFamilySpec(
    string Name,
    string SchemaFileName,
    Func<JsonElement, IEnumerable<ContentReference>> ExtractReferences)
{
    public static readonly Func<JsonElement, IEnumerable<ContentReference>> NoReferences =
        static _ => Array.Empty<ContentReference>();
}
