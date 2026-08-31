using Gens.Simulation.Identity;

namespace Gens.Simulation.Languages;

/// <summary>
/// The real linguistic geography §2 lays out, grouped by family exactly as §2's own subsections do.
/// Real content, not a fixture — mirrors <see cref="Cultures.KnownWorldCultures"/>'s identical "the
/// design doc's own real roster, authored directly" status. Deliberately excludes §2.11's two
/// ritual-only extinct languages (see <see cref="LanguageDefinition"/>'s own doc comment) and the
/// three §2.10 "footnote" tongues design explicitly calls out as thin remnants without their own
/// living Proficiency option — Phrygian, Umbrian, Ligurian, Venetic (§2.1, §2.10's own closing lines)
/// stay unlanguaged the same way Etruscan/Sicel stay unlanguaged, since §2 itself never gives any of
/// them more than a footnote mention, unlike every language actually catalogued below.
///
/// One real, disclosed gap-fill: §2 has no dedicated entry for a plain "Germanic" language, despite
/// §6's own hard-gate example naming "Gallic Frontier's own Germanic neighbor" as exactly the kind of
/// negotiation the gate applies to — the Germanic culture needs a native language for that example (and
/// for native acquisition, §5) to resolve at all. <see cref="Germanic"/> below closes that gap, the same
/// spirit as this design pass's own stated Oscan/Noric/Phrygian corrections, openly named rather than
/// silently patched.
/// </summary>
public static class KnownWorldLanguages
{
    // Families (§2's own subsection groupings).
    public static readonly DefinitionId<LanguageFamily> Italic = new("italic");
    public static readonly DefinitionId<LanguageFamily> HellenicFamily = new("hellenic-family");
    public static readonly DefinitionId<LanguageFamily> Celtic = new("celtic");
    public static readonly DefinitionId<LanguageFamily> GermanicFamily = new("germanic-family");
    public static readonly DefinitionId<LanguageFamily> SemiticNorthwest = new("semitic-northwest");
    public static readonly DefinitionId<LanguageFamily> SemiticWest = new("semitic-west");
    public static readonly DefinitionId<LanguageFamily> SemiticSouth = new("semitic-south");
    public static readonly DefinitionId<LanguageFamily> SemiticEthiopic = new("semitic-ethiopic");
    public static readonly DefinitionId<LanguageFamily> AfroasiaticEgyptian = new("afroasiatic-egyptian");
    public static readonly DefinitionId<LanguageFamily> AfroasiaticBerber = new("afroasiatic-berber");
    public static readonly DefinitionId<LanguageFamily> Iranian = new("iranian");
    public static readonly DefinitionId<LanguageFamily> ArmenianBranch = new("armenian-branch");
    public static readonly DefinitionId<LanguageFamily> IndoAryan = new("indo-aryan");
    public static readonly DefinitionId<LanguageFamily> SinoTibetan = new("sino-tibetan");
    public static readonly DefinitionId<LanguageFamily> BasqueIsolate = new("basque-isolate");
    public static readonly DefinitionId<LanguageFamily> IberianCeltiberianFamily = new("iberian-celtiberian");
    public static readonly DefinitionId<LanguageFamily> MeroiticFamily = new("meroitic-family");
    public static readonly DefinitionId<LanguageFamily> BalkanThin = new("balkan-thin");
    public static readonly DefinitionId<LanguageFamily> AlpineThin = new("alpine-thin");
    public static readonly DefinitionId<LanguageFamily> BritishThin = new("british-thin");
    public static readonly DefinitionId<LanguageFamily> AnatolianThin = new("anatolian-thin");
    public static readonly DefinitionId<LanguageFamily> CypriotThin = new("cypriot-thin");

