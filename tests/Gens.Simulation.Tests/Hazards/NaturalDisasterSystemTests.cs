using Gens.Simulation.Buildings;
using Gens.Simulation.Hazards;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Hazards;

public sealed class NaturalDisasterSystemTests
{
    private const string StreamName = "test-natural-disaster";

    // A non-dry-season, non-storm-season month (February), so Fire/Drought's dry-season bonus and
    // Storm's own storm-season bonus stay out of the exposure math below.
    private static readonly GameDate NonSeasonalDate = new(1);

    [Test]
    public void AnEmptyCampaignProducesNoEvents()
    {
        var state = new WorldState(NonSeasonalDate);
        var system = new NaturalDisasterSystem(StreamName);
        var events = system.Tick(state, new MonthlyTickContext(NonSeasonalDate, new RandomStreamSet()));

        Assert.That(events, Is.Empty);
    }

    [Test]
    public void EarthquakeIgnitionDamagesAnEligibleBuildingAndWritesADisasterEvent()
    {
        var state = new WorldState(NonSeasonalDate);
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId, TerrainType.FertilePlain, capacity: 1));

        var buildingDefinition = new BuildingDefinition(new DefinitionId<Building>("test-building"), BuildingTier.Tier1, 1, 1);
        var buildingId = state.BuildingIds.Issue();
        state.Buildings.Add(buildingId, new BuildingInstance(buildingId, plotId, buildingDefinition));

        // Fire's own Exposure is driven to 55 by this single building fully occupying the plot's one
        // capacity unit (buildingDensity = 1/1), so it needs its own explicit "do not ignite" draw,
        // exactly like Earthquake's own flat baseline needs an explicit "do ignite" draw.
        const int fireExposure = 55;
        const int earthquakeExposure = 12;

        var seed = FindSeedForSequentialDraws(
            v => v >= Threshold(DisasterSeverityCalculator.MonthlyIgnitionProbability(fireExposure)), // Fire: do not ignite.
            _ => true, // Flood: Exposure 0 here, never ignites regardless of the draw.
            v => v < Threshold(DisasterSeverityCalculator.MonthlyIgnitionProbability(earthquakeExposure)), // Earthquake: ignite.
            v => DisasterSeverityCalculator.RollSeverity(earthquakeExposure, v) == DisasterSeverity.Minor, // Earthquake: Minor.
            v => v < Threshold(DisasterDamageCalculator.BuildingHitProbability(DisasterSeverity.Minor))); // The building: hit.

        var streams = new RandomStreamSet();
        streams.Add(StreamName, seed, 1);

        var system = new NaturalDisasterSystem(StreamName);
        var events = system.Tick(state, new MonthlyTickContext(NonSeasonalDate, streams));

        var earthquakeEvents = events.OfType<DisasterEventOccurredEvent>()
            .Where(e => e.HazardType == HazardType.Earthquake).ToArray();
        Assert.That(earthquakeEvents, Has.Length.EqualTo(1));
        Assert.That(earthquakeEvents[0].Severity, Is.EqualTo(DisasterSeverity.Minor));
        Assert.That(earthquakeEvents[0].TriggeredByCompounding, Is.False);

        var stored = HazardQueries.EventsFor(state, settlementId).Single(e => e.HazardType == HazardType.Earthquake);
        Assert.That(stored.BuildingsDamaged, Is.EqualTo(1));

        state.Buildings.TryGet(buildingId, out var damagedBuilding);
        Assert.That(damagedBuilding.Condition, Is.LessThan(BuildingCondition.Pristine));
    }

    [Test]
    public void ASevereOrCatastrophicStormCanChainDirectlyIntoAFloodOnARiverAdjacentPlot()
    {
        var state = new WorldState(NonSeasonalDate);
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(
            plotId, settlementId, TerrainType.Coast, TerrainFeature.RiverAdjacent, capacity: 1));

        const int floodExposure = 70; // FloodExposure(riverAdjacentFraction: 1.0, forestCoverFraction: 0.0).
        const int earthquakeExposure = 12;
        const int droughtFamineExposure = 14; // Non-dry-season baseline.
        const int stormExposure = 60; // StormExposure(coastalFraction: 1.0), no storm-season bonus at this date.
        const DisasterSeverity stormSeverity = DisasterSeverity.Catastrophic;

        var seed = FindSeedForSequentialDraws(
            _ => true, // Fire: Exposure 0 (no building), never ignites regardless of the draw.
            v => v >= Threshold(DisasterSeverityCalculator.MonthlyIgnitionProbability(floodExposure)), // Flood: do not ignite directly.
            v => v >= Threshold(DisasterSeverityCalculator.MonthlyIgnitionProbability(earthquakeExposure)), // Earthquake: do not ignite.
            v => v >= Threshold(DisasterSeverityCalculator.MonthlyIgnitionProbability(droughtFamineExposure)), // Drought/Famine: do not ignite.
            v => v < Threshold(DisasterSeverityCalculator.MonthlyIgnitionProbability(stormExposure)), // Storm: ignite.
            v => DisasterSeverityCalculator.RollSeverity(stormExposure, v) == stormSeverity, // Storm: Catastrophic.
            v => v < Threshold(DisasterCompoundingCalculator.StormToFloodChainProbability(stormSeverity))); // Chain: trigger.

        var streams = new RandomStreamSet();
        streams.Add(StreamName, seed, 1);

        var system = new NaturalDisasterSystem(StreamName);
        var events = system.Tick(state, new MonthlyTickContext(NonSeasonalDate, streams));

        var stormEvent = events.OfType<DisasterEventOccurredEvent>().Single(e => e.HazardType == HazardType.Storm);
        Assert.That(stormEvent.Severity, Is.EqualTo(DisasterSeverity.Catastrophic));
        Assert.That(stormEvent.TriggeredByCompounding, Is.False);

        var floodEvent = events.OfType<DisasterEventOccurredEvent>().Single(e => e.HazardType == HazardType.Flood);
        Assert.That(floodEvent.TriggeredByCompounding, Is.True);
        Assert.That(floodEvent.Severity, Is.EqualTo(DisasterCompoundingCalculator.ChainedFloodSeverity(stormSeverity)));

        var storedFlood = HazardQueries.EventsFor(state, settlementId).Single(e => e.HazardType == HazardType.Flood);
        Assert.That(storedFlood.TriggeredByCompounding, Is.True);
    }

    private static uint Threshold(double probability) =>
        (uint)Math.Clamp(probability * DisasterSeverityCalculator.RollPrecision, 0, DisasterSeverityCalculator.RollPrecision);

    private static ulong FindSeedForSequentialDraws(params Predicate<uint>[] matchesDraw)
    {
        for (ulong seed = 0; seed < 500_000; seed++)
        {
            var probe = new RandomStreamSet();
            probe.Add(StreamName, seed, 1);
            var matched = true;
            foreach (var predicate in matchesDraw)
            {
                if (!predicate(probe.NextUInt(StreamName, DisasterSeverityCalculator.RollPrecision)))
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
                return seed;
        }

        throw new InvalidOperationException("No seed found matching the requested draw sequence within the search bound.");
    }
}
