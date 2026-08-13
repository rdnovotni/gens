using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Land;

public sealed class HoldingTests
{
    [Test]
    public void CreateStoresAllFields()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();
        var holdingId = state.HoldingIds.Issue();

        var holding = Holding.Create(holdingId, settlementId);

        Assert.That(holding.Id, Is.EqualTo(holdingId));
        Assert.That(holding.SettlementId, Is.EqualTo(settlementId));
    }

    [Test]
    public void HoldingsRegistryIteratesInAscendingIdOrder()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();
        var idA = state.HoldingIds.Issue();
        var idB = state.HoldingIds.Issue();

        state.Holdings.Add(idB, Holding.Create(idB, settlementId));
        state.Holdings.Add(idA, Holding.Create(idA, settlementId));

        var ids = state.Holdings.InAscendingOrder().Select(e => e.Key).ToArray();
        Assert.That(ids, Is.EqualTo(new[] { idA, idB }));
    }
}
