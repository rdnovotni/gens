using Gens.Simulation.Buildings;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Numerics;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class JobCapacitySystemTests
{
    [Test]
    public void AgricultureBuildingsRaiseColoniEmploymentRatio()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        AddBuilding(state, settlementId, Farm(capacity: 20));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Coloni), PopGroup.Create(settlementId, PopGroupType.Coloni, 10));

        new JobCapacitySystem().Tick(state, Context(state));

        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Coloni), out var coloni);
        Assert.That(coloni.EmploymentRatio, Is.EqualTo(Fixed64.Divide(Fixed64.FromInt(20), Fixed64.FromInt(10))));
    }

    [Test]
    public void CommerceBuildingsFeedBothNegotiatoresAndAnOperariiBaseline()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        AddBuilding(state, settlementId, Market(capacity: 6));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Negotiatores), PopGroup.Create(settlementId, PopGroupType.Negotiatores, 3));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Operarii), PopGroup.Create(settlementId, PopGroupType.Operarii, 12));

        new JobCapacitySystem().Tick(state, Context(state));

        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Negotiatores), out var negotiatores);
        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Operarii), out var operarii);
        Assert.Multiple(() =>
        {
            Assert.That(negotiatores.EmploymentRatio, Is.EqualTo(Fixed64.Divide(Fixed64.FromInt(6), Fixed64.FromInt(3))));
            Assert.That(operarii.EmploymentRatio, Is.EqualTo(Fixed64.Divide(Fixed64.FromInt(6), Fixed64.FromInt(12))));
        });
    }

    [Test]
    public void RuinedBuildingsContributeNoCapacity()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        var farm = AddBuilding(state, settlementId, Farm(capacity: 20));
        for (var i = 0; i < (int)BuildingCondition.Pristine; i++)
            farm.ApplyUpkeep(paid: false);
        Assert.That(farm.Condition, Is.EqualTo(BuildingCondition.Ruined), "Precondition: the building is fully ruined.");
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Coloni), PopGroup.Create(settlementId, PopGroupType.Coloni, 10));

        new JobCapacitySystem().Tick(state, Context(state));

        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Coloni), out var coloni);
        Assert.That(coloni.EmploymentRatio, Is.EqualTo(Fixed64.Zero));
    }

    [Test]
    public void CurialesCapacityScalesWithTotalPopulationAndSettlementStage()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state, SettlementStage.Town);
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Coloni), PopGroup.Create(settlementId, PopGroupType.Coloni, 800));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Curiales), PopGroup.Create(settlementId, PopGroupType.Curiales, 20));

        new JobCapacitySystem().Tick(state, Context(state));

        // Town = 50 basis points; total population (800 Coloni + 20 Curiales) = 820 -> 820 * 50 / 1000 = 41 slots.
        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Curiales), out var curiales);
        Assert.That(curiales.EmploymentRatio, Is.EqualTo(Fixed64.Divide(Fixed64.FromInt(41), Fixed64.FromInt(20))));
    }

    [Test]
    public void MissingPopGroupsAreLeftUncreated()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        AddBuilding(state, settlementId, Farm(capacity: 20));
        // No Coloni PopGroup registered for this settlement at all.

        Assert.That(() => new JobCapacitySystem().Tick(state, Context(state)), Throws.Nothing);
        Assert.That(state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Coloni), out _), Is.False);
    }

    [Test]
    public void EmitsOneCapacityEventPerSettlementWithSortedLines()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        AddBuilding(state, settlementId, Farm(capacity: 20));
        AddBuilding(state, settlementId, Temple(capacity: 4));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Coloni), PopGroup.Create(settlementId, PopGroupType.Coloni, 10));

        var events = new JobCapacitySystem().Tick(state, Context(state));

        var resolved = (BackgroundJobCapacityComputedEvent)events.Single();
        Assert.That(resolved.SettlementId, Is.EqualTo(settlementId));
        // Every sector-driven group gets a line, even at zero, plus Curiales — ascending PopGroupType
        // declaration order: Coloni, Operarii, Opifices, Negotiatores, Aeditui, Curiales.
        Assert.That(resolved.Capacities.Select(line => line.GroupType), Is.EqualTo(new[]
        {
            PopGroupType.Coloni, PopGroupType.Operarii, PopGroupType.Opifices,
            PopGroupType.Negotiatores, PopGroupType.Aeditui, PopGroupType.Curiales,
        }));
        Assert.That(resolved.Capacities.Single(line => line.GroupType == PopGroupType.Coloni).AvailableSlots, Is.EqualTo(20));
        Assert.That(resolved.Capacities.Single(line => line.GroupType == PopGroupType.Aeditui).AvailableSlots, Is.EqualTo(4));
        Assert.That(resolved.Capacities.Single(line => line.GroupType == PopGroupType.Operarii).AvailableSlots, Is.EqualTo(0));
    }

    private static readonly DefinitionId<Building> FarmId = new("farm");
    private static readonly DefinitionId<Building> MarketId = new("market");
    private static readonly DefinitionId<Building> TempleId = new("temple");

    private static BuildingDefinition Farm(int capacity) => new(
        FarmId, BuildingTier.Tier1, constructionMonths: 1, plotCapacity: 1,
        sector: BuildingSector.Agriculture, backgroundJobCapacity: capacity);

    private static BuildingDefinition Market(int capacity) => new(
        MarketId, BuildingTier.Tier1, constructionMonths: 1, plotCapacity: 1,
        sector: BuildingSector.Commerce, backgroundJobCapacity: capacity);

    private static BuildingDefinition Temple(int capacity) => new(
        TempleId, BuildingTier.Tier1, constructionMonths: 1, plotCapacity: 1,
        sector: BuildingSector.Religion, backgroundJobCapacity: capacity);

    private static WorldState NewWorldState() => new(new GameDate(0));

    private static MonthlyTickContext Context(WorldState state) => new(state.Date, new RandomStreamSet());

    private static RuntimeId<Settlement> SetupSettlement(WorldState state, SettlementStage stage = SettlementStage.Villa)
    {
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId, stage));
        return settlementId;
    }

    private static BuildingInstance AddBuilding(WorldState state, RuntimeId<Settlement> settlementId, BuildingDefinition definition)
    {
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId));
        var building = new BuildingInstance(state.BuildingIds.Issue(), plotId, definition);
        state.Buildings.Add(building.Id, building);
        return building;
    }
}
