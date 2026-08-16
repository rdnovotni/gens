using Gens.Simulation.Buildings;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class EmploymentMatchingSystemTests
{
    [Test]
    public void EmployedCountIsCappedAtAvailableSlots()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        AddBuilding(state, settlementId, Farm(capacity: 6));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Coloni), PopGroup.Create(settlementId, PopGroupType.Coloni, 10));

        var events = new EmploymentMatchingSystem().Tick(state, Context(state));

        var line = Line(events, PopGroupType.Coloni);
        Assert.Multiple(() =>
        {
            Assert.That(line.Employed, Is.EqualTo(6));
            Assert.That(line.Underemployed, Is.EqualTo(4));
        });
    }

    [Test]
    public void EmployedCountIsCappedAtGroupSizeWhenSlotsExceedPopulation()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        AddBuilding(state, settlementId, Farm(capacity: 20));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Coloni), PopGroup.Create(settlementId, PopGroupType.Coloni, 10));

        var events = new EmploymentMatchingSystem().Tick(state, Context(state));

        var line = Line(events, PopGroupType.Coloni);
        Assert.Multiple(() =>
        {
            Assert.That(line.Employed, Is.EqualTo(10));
            Assert.That(line.Underemployed, Is.EqualTo(0));
        });
    }

    [Test]
    public void WagesOfferedAreEmployedCountTimesTheGroupsWageRate()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        AddBuilding(state, settlementId, Market(capacity: 4));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Opifices), PopGroup.Create(settlementId, PopGroupType.Opifices, 4));

        var events = new EmploymentMatchingSystem().Tick(state, Context(state));

        // Opifices don't draw a Commerce-sourced slot (only Industry does), so with no Industry
        // building this group is entirely underemployed and offered zero wages.
        var line = Line(events, PopGroupType.Opifices);
        Assert.Multiple(() =>
        {
            Assert.That(line.Employed, Is.EqualTo(0));
            Assert.That(line.WagesOfferedDenarii, Is.EqualTo(0));
        });
    }

    [Test]
    public void WagesOfferedScaleWithSkilledVersusUnskilledWageRates()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        AddBuilding(state, settlementId, Farm(capacity: 5));
        AddBuilding(state, settlementId, Workshop(capacity: 5));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Coloni), PopGroup.Create(settlementId, PopGroupType.Coloni, 5));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Opifices), PopGroup.Create(settlementId, PopGroupType.Opifices, 5));

        var events = new EmploymentMatchingSystem().Tick(state, Context(state));

        // Coloni (unskilled, 4 denarii/month) vs. Opifices (skilled, 8 denarii/month) — the proposed
        // vertical-slice figures (gens-vertical-slice-quantification.md §5).
        Assert.Multiple(() =>
        {
            Assert.That(Line(events, PopGroupType.Coloni).WagesOfferedDenarii, Is.EqualTo(5 * 4));
            Assert.That(Line(events, PopGroupType.Opifices).WagesOfferedDenarii, Is.EqualTo(5 * 8));
        });
    }

    [Test]
    public void NonHouseholdEnslavedAndVeteransAreExcludedFromMatching()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.NonHouseholdEnslaved),
            PopGroup.Create(settlementId, PopGroupType.NonHouseholdEnslaved, 50));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Veterans), PopGroup.Create(settlementId, PopGroupType.Veterans, 20));

        var events = new EmploymentMatchingSystem().Tick(state, Context(state));

        var resolved = (EmploymentMatchedEvent)events.Single();
        Assert.That(resolved.Matches.Select(m => m.GroupType), Does.Not.Contain(PopGroupType.NonHouseholdEnslaved));
        Assert.That(resolved.Matches.Select(m => m.GroupType), Does.Not.Contain(PopGroupType.Veterans));
    }

    [Test]
    public void MissingPopGroupsReadAsZeroSizeAndAreStillReported()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        AddBuilding(state, settlementId, Farm(capacity: 20));
        // No Coloni PopGroup registered for this settlement at all.

        var events = new EmploymentMatchingSystem().Tick(state, Context(state));

        var line = Line(events, PopGroupType.Coloni);
        Assert.Multiple(() =>
        {
            Assert.That(line.Employed, Is.EqualTo(0));
            Assert.That(line.Underemployed, Is.EqualTo(0));
            Assert.That(line.WagesOfferedDenarii, Is.EqualTo(0));
        });
    }

    [Test]
    public void EmitsOneMatchEventPerSettlementWithSortedLinesForAllSixWageEligibleGroups()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);

        var events = new EmploymentMatchingSystem().Tick(state, Context(state));

        var resolved = (EmploymentMatchedEvent)events.Single();
        Assert.That(resolved.SettlementId, Is.EqualTo(settlementId));
        Assert.That(resolved.Matches.Select(m => m.GroupType), Is.EqualTo(new[]
        {
            PopGroupType.Coloni, PopGroupType.Operarii, PopGroupType.Opifices,
            PopGroupType.Negotiatores, PopGroupType.Aeditui, PopGroupType.Curiales,
        }));
    }

    private static EmploymentMatchLine Line(IReadOnlyList<IDomainEvent> events, PopGroupType groupType) =>
        ((EmploymentMatchedEvent)events.Single()).Matches.Single(m => m.GroupType == groupType);

    private static readonly DefinitionId<Building> FarmId = new("farm");
    private static readonly DefinitionId<Building> MarketId = new("market");
    private static readonly DefinitionId<Building> WorkshopId = new("workshop");

    private static BuildingDefinition Farm(int capacity) => new(
        FarmId, BuildingTier.Tier1, constructionMonths: 1, plotCapacity: 1,
        sector: BuildingSector.Agriculture, backgroundJobCapacity: capacity);

    private static BuildingDefinition Market(int capacity) => new(
        MarketId, BuildingTier.Tier1, constructionMonths: 1, plotCapacity: 1,
        sector: BuildingSector.Commerce, backgroundJobCapacity: capacity);

    private static BuildingDefinition Workshop(int capacity) => new(
        WorkshopId, BuildingTier.Tier1, constructionMonths: 1, plotCapacity: 1,
        sector: BuildingSector.Industry, backgroundJobCapacity: capacity);

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