    // Languages (§2.1-§2.10).
    public static readonly DefinitionId<LanguageDefinition> Latin = new("latin");
    public static readonly DefinitionId<LanguageDefinition> Oscan = new("oscan");
    public static readonly DefinitionId<LanguageDefinition> GreekKoine = new("greek-koine");
    public static readonly DefinitionId<LanguageDefinition> Gaulish = new("gaulish");
    public static readonly DefinitionId<LanguageDefinition> Brythonic = new("brythonic");
    public static readonly DefinitionId<LanguageDefinition> Goidelic = new("goidelic");
    public static readonly DefinitionId<LanguageDefinition> GalatianLanguage = new("galatian-language");
    public static readonly DefinitionId<LanguageDefinition> Noric = new("noric");
    public static readonly DefinitionId<LanguageDefinition> Germanic = new("germanic-language");
    public static readonly DefinitionId<LanguageDefinition> Punic = new("punic-language");
    public static readonly DefinitionId<LanguageDefinition> Aramaic = new("aramaic");
    public static readonly DefinitionId<LanguageDefinition> Hebrew = new("hebrew");
    public static readonly DefinitionId<LanguageDefinition> SouthArabian = new("south-arabian");
    public static readonly DefinitionId<LanguageDefinition> EgyptianDemotic = new("egyptian-demotic");
    public static readonly DefinitionId<LanguageDefinition> Parthian = new("parthian-language");
    public static readonly DefinitionId<LanguageDefinition> SarmatianScythian = new("sarmatian-scythian-language");
    public static readonly DefinitionId<LanguageDefinition> Sogdian = new("sogdian-language");
    public static readonly DefinitionId<LanguageDefinition> Armenian = new("armenian-language");
    public static readonly DefinitionId<LanguageDefinition> SanskritPrakrit = new("sanskrit-prakrit");
    public static readonly DefinitionId<LanguageDefinition> Chinese = new("chinese-language");
    public static readonly DefinitionId<LanguageDefinition> Geez = new("geez");
    public static readonly DefinitionId<LanguageDefinition> BasqueAquitanian = new("basque-aquitanian");
    public static readonly DefinitionId<LanguageDefinition> NumidianBerber = new("numidian-berber");
    public static readonly DefinitionId<LanguageDefinition> IberianCeltiberian = new("iberian-celtiberian-language");
    public static readonly DefinitionId<LanguageDefinition> Meroitic = new("meroitic");
    public static readonly DefinitionId<LanguageDefinition> IllyrianThracianDacian = new("illyrian-thracian-dacian");
    public static readonly DefinitionId<LanguageDefinition> Rhaetic = new("rhaetic");
    public static readonly DefinitionId<LanguageDefinition> CaledonianPictish = new("caledonian-pictish");
    public static readonly DefinitionId<LanguageDefinition> Cappadocian = new("cappadocian-language");
    public static readonly DefinitionId<LanguageDefinition> Eteocypriot = new("eteocypriot");

