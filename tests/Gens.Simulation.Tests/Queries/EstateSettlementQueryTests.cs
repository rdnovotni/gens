using Gens.Simulation.Buildings;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Queries;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Queries;

public sealed class EstateSettlementQueryTests
{
    [Test]
    public void ProjectsOnlyTheHoldingsBuildingsAndPlotsOwnedByTheRequestedHousehold()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, state.RegionIds.Issue()));

        var householdId = state.HouseholdIds.Issue();
        var otherHouseholdId = state.HouseholdIds.Issue();

        var holdingId = state.HoldingIds.Issue();
        state.Holdings.Add(holdingId, Holding.Create(
            holdingId, settlementId, occupantId: householdId.ToTaggedString(), residentCapacity: 6));

        var otherHoldingId = state.HoldingIds.Issue();
        state.Holdings.Add(otherHoldingId, Holding.Create(
            otherHoldingId, settlementId, occupantId: otherHouseholdId.ToTaggedString(), residentCapacity: 6));

        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(
            plotId, settlementId, terrain: TerrainType.Hills, occupyingHoldingId: holdingId));

        var buildingDefinition = new BuildingDefinition(new DefinitionId<Building>("mine"), BuildingTier.Tier1, 2, 1);
        var buildingId = state.BuildingIds.Issue();
        state.Buildings.Add(buildingId, new BuildingInstance(buildingId, plotId, buildingDefinition));

        var projection = new EstateSettlementQuery(settlementId, householdId).Execute(state, "player");

        Assert.That(projection.Holdings, Has.Count.EqualTo(1));
        var holding = projection.Holdings[0];
        Assert.That(holding.HoldingId, Is.EqualTo(holdingId.ToTaggedString()));
        Assert.That(holding.Plots, Has.Count.EqualTo(1));
        Assert.That(holding.Plots[0].Buildings, Has.Count.EqualTo(1));
        Assert.That(holding.Plots[0].Buildings[0].DefinitionKey, Is.EqualTo("mine"));
    }
}
