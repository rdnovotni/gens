using Gens.Simulation.Identity;

namespace Gens.Simulation.Regions;

/// <summary>
/// The content-authored Region Profile schema (Phase 13 item 1; <c>gens-starting-regions-design.md</c>
/// §4, §12): the fixed checklist every region-specific design document fills out, so two region
/// documents read as two instances of one system rather than two unrelated documents. Every §4
/// subsection is represented — most as a qualitative ref/tag pointing at whichever system actually
/// owns that content family (§4.1 terrain, §4.2 economic character, §4.3 political/legal, §4.4
/// diplomatic/military, §4.5 religious/cultural, §4.6 goods/trade), since "numbers are deferred
/// everywhere... a region document specifies shape and direction, not magnitudes" (§4's own framing).
/// Mirrors <see cref="Gens.Simulation.Goods.GoodDefinition"/>/<see
/// cref="Gens.Simulation.Events.EventDefinition"/>'s identical "sealed record, constructor validates,
/// content is data" shape.
/// </summary>
public sealed record RegionProfileDefinition
{
    public RegionProfileDefinition(
        DefinitionId<RegionProfileDefinition> id,
        string name,
        RegionStatus status,
        string terrainProfileRef,
        string economicCharacterTag,
        string politicalLegalProfileRef,
        string diplomaticMilitaryProfileRef,
        string religiousCulturalDefaultRef,
        string regionalGoodsProfileRef,
        IReadOnlyList<CultureDistributionEntry> cultureDistributionTable,
        DatedRule<ReputationDualityMode> reputationDuality,
        DefinitionId<GazetteerLocationDefinition> homeAnchorLocationId,
        IReadOnlyList<GazetteerLocationDefinition> gazetteer)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A region profile requires a non-empty name.", nameof(name));
        RequireTag(terrainProfileRef, nameof(terrainProfileRef));
        RequireTag(economicCharacterTag, nameof(economicCharacterTag));
        RequireTag(politicalLegalProfileRef, nameof(politicalLegalProfileRef));
        RequireTag(diplomaticMilitaryProfileRef, nameof(diplomaticMilitaryProfileRef));
        RequireTag(religiousCulturalDefaultRef, nameof(religiousCulturalDefaultRef));
        RequireTag(regionalGoodsProfileRef, nameof(regionalGoodsProfileRef));

        if (gazetteer is null || gazetteer.Count == 0)
            throw new ArgumentException("A region profile requires at least one gazetteer entry.", nameof(gazetteer));
        var duplicateLocation = gazetteer.GroupBy(entry => entry.Id).FirstOrDefault(g => g.Count() > 1);
        if (duplicateLocation is not null)
            throw new ArgumentException($"Duplicate gazetteer location ID '{duplicateLocation.Key}'.", nameof(gazetteer));
        var foreignEntry = gazetteer.FirstOrDefault(entry => !entry.RegionId.Equals(id));
        if (foreignEntry is not null)
        {
            throw new ArgumentException(
                $"Gazetteer entry '{foreignEntry.Id}' belongs to region '{foreignEntry.RegionId}', not '{id}'.",
                nameof(gazetteer));
        }
        if (gazetteer.All(entry => !entry.Id.Equals(homeAnchorLocationId)))
        {
            throw new ArgumentException(
                $"Home anchor '{homeAnchorLocationId}' is not a gazetteer entry of region '{id}'.",
                nameof(homeAnchorLocationId));
        }

        if (cultureDistributionTable is null || cultureDistributionTable.Count == 0)
            throw new ArgumentException("A region profile requires at least one culture distribution entry.", nameof(cultureDistributionTable));
        var duplicateCulture = cultureDistributionTable.GroupBy(entry => entry.CultureRef).FirstOrDefault(g => g.Count() > 1);
        if (duplicateCulture is not null)
            throw new ArgumentException($"Duplicate culture reference '{duplicateCulture.Key}'.", nameof(cultureDistributionTable));
        var outlierCount = cultureDistributionTable.Count(entry => entry.IsOutlierResidual);
        if (outlierCount != 1)
        {
            throw new ArgumentException(
                "A culture distribution table needs exactly one outlier-residual entry (§4.7).",
                nameof(cultureDistributionTable));
        }

