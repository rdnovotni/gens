using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class AssimilationSystemTests
{
    [Test]
    public void ShiftsDistributionTowardHigherStatusAndEmitsAnEvent()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        var key = new PopGroupKey(settlementId, PopGroupType.Coloni);
        state.PopGroups.Add(key, PopGroup.Create(
            settlementId, PopGroupType.Coloni, 1_000, legalStatusDistribution: new LegalStatusDistribution(0, 0, 1_000, 0)));

        var events = new AssimilationSystem().Tick(state, Context(state));

        state.PopGroups.TryGet(key, out var group);
        Assert.That(group.LegalStatusDistribution.Freedman, Is.GreaterThan(0));
        Assert.That(group.LegalStatusDistribution.Total, Is.EqualTo(1_000));
        var resolved = (AssimilationAppliedEvent)events.Single();
        Assert.That(resolved.GroupType, Is.EqualTo(PopGroupType.Coloni));
    }

    [Test]
    public void NeverTouchesSize()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        var key = new PopGroupKey(settlementId, PopGroupType.Coloni);
        state.PopGroups.Add(key, PopGroup.Create(
            settlementId, PopGroupType.Coloni, 1_000, legalStatusDistribution: new LegalStatusDistribution(0, 0, 1_000, 0)));

        new AssimilationSystem().Tick(state, Context(state));

        state.PopGroups.TryGet(key, out var group);
        Assert.That(group.Size, Is.EqualTo(1_000));
    }

    [Test]
    public void NonHouseholdEnslavedIsANoOpAndEmitsNoEvent()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.NonHouseholdEnslaved),
            PopGroup.Create(settlementId, PopGroupType.NonHouseholdEnslaved, 1_000));

        var events = new AssimilationSystem().Tick(state, Context(state));

        Assert.That(events, Is.Empty);
    }

    [Test]
    public void SmallGroupsBelowTheFlooredRateEmitNoEvent()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.Coloni),
            PopGroup.Create(settlementId, PopGroupType.Coloni, 5, legalStatusDistribution: new LegalStatusDistribution(0, 0, 5, 0)));

        var events = new AssimilationSystem().Tick(state, Context(state));

        Assert.That(events, Is.Empty);
    }

    private static WorldState NewWorldState() => new(new GameDate(0));

    private static MonthlyTickContext Context(WorldState state) => new(state.Date, new RandomStreamSet());

    private static RuntimeId<Settlement> SetupSettlement(WorldState state)
    {
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        return settlementId;
    }
}
