using Gens.Simulation.Regions;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Regions;

/// <summary>End-to-end coverage over the fixture region itself: proves the schema and the date-aware
/// override mechanism resolve correctly at dates before, at, and after the Cantabrian Wars-style
/// boundary the fixture models (Phase 13 item 1's own requirement).</summary>
public sealed class SampleRegionProfileDefinitionsTests
{
    [Test]
    public void ReputationDualityReadsFullBeforeTheConquestArcCloses()
    {
        var region = SampleRegionProfileDefinitions.BuildSampleFrontier();
        var before = new GameDate(SampleRegionProfileDefinitions.ConquestArcCloses.TotalMonths - 12);

        Assert.That(region.ReputationDualityAsOf(before), Is.EqualTo(ReputationDualityMode.Full));
    }

    [Test]
    public void ReputationDualityReadsTaperingAtTheConquestArcClose()
    {
        var region = SampleRegionProfileDefinitions.BuildSampleFrontier();

        Assert.That(
            region.ReputationDualityAsOf(SampleRegionProfileDefinitions.ConquestArcCloses),
            Is.EqualTo(ReputationDualityMode.Tapering));
    }

    [Test]
    public void ReputationDualityStaysTaperingWellAfterTheConquestArcCloses()
    {
        var region = SampleRegionProfileDefinitions.BuildSampleFrontier();
        var after = new GameDate(SampleRegionProfileDefinitions.ConquestArcCloses.TotalMonths + 240);

        Assert.That(region.ReputationDualityAsOf(after), Is.EqualTo(ReputationDualityMode.Tapering));
    }

    [Test]
    public void HomeAnchorResolvesToARealGazetteerEntry()
    {
        var region = SampleRegionProfileDefinitions.BuildSampleFrontier();

        Assert.That(region.HomeAnchor.Id, Is.EqualTo(SampleRegionProfileDefinitions.HomeAnchorLocation));
    }

    [Test]
    public void BuildCatalogRegistersExactlyTheFixtureRegion()
    {
        var catalog = SampleRegionProfileDefinitions.BuildCatalog();

        Assert.That(catalog.All().Select(r => r.Id), Is.EquivalentTo(new[] { SampleRegionProfileDefinitions.SampleFrontier }));
    }
}