        Id = id;
        Name = name;
        Status = status;
        TerrainProfileRef = terrainProfileRef;
        EconomicCharacterTag = economicCharacterTag;
        PoliticalLegalProfileRef = politicalLegalProfileRef;
        DiplomaticMilitaryProfileRef = diplomaticMilitaryProfileRef;
        ReligiousCulturalDefaultRef = religiousCulturalDefaultRef;
        RegionalGoodsProfileRef = regionalGoodsProfileRef;
        CultureDistributionTable = cultureDistributionTable;
        ReputationDuality = reputationDuality ?? throw new ArgumentNullException(nameof(reputationDuality));
        HomeAnchorLocationId = homeAnchorLocationId;
        Gazetteer = gazetteer;
    }

    public DefinitionId<RegionProfileDefinition> Id { get; }
    public string Name { get; }
    public RegionStatus Status { get; }

    /// <summary>§4.1 — points into Buildings' terrain-gate table (Coast, River, Forest, Hills/Mountain
    /// + Deposit, Fertility, Meadow); this schema names which gates apply, not the gate table itself.</summary>
    public string TerrainProfileRef { get; }

    /// <summary>§4.2 — qualitative only ("expensive-but-liquid vs. cheap-but-thin"); the numeric
    /// resource envelope stays Start Modes' own territory.</summary>
    public string EconomicCharacterTag { get; }

    /// <summary>§4.3 — cursus honorum access, typical local Faction exposure, baseline Legal Status mix.</summary>
    public string PoliticalLegalProfileRef { get; }

    /// <summary>§4.4 — default neighboring people(s), raid-exposure weighting, regional recruitment flavor.</summary>
    public string DiplomaticMilitaryProfileRef { get; }

    /// <summary>§4.5 — default local cult/pantheon and starting Cultural Drift lean.</summary>
    public string ReligiousCulturalDefaultRef { get; }

    /// <summary>§4.6 — points into Resources &amp; Goods' region-tagged goods table; this schema
    /// names the resulting production identity, not the goods list itself.</summary>
    public string RegionalGoodsProfileRef { get; }

    /// <summary>§4.7 — always includes exactly one outlier-residual row (validated above).</summary>
    public IReadOnlyList<CultureDistributionEntry> CultureDistributionTable { get; }

    /// <summary>§4.10/§6/§12 — the date-aware rule override this item exists to generalize: §6's
    /// tapering shape (Iberian Colony, North African Colony) is expressed as a <see
    /// cref="DatedOverride{TValue}"/> window on top of a base mode.</summary>
    public DatedRule<ReputationDualityMode> ReputationDuality { get; }

    /// <summary>§8.1 — must be one of <see cref="Gazetteer"/>'s own entries (validated above).</summary>
    public DefinitionId<GazetteerLocationDefinition> HomeAnchorLocationId { get; }

    /// <summary>§4.9/§8/§12 — every entry's own <see cref="GazetteerLocationDefinition.RegionId"/> must
    /// equal <see cref="Id"/> (validated above).</summary>
    public IReadOnlyList<GazetteerLocationDefinition> Gazetteer { get; }

    public GazetteerLocationDefinition HomeAnchor => Gazetteer.First(entry => entry.Id.Equals(HomeAnchorLocationId));

    public bool TryGetGazetteerEntry(DefinitionId<GazetteerLocationDefinition> locationId, out GazetteerLocationDefinition entry)
    {
        foreach (var candidate in Gazetteer)
        {
            if (candidate.Id.Equals(locationId))
            {
                entry = candidate;
                return true;
            }
        }

        entry = null!;
        return false;
    }

    /// <summary>This region's Reputation Duality applicability as of <paramref name="date"/> — the
    /// worked, general-purpose consumer of <see cref="ReputationDuality"/>'s date-aware resolution.</summary>
    public ReputationDualityMode ReputationDualityAsOf(Time.GameDate date) => ReputationDuality.EffectiveAsOf(date);

    private static void RequireTag(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A region profile requires a non-empty value for this field.", paramName);
    }
}
