using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Land;

public sealed class PlotTests
{
    [Test]
    public void CreateStoresAllFields()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();
        var plotId = state.PlotIds.Issue();

        var plot = Plot.Create(plotId, settlementId);

        Assert.That(plot.Id, Is.EqualTo(plotId));
        Assert.That(plot.SettlementId, Is.EqualTo(settlementId));
    }

    [Test]
    public void PlotsRegistryIteratesInAscendingIdOrder()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();
        var idA = state.PlotIds.Issue();
        var idB = state.PlotIds.Issue();

        state.Plots.Add(idB, Plot.Create(idB, settlementId));
        state.Plots.Add(idA, Plot.Create(idA, settlementId));

        var ids = state.Plots.InAscendingOrder().Select(e => e.Key).ToArray();
        Assert.That(ids, Is.EqualTo(new[] { idA, idB }));
    }
}
