using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Land;

public sealed class LandAcquisitionTests
{
    [Test]
    public void AcquireRecordsOwnerProvenanceAndEmitsEvent()
    {
        var state = new WorldState(new GameDate(12));
        var plotId = state.PlotIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId, TerrainType.Hills, TerrainFeature.MineralDeposit));
        var command = new AcquirePlotCommand(
            state.CommandIds.Issue(), "household_0000001", state.Date, null, plotId,
            "household_0000001", AcquisitionMethod.LandGrant, "contract_0000003");

        var result = AcquirePlotCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        Assert.That(result.Events.Single(), Is.TypeOf<PlotAcquiredEvent>());
        Assert.That(state.Plots.TryGet(plotId, out var plot), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(plot.OwnerId, Is.EqualTo("household_0000001"));
            Assert.That(plot.Acquisition, Is.EqualTo(new LandAcquisition(
                AcquisitionMethod.LandGrant, state.Date, "contract_0000003")));
            Assert.That(plot.Terrain, Is.EqualTo(TerrainType.Hills));
        });
    }

    [TestCase(true, null, "land.acquire.plotContested")]
    [TestCase(false, "household_0000002", "land.acquire.alreadyOwned")]
    public void AcquireRejectsUnavailableLandWithoutMutation(bool contested, string? owner, string error)
    {
        var state = new WorldState(new GameDate(0));
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, state.SettlementIds.Issue(), isContested: contested, ownerId: owner));

        var result = AcquirePlotCommands.Pipeline.Execute(state, new AcquirePlotCommand(
            state.CommandIds.Issue(), "system", state.Date, null, plotId,
            "household_0000001", AcquisitionMethod.Purchase));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error?.Code, Is.EqualTo(error));
        Assert.That(state.NextCommandSequenceNumber, Is.Zero);
    }

    [Test]
    public void AcquireRejectsUndefinedAcquisitionMethodWithoutMutation()
    {
        var state = new WorldState(new GameDate(0));
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, state.SettlementIds.Issue()));

        var result = AcquirePlotCommands.Pipeline.Execute(state, new AcquirePlotCommand(
            state.CommandIds.Issue(), "system", state.Date, null, plotId,
            "household_0000001", (AcquisitionMethod)int.MaxValue));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error?.Code, Is.EqualTo(AcquirePlotCommands.InvalidMethod.Code));
        Assert.That(state.Plots.TryGet(plotId, out var plot), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(plot.OwnerId, Is.Null);
            Assert.That(plot.Acquisition, Is.Null);
            Assert.That(state.NextCommandSequenceNumber, Is.Zero);
        });
    }

    [Test]
    public void OccupyLinksCoLocatedSameOwnerHolding()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();
        var plotId = state.PlotIds.Issue();
        var holdingId = state.HoldingIds.Issue();
        const string owner = "household_0000001";
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId, ownerId: owner));
        state.Holdings.Add(holdingId, Holding.Create(holdingId, settlementId, owner));

        var result = OccupyPlotCommands.Pipeline.Execute(state, new OccupyPlotCommand(
            state.CommandIds.Issue(), owner, state.Date, null, plotId, holdingId));

        Assert.That(result.Accepted, Is.True);
        Assert.That(state.Plots.TryGet(plotId, out var plot), Is.True);
        Assert.That(plot.OccupyingHoldingId, Is.EqualTo(holdingId));
    }
}
