using Gens.Simulation.Hazards;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Hazards;

public sealed class DesignateDormantVolcanoCommandTests
{
    [Test]
    public void DesignatingAPlotRecordsADormantVolcanoAndEmitsAnEvent()
    {
        var state = new WorldState(new GameDate(10));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId, TerrainType.Hills));

        var command = new DesignateDormantVolcanoCommand(state.CommandIds.Issue(), "system", new GameDate(10), null, plotId);
        var result = DesignateDormantVolcanoCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        Assert.That(state.DormantVolcanoes.TryGet(plotId, out var volcano), Is.True);
        Assert.That(volcano.SettlementId, Is.EqualTo(settlementId));
        Assert.That(volcano.HasErupted, Is.False);
        var applied = (DormantVolcanoDesignatedEvent)result.Events.Single();
        Assert.That(applied.PlotId, Is.EqualTo(plotId));
        Assert.That(applied.SettlementId, Is.EqualTo(settlementId));
    }

    [Test]
    public void ValidationRejectsAMissingPlot()
    {
        var state = new WorldState(new GameDate(10));
        var command = new DesignateDormantVolcanoCommand(
            state.CommandIds.Issue(), "system", new GameDate(10), null, new RuntimeIdCounter<Plot>().Issue());
        var result = DesignateDormantVolcanoCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(DesignateDormantVolcanoCommands.PlotNotFound));
    }

    [Test]
    public void ValidationRejectsADoubleDesignation()
    {
        var state = new WorldState(new GameDate(10));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId, TerrainType.Hills));
        state.DormantVolcanoes.Add(plotId, DormantVolcano.Create(plotId, settlementId));

        var command = new DesignateDormantVolcanoCommand(state.CommandIds.Issue(), "system", new GameDate(10), null, plotId);
        var result = DesignateDormantVolcanoCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(DesignateDormantVolcanoCommands.AlreadyDesignated));
    }
}
