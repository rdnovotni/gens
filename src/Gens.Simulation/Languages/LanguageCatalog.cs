using Gens.Simulation.Identity;

namespace Gens.Simulation.Languages;

/// <summary>Rejects a duplicate language or family ID, and any language whose <see
/// cref="LanguageDefinition.FamilyId"/> doesn't resolve to a registered family, at construction —
/// mirrors <see cref="Cultures.CultureCatalog"/>'s identical shape.</summary>
public sealed class LanguageCatalog
{
    private readonly Dictionary<string, LanguageDefinition> _languages;
    private readonly Dictionary<string, LanguageFamily> _families;

    public LanguageCatalog(IEnumerable<LanguageDefinition> languages, IEnumerable<LanguageFamily> families)
    {
        if (languages is null)
            throw new ArgumentNullException(nameof(languages));
        if (families is null)
            throw new ArgumentNullException(nameof(families));

        var familyMap = new Dictionary<string, LanguageFamily>(StringComparer.Ordinal);
        foreach (var family in families)
        {
            if (!familyMap.TryAdd(family.Id.Value, family))
                throw new ArgumentException($"Duplicate language family ID '{family.Id.Value}'.", nameof(families));
        }

        var languageMap = new Dictionary<string, LanguageDefinition>(StringComparer.Ordinal);
        foreach (var language in languages)
        {
            if (!languageMap.TryAdd(language.Id.Value, language))
                throw new ArgumentException($"Duplicate language ID '{language.Id.Value}'.", nameof(languages));
            if (!familyMap.ContainsKey(language.FamilyId.Value))
            {
                throw new ArgumentException(
                    $"Language '{language.Id.Value}' references unknown family '{language.FamilyId.Value}'.",
                    nameof(languages));
            }
        }

        _languages = languageMap;
        _families = familyMap;
    }

    public int LanguageCount => _languages.Count;
    public int FamilyCount => _families.Count;

    public bool TryGetLanguage(DefinitionId<LanguageDefinition> id, out LanguageDefinition language) =>
        _languages.TryGetValue(id.Value, out language!);

    public LanguageDefinition GetLanguage(DefinitionId<LanguageDefinition> id) =>
        TryGetLanguage(id, out var language)
            ? language
            : throw new KeyNotFoundException($"No language is registered for ID '{id.Value}'.");

    public bool TryGetFamily(DefinitionId<LanguageFamily> id, out LanguageFamily family) =>
        _families.TryGetValue(id.Value, out family!);

    public IEnumerable<LanguageDefinition> AllLanguages() => _languages.Values;

    public IEnumerable<LanguageFamily> AllFamilies() => _families.Values;

    /// <summary>§5's family-relationship acquisition discount, generalized to a pure yes/no capability
    /// check — whether <paramref name="knownLanguage"/> and <paramref name="targetLanguage"/> share a
    /// family that is not an isolate (Basque never discounts, per <see cref="LanguageFamily.IsIsolate"/>).
    /// The actual discount magnitude stays unsized (§11's own open question: "Galatian and Gaulish
    /// mutual intelligibility's own exact discount value... isn't sized here") — this only answers
    /// "does a discount apply at all," which is all a caller needs until that magnitude is authored.</summary>
    public bool SharesNonIsolateFamily(DefinitionId<LanguageDefinition> knownLanguage, DefinitionId<LanguageDefinition> targetLanguage)
    {
        if (!TryGetLanguage(knownLanguage, out var known) || !TryGetLanguage(targetLanguage, out var target))
            return false;
        if (known.FamilyId != target.FamilyId)
            return false;

        return _families.TryGetValue(known.FamilyId.Value, out var family) && !family.IsIsolate;
    }
}
