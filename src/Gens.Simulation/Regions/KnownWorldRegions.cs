using Gens.Simulation.Cultures;
using Gens.Simulation.Identity;

namespace Gens.Simulation.Regions;

/// <summary>
/// Phase 13 item 6's real, authored region content — "implement one complete region profile and only
/// then expand region content in waves," per the roadmap's own item 1 note that item 6's actual
/// authored content stays out of that item's scope. This is the first wave: <b>Latium</b>, filled in
/// from <c>gens-starting-regions-italian-heartland-design.md</c> §3 against the Region Profile schema
/// item 1 built. Latium is the natural region to start with — it is the launch roster's most central
/// region (Rome's own immediate political backyard, per that design document's §5), and its own
/// simplicity (no Reputation Duality, no dated rule overrides, per §2's "Shared Italian Identity")
/// keeps this first wave a clean, uncomplicated proof that authored content, not just fixture content,
/// fits the schema end to end. Campania and the rest of the launch roster (§5.1 of that document) are
/// explicitly a future wave, matching this same item's own construction-order framing.
///
/// Rome itself (§5 of that document) "belongs to neither Latium nor Campania exclusively," but the
/// schema requires every <see cref="GazetteerLocationDefinition"/> to declare one owning region, and
/// only Latium exists yet — so Rome is seated here as a Latium gazetteer entry (the historically
/// accurate reading: Rome sits geographically within Latium proper), carrying the catalog-unique
/// <see cref="GazetteerRole.Capital"/> role. A future Campania wave does not re-seat Rome; it only
/// gains the shorter Distance Tier relationship §6 of that document describes.
/// </summary>
public static class KnownWorldRegions
{
    public static readonly DefinitionId<RegionProfileDefinition> Latium = new("latium");

    public static readonly DefinitionId<GazetteerLocationDefinition> Rome = new("rome");
    public static readonly DefinitionId<GazetteerLocationDefinition> Ostia = new("ostia");
    public static readonly DefinitionId<GazetteerLocationDefinition> Tusculum = new("tusculum");
    public static readonly DefinitionId<GazetteerLocationDefinition> Praeneste = new("praeneste");
    public static readonly DefinitionId<GazetteerLocationDefinition> Tibur = new("tibur");
    public static readonly DefinitionId<GazetteerLocationDefinition> Antium = new("antium");
    public static readonly DefinitionId<GazetteerLocationDefinition> AlbaLonga = new("alba-longa");
    public static readonly DefinitionId<GazetteerLocationDefinition> Lavinium = new("lavinium");
    public static readonly DefinitionId<GazetteerLocationDefinition> Gabii = new("gabii");

    public static RegionProfileCatalog BuildCatalog() => new(new[] { BuildLatium() });

