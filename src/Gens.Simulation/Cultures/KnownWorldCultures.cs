using Gens.Simulation.Identity;
using Gens.Simulation.Regions;
using Gens.Simulation.Time;

namespace Gens.Simulation.Cultures;

/// <summary>
/// The real, authored roster <c>gens-cultures-of-the-known-world-design.md</c> §12/§17 names — every
/// one of the 37 real <c>culture</c> enum values §17's own data model literally lists, each with its
/// real <see cref="CultureCategory"/> and flags. The doc's own intro prose says "thirty-six real,
/// playable cultures," one short of §17's actual enum count — the honest reconciliation, read off
/// §12's own quick-reference table, is that Roman itself is called out there as "— (the default)"
/// rather than one of the thirty-six added entries, so this catalog registers all 37 real enum values
/// (Roman included) rather than silently dropping one to match the prose count. Unlike <see
/// cref="Regions.SampleRegionProfileDefinitions"/>,
/// this is real content, not a fixture — Cultures' own §1 scope is "expand the roster," and every
/// value below reads directly off §3-§10's tables and §12's own quick-reference. §11's Legendary
/// Places (Hyperborea, Thule, the Fortunate Isles) and §3.2/§5.1/§6.1's flavor-only minor sub-groups
/// (Belgae, Cantabrian, Rhodian, Spartan/Laconian, Cypriot, Samaritan, Idumaean) are deliberately not
/// represented here — §17's own data model keeps both explicitly outside the <c>culture</c> enum.
///
/// Category-shift dates are this item's own honest reading of §12's prose column into a concrete <see
/// cref="GameDate"/>: British (Frontier → Provincial, AD 43), Dacian and Nabataean (both AD 106),
/// Egyptian (30 BC), and Illyrian/Pannonian (~AD 9, standing in for Pannonia's own shift — §18's own
/// open question flags the Illyrian/Pannonian internal split as still unresolved, so this catalog
/// tracks the single tag at its later, Pannonian date rather than inventing a two-culture split §17's
/// own enum doesn't have room for). Cilician's "Provincial (post-67 BC)" and every Client-category
/// culture's own conquest/annexation date (Numidian/Mauri, Cappadocian, Judaean, Bosporan) carry no
/// second, pre-annexation Frontier phase in the source prose worth modeling — each resolves as a
/// plain <see cref="CultureCategory.Provincial"/> base value with no override, per <see
/// cref="CultureCategory"/>'s own doc comment on Client resolving as Provincial.
/// </summary>
public static class KnownWorldCultures
{
    public static readonly DefinitionId<Identity.Culture> Roman = new("roman");
    public static readonly DefinitionId<Identity.Culture> Gallic = new("gallic");
    public static readonly DefinitionId<Identity.Culture> Iberian = new("iberian");
    public static readonly DefinitionId<Identity.Culture> Hellenic = new("hellenic");
    public static readonly DefinitionId<Identity.Culture> Germanic = new("germanic");
    public static readonly DefinitionId<Identity.Culture> British = new("british");
    public static readonly DefinitionId<Identity.Culture> Hibernian = new("hibernian");
    public static readonly DefinitionId<Identity.Culture> Caledonian = new("caledonian");
    public static readonly DefinitionId<Identity.Culture> Batavian = new("batavian");
    public static readonly DefinitionId<Identity.Culture> NumidianMauri = new("numidian-mauri");
    public static readonly DefinitionId<Identity.Culture> Punic = new("punic");
    public static readonly DefinitionId<Identity.Culture> Etruscan = new("etruscan");
    public static readonly DefinitionId<Identity.Culture> Galatian = new("galatian");
    public static readonly DefinitionId<Identity.Culture> CappadocianAnatolian = new("cappadocian-anatolian");
    public static readonly DefinitionId<Identity.Culture> Thracian = new("thracian");
    public static readonly DefinitionId<Identity.Culture> Dacian = new("dacian");
    public static readonly DefinitionId<Identity.Culture> IllyrianPannonian = new("illyrian-pannonian");
    public static readonly DefinitionId<Identity.Culture> Cretan = new("cretan");
    public static readonly DefinitionId<Identity.Culture> Judaean = new("judaean");
    public static readonly DefinitionId<Identity.Culture> SyrianLevantine = new("syrian-levantine");
    public static readonly DefinitionId<Identity.Culture> Nabataean = new("nabataean");
    public static readonly DefinitionId<Identity.Culture> Cilician = new("cilician");
    public static readonly DefinitionId<Identity.Culture> Palmyrene = new("palmyrene");
    public static readonly DefinitionId<Identity.Culture> Egyptian = new("egyptian");
    public static readonly DefinitionId<Identity.Culture> AlexandrianGreek = new("alexandrian-greek");
    public static readonly DefinitionId<Identity.Culture> NubianKushite = new("nubian-kushite");
    public static readonly DefinitionId<Identity.Culture> Blemmyes = new("blemmyes");
    public static readonly DefinitionId<Identity.Culture> SarmatianScythian = new("sarmatian-scythian");
    public static readonly DefinitionId<Identity.Culture> Bosporan = new("bosporan");
    public static readonly DefinitionId<Identity.Culture> Parthian = new("parthian");
    public static readonly DefinitionId<Identity.Culture> Armenian = new("armenian");
    public static readonly DefinitionId<Identity.Culture> Indian = new("indian");
    public static readonly DefinitionId<Identity.Culture> Chinese = new("chinese");
    public static readonly DefinitionId<Identity.Culture> Garamantian = new("garamantian");
    public static readonly DefinitionId<Identity.Culture> Aksumite = new("aksumite");
    public static readonly DefinitionId<Identity.Culture> Taprobane = new("taprobane");
    public static readonly DefinitionId<Identity.Culture> Sogdian = new("sogdian");

