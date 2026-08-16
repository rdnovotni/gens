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

public sealed class HousingCapacitySystemTests
{
    [Test]
    public void UrbanCapacityIsSharedAcrossTheFiveUrbanGroups()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        AddBuilding(state, settlementId, Insula(capacity: 20));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Operarii), PopGroup.Create(settlementId, PopGroupType.Operarii, 10));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Opifices), PopGroup.Create(settlementId, PopGroupType.Opifices, 10));

        new HousingCapacitySystem().Tick(state, Context(state));

        // 20 capacity / 20 combined urban population = 1.0 satisfaction, shared by both groups.
        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Operarii), out var operarii);
        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Opifices), out var opifices);
        Assert.That(operarii.HousingSatisfaction, Is.EqualTo(Fixed64.One));
        Assert.That(opifices.HousingSatisfaction, Is.EqualTo(Fixed64.One));
    }

    [Test]
    public void RuralCapacityComesFromUnclaimedFertilePlotsOnly()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        AddPlot(state, settlementId, TerrainType.FertilePlain, owned: false);
        AddPlot(state, settlementId, TerrainType.FertilePlain, owned: false);
        AddPlot(state, settlementId, TerrainType.FertilePlain, owned: true); // Claimed: doesn't count.
        AddPlot(state, settlementId, TerrainType.Hills, owned: false); // Not fertile: doesn't count.
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Coloni), PopGroup.Create(
            settlementId, PopGroupType.Coloni, HousingCapacityCalculator.ResidentsPerUnclaimedFertilePlot * 2));

        new HousingCapacitySystem().Tick(state, Context(state));

        // 2 unclaimed fertile plots * ResidentsPerUnclaimedFertilePlot = exactly the Coloni size -> 1.0.
        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Coloni), out var coloni);
        Assert.That(coloni.HousingSatisfaction, Is.EqualTo(Fixed64.One));
    }

    [Test]
    public void NonHouseholdEnslavedIsNeverWritten()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.NonHouseholdEnslaved),
            PopGroup.Create(settlementId, PopGroupType.NonHouseholdEnslaved, 10));

        new HousingCapacitySystem().Tick(state, Context(state));

        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.NonHouseholdEnslaved), out var group);
        Assert.That(group.HousingSatisfaction, Is.EqualTo(Fixed64.One)); // Untouched default.
    }

    [Test]
    public void MissingPopGroupsAreLeftUncreated()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        AddBuilding(state, settlementId, Insula(capacity: 20));

        Assert.That(() => new HousingCapacitySystem().Tick(state, Context(state)), Throws.Nothing);
        Assert.That(state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Operarii), out _), Is.False);
    }

    private static readonly DefinitionId<Building> InsulaId = new("insula");

    private static BuildingDefinition Insula(int capacity) => new(
        InsulaId, BuildingTier.Tier1, constructionMonths: 1, plotCapacity: 1, residentialCapacity: capacity);

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

    private static void AddBuilding(WorldState state, RuntimeId<Settlement> settlementId, BuildingDefinition definition)
    {
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId));
        var building = new BuildingInstance(state.BuildingIds.Issue(), plotId, definition);
        state.Buildings.Add(building.Id, building);
    }

    private static void AddPlot(WorldState state, RuntimeId<Settlement> settlementId, TerrainType terrain, bool owned)
    {
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId, terrain: terrain, ownerId: owned ? "owner-1" : null));
    }
}
