using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Regions;

/// <summary>
/// A small, self-contained worked example proving every field of the Region Profile schema and the
/// date-aware override mechanism end-to-end (Phase 13 item 1), mirroring <see
/// cref="Gens.Simulation.Events.SampleEventDefinitions"/>'s identical "vertical slice, not the final
/// content ceiling" framing. This is a fixture region, not an authored one — item 6 ("implement one
/// complete region profile") and the subsequent region-content waves are explicitly out of this item's
/// scope. The dated override reproduces §6's own tapering case: Reputation Duality reads <see
/// cref="ReputationDualityMode.Full"/> before the Cantabrian Wars close and <see
/// cref="ReputationDualityMode.Tapering"/> from that date on.
/// </summary>
public static class SampleRegionProfileDefinitions
{
    public static readonly DefinitionId<RegionProfileDefinition> SampleFrontier = new("sample-frontier-colony");
    public static readonly DefinitionId<GazetteerLocationDefinition> HomeAnchorLocation = new("sample-provincial-seat");
    public static readonly DefinitionId<GazetteerLocationDefinition> OutpostLocation = new("sample-frontier-outpost");

    /// <summary>The astronomical-year stand-in for the Cantabrian Wars' historical close (29-19 BC,
    /// §6) — the boundary this fixture's own dated override resolves against.</summary>
    public static readonly GameDate ConquestArcCloses = new((-19 - GameDate.EpochAstronomicalYear) * 12);

    public static RegionProfileCatalog BuildCatalog() => new(new[] { BuildSampleFrontier() });

    public static RegionProfileDefinition BuildSampleFrontier()
    {
        var homeAnchor = new GazetteerLocationDefinition(
            id: HomeAnchorLocation,
            regionId: SampleFrontier,
            name: "Sample Provincial Seat",
            roles: new[] { GazetteerRole.ProvincialSeat, GazetteerRole.MarketHub },
            prominenceTier: ProminenceTier.ProvincialSeat,
            groundingNote: "Stands in for a real regional capital; a fixture, not authored content (item 6 is out of scope).",
            rivalSeatHouseId: "sample-rival-house");

        var outpost = new GazetteerLocationDefinition(
            id: OutpostLocation,
            regionId: SampleFrontier,
            name: "Sample Frontier Outpost",
            roles: new[] { GazetteerRole.FrontierOutpost },
            prominenceTier: ProminenceTier.Outpost,
            groundingNote: "Stands in for a real frontier post anchoring a neighboring people (§8.3).");

        var cultureDistribution = new[]
        {
            new CultureDistributionEntry("sample-dominant-culture", weight: 60),
            new CultureDistributionEntry("sample-secondary-culture", weight: 30),
            new CultureDistributionEntry("outlier", weight: 10, isOutlierResidual: true),
        };

        var reputationDuality = new DatedRule<ReputationDualityMode>(
            baseValue: ReputationDualityMode.Full,
            overrides: new[]
            {
                new DatedOverride<ReputationDualityMode>(
                    ReputationDualityMode.Tapering, effectiveFrom: ConquestArcCloses),
            });

        return new RegionProfileDefinition(
            id: SampleFrontier,
            name: "Sample Frontier Colony",
            status: RegionStatus.ExtensibleSlate,
            terrainProfileRef: "hills-and-river-mixed",
            economicCharacterTag: "cheap-land-thin-market",
            politicalLegalProfileRef: "peregrine-majority-latin-minority",
            diplomaticMilitaryProfileRef: "frontier-people-land-raid-exposure",
            religiousCulturalDefaultRef: "local-cult-moderate-drift",
            regionalGoodsProfileRef: "mining-and-metals-identity",
            cultureDistributionTable: cultureDistribution,
            reputationDuality: reputationDuality,
            homeAnchorLocationId: HomeAnchorLocation,
            gazetteer: new[] { homeAnchor, outpost });
    }
}
