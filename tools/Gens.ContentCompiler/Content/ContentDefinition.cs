using System.Text.Json;

namespace Gens.ContentCompiler.Content;

/// <summary>One authored definition: which family it belongs to, its <c>DefinitionId</c>, which
/// source file it came from (for error messages and manifest source hashes), and its parsed JSON.</summary>
public sealed record ContentDefinition(string Family, string Id, string SourceFile, JsonElement Element)
{
    public bool IsSuperseded =>
        Element.TryGetProperty("status", out var status) && status.GetString() == "superseded";
}

/// <summary>One reference a definition makes to another definition, e.g. a Building recipe line's
/// <c>good</c> field pointing at a Good's <c>DefinitionId</c> (ADR 0012).</summary>
public sealed record ContentReference(string TargetFamily, string TargetId);
