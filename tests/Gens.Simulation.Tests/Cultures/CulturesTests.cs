using Gens.Simulation.Cultures;
using Gens.Simulation.Identity;
using Gens.Simulation.Regions;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Cultures;

public sealed class CulturesTests
{
    // ---- CultureDefinition ----------------------------------------------------------------------

    [Test]
    public void ConstructorRejectsEmptyName()
    {
        Assert.Throws<ArgumentException>(() =>
            new CultureDefinition(
                new DefinitionId<Simulation.Identity.Culture>("test-culture"), " ",
                new DatedRule<CultureCategory>(CultureCategory.Provincial)));
    }

    [Test]
    public void ConstructorRejectsRarityTierOnANonTradeContactCulture()
    {
        Assert.Throws<ArgumentException>(() =>
            new CultureDefinition(
                new DefinitionId<Simulation.Identity.Culture>("test-culture"), "Test",
                new DatedRule<CultureCategory>(CultureCategory.Provincial),
                encounterRarityTier: EncounterRarityTier.Rare));
    }

    [Test]
    public void ConstructorRequiresRarityTierOnATradeContactCulture()
    {
        Assert.Throws<ArgumentException>(() =>
            new CultureDefinition(
                new DefinitionId<Simulation.Identity.Culture>("test-culture"), "Test",
                new DatedRule<CultureCategory>(CultureCategory.TradeContactOnly)));
    }

    [Test]
    public void CategoryAsOfReadsTheDatedRule()
    {
        var shiftDate = new GameDate(100);
        var definition = new CultureDefinition(
            new DefinitionId<Simulation.Identity.Culture>("test-culture"), "Test",
            new DatedRule<CultureCategory>(
                CultureCategory.Frontier,
                new[] { new DatedOverride<CultureCategory>(CultureCategory.Provincial, effectiveFrom: shiftDate) }));

        Assert.Multiple(() =>
        {
            Assert.That(definition.CategoryAsOf(new GameDate(0)), Is.EqualTo(CultureCategory.Frontier));
            Assert.That(definition.CategoryAsOf(shiftDate), Is.EqualTo(CultureCategory.Provincial));
        });
    }

    // ---- CultureCatalog --------------------------------------------------------------------------

    [Test]
    public void CatalogRejectsDuplicateCultureIds()
    {
        var id = new DefinitionId<Simulation.Identity.Culture>("test-culture");
        var definition = new CultureDefinition(id, "Test", new DatedRule<CultureCategory>(CultureCategory.Provincial));

        Assert.Throws<ArgumentException>(() => new CultureCatalog(new[] { definition, definition }));
    }

    [Test]
    public void CatalogTryGetFindsARegisteredCultureAndMissesAnUnregisteredOne()
    {
        var id = new DefinitionId<Simulation.Identity.Culture>("test-culture");
        var catalog = new CultureCatalog(new[]
        {
            new CultureDefinition(id, "Test", new DatedRule<CultureCategory>(CultureCategory.Provincial)),
        });

        Assert.Multiple(() =>
        {
            Assert.That(catalog.TryGet(id, out _), Is.True);
            Assert.That(catalog.TryGet(new DefinitionId<Simulation.Identity.Culture>("unknown"), out _), Is.False);
        });
    }

    [Test]
    public void CatalogGetThrowsForAnUnregisteredCulture()
    {
        var catalog = new CultureCatalog(Array.Empty<CultureDefinition>());
        Assert.Throws<KeyNotFoundException>(() => catalog.Get(new DefinitionId<Simulation.Identity.Culture>("unknown")));
    }

    // ---- KnownWorldCultures -----------------------------------------------------------------------

    [Test]
    public void KnownWorldCatalogHasEveryEntryTheSectionSeventeenEnumLists()
    {
        // §17's own enum literally lists 37 values (including "roman"), even though the doc's own
        // intro prose says "thirty-six real, playable cultures" — see KnownWorldCultures's own doc
        // comment for why Roman is the honest, disclosed +1 (§12's own table calls it "the default,"
        // not one of the thirty-six added entries).
        var catalog = KnownWorldCultures.BuildCatalog();
        Assert.That(catalog.Count, Is.EqualTo(37));
    }