    /// <summary>§3 in full: terrain (§3.1), economic package (§3.2), political/legal texture (§3.3),
    /// diplomatic/military exposure (§3.4), religious/cultural defaults (§3.5), regional goods (§3.6),
    /// population/culture distribution (§3.7), the Gazetteer (§3.8, plus Rome per this class's own doc
    /// comment), and the Home Anchor (§3.10, Tusculum). §3.9's Rival Seeding and §3.11's Templated
    /// Background flavor live outside this schema's own fields (no typed field exists for either yet,
    /// per <see cref="GazetteerLocationDefinition.RivalSeatHouseId"/>'s own "item 6/9 territory, not
    /// this schema's" note) and are carried instead as each seated Gazetteer entry's free-form
    /// <c>rivalSeatHouseId</c> tag.</summary>
    public static RegionProfileDefinition BuildLatium()
    {
        var rome = new GazetteerLocationDefinition(
            id: Rome,
            regionId: Latium,
            name: "Rome",
            roles: new[] { GazetteerRole.Capital, GazetteerRole.MarketHub },
            prominenceTier: ProminenceTier.ProvincialSeat,
            groundingNote:
                "The single unique seat of the cursus honorum, the Senate, and the Vestal institution " +
                "(§5) — outside the ordinary Provincial Seat category since Italy was never organized as " +
                "a province under the Principate, but geographically Latium's own city all the same.",
            rivalSeatHouseId: "gens-fabricia");

        var ostia = new GazetteerLocationDefinition(
            id: Ostia,
            regionId: Latium,
            name: "Ostia",
            roles: new[] { GazetteerRole.MajorPort, GazetteerRole.MarketHub },
            prominenceTier: ProminenceTier.RegionalCenter,
            groundingNote:
                "Rome's own real port and the empire's grain-import gateway (§3.2) — the physical " +
                "location where Latium's own supply-dependency vulnerability actually lands — and the " +
                "real home of Latium's own Via Salaria salt-pan tradition (§3.1).");

        var tusculum = new GazetteerLocationDefinition(
            id: Tusculum,
            regionId: Latium,
            name: "Tusculum",
            roles: new[] { GazetteerRole.MarketHub },
            prominenceTier: ProminenceTier.RegionalCenter,
            groundingNote:
                "A real, favored country-villa retreat for Rome's senatorial class — the region's own " +
                "Home Anchor (§3.10), close enough for Latium's political proximity to feel immediate.",
            rivalSeatHouseId: "gens-octavinia");

        var praeneste = new GazetteerLocationDefinition(
            id: Praeneste,
            regionId: Latium,
            name: "Praeneste",
            roles: new[] { GazetteerRole.Sanctuary },
            prominenceTier: ProminenceTier.RegionalCenter,
            groundingNote:
                "Home to the real, genuinely major Temple of Fortuna Primigenia — one of the largest " +
                "religious complexes in the ancient Italian world.",
            rivalSeatHouseId: "gens-sergiana");

        var tibur = new GazetteerLocationDefinition(
            id: Tibur,
            regionId: Latium,
            name: "Tibur",
            roles: new[] { GazetteerRole.MarketHub },
            prominenceTier: ProminenceTier.RegionalCenter,
            groundingNote:
                "Modern Tivoli — another real elite retreat town, kept deliberately generic here so it " +
                "stays plausible across this project's own flexible era range.");

        var antium = new GazetteerLocationDefinition(
            id: Antium,
            regionId: Latium,
            name: "Antium",
            roles: new[] { GazetteerRole.MajorPort },
            prominenceTier: ProminenceTier.RegionalCenter,
            groundingNote:
                "A real coastal town with its own genuine elite-villa history and port function, giving " +
                "Latium a second, lesser coastal outlet beyond Ostia.");

        var albaLonga = new GazetteerLocationDefinition(
            id: AlbaLonga,
            regionId: Latium,
            name: "Alba Longa",
            roles: new[] { GazetteerRole.Sanctuary },
            prominenceTier: ProminenceTier.Outpost,
            groundingNote:
                "The real, legendary mother-city of Rome itself in Roman foundation myth — mechanically " +
                "minor, but an unmatched Dynasty Chronicle and Religion flavor location.");

        var lavinium = new GazetteerLocationDefinition(
            id: Lavinium,
            regionId: Latium,
            name: "Lavinium",
            roles: new[] { GazetteerRole.Sanctuary },
            prominenceTier: ProminenceTier.Outpost,
            groundingNote:
                "A real, genuinely ancient Latin town with its own foundational role in Roman myth " +
                "(traditionally linked to Aeneas) and a real historical seat of shared Latin League " +
                "religious ritual.");

        var gabii = new GazetteerLocationDefinition(
            id: Gabii,
            regionId: Latium,
            name: "Gabii",
            roles: new[] { GazetteerRole.MarketHub },
            prominenceTier: ProminenceTier.Outpost,
            groundingNote:
                "A real, genuinely ancient Latin town, notable historically for retaining a distinct " +
                "local ritual/augural tradition of its own even after full absorption into Rome's orbit.",
            rivalSeatHouseId: "gens-considia");

        var cultureDistribution = new[]
        {
            new CultureDistributionEntry(KnownWorldCultures.Roman.Value, weight: 85),
            new CultureDistributionEntry(
                KnownWorldCultures.Etruscan.Value, weight: 10),
            new CultureDistributionEntry("outlier", weight: 5, isOutlierResidual: true),
        };

        var reputationDuality = new DatedRule<ReputationDualityMode>(baseValue: ReputationDualityMode.None);

        return new RegionProfileDefinition(
            id: Latium,
            name: "Latium",
            status: RegionStatus.Launch,
            terrainProfileRef: "river-plain-fertile-no-mineral-deposits-salt-pans",
            economicCharacterTag: "most-expensive-land-thin-expansion-grain-import-dependent",
            politicalLegalProfileRef: "maximum-curia-contest-fastest-cursus-honorum-citizen-majority",
            diplomaticMilitaryProfileRef: "no-frontier-patronage-officer-recruitment-urban-cohort-security",
            religiousCulturalDefaultRef: "roman-state-religion-mos-maiorum-etruscan-haruspicy-residue",
            regionalGoodsProfileRef: "wine-olive-oil-salt-peperino-stone-identity",
            cultureDistributionTable: cultureDistribution,
            reputationDuality: reputationDuality,
            homeAnchorLocationId: Tusculum,
            gazetteer: new[] { rome, ostia, tusculum, praeneste, tibur, antium, albaLonga, lavinium, gabii });
    }
}
