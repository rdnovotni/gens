using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Numerics;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class GrowthMortalitySystemTests
{
    private const string StreamName = CampaignBootstrapper.PopGroupGrowthMortalityStreamName;

    [Test]
    public void HighContentmentGrowsTheGroupAndConservesTheDistributionTotal()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        var key = new PopGroupKey(settlementId, PopGroupType.Coloni);
        // A large group so a small positive rate reliably produces at least a whole-unit gain even
        // before the stochastic remainder is resolved.
        state.PopGroups.Add(key, PopGroup.Create(settlementId, PopGroupType.Coloni, 100_000, contentment: Fixed64.One, healthExposure: Fixed64.Zero));

        var events = new GrowthMortalitySystem(StreamName).Tick(state, Context(state));

        state.PopGroups.TryGet(key, out var group);
        Assert.That(group.Size, Is.GreaterThan(100_000));
        Assert.That(group.LegalStatusDistribution.Total, Is.EqualTo(group.Size));
        Assert.That(events, Has.Count.EqualTo(1));
    }

    [Test]
    public void SevereHardshipShrinksTheGroup()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        var key = new PopGroupKey(settlementId, PopGroupType.Coloni);
        // 1,000 * the clamped minimum rate (-0.02) is an exact whole number (-20), so this shrink is
        // deterministic regardless of the stochastic-rounding roll.
        state.PopGroups.Add(key, PopGroup.Create(settlementId, PopGroupType.Coloni, 1_000, contentment: Fixed64.Zero, healthExposure: Fixed64.One));

        new GrowthMortalitySystem(StreamName).Tick(state, Context(state));

        state.PopGroups.TryGet(key, out var group);
        Assert.That(group.Size, Is.EqualTo(980));
    }

    [Test]
    public void ClampingKeepsSizeFromGoingNegativeForATinyGroup()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        var key = new PopGroupKey(settlementId, PopGroupType.Coloni);
        state.PopGroups.Add(key, PopGroup.Create(settlementId, PopGroupType.Coloni, 1, contentment: Fixed64.Zero, healthExposure: Fixed64.One));

        new GrowthMortalitySystem(StreamName).Tick(state, Context(state));

        state.PopGroups.TryGet(key, out var group);
        Assert.That(group.Size, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void SameSeedProducesTheSameOutcome()
    {
        var stateA = NewWorldState();
        var settlementA = SetupSettlement(stateA);
        var keyA = new PopGroupKey(settlementA, PopGroupType.Coloni);
        stateA.PopGroups.Add(keyA, PopGroup.Create(settlementA, PopGroupType.Coloni, 41, contentment: Fixed64.One));
        var streamsA = new RandomStreamSet();
        streamsA.AddDerived(StreamName, 12345UL);

        var stateB = NewWorldState();
        var settlementB = SetupSettlement(stateB);
        var keyB = new PopGroupKey(settlementB, PopGroupType.Coloni);
        stateB.PopGroups.Add(keyB, PopGroup.Create(settlementB, PopGroupType.Coloni, 41, contentment: Fixed64.One));
        var streamsB = new RandomStreamSet();
        streamsB.AddDerived(StreamName, 12345UL);

        new GrowthMortalitySystem(StreamName).Tick(stateA, new MonthlyTickContext(stateA.Date, streamsA));
        new GrowthMortalitySystem(StreamName).Tick(stateB, new MonthlyTickContext(stateB.Date, streamsB));

        stateA.PopGroups.TryGet(keyA, out var groupA);
        stateB.PopGroups.TryGet(keyB, out var groupB);
        Assert.That(groupA.Size, Is.EqualTo(groupB.Size));
    }

    [Test]
    public void ConstructorRejectsAnEmptyStreamName()
    {
        Assert.Throws<ArgumentException>(() => new GrowthMortalitySystem(""));
    }

    private static WorldState NewWorldState() => new(new GameDate(0));

    private static MonthlyTickContext Context(WorldState state)
    {
        var streams = new RandomStreamSet();
        streams.AddDerived(StreamName, 999UL);
        return new MonthlyTickContext(state.Date, streams);
    }

    private static RuntimeId<Settlement> SetupSettlement(WorldState state)
    {
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        return settlementId;
    }
}
