using Gens.Simulation.Identity;
using Gens.Simulation.Regions;
using Gens.Simulation.Wanderers;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Wanderers;

public sealed class WandererItineraryCalculatorTests
{
    private static readonly WandererTypeCatalog Catalog = WandererTypeCatalog.BuildDefault();

    [Test]
    public void EveryDestinationCarriesAtLeastTheBaseWeightEvenWhenNothingMatches()
    {
        var merchant = Catalog.Get(WandererType.MerchantPeddler);

        var weight = WandererItineraryCalculator.MovementWeight(merchant, WandererTestFixtures.ShrineLocation);

        Assert.That(weight, Is.EqualTo(WandererItineraryCalculator.BaseWeight));
    }

    [Test]
    public void APhilosopherPrefersTheHighProminenceSeatOverEveryOtherFixtureLocation()
    {
        var philosopher = Catalog.Get(WandererType.PhilosopherRhetorician);

        var seat = WandererItineraryCalculator.MovementWeight(philosopher, WandererTestFixtures.SeatLocation);
        var port = WandererItineraryCalculator.MovementWeight(philosopher, WandererTestFixtures.PortLocation);
        var outpost = WandererItineraryCalculator.MovementWeight(philosopher, WandererTestFixtures.OutpostLocation);

        Assert.Multiple(() =>
        {
            Assert.That(seat, Is.GreaterThan(port));
            Assert.That(port, Is.GreaterThan(outpost));
        });
    }

    [Test]
    public void AHolyManPrefersTheSanctuaryTheSeatSeekingPhilosopherIgnores()
    {
        var holyMan = Catalog.Get(WandererType.HolyManAstrologer);
        var philosopher = Catalog.Get(WandererType.PhilosopherRhetorician);

        Assert.Multiple(() =>
        {
            Assert.That(
                WandererItineraryCalculator.MovementWeight(holyMan, WandererTestFixtures.ShrineLocation),
                Is.GreaterThan(WandererItineraryCalculator.MovementWeight(holyMan, WandererTestFixtures.SeatLocation)));
            Assert.That(
                WandererItineraryCalculator.MovementWeight(philosopher, WandererTestFixtures.SeatLocation),
                Is.GreaterThan(WandererItineraryCalculator.MovementWeight(philosopher, WandererTestFixtures.ShrineLocation)));
        });
    }

    [Test]
    public void AMultiRoleMatchStacksAboveASingleRoleMatch()
    {
        // The merchant prefers MajorPort and MarketHub; the fixture seat carries MarketHub only, the
        // fixture port MajorPort only — so a location carrying both must outweigh either.
        var merchant = Catalog.Get(WandererType.MerchantPeddler);
        var both = new GazetteerLocationDefinition(
            new DefinitionId<GazetteerLocationDefinition>("wanderer-test-emporium"),
            WandererTestFixtures.Region,
            "Test Emporium",
            new[] { GazetteerRole.MajorPort, GazetteerRole.MarketHub },
            ProminenceTier.RegionalCenter,
            "Fixture stand-in for a port that is also a market hub.");

        Assert.That(
            WandererItineraryCalculator.MovementWeight(merchant, both),
            Is.EqualTo(WandererItineraryCalculator.BaseWeight + (2 * WandererItineraryCalculator.PreferredRoleWeight)));
    }

    [Test]
    public void TheCurrentLocationIsNeverOfferedAsADestination()
    {
        var philosopher = Catalog.Get(WandererType.PhilosopherRhetorician);
        var catalog = WandererTestFixtures.BuildRegionCatalog();

        var destinations = WandererItineraryCalculator.WeightedDestinations(
            philosopher, catalog, WandererTestFixtures.Seat);

        Assert.Multiple(() =>
        {
            Assert.That(destinations, Has.Count.EqualTo(3));
            Assert.That(destinations.Select(d => d.Location.Id), Does.Not.Contain(WandererTestFixtures.Seat));
        });
    }

