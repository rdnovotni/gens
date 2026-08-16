using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Numerics;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class ContentmentSystemTests
{
    [Test]
    public void WritesContentmentAndHealthExposureFromEmploymentAndHousing()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        var key = new PopGroupKey(settlementId, PopGroupType.Coloni);
        state.PopGroups.Add(key, PopGroup.Create(
            settlementId, PopGroupType.Coloni, 10,
            needsProfile: DietTier.Generous,
            employmentRatio: Fixed64.One,
            housingSatisfaction: Fixed64.One));

        var events = new ContentmentSystem().Tick(state, Context(state));

        state.PopGroups.TryGet(key, out var group);
        Assert.That(group.Contentment, Is.EqualTo(Fixed64.One));
        Assert.That(group.HealthExposure, Is.EqualTo(Fixed64.Zero));
        var line = ((ContentmentComputedEvent)events.Single()).Lines.Single();
        Assert.That(line.Contentment, Is.EqualTo(Fixed64.One));
    }

    [Test]
    public void OvercrowdedHousingRaisesHealthExposure()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        var key = new PopGroupKey(settlementId, PopGroupType.Operarii);
        state.PopGroups.Add(key, PopGroup.Create(
            settlementId, PopGroupType.Operarii, 10, housingSatisfaction: Fixed64.FromRaw(400_000)));

        new ContentmentSystem().Tick(state, Context(state));

        state.PopGroups.TryGet(key, out var group);
        Assert.That(group.HealthExposure, Is.EqualTo(Fixed64.FromRaw(600_000)));
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
