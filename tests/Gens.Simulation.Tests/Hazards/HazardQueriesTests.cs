using Gens.Simulation.Hazards;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Hazards;

public sealed class HazardQueriesTests
{
    [Test]
    public void CurrentExposureMatchesTheLiveHazardExposureProfileComputation()
    {
        var state = new WorldState(new GameDate(10));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));

        var date = new GameDate(10);
        var expected = HazardExposureProfile.Compute(state, settlementId, date).ExposureFor(HazardType.Earthquake);
        var actual = HazardQueries.CurrentExposure(state, settlementId, HazardType.Earthquake, date);

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void EventsForOnlyReturnsEventsForTheRequestedSettlement()
    {
        var state = new WorldState(new GameDate(10));
        var regionId = state.RegionIds.Issue();
        var firstSettlementId = state.SettlementIds.Issue();
        var secondSettlementId = state.SettlementIds.Issue();
        state.Settlements.Add(firstSettlementId, Settlement.Create(firstSettlementId, regionId));
        state.Settlements.Add(secondSettlementId, Settlement.Create(secondSettlementId, regionId));

        var firstEventId = state.DisasterEventIds.Issue();
        state.DisasterEvents.Add(firstEventId, DisasterEvent.Create(
            firstEventId, firstSettlementId, new GameDate(10), HazardType.Fire, DisasterSeverity.Minor));
        var secondEventId = state.DisasterEventIds.Issue();
        state.DisasterEvents.Add(secondEventId, DisasterEvent.Create(
            secondEventId, secondSettlementId, new GameDate(10), HazardType.Flood, DisasterSeverity.Moderate));

        var events = HazardQueries.EventsFor(state, firstSettlementId).ToArray();

        Assert.That(events, Has.Length.EqualTo(1));
        Assert.That(events[0].Id, Is.EqualTo(firstEventId));
    }

    [Test]
    public void DormantVolcanoOnReturnsNullForAPlotWithNoDesignation()
    {
        var state = new WorldState(new GameDate(10));
        var plotId = state.PlotIds.Issue();

        Assert.That(HazardQueries.DormantVolcanoOn(state, plotId), Is.Null);
    }
}
