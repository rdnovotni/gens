using Gens.Simulation.Identity;
using Gens.Simulation.Regions;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Regions;

public sealed class RegionProfileDefinitionTests
{
    private static readonly DefinitionId<RegionProfileDefinition> RegionId = new("test-region");
    private static readonly DefinitionId<GazetteerLocationDefinition> AnchorId = new("test-anchor");
    private static readonly DefinitionId<GazetteerLocationDefinition> OtherId = new("test-other");

    [Test]
    public void ConstructorAcceptsAWellFormedProfile()
    {
        var region = Build();

        Assert.Multiple(() =>
        {
            Assert.That(region.Id, Is.EqualTo(RegionId));
            Assert.That(region.HomeAnchor.Id, Is.EqualTo(AnchorId));
            Assert.That(region.Gazetteer, Has.Count.EqualTo(2));
            Assert.That(region.CultureDistributionTable, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void TryGetGazetteerEntryFindsARegisteredLocation()
    {
        var region = Build();

        Assert.That(region.TryGetGazetteerEntry(OtherId, out var entry), Is.True);
        Assert.That(entry.Id, Is.EqualTo(OtherId));
    }

    [Test]
    public void TryGetGazetteerEntryFailsForAnUnregisteredLocation()
    {
        var region = Build();

        Assert.That(region.TryGetGazetteerEntry(new DefinitionId<GazetteerLocationDefinition>("unknown"), out _), Is.False);
    }

    [Test]
    public void ConstructorRejectsAHomeAnchorNotInTheGazetteer()
    {
        var anchor = MakeLocation(AnchorId);

        Assert.Throws<ArgumentException>(() => Build(
            homeAnchorId: new DefinitionId<GazetteerLocationDefinition>("not-in-gazetteer"),
            gazetteer: new[] { anchor }));
    }

    [Test]
    public void ConstructorRejectsAGazetteerEntryFromAnotherRegion()
    {
        var foreignAnchor = new GazetteerLocationDefinition(
            AnchorId, new DefinitionId<RegionProfileDefinition>("some-other-region"), "Anchor",
            new[] { GazetteerRole.ProvincialSeat }, ProminenceTier.ProvincialSeat, "note");

        Assert.Throws<ArgumentException>(() => Build(gazetteer: new[] { foreignAnchor }));
    }

    [Test]
    public void ConstructorRejectsAnEmptyGazetteer()
    {
        Assert.Throws<ArgumentException>(() => Build(gazetteer: Array.Empty<GazetteerLocationDefinition>()));
    }

    [Test]
    public void ConstructorRejectsDuplicateGazetteerIds()
    {
        var anchor = MakeLocation(AnchorId);
        var duplicate = MakeLocation(AnchorId);

        Assert.Throws<ArgumentException>(() => Build(gazetteer: new[] { anchor, duplicate }));
    }

    [Test]
    public void ConstructorRejectsACultureDistributionTableWithNoOutlierResidual()
    {
        var table = new[] { new CultureDistributionEntry("only-culture", 100) };

        Assert.Throws<ArgumentException>(() => Build(cultureDistribution: table));
    }

    [Test]
    public void ConstructorRejectsACultureDistributionTableWithMoreThanOneOutlierResidual()
    {
        var table = new[]
        {
            new CultureDistributionEntry("outlier-a", 5, isOutlierResidual: true),
            new CultureDistributionEntry("outlier-b", 5, isOutlierResidual: true),
        };

        Assert.Throws<ArgumentException>(() => Build(cultureDistribution: table));
    }

    [Test]
    public void ConstructorRejectsDuplicateCultureReferences()
    {
        var table = new[]
        {
            new CultureDistributionEntry("dominant", 50),
            new CultureDistributionEntry("dominant", 40),
            new CultureDistributionEntry("outlier", 10, isOutlierResidual: true),
        };

        Assert.Throws<ArgumentException>(() => Build(cultureDistribution: table));
    }

    [Test]
    public void ConstructorRejectsANonPositiveCultureWeight()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CultureDistributionEntry("dominant", 0));
    }

    [Test]
    public void ConstructorRejectsABlankQualitativeTag()
    {
        Assert.Throws<ArgumentException>(() => Build(economicCharacterTag: " "));
    }

    private static RegionProfileDefinition Build(
        DefinitionId<GazetteerLocationDefinition>? homeAnchorId = null,
        IReadOnlyList<GazetteerLocationDefinition>? gazetteer = null,
        IReadOnlyList<CultureDistributionEntry>? cultureDistribution = null,
        string economicCharacterTag = "cheap-land-thin-market")
    {
        gazetteer ??= new[] { MakeLocation(AnchorId), MakeLocation(OtherId) };
        cultureDistribution ??= new[]
        {
            new CultureDistributionEntry("dominant", 80),
            new CultureDistributionEntry("outlier", 20, isOutlierResidual: true),
        };

        return new RegionProfileDefinition(
            id: RegionId,
            name: "Test Region",
            status: RegionStatus.ExtensibleSlate,
            terrainProfileRef: "hills-and-river-mixed",
            economicCharacterTag: economicCharacterTag,
            politicalLegalProfileRef: "peregrine-majority",
            diplomaticMilitaryProfileRef: "frontier-people-exposure",
            religiousCulturalDefaultRef: "local-cult-moderate-drift",
            regionalGoodsProfileRef: "mining-and-metals-identity",
            cultureDistributionTable: cultureDistribution,
            reputationDuality: new DatedRule<ReputationDualityMode>(ReputationDualityMode.Full),
            homeAnchorLocationId: homeAnchorId ?? AnchorId,
            gazetteer: gazetteer);
    }

    private static GazetteerLocationDefinition MakeLocation(DefinitionId<GazetteerLocationDefinition> id) =>
        new(id, RegionId, id.ToString(), new[] { GazetteerRole.MarketHub }, ProminenceTier.RegionalCenter, "A grounding note.");
}