    [Test]
    public void SelectionIsDeterministicAndCoversEveryWeightedBand()
    {
        var philosopher = Catalog.Get(WandererType.PhilosopherRhetorician);
        var catalog = WandererTestFixtures.BuildRegionCatalog();
        var destinations = WandererItineraryCalculator.WeightedDestinations(
            philosopher, catalog, WandererTestFixtures.Seat);
        var total = destinations.Sum(d => d.Weight);

        // Walk every roll in the weight space and confirm each destination's share is exactly its
        // weight — the whole determinism guarantee the System leans on.
        var counts = new Dictionary<DefinitionId<GazetteerLocationDefinition>, int>();
        for (var roll = 0u; roll < (uint)total; roll++)
        {
            var picked = WandererItineraryCalculator.SelectNextDestination(
                philosopher, catalog, WandererTestFixtures.Seat, roll);
            Assert.That(picked, Is.Not.Null);
            counts[picked!.Value] = counts.GetValueOrDefault(picked.Value) + 1;
        }

        foreach (var (location, weight) in destinations)
            Assert.That(counts[location.Id], Is.EqualTo(weight), $"{location.Name} did not get its weighted share.");
    }

    [Test]
    public void ASingleEntryRosterOffersNowhereToGo()
    {
        var singleEntry = new RegionProfileCatalog(new[]
        {
            new RegionProfileDefinition(
                id: WandererTestFixtures.Region,
                name: "Single Stop Region",
                status: RegionStatus.ExtensibleSlate,
                terrainProfileRef: "fixture-terrain",
                economicCharacterTag: "fixture-economy",
                politicalLegalProfileRef: "fixture-politics",
                diplomaticMilitaryProfileRef: "fixture-diplomacy",
                religiousCulturalDefaultRef: "fixture-religion",
                regionalGoodsProfileRef: "fixture-goods",
                cultureDistributionTable: new[]
                {
                    new CultureDistributionEntry("wanderer-test-culture", 90),
                    new CultureDistributionEntry("outlier", 10, isOutlierResidual: true),
                },
                reputationDuality: new DatedRule<ReputationDualityMode>(ReputationDualityMode.Full),
                homeAnchorLocationId: WandererTestFixtures.Seat,
                gazetteer: new[] { WandererTestFixtures.SeatLocation }),
        });

        var picked = WandererItineraryCalculator.SelectNextDestination(
            Catalog.Get(WandererType.PhilosopherRhetorician), singleEntry, WandererTestFixtures.Seat, roll: 0);

        Assert.That(picked, Is.Null);
    }

    [Test]
    public void AWandererIsOnlyDueToMoveOnceTheDwellPeriodHasElapsed()
    {
        Assert.Multiple(() =>
        {
            Assert.That(WandererItineraryCalculator.IsDueToMove(10, 10), Is.False);
            Assert.That(
                WandererItineraryCalculator.IsDueToMove(10, 10 + WandererItineraryCalculator.MonthsPerStop - 1),
                Is.False);
            Assert.That(
                WandererItineraryCalculator.IsDueToMove(10, 10 + WandererItineraryCalculator.MonthsPerStop),
                Is.True);
        });
    }

    [Test]
    public void TheItineraryIsCappedAtItsMaximumLengthDroppingTheOldestStop()
    {
        IReadOnlyList<WandererItineraryStop> itinerary = Array.Empty<WandererItineraryStop>();
        for (var month = 0; month < WandererItineraryCalculator.MaxItineraryLength + 3; month++)
            itinerary = WandererItineraryCalculator.Append(itinerary, new WandererItineraryStop(WandererTestFixtures.Seat, month));

        Assert.Multiple(() =>
        {
            Assert.That(itinerary, Has.Count.EqualTo(WandererItineraryCalculator.MaxItineraryLength));
            Assert.That(itinerary[0].ArrivalMonth, Is.EqualTo(3));
            Assert.That(itinerary[^1].ArrivalMonth, Is.EqualTo(WandererItineraryCalculator.MaxItineraryLength + 2));
        });
    }
}
