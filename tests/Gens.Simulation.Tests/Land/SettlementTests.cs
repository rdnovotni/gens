using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Land;

public sealed class SettlementTests
{
    [Test]
    public void CreateDefaultsToVillaStage()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();

        var settlement = Settlement.Create(settlementId, regionId);

        Assert.That(settlement.Stage, Is.EqualTo(SettlementStage.Villa));
    }

    [Test]
    public void CreateStoresAllFields()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();

        var settlement = Settlement.Create(settlementId, regionId, SettlementStage.Town);

        Assert.That(settlement.Id, Is.EqualTo(settlementId));
        Assert.That(settlement.RegionId, Is.EqualTo(regionId));
        Assert.That(settlement.Stage, Is.EqualTo(SettlementStage.Town));
    }

    [Test]
    public void SettlementsRegistryIteratesInAscendingIdOrder()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        var idA = state.SettlementIds.Issue();
        var idB = state.SettlementIds.Issue();

        state.Settlements.Add(idB, Settlement.Create(idB, regionId));
        state.Settlements.Add(idA, Settlement.Create(idA, regionId));

        var ids = state.Settlements.InAscendingOrder().Select(e => e.Key).ToArray();
        Assert.That(ids, Is.EqualTo(new[] { idA, idB }));
    }
}