    public static LanguageCatalog BuildCatalog() => new(
        languages: new[]
        {
            new LanguageDefinition(Latin, "Latin", Italic),
            new LanguageDefinition(Oscan, "Oscan", Italic),
            new LanguageDefinition(GreekKoine, "Greek (Koine)", HellenicFamily),
            new LanguageDefinition(Gaulish, "Gaulish", Celtic),
            new LanguageDefinition(Brythonic, "Brythonic", Celtic),
            new LanguageDefinition(Goidelic, "Goidelic", Celtic),
            new LanguageDefinition(GalatianLanguage, "Galatian", Celtic),
            new LanguageDefinition(Noric, "Noric", Celtic),
            new LanguageDefinition(Germanic, "Germanic", GermanicFamily),
            new LanguageDefinition(Punic, "Punic", SemiticWest),
            new LanguageDefinition(Aramaic, "Aramaic", SemiticNorthwest),
            new LanguageDefinition(Hebrew, "Hebrew", SemiticNorthwest),
            new LanguageDefinition(SouthArabian, "South Arabian", SemiticSouth),
            new LanguageDefinition(EgyptianDemotic, "Egyptian (Demotic)", AfroasiaticEgyptian),
            new LanguageDefinition(Parthian, "Parthian", Iranian),
            new LanguageDefinition(SarmatianScythian, "Sarmatian/Scythian", Iranian),
            new LanguageDefinition(Sogdian, "Sogdian", Iranian),
            new LanguageDefinition(Armenian, "Armenian", ArmenianBranch),
            new LanguageDefinition(SanskritPrakrit, "Sanskrit/Prakrit", IndoAryan),
            new LanguageDefinition(Chinese, "Chinese", SinoTibetan),
            new LanguageDefinition(Geez, "Ge'ez", SemiticEthiopic),
            new LanguageDefinition(BasqueAquitanian, "Basque/Aquitanian", BasqueIsolate),
            new LanguageDefinition(NumidianBerber, "Numidian/Berber", AfroasiaticBerber),
            new LanguageDefinition(IberianCeltiberian, "Iberian/Celtiberian", IberianCeltiberianFamily),
            new LanguageDefinition(Meroitic, "Meroitic", MeroiticFamily),
            new LanguageDefinition(IllyrianThracianDacian, "Illyrian/Thracian/Dacian", BalkanThin),
            new LanguageDefinition(Rhaetic, "Rhaetic", AlpineThin),
            new LanguageDefinition(CaledonianPictish, "Caledonian (Pictish-ancestral)", BritishThin),
            new LanguageDefinition(Cappadocian, "Cappadocian", AnatolianThin),
            new LanguageDefinition(Eteocypriot, "Eteocypriot", CypriotThin),
        },
        families: new[]
        {
            new LanguageFamily(Italic, "Italic", new[] { Latin, Oscan }),
            new LanguageFamily(HellenicFamily, "Hellenic", new[] { GreekKoine }),
            new LanguageFamily(Celtic, "Celtic", new[] { Gaulish, Brythonic, Goidelic, GalatianLanguage, Noric }),
            new LanguageFamily(GermanicFamily, "Germanic", new[] { Germanic }),
            new LanguageFamily(SemiticNorthwest, "Semitic (Northwest)", new[] { Aramaic, Hebrew }),
            new LanguageFamily(SemiticWest, "Semitic (West/Punic)", new[] { Punic }),
            new LanguageFamily(SemiticSouth, "Semitic (South Arabian)", new[] { SouthArabian }),
            new LanguageFamily(SemiticEthiopic, "Semitic (Ethiopic)", new[] { Geez }),
            new LanguageFamily(AfroasiaticEgyptian, "Afroasiatic (Egyptian)", new[] { EgyptianDemotic }),
            new LanguageFamily(AfroasiaticBerber, "Afroasiatic (Berber)", new[] { NumidianBerber }),
            new LanguageFamily(Iranian, "Iranian", new[] { Parthian, SarmatianScythian, Sogdian }),
            new LanguageFamily(ArmenianBranch, "Armenian (own Indo-European branch)", new[] { Armenian }),
            new LanguageFamily(IndoAryan, "Indo-Aryan", new[] { SanskritPrakrit }),
            new LanguageFamily(SinoTibetan, "Sino-Tibetan", new[] { Chinese }),
            new LanguageFamily(BasqueIsolate, "Basque isolate", new[] { BasqueAquitanian }, isIsolate: true),
            new LanguageFamily(IberianCeltiberianFamily, "Iberian/Celtiberian", new[] { IberianCeltiberian }),
            new LanguageFamily(MeroiticFamily, "Meroitic", new[] { Meroitic }),
            new LanguageFamily(BalkanThin, "Balkan (thinly attested)", new[] { IllyrianThracianDacian }),
            new LanguageFamily(AlpineThin, "Alpine (thinly attested)", new[] { Rhaetic }),
            new LanguageFamily(BritishThin, "British (thinly attested)", new[] { CaledonianPictish }),
            new LanguageFamily(AnatolianThin, "Anatolian (thinly attested)", new[] { Cappadocian }),
            new LanguageFamily(CypriotThin, "Cypriot (pre-Greek)", new[] { Eteocypriot }),
        });
}
