using Gens.Simulation.Hazards;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Hazards;

public sealed class HazardExposureProfileTests
{
    // October (storm season, §3.1) vs. February (neither dry nor storm season).
    private static readonly GameDate StormSeasonDate = new(9);
    private static readonly GameDate NonSeasonalDate = new(1);

    [Test]
    public void FloodExposureAlsoRisesDuringStormSeasonNotOnlyStorm()
    {
        var state = new WorldState(NonSeasonalDate);
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId, TerrainType.River, capacity: 1));

        var inSeason = HazardExposureProfile.Compute(state, settlementId, StormSeasonDate).ExposureFor(HazardType.Flood);
        var outOfSeason = HazardExposureProfile.Compute(state, settlementId, NonSeasonalDate).ExposureFor(HazardType.Flood);

        Assert.That(inSeason, Is.GreaterThan(outOfSeason));
    }
}
