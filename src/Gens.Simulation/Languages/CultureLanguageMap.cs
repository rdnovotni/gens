using Gens.Simulation.Identity;

namespace Gens.Simulation.Languages;

/// <summary>§5's native-acquisition source: "a Character's own origin culture... grants Fluent/Native
/// status in that language's own group automatically." Maps each of <see
/// cref="Cultures.KnownWorldCultures"/>'s thirty-six cultures to the one native language §2's own prose
/// most directly ties it to. Deliberately a single language per culture, not a weighted secondary-
/// language list — §2's own narrative asides about a default second language (Galatian speakers'
/// "real Greek proficiency alongside their own native tongue," §2.3; a multilingual elite generally,
/// §8) describe a real texture this map doesn't also encode as an automatic grant, since §11 leaves
/// "all numeric sizing" and any acquisition-rate mechanic open — a caller wanting that texture uses <see
/// cref="LanguageAcquisitionMethod.FormalEducation"/> or <see
/// cref="LanguageAcquisitionMethod.SustainedExposure"/> exactly like any other second language.
///
/// Three cultures are deliberately left unmapped — Blemmyes, Garamantian, and Taprobane — because §2
/// never names a language for any of them (a real, honest gap in the source document, not an oversight
/// here); <see cref="Resolve"/> returns <c>null</c> for these, matching <see
/// cref="Correspondence.CorrespondenceReachabilityCatalog"/>'s own "an honest, disclosed gap resolves to
/// a safe default rather than a fabricated answer" precedent.</summary>
public sealed class CultureLanguageMap
{
    private readonly Dictionary<string, DefinitionId<LanguageDefinition>> _entries;

    public CultureLanguageMap(IEnumerable<KeyValuePair<DefinitionId<Identity.Culture>, DefinitionId<LanguageDefinition>>> entries)
    {
        if (entries is null)
            throw new ArgumentNullException(nameof(entries));

        var map = new Dictionary<string, DefinitionId<LanguageDefinition>>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            if (!map.TryAdd(entry.Key.Value, entry.Value))
                throw new ArgumentException($"Duplicate culture-to-language mapping for '{entry.Key.Value}'.", nameof(entries));
        }

        _entries = map;
    }

    public DefinitionId<LanguageDefinition>? Resolve(DefinitionId<Identity.Culture> cultureId) =>
        _entries.TryGetValue(cultureId.Value, out var language) ? language : null;

    public static CultureLanguageMap BuildKnownWorldMap() =>
        new(new Dictionary<DefinitionId<Identity.Culture>, DefinitionId<LanguageDefinition>>
        {
            [Cultures.KnownWorldCultures.Roman] = KnownWorldLanguages.Latin,
            [Cultures.KnownWorldCultures.Gallic] = KnownWorldLanguages.Gaulish,
            [Cultures.KnownWorldCultures.Iberian] = KnownWorldLanguages.IberianCeltiberian,
            [Cultures.KnownWorldCultures.Hellenic] = KnownWorldLanguages.GreekKoine,
            [Cultures.KnownWorldCultures.Germanic] = KnownWorldLanguages.Germanic,
            [Cultures.KnownWorldCultures.British] = KnownWorldLanguages.Brythonic,
            [Cultures.KnownWorldCultures.Hibernian] = KnownWorldLanguages.Goidelic,
            [Cultures.KnownWorldCultures.Caledonian] = KnownWorldLanguages.CaledonianPictish,
            [Cultures.KnownWorldCultures.Batavian] = KnownWorldLanguages.Germanic,
            [Cultures.KnownWorldCultures.NumidianMauri] = KnownWorldLanguages.NumidianBerber,
            [Cultures.KnownWorldCultures.Punic] = KnownWorldLanguages.Punic,
            [Cultures.KnownWorldCultures.Etruscan] = KnownWorldLanguages.Latin,
            [Cultures.KnownWorldCultures.Galatian] = KnownWorldLanguages.GalatianLanguage,
            [Cultures.KnownWorldCultures.CappadocianAnatolian] = KnownWorldLanguages.Cappadocian,
            [Cultures.KnownWorldCultures.Thracian] = KnownWorldLanguages.IllyrianThracianDacian,
            [Cultures.KnownWorldCultures.Dacian] = KnownWorldLanguages.IllyrianThracianDacian,
            [Cultures.KnownWorldCultures.IllyrianPannonian] = KnownWorldLanguages.IllyrianThracianDacian,
            [Cultures.KnownWorldCultures.Cretan] = KnownWorldLanguages.GreekKoine,
            [Cultures.KnownWorldCultures.Judaean] = KnownWorldLanguages.Aramaic,
            [Cultures.KnownWorldCultures.SyrianLevantine] = KnownWorldLanguages.Aramaic,
            [Cultures.KnownWorldCultures.Nabataean] = KnownWorldLanguages.Aramaic,
            [Cultures.KnownWorldCultures.Cilician] = KnownWorldLanguages.GreekKoine,
            [Cultures.KnownWorldCultures.Palmyrene] = KnownWorldLanguages.Aramaic,
            [Cultures.KnownWorldCultures.Egyptian] = KnownWorldLanguages.EgyptianDemotic,
            [Cultures.KnownWorldCultures.AlexandrianGreek] = KnownWorldLanguages.GreekKoine,
            [Cultures.KnownWorldCultures.NubianKushite] = KnownWorldLanguages.Meroitic,
            [Cultures.KnownWorldCultures.SarmatianScythian] = KnownWorldLanguages.SarmatianScythian,
            [Cultures.KnownWorldCultures.Bosporan] = KnownWorldLanguages.GreekKoine,
            [Cultures.KnownWorldCultures.Parthian] = KnownWorldLanguages.Parthian,
            [Cultures.KnownWorldCultures.Armenian] = KnownWorldLanguages.Armenian,
            [Cultures.KnownWorldCultures.Indian] = KnownWorldLanguages.SanskritPrakrit,
            [Cultures.KnownWorldCultures.Chinese] = KnownWorldLanguages.Chinese,
            [Cultures.KnownWorldCultures.Aksumite] = KnownWorldLanguages.Geez,
            [Cultures.KnownWorldCultures.Sogdian] = KnownWorldLanguages.Sogdian,
        });
}