    private static GameDate AtYear(int astronomicalYear) => new((astronomicalYear - GameDate.EpochAstronomicalYear) * 12);

    public static readonly GameDate BritishShift = AtYear(43);
    public static readonly GameDate DacianShift = AtYear(106);
    public static readonly GameDate NabataeanShift = AtYear(106);
    public static readonly GameDate EgyptianShift = AtYear(-30);
    public static readonly GameDate PannonianShift = AtYear(9);

    public static CultureCatalog BuildCatalog() => new(new[]
    {
        Provincial(Roman, "Roman"),
        Provincial(Gallic, "Gallic"),
        Provincial(Iberian, "Iberian"),
        Provincial(Hellenic, "Hellenic"),
        Frontier(Germanic, "Germanic"),
        new CultureDefinition(British, "British", ShiftingToProvincial(BritishShift)),
        Frontier(Hibernian, "Hibernian", permanentlyUnconquered: true),
        Frontier(Caledonian, "Caledonian", permanentlyUnconquered: true),
        Frontier(Batavian, "Batavian", isAuxiliaryServiceCulture: true),
        Provincial(NumidianMauri, "Numidian/Mauri"),
        Provincial(Punic, "Punic"),
        Provincial(Etruscan, "Etruscan"),
        Provincial(Galatian, "Galatian"),
        Provincial(CappadocianAnatolian, "Cappadocian/Anatolian"),
        Provincial(Thracian, "Thracian"),
        new CultureDefinition(Dacian, "Dacian", ShiftingToProvincial(DacianShift)),
        new CultureDefinition(IllyrianPannonian, "Illyrian/Pannonian", ShiftingToProvincial(PannonianShift)),
        Provincial(Cretan, "Cretan", isAuxiliaryServiceCulture: true),
        Provincial(Judaean, "Judaean"),
        Provincial(SyrianLevantine, "Syrian/Levantine"),
        new CultureDefinition(Nabataean, "Nabataean", ShiftingToProvincial(NabataeanShift)),
        Provincial(Cilician, "Cilician"),
        Provincial(Palmyrene, "Palmyrene"),
        new CultureDefinition(Egyptian, "Egyptian", ShiftingToProvincial(EgyptianShift)),
        Provincial(AlexandrianGreek, "Alexandrian Greek"),
        Frontier(NubianKushite, "Nubian/Kushite", permanentlyUnconquered: true),
        Frontier(Blemmyes, "Blemmyes", isRaidingFrontier: true),
        Frontier(SarmatianScythian, "Sarmatian/Scythian"),
        Provincial(Bosporan, "Bosporan"),
        new CultureDefinition(Parthian, "Parthian", new DatedRule<CultureCategory>(CultureCategory.GreatPower)),
        new CultureDefinition(Armenian, "Armenian", new DatedRule<CultureCategory>(CultureCategory.ContestedBuffer)),
        TradeContact(Indian, "Indian", EncounterRarityTier.Rare),
        TradeContact(Chinese, "Chinese", EncounterRarityTier.ExceptionallyRare),
        TradeContact(Garamantian, "Garamantian", EncounterRarityTier.Rare),
        TradeContact(Aksumite, "Aksumite", EncounterRarityTier.Rare),
        TradeContact(Taprobane, "Taprobane", EncounterRarityTier.Rare),
        TradeContact(Sogdian, "Sogdian", EncounterRarityTier.Rare),
    });

    private static CultureDefinition Provincial(
        DefinitionId<Identity.Culture> id, string name, bool isAuxiliaryServiceCulture = false) =>
        new(id, name, new DatedRule<CultureCategory>(CultureCategory.Provincial), isAuxiliaryServiceCulture: isAuxiliaryServiceCulture);

    private static CultureDefinition Frontier(
        DefinitionId<Identity.Culture> id, string name,
        bool permanentlyUnconquered = false, bool isRaidingFrontier = false, bool isAuxiliaryServiceCulture = false) =>
        new(
            id, name, new DatedRule<CultureCategory>(CultureCategory.Frontier),
            permanentlyUnconquered: permanentlyUnconquered, isRaidingFrontier: isRaidingFrontier,
            isAuxiliaryServiceCulture: isAuxiliaryServiceCulture);

    private static CultureDefinition TradeContact(DefinitionId<Identity.Culture> id, string name, EncounterRarityTier tier) =>
        new(id, name, new DatedRule<CultureCategory>(CultureCategory.TradeContactOnly), encounterRarityTier: tier, noveltyDignitasBonus: true);

    private static DatedRule<CultureCategory> ShiftingToProvincial(GameDate shiftYear) =>
        new(
            CultureCategory.Frontier,
            new[] { new DatedOverride<CultureCategory>(CultureCategory.Provincial, effectiveFrom: shiftYear) });
}
