using Gens.Simulation.Identity;
using Gens.Simulation.Regions;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Regions;

public sealed class RegionProfileCatalogTests
{
    [Test]
    public void GetReturnsARegisteredDefinition()
    {
        var catalog = SampleRegionProfileDefinitions.BuildCatalog();

        Assert.That(catalog.Get(SampleRegionProfileDefinitions.SampleFrontier).Id,
            Is.EqualTo(SampleRegionProfileDefinitions.SampleFrontier));
    }

    [Test]
    public void GetThrowsForAnUnregisteredId()
    {
        var catalog = SampleRegionProfileDefinitions.BuildCatalog();

        Assert.Throws<KeyNotFoundException>(() => catalog.Get(new DefinitionId<RegionProfileDefinition>("unregistered")));
    }

    [Test]
    public void ConstructorRejectsDuplicateIds()
    {
        var region = SampleRegionProfileDefinitions.BuildSampleFrontier();

        Assert.Throws<ArgumentException>(() => new RegionProfileCatalog(new[] { region, region }));
    }

    [Test]
    public void ConstructorRejectsMoreThanOneCapitalAcrossTheCatalog()
    {
        var romeAnchor = new GazetteerLocationDefinition(
            new DefinitionId<GazetteerLocationDefinition>("rome"),
            new DefinitionId<RegionProfileDefinition>("region-a"),
            "Rome", new[] { GazetteerRole.Capital }, ProminenceTier.ProvincialSeat, "The capital.");
        var regionA = MakeSingleLocationRegion("region-a", romeAnchor);

        var secondCapitalAnchor = new GazetteerLocationDefinition(
            new DefinitionId<GazetteerLocationDefinition>("second-capital"),
            new DefinitionId<RegionProfileDefinition>("region-b"),
            "Second Capital", new[] { GazetteerRole.Capital }, ProminenceTier.ProvincialSeat, "Not allowed.");
        var regionB = MakeSingleLocationRegion("region-b", secondCapitalAnchor);

        Assert.Throws<ArgumentException>(() => new RegionProfileCatalog(new[] { regionA, regionB }));
    }

    [Test]
    public void ForStatusFiltersToOnlyThatStatus()
    {
        var catalog = SampleRegionProfileDefinitions.BuildCatalog();

        var slate = catalog.ForStatus(RegionStatus.ExtensibleSlate).ToArray();
        var launch = catalog.ForStatus(RegionStatus.Launch).ToArray();

        Assert.That(slate, Has.Length.EqualTo(1));
        Assert.That(launch, Is.Empty);
    }

    private static RegionProfileDefinition MakeSingleLocationRegion(string regionIdValue, GazetteerLocationDefinition anchor) =>
        new(
            id: new DefinitionId<RegionProfileDefinition>(regionIdValue),
            name: regionIdValue,
            status: RegionStatus.ExtensibleSlate,
            terrainProfileRef: "mixed",
            economicCharacterTag: "cheap-land",
            politicalLegalProfileRef: "peregrine-majority",
            diplomaticMilitaryProfileRef: "no-exposure",
            religiousCulturalDefaultRef: "local-cult",
            regionalGoodsProfileRef: "generic-goods",
            cultureDistributionTable: new[]
            {
                new CultureDistributionEntry("dominant", 90),
                new CultureDistributionEntry("outlier", 10, isOutlierResidual: true),
            },
            reputationDuality: new DatedRule<ReputationDualityMode>(ReputationDualityMode.None),
            homeAnchorLocationId: anchor.Id,
            gazetteer: new[] { anchor });
}
