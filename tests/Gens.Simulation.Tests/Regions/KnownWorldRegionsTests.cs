using Gens.Simulation.Cultures;
using Gens.Simulation.Regions;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Regions;

/// <summary>Phase 13 item 6's real, authored content: Latium, the first region-content wave. Proves the
/// authored region resolves through the same schema the fixture region (item 1) already proved, and
/// pins down this wave's own real, disclosed authoring choices — Rome seated in Latium's own Gazetteer
/// carrying the catalog-unique Capital role, no Reputation Duality at any date, and Tusculum as the
/// Home Anchor.</summary>
public sealed class KnownWorldRegionsTests
{
    [Test]
    public void BuildCatalogRegistersExactlyLatium()
    {
        var catalog = KnownWorldRegions.BuildCatalog();

        Assert.That(catalog.All().Select(r => r.Id), Is.EquivalentTo(new[] { KnownWorldRegions.Latium }));
    }

    [Test]
    public void LatiumIsALaunchRegion()
    {
        var latium = KnownWorldRegions.BuildLatium();

        Assert.That(latium.Status, Is.EqualTo(RegionStatus.Launch));
    }

    [Test]
    public void HomeAnchorIsTusculum()
    {
        var latium = KnownWorldRegions.BuildLatium();

        Assert.That(latium.HomeAnchor.Id, Is.EqualTo(KnownWorldRegions.Tusculum));
    }

    [Test]
    public void GazetteerContainsEveryAuthoredLocation()
    {
        var latium = KnownWorldRegions.BuildLatium();

        Assert.That(
            latium.Gazetteer.Select(entry => entry.Id),
            Is.EquivalentTo(new[]
            {
                KnownWorldRegions.Rome,
                KnownWorldRegions.Ostia,
                KnownWorldRegions.Tusculum,
                KnownWorldRegions.Praeneste,
                KnownWorldRegions.Tibur,
                KnownWorldRegions.Antium,
                KnownWorldRegions.AlbaLonga,
                KnownWorldRegions.Lavinium,
                KnownWorldRegions.Gabii,
            }));
    }

    [Test]
    public void RomeCarriesTheCatalogUniqueCapitalRole()
    {
        var latium = KnownWorldRegions.BuildLatium();

        Assert.That(latium.TryGetGazetteerEntry(KnownWorldRegions.Rome, out var rome), Is.True);
        Assert.That(rome.Roles, Does.Contain(GazetteerRole.Capital));
    }

    [Test]
    public void ReputationDualityIsNoneAtEveryDate()
    {
        var latium = KnownWorldRegions.BuildLatium();

        Assert.That(latium.ReputationDualityAsOf(new GameDate(0)), Is.EqualTo(ReputationDualityMode.None));
        Assert.That(latium.ReputationDualityAsOf(new GameDate(1200)), Is.EqualTo(ReputationDualityMode.None));
    }

    [Test]
    public void CultureDistributionNamesRomanAsDominantAndCarriesExactlyOneOutlierResidualRow()
    {
        var latium = KnownWorldRegions.BuildLatium();
        var table = latium.CultureDistributionTable;

        var roman = table.Single(entry => entry.CultureRef == KnownWorldCultures.Roman.Value);
        var others = table.Where(entry => entry.CultureRef != KnownWorldCultures.Roman.Value);
        Assert.That(others.All(entry => entry.Weight < roman.Weight), Is.True);
        Assert.That(table.Count(entry => entry.IsOutlierResidual), Is.EqualTo(1));
    }

    [Test]
    public void BuildingTheCatalogTwiceProducesEquivalentContent()
    {
        var first = KnownWorldRegions.BuildCatalog();
        var second = KnownWorldRegions.BuildCatalog();

        Assert.That(first.Get(KnownWorldRegions.Latium).Name, Is.EqualTo(second.Get(KnownWorldRegions.Latium).Name));
    }
}
