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

public sealed class MigrationSystemTests
{
    private const string EmigrationStream = CampaignBootstrapper.PopGroupEmigrationStreamName;
    private const string ImmigrationStream = CampaignBootstrapper.PopGroupImmigrationStreamName;

    [Test]
    public void LowContentmentGroupLosesPopulationAndKeepsTheDistributionConsistent()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        var key = new PopGroupKey(settlementId, PopGroupType.Aeditui);
        // 1,000 * the emigration rate at Contentment=0 (0.03) is an exact whole number (-30),
        // deterministic regardless of the stochastic-rounding roll.
        state.PopGroups.Add(key, PopGroup.Create(settlementId, PopGroupType.Aeditui, 1_000, contentment: Fixed64.Zero));

        var events = new MigrationSystem(EmigrationStream, ImmigrationStream).Tick(state, Context(state));

        state.PopGroups.TryGet(key, out var group);
        Assert.That(group.Size, Is.EqualTo(970));
        Assert.That(group.LegalStatusDistribution.Total, Is.EqualTo(group.Size));
        var line = events.Cast<MigrationAppliedEvent>().Single(e => e.GroupType == PopGroupType.Aeditui);
        Assert.That(line.Direction, Is.EqualTo(MigrationDirection.Emigration));
        Assert.That(line.Count, Is.EqualTo(30));
    }

    [Test]
    public void ContentSettlementWithRoomAttractsImmigrantsIntoColoniAndOperarii()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.Coloni),
            PopGroup.Create(settlementId, PopGroupType.Coloni, 10_000, contentment: Fixed64.One, employmentRatio: Fixed64.FromInt(2), housingSatisfaction: Fixed64.FromInt(2)));
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.Operarii),
            PopGroup.Create(settlementId, PopGroupType.Operarii, 10_000, contentment: Fixed64.One, employmentRatio: Fixed64.FromInt(2), housingSatisfaction: Fixed64.FromInt(2)));

        new MigrationSystem(EmigrationStream, ImmigrationStream).Tick(state, Context(state));

        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Coloni), out var coloni);
        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Operarii), out var operarii);
        Assert.That(coloni.Size + operarii.Size, Is.GreaterThan(20_000));
    }

    [Test]
    public void SettlementWithNeitherDefaultEntryGroupReceivesNoImmigration()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.Curiales),
            PopGroup.Create(settlementId, PopGroupType.Curiales, 1_000, contentment: Fixed64.One, employmentRatio: Fixed64.FromInt(2), housingSatisfaction: Fixed64.FromInt(2)));

        var events = new MigrationSystem(EmigrationStream, ImmigrationStream).Tick(state, Context(state));

        Assert.That(events.Cast<MigrationAppliedEvent>().Any(e => e.Direction == MigrationDirection.Immigration), Is.False);
    }

    [Test]
    public void ConstructorRejectsEmptyStreamNames()
    {
        Assert.Throws<ArgumentException>(() => new MigrationSystem("", ImmigrationStream));
        Assert.Throws<ArgumentException>(() => new MigrationSystem(EmigrationStream, ""));
    }

    private static WorldState NewWorldState() => new(new GameDate(0));

    private static MonthlyTickContext Context(WorldState state)
    {
        var streams = new RandomStreamSet();
        streams.AddDerived(EmigrationStream, 111UL);
        streams.AddDerived(ImmigrationStream, 222UL);
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
