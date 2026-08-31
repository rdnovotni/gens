using Gens.Simulation.Identity;
using Gens.Simulation.Regions;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Tests.Travel;

/// <summary>Shared region/distance-tier fixtures for the Travel test suite — mirrors <see
/// cref="Gens.Simulation.Tests.Characters.CharacterTestFixtures"/>'s identical "small, self-contained,
/// not real content" shape. One region carries the Capital-role gazetteer entry Rome's own Distance
/// Tier resolves against (<see cref="Gens.Simulation.Travel.TravelRoute"/>'s own doc comment).</summary>
public static class TravelTestFixtures
{
    public static readonly DefinitionId<RegionProfileDefinition> HomeRegionId = new("test-home-region");
    public static readonly DefinitionId<RegionProfileDefinition> CapitalRegionId = new("test-capital-region");
    public static readonly DefinitionId<RegionProfileDefinition> NearRegionId = new("test-near-region");
    public static readonly DefinitionId<RegionProfileDefinition> FarRegionId = new("test-far-region");
    public static readonly DefinitionId<RegionProfileDefinition> UnlistedRegionId = new("test-unlisted-region");

    public static RegionProfileCatalog BuildRegionCatalog(bool includeCapital = true) =>
        new(new[]
        {
            BuildRegion(HomeRegionId, "Test Home Region", capital: false),
            BuildRegion(CapitalRegionId, "Test Capital Region", capital: includeCapital),
            BuildRegion(NearRegionId, "Test Near Region", capital: false),
            BuildRegion(FarRegionId, "Test Far Region", capital: false),
            BuildRegion(UnlistedRegionId, "Test Unlisted Region", capital: false),
        });

    public static DistanceTierCatalog BuildDistanceTierCatalog() =>
        new(new[]
        {
            new RegionDistanceTierEntry(HomeRegionId, CapitalRegionId, DistanceTier.Moderate),
            new RegionDistanceTierEntry(HomeRegionId, NearRegionId, DistanceTier.Near),
            new RegionDistanceTierEntry(HomeRegionId, FarRegionId, DistanceTier.Far),
        });

    public static RegionProfileDefinition BuildRegion(DefinitionId<RegionProfileDefinition> id, string name, bool capital)
    {
        var anchorId = new DefinitionId<GazetteerLocationDefinition>(id.Value + "-anchor");
        var roles = capital
            ? new[] { GazetteerRole.Capital, GazetteerRole.ProvincialSeat }
            : new[] { GazetteerRole.ProvincialSeat };

        var anchor = new GazetteerLocationDefinition(
            id: anchorId,
            regionId: id,
            name: name + " Seat",
            roles: roles,
            prominenceTier: ProminenceTier.ProvincialSeat,
            groundingNote: "Test fixture, not authored content.");

        var cultureDistribution = new[]
        {
            new CultureDistributionEntry("test-culture", weight: 90),
            new CultureDistributionEntry("outlier", weight: 10, isOutlierResidual: true),
        };

        return new RegionProfileDefinition(
            id: id,
            name: name,
            status: RegionStatus.ExtensibleSlate,
            terrainProfileRef: "test-terrain",
            economicCharacterTag: "test-economy",
            politicalLegalProfileRef: "test-political",
            diplomaticMilitaryProfileRef: "test-diplomatic",
            religiousCulturalDefaultRef: "test-religion",
            regionalGoodsProfileRef: "test-goods",
            cultureDistributionTable: cultureDistribution,
            reputationDuality: new DatedRule<ReputationDualityMode>(ReputationDualityMode.None),
            homeAnchorLocationId: anchorId,
            gazetteer: new[] { anchor });
    }
}
