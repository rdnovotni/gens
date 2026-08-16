using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class NeedsConsumptionSystemTests
{
    [Test]
    public void EmitsOneEventPerSettlementWithOneLinePerPopGroup()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.Coloni),
            PopGroup.Create(settlementId, PopGroupType.Coloni, 10, needsProfile: DietTier.Adequate));

        var events = new NeedsConsumptionSystem().Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        var resolved = (NeedsConsumptionComputedEvent)events.Single();
        Assert.That(resolved.SettlementId, Is.EqualTo(settlementId));
        var line = resolved.Lines.Single();
        Assert.That(line.GroupType, Is.EqualTo(PopGroupType.Coloni));
        Assert.That(line.GoodId, Is.EqualTo(NeedsConsumptionCalculator.ConsumptionGood));
        Assert.That(line.Quantity, Is.EqualTo(20L));
    }

    [Test]
    public void EmitsAnEventForASettlementWithNoPopGroups()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));

        var events = new NeedsConsumptionSystem().Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        var resolved = (NeedsConsumptionComputedEvent)events.Single();
        Assert.That(resolved.Lines, Is.Empty);
    }
}
