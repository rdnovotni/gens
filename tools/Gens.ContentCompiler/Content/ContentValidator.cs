using System.Text.Json;
using Json.Schema;

namespace Gens.ContentCompiler.Content;

/// <summary>
/// Runs every validation Phase 3, item 5 requires, gating a content pack build exactly as ADR 0012
/// specifies: per-family JSON Schema validation, duplicate-<c>DefinitionId</c>-within-family
/// detection, cross-file reference validation, and localization-key checks. Every failure is
/// collected (not short-circuited) so one run reports everything wrong with a pack, matching this
/// project's other validators (e.g. <c>InvariantCheckRunner</c>'s "collect, don't stop at the first").
/// </summary>
public sealed class ContentValidator
{
    private readonly string _schemasRoot;

    // An instance-scoped registry, deliberately not Json.Schema.SchemaRegistry.Global: the global
    // registry rejects re-registering a schema $id, which would make a second ContentValidator in
    // the same process (one per test, or validate-then-compile in one CLI run) fail with "Overwriting
    // registered schemas is not permitted" purely from re-reading identical schema files. Passing
    // this registry via BuildOptions to every JsonSchema.FromText call scopes both registration and
    // $ref resolution to this instance.
    private readonly SchemaRegistry _schemaRegistry = new();
    private readonly BuildOptions _buildOptions;

    public ContentValidator(string schemasRoot)
    {
        _schemasRoot = schemasRoot;
        _buildOptions = new BuildOptions { SchemaRegistry = _schemaRegistry };

        var envelopeSchema = JsonSchema.FromText(
            File.ReadAllText(Path.Combine(schemasRoot, "_envelope.schema.json")), _buildOptions);
        _schemaRegistry.Register(envelopeSchema);
    }

    public IReadOnlyList<ContentValidationError> Validate(ContentPack pack)
    {
        var errors = new List<ContentValidationError>();

        foreach (var family in ContentFamilies.All)
        {
            ValidateSchema(pack, family, errors);
            ValidateNoDuplicateIds(pack, family, errors);
            ValidateLocalizationKeys(pack, family, errors);
        }

        ValidateReferences(pack, errors);

        return errors;
    }

    private void ValidateSchema(ContentPack pack, ContentFamilySpec family, List<ContentValidationError> errors)
    {
        var schema = JsonSchema.FromText(File.ReadAllText(Path.Combine(_schemasRoot, family.SchemaFileName)), _buildOptions);
        var options = new EvaluationOptions { OutputFormat = OutputFormat.List };

        // Evaluated one definition at a time (rather than the whole source array) so a schema
        // failure names the offending definition's ID instead of an opaque array index.
        foreach (var definition in pack.DefinitionsIn(family.Name))
        {
            var result = schema.Evaluate(definition.Element, options);
            if (!result.IsValid)
                errors.Add(new ContentValidationError(family.Name, definition.Id,
                    $"failed schema validation in {definition.SourceFile}."));
        }
    }

    private static void ValidateNoDuplicateIds(ContentPack pack, ContentFamilySpec family, List<ContentValidationError> errors)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var definition in pack.DefinitionsIn(family.Name))
        {
            if (!seen.Add(definition.Id))
                errors.Add(new ContentValidationError(family.Name, definition.Id,
                    $"duplicate definition ID within family '{family.Name}' (also found before {definition.SourceFile})."));
        }
    }

    private static void ValidateLocalizationKeys(ContentPack pack, ContentFamilySpec family, List<ContentValidationError> errors)
    {
        foreach (var definition in pack.DefinitionsIn(family.Name))
        {
            foreach (var keyProperty in new[] { "nameKey", "descriptionKey" })
            {
                if (!definition.Element.TryGetProperty(keyProperty, out var keyElement))
                    continue;
                var key = keyElement.GetString()!;
                if (!pack.Localization.ContainsKey(key))
                    errors.Add(new ContentValidationError(family.Name, definition.Id,
                        $"'{keyProperty}' references localization key '{key}', which has no entry in localization/en.json."));
            }
        }
    }

    private static void ValidateReferences(ContentPack pack, List<ContentValidationError> errors)
    {
        var knownIdsByFamily = ContentFamilies.All.ToDictionary(
            static family => family.Name,
            family => new HashSet<string>(pack.DefinitionsIn(family.Name).Select(static d => d.Id), StringComparer.Ordinal));

        foreach (var family in ContentFamilies.All)
        {
            foreach (var definition in pack.DefinitionsIn(family.Name))
            {
                foreach (var reference in family.ExtractReferences(definition.Element))
                {
                    if (!knownIdsByFamily.TryGetValue(reference.TargetFamily, out var knownIds))
                    {
                        errors.Add(new ContentValidationError(family.Name, definition.Id,
                            $"references unknown content family '{reference.TargetFamily}'."));
                        continue;
                    }

                    if (!knownIds.Contains(reference.TargetId))
                        errors.Add(new ContentValidationError(family.Name, definition.Id,
                            $"references undefined {reference.TargetFamily} definition '{reference.TargetId}'."));
                }
            }
        }
    }
}
