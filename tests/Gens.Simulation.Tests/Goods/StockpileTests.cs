using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Goods;

public sealed class StockpileTests
{
    private static readonly DefinitionId<Good> GrainId = new("grain");
    private static readonly DefinitionId<Good> JewelryId = new("jewelry");

    [Test]
    public void CapacityAndMetadataAreRetainedOnRemoval()
    {
        var stockpile = new Stockpile(10);
        var jewelry = new GoodDefinition(JewelryId, Perishability.NonPerishable, qualityEligible: true, conditionTracked: true);
        var provenance = new StockProvenance("workshop_0000001", "event_0000004", "object_0000001");

        stockpile.Add(jewelry, 4, GoodQuality.Exceptional, new StockCondition(92), provenance);
        var removed = stockpile.ConsumeAvailable(JewelryId, 3, GoodQuality.Exceptional).Single();

        Assert.Multiple(() =>
        {
            Assert.That(stockpile.Quantity, Is.EqualTo(1));
            Assert.That(stockpile.RemainingCapacity, Is.EqualTo(9));
            Assert.That(removed.Quality, Is.EqualTo(GoodQuality.Exceptional));
            Assert.That(removed.Condition, Is.EqualTo(new StockCondition(92)));
            Assert.That(removed.Provenance, Is.EqualTo(provenance));
        });
    }

    [Test]
    public void CapacityIsCheckedBeforeMutation()
    {
        var stockpile = new Stockpile(5);
        var grain = Grain(shelfLife: 4);
        stockpile.Add(grain, 4);

        Assert.That(() => stockpile.Add(grain, 2), Throws.InvalidOperationException);
        Assert.That(stockpile.Quantity, Is.EqualTo(4));
    }

    [Test]
    public void ReservationProtectsStockUntilItIsConsumedOrReleased()
    {
        var stockpile = new Stockpile(20);
        stockpile.Add(Grain(shelfLife: 4), 12);
        stockpile.Reserve("recipe-1", GrainId, 8);

        Assert.Multiple(() =>
        {
            Assert.That(stockpile.Quantity, Is.EqualTo(12));
            Assert.That(stockpile.AvailableQuantityOf(GrainId), Is.EqualTo(4));
            Assert.That(() => stockpile.ConsumeAvailable(GrainId, 5), Throws.InvalidOperationException);
        });

        var consumed = stockpile.ConsumeReserved("recipe-1");
        Assert.Multiple(() =>
        {
            Assert.That(consumed.Sum(batch => batch.Quantity), Is.EqualTo(8));
            Assert.That(stockpile.Quantity, Is.EqualTo(4));
            Assert.That(stockpile.Reservations, Is.Empty);
        });
    }

    [Test]
    public void OldestCompatibleLotsAreConsumedFirst()
    {
        var stockpile = new Stockpile(20);
        var grain = Grain(shelfLife: 10);
        stockpile.Add(grain, 5, provenance: new StockProvenance("new"), ageInTicks: 1);
        stockpile.Add(grain, 4, provenance: new StockProvenance("old"), ageInTicks: 7);

        var removed = stockpile.ConsumeAvailable(GrainId, 6);

        Assert.Multiple(() =>
        {
            Assert.That(removed[0].Provenance?.SourceId, Is.EqualTo("old"));
            Assert.That(removed[0].Quantity, Is.EqualTo(4));
            Assert.That(removed[1].Provenance?.SourceId, Is.EqualTo("new"));
            Assert.That(removed[1].Quantity, Is.EqualTo(2));
        });
    }

    [Test]
    public void SpoilageRemovesExpiredLotsReleasesReservationsAndCallsHook()
    {
        var stockpile = new Stockpile(20);
        stockpile.Add(Grain(shelfLife: 2), 7, ageInTicks: 1);
        stockpile.Reserve("meal", GrainId, 5);
        SpoilageRecord? observed = null;

        var spoiled = stockpile.AdvanceSpoilage(record => observed = record);

        Assert.Multiple(() =>
        {
            Assert.That(spoiled, Has.Count.EqualTo(1));
            Assert.That(spoiled[0].Stock.Quantity, Is.EqualTo(7));
            Assert.That(spoiled[0].ReservedQuantityReleased, Is.EqualTo(5));
            Assert.That(observed, Is.EqualTo(spoiled[0]));
            Assert.That(stockpile.Quantity, Is.Zero);
            Assert.That(stockpile.Reservations, Is.Empty);
        });
    }

    [Test]
    public void SpoilageKeepsReservationsThatRemainingStockCanFulfil()
    {
        var stockpile = new Stockpile(20);
        var grain = Grain(shelfLife: 3);
        stockpile.Add(grain, 2, ageInTicks: 2);
        stockpile.Add(grain, 8);
        stockpile.Reserve("meal", GrainId, 5);

        var spoiled = stockpile.AdvanceSpoilage();

        Assert.Multiple(() =>
        {
            Assert.That(spoiled[0].ReservedQuantityReleased, Is.Zero);
            Assert.That(stockpile.ReservedQuantityOf(GrainId), Is.EqualTo(5));
            Assert.That(stockpile.AvailableQuantityOf(GrainId), Is.EqualTo(3));
        });
    }

    [Test]
    public void QualityAndConditionMustMatchDefinition()
    {
        var stockpile = new Stockpile(10);
        var ordinary = Grain(shelfLife: 4);
        var jewelry = new GoodDefinition(JewelryId, Perishability.NonPerishable, qualityEligible: true, conditionTracked: true);

        Assert.Multiple(() =>
        {
            Assert.That(() => stockpile.Add(ordinary, 1, GoodQuality.Fine), Throws.ArgumentException);
            Assert.That(() => stockpile.Add(jewelry, 1), Throws.ArgumentException);
            Assert.That(() => stockpile.Add(jewelry, 1, GoodQuality.Fine), Throws.ArgumentException);
        });
    }

    private static GoodDefinition Grain(int shelfLife) => new(GrainId, Perishability.SemiPerishable, shelfLifeTicks: shelfLife);
}
