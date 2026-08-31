using Gens.Simulation.Cultures;

namespace Gens.Simulation.Correspondence;

/// <summary>
/// The real (culture, reachability) content <see cref="CorrespondenceReachabilityCatalog"/>'s own doc
/// comment names as Phase 13 item 4's job, now buildable against <see cref="KnownWorldCultures"/> and
/// <see cref="Languages.KnownWorldLanguages"/> — this item's own real culture and language catalogs.
///
/// §7's own text names three cultures explicitly (<see cref="KnownWorldCultures.Gallic"/>, <see
/// cref="KnownWorldCultures.British"/>, <see cref="KnownWorldCultures.Germanic"/> — "Gallic/British/
/// Germanic religious or druidic-adjacent leadership") plus its own deliberately open-ended "by
/// extension" for Cultures' own Thin-Record peoples. This item's honest, disclosed reading of that
/// extension: every culture §7 names directly, every other culture sharing that same druidic-adjacent
/// religious tradition (Hibernian, Caledonian — both named in Cultures' own §3 table as "related to but
/// genuinely separate from the British/Gallic druidic complex"; Batavian, a named Germanic sub-tribe),
/// and every culture whose own native language (<see cref="Languages.CultureLanguageMap"/>) resolves
/// into one of <see cref="Languages.KnownWorldLanguages"/>'s own thinly-attested families (Balkan,
/// British, Anatolian) — the direct mechanical reading of "Thin-Record." Nubian/Kushite gets the single
/// <see cref="CorrespondenceReachability.OralTraditionBlocked"/> entry on its own distinct real-historical
/// grounds: Meroitic (§2.10 of the language doc) is a real, attested written script modern scholarship
/// still has not fully deciphered — the honest, disclosed reading of §7's own "some content simply
/// cannot be transmitted this way at all" extreme case, not an oral-tradition culture at all but a
/// written one this project's own fiction cannot reliably render. Every other real culture on the
/// thirty-seven-value roster is left at the catalog's own honest <see
/// cref="CorrespondenceReachability.FullyLiterate"/> default.
/// </summary>
public static class KnownWorldCorrespondenceReachability
{
    public static CorrespondenceReachabilityCatalog BuildCatalog() => new(new[]
    {
        Partial(KnownWorldCultures.Gallic),
        Partial(KnownWorldCultures.British),
        Partial(KnownWorldCultures.Hibernian),
        Partial(KnownWorldCultures.Caledonian),
        Partial(KnownWorldCultures.Germanic),
        Partial(KnownWorldCultures.Batavian),
        Partial(KnownWorldCultures.Thracian),
        Partial(KnownWorldCultures.Dacian),
        Partial(KnownWorldCultures.IllyrianPannonian),
        Partial(KnownWorldCultures.CappadocianAnatolian),
        Blocked(KnownWorldCultures.NubianKushite),
    });

    private static CultureReachabilityEntry Partial(Identity.DefinitionId<Identity.Culture> cultureId) =>
        new(cultureId, CorrespondenceReachability.OralTraditionPartial);

    private static CultureReachabilityEntry Blocked(Identity.DefinitionId<Identity.Culture> cultureId) =>
        new(cultureId, CorrespondenceReachability.OralTraditionBlocked);
}