    [Test]
    public void BritishShiftsFromFrontierToProvincialAtAD43()
    {
        var catalog = KnownWorldCultures.BuildCatalog();
        var british = catalog.Get(KnownWorldCultures.British);

        Assert.Multiple(() =>
        {
            Assert.That(british.CategoryAsOf(new GameDate(0)), Is.EqualTo(CultureCategory.Frontier));
            Assert.That(british.CategoryAsOf(KnownWorldCultures.BritishShift), Is.EqualTo(CultureCategory.Provincial));
        });
    }

    [TestCase("hibernian")]
    [TestCase("caledonian")]
    [TestCase("nubian-kushite")]
    public void PermanentlyUnconqueredCulturesAreFlagged(string cultureId)
    {
        var catalog = KnownWorldCultures.BuildCatalog();
        var definition = catalog.Get(new DefinitionId<Simulation.Identity.Culture>(cultureId));

        Assert.Multiple(() =>
        {
            Assert.That(definition.PermanentlyUnconquered, Is.True);
            Assert.That(definition.CategoryAsOf(new GameDate(0)), Is.EqualTo(CultureCategory.Frontier));
        });
    }

    [Test]
    public void BlemmyesIsFlaggedAsARaidingFrontier()
    {
        var catalog = KnownWorldCultures.BuildCatalog();
        Assert.That(catalog.Get(KnownWorldCultures.Blemmyes).IsRaidingFrontier, Is.True);
    }

    [TestCase("batavian")]
    [TestCase("cretan")]
    public void AuxiliaryServiceCulturesAreFlagged(string cultureId)
    {
        var catalog = KnownWorldCultures.BuildCatalog();
        Assert.That(catalog.Get(new DefinitionId<Simulation.Identity.Culture>(cultureId)).IsAuxiliaryServiceCulture, Is.True);
    }

    [Test]
    public void ChineseIsTheOnlyExceptionallyRareTradeContactCulture()
    {
        var catalog = KnownWorldCultures.BuildCatalog();

        Assert.Multiple(() =>
        {
            Assert.That(catalog.Get(KnownWorldCultures.Chinese).EncounterRarityTier, Is.EqualTo(EncounterRarityTier.ExceptionallyRare));
            Assert.That(catalog.Get(KnownWorldCultures.Indian).EncounterRarityTier, Is.EqualTo(EncounterRarityTier.Rare));
        });
    }

    [Test]
    public void EveryTradeContactOnlyCultureCarriesTheNoveltyDignitasBonus()
    {
        var catalog = KnownWorldCultures.BuildCatalog();
        foreach (var definition in catalog.All())
        {
            if (definition.CategoryAsOf(new GameDate(0)) == CultureCategory.TradeContactOnly)
                Assert.That(definition.NoveltyDignitasBonus, Is.True, $"'{definition.Id.Value}' should carry the novelty Dignitas bonus.");
        }
    }

    [Test]
    public void ParthianIsAGreatPowerAndArmenianIsAContestedBuffer()
    {
        var catalog = KnownWorldCultures.BuildCatalog();

        Assert.Multiple(() =>
        {
            Assert.That(catalog.Get(KnownWorldCultures.Parthian).CategoryAsOf(new GameDate(0)), Is.EqualTo(CultureCategory.GreatPower));
            Assert.That(catalog.Get(KnownWorldCultures.Armenian).CategoryAsOf(new GameDate(0)), Is.EqualTo(CultureCategory.ContestedBuffer));
        });
    }

    // ---- CultureNamingPoolCatalog -----------------------------------------------------------------

    [Test]
    public void NamingPoolMapCoversEveryCultureItNames()
    {
        var map = CultureNamingPoolCatalog.BuildMap();

        Assert.Multiple(() =>
        {
            Assert.That(map.ContainsKey(KnownWorldCultures.Roman), Is.True);
            Assert.That(map.ContainsKey(KnownWorldCultures.Gallic), Is.True);
            Assert.That(map.ContainsKey(KnownWorldCultures.Etruscan), Is.True);
            Assert.That(map[KnownWorldCultures.Gallic], Is.SameAs(CultureNamingPoolCatalog.GallicBritishHibernian));
        });
    }
}
