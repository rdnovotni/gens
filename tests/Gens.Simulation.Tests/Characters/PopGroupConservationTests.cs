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
/// Roadmap Phase 7 item 6's conservation tests: proves every <see cref="PopGroup.Size"/> change the
/// full demographics pipeline (<see cref="PopulationDemographicsPipelineTests"/>'s identical system
/// wiring) makes across many months is fully explained by the domain events that document its cause —
/// <see cref="PopGroupGrowthMortalityAppliedEvent"/>, <see cref="MigrationAppliedEvent"/>, and <see
/// cref="SocialMobilityAppliedEvent"/> — and that no <see cref="PopGroupKey"/> present at the start of
/// a run is ever silently dropped or duplicated (<c>gens-settlement-demographics-design.md</c> §5).
/// <see cref="AssimilationAppliedEvent"/> is deliberately excluded from the size reconciliation: per
/// <see cref="AssimilationSystem"/>'s own doc comment it only reshapes <see
/// cref="PopGroup.LegalStatusDistribution"/>, never <see cref="PopGroup.Size"/>.
/// </summary>
public sealed class PopGroupConservationTests
{
    [Test]
    public void EveryGroupsMonthlySizeChangeEqualsTheSumOfItsCauseEvents()
    {
        var (state, streams) = BuildScenario();
        var simulation = BuildSimulation();

        for (var month = 0; month < 24; month++)
        {
            var sizesBefore = state.PopGroups.InAscendingOrder()
                .ToDictionary(entry => entry.Key, entry => entry.Value.Size);

            var events = simulation.Tick(state, state.Date, streams);

            var causedDelta = new Dictionary<PopGroupKey, int>();
            void Add(PopGroupKey key, int delta) =>
                causedDelta[key] = causedDelta.GetValueOrDefault(key) + delta;

            foreach (var domainEvent in events)
            {
                switch (domainEvent)
                {
                    case PopGroupGrowthMortalityAppliedEvent e:
                        Add(new PopGroupKey(e.SettlementId, e.GroupType), e.Delta);
                        break;
                    case MigrationAppliedEvent e:
                        Add(new PopGroupKey(e.SettlementId, e.GroupType),
                            e.Direction == MigrationDirection.Immigration ? e.Count : -e.Count);
                        break;
                    case SocialMobilityAppliedEvent e:
                        Add(new PopGroupKey(e.SettlementId, e.Source), -e.Count);
                        Add(new PopGroupKey(e.SettlementId, e.Destination), e.Count);
                        break;
                }
            }

            foreach (var key in sizesBefore.Keys)
            {
                Assert.That(state.PopGroups.TryGet(key, out var after), Is.True,
                    $"{key} disappeared without a promotion or removal cause.");
                var actualDelta = after.Size - sizesBefore[key];
                var expectedDelta = causedDelta.GetValueOrDefault(key);
                Assert.That(actualDelta, Is.EqualTo(expectedDelta),
                    $"{key}'s size changed by {actualDelta} this month but its cause events only account for {expectedDelta}.");
            }

            state.AdvanceMonth();
        }
    }

    [Test]
    public void NoGroupSilentlyDuplicatesOrDisappearsAcrossManyMonths()
    {
        var (state, streams) = BuildScenario();
        var simulation = BuildSimulation();

        var originalKeys = state.PopGroups.InAscendingOrder().Select(entry => entry.Key).ToHashSet();

        for (var month = 0; month < 24; month++)
        {
            simulation.Tick(state, state.Date, streams);

            var currentKeys = state.PopGroups.InAscendingOrder().Select(entry => entry.Key).ToHashSet();
            Assert.That(currentKeys, Is.EquivalentTo(originalKeys),
                "The set of registered pop groups must stay exactly stable — this pipeline never " +
                "introduces or removes a group, only moves Size between existing ones.");
            Assert.That(state.PopGroups.InAscendingOrder().Count(), Is.EqualTo(originalKeys.Count),
                "Duplicate registrations would inflate the entry count beyond the original group set.");

            state.AdvanceMonth();
        }
    }

    [Test]
    public void GlobalPopulationChangeEqualsNetGrowthMortalityAndMigration()
    {
        var (state, streams) = BuildScenario();
        var simulation = BuildSimulation();

        for (var month = 0; month < 24; month++)
        {
            var totalBefore = state.PopGroups.InAscendingOrder().Sum(entry => entry.Value.Size);

            var events = simulation.Tick(state, state.Date, streams);

            // Social mobility only moves Size between two existing groups in the same settlement — its
            // events always net to zero globally, so only growth/mortality and migration can change the
            // world's total population.
            var netCause = events.Sum(domainEvent => domainEvent switch
            {
                PopGroupGrowthMortalityAppliedEvent e => e.Delta,
                MigrationAppliedEvent e => e.Direction == MigrationDirection.Immigration ? e.Count : -e.Count,
                _ => 0,
            });

            var totalAfter = state.PopGroups.InAscendingOrder().Sum(entry => entry.Value.Size);

            Assert.That(totalAfter - totalBefore, Is.EqualTo(netCause));

            state.AdvanceMonth();
        }
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
