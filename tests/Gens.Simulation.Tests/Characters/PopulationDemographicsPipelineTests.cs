using Gens.Simulation.Buildings;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

/// <summary>
/// Integration coverage for Roadmap Phase 7 item 4's whole chain — job capacity → employment matching
/// → needs/consumption → housing capacity → contentment → growth/mortality → migration → social
/// mobility → assimilation — wired together via <see cref="MonthlySimulation{TState}"/> exactly as a
/// real campaign would, proving the declared <see cref="IMonthlySystem{TState}.Prerequisites"/> chain
/// actually orders correctly and the whole pipeline runs cleanly over many months without throwing,
/// producing negative population, or losing determinism.
/// </summary>
public sealed class PopulationDemographicsPipelineTests
{
    [Test]
    public void RunsManyMonthsWithoutExceptionsOrNegativePopulation()
    {
        var (state, streams) = BuildScenario();
        var simulation = BuildSimulation();

        for (var month = 0; month < 24; month++)
        {
            Assert.That(() => simulation.Tick(state, state.Date, streams), Throws.Nothing);
            foreach (var entry in state.PopGroups.InAscendingOrder())
                Assert.That(entry.Value.Size, Is.GreaterThanOrEqualTo(0));
            state.AdvanceMonth();
        }
    }

    [Test]
    public void SameSeedProducesIdenticalStateHashesAcrossRepeatedRuns()
    {
        var hashesA = RunAndHashEachMonth();
        var hashesB = RunAndHashEachMonth();

        Assert.That(hashesA, Is.EqualTo(hashesB));
    }

    private static List<ulong> RunAndHashEachMonth()
    {
        var (state, streams) = BuildScenario();
        var simulation = BuildSimulation();
        var hashes = new List<ulong>();

        for (var month = 0; month < 12; month++)
        {
            simulation.Tick(state, state.Date, streams);
            hashes.Add(StateHasher.Hash(state));
            state.AdvanceMonth();
        }

        return hashes;
    }

    private static MonthlySimulation<WorldState> BuildSimulation() => new(new IMonthlySystem<WorldState>[]
    {
        new JobCapacitySystem(),
        new EmploymentMatchingSystem(),
        new NeedsConsumptionSystem(),
        new HousingCapacitySystem(),
        new ContentmentSystem(),
        new GrowthMortalitySystem(CampaignBootstrapper.PopGroupGrowthMortalityStreamName),
        new MigrationSystem(CampaignBootstrapper.PopGroupEmigrationStreamName, CampaignBootstrapper.PopGroupImmigrationStreamName),
        new SocialMobilitySystem(),
        new AssimilationSystem(),
    });

    private static (WorldState State, RandomStreamSet Streams) BuildScenario()
    {
        var state = new WorldState(new GameDate(0));
        var streams = new RandomStreamSet();
        const ulong seed = 424242UL;
        streams.AddDerived(CampaignBootstrapper.PopGroupGrowthMortalityStreamName, seed);
        streams.AddDerived(CampaignBootstrapper.PopGroupEmigrationStreamName, seed);
        streams.AddDerived(CampaignBootstrapper.PopGroupImmigrationStreamName, seed);

        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId, SettlementStage.Town));

        AddBuilding(state, settlementId, new DefinitionId<Building>("farm"), BuildingSector.Agriculture, backgroundJobCapacity: 400);
        AddBuilding(state, settlementId, new DefinitionId<Building>("market"), BuildingSector.Commerce, backgroundJobCapacity: 150);
        AddBuilding(state, settlementId, new DefinitionId<Building>("workshop"), BuildingSector.Industry, backgroundJobCapacity: 100);
        AddBuilding(state, settlementId, new DefinitionId<Building>("insula"), BuildingSector.None, residentialCapacity: 300);

        for (var i = 0; i < 10; i++)
        {
            var plotId = state.PlotIds.Issue();
            state.Plots.Add(plotId, Plot.Create(plotId, settlementId, terrain: TerrainType.FertilePlain));
        }

        foreach (var groupType in Enum.GetValues<PopGroupType>())
        {
            var key = new PopGroupKey(settlementId, groupType);
            state.PopGroups.Add(key, PopGroup.Create(settlementId, groupType, groupType == PopGroupType.Coloni ? 400 : 60));
        }

        return (state, streams);
    }

    private static void AddBuilding(
        WorldState state, RuntimeId<Settlement> settlementId, DefinitionId<Building> id, BuildingSector sector,
        int backgroundJobCapacity = 0, int residentialCapacity = 0)
    {
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId));
        var definition = new BuildingDefinition(
            id, BuildingTier.Tier1, constructionMonths: 1, plotCapacity: 1,
            sector: sector, backgroundJobCapacity: backgroundJobCapacity, residentialCapacity: residentialCapacity);
        var building = new BuildingInstance(state.BuildingIds.Issue(), plotId, definition);
        state.Buildings.Add(building.Id, building);
    }
}
