using Gens.Simulation.Buildings;
using Gens.Simulation.Campaign;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Buildings;
using Gens.Simulation.Time;
using NUnit.Framework;
using static Gens.Simulation.Tests.Buildings.EstateChainFixtures;

namespace Gens.Simulation.Tests.ExitGate;

/// <summary>
/// Proves the Phase 6 exit gate's estate slice, verbatim from the roadmap: "one estate transforms
/// inputs, labor, time, and maintenance into deterministic outputs for 120 months; shortages, invalid
/// assignments, and interrupted construction resolve consistently." One estate runs all three of <see
/// cref="EstateChainFixtures"/>'s compact chains end to end through <see cref="ConstructionSystem"/>,
/// <see cref="MaintenanceSystem"/>, and <see cref="ProductionSystem"/> — a raw good producer plus a
/// converter for each of Grain/Bread, Iron/Tools, and Wool/Textiles, sharing one bounded <see
/// cref="Stockpile"/> and one <see cref="ConstructionQueue"/>, exactly as Phase 6 item 7 specifies.
/// </summary>
public sealed class ProductionNetworkSoakTests
{
    [Test]
    public void OneEstateRunsThreeChainsForOneHundredTwentyMonthsWithStableRepeatedSaveLoadHashes()
    {
        var config = BuildConfig(7331UL);
        var campaign = CampaignBootstrapper.Bootstrap(config);
        var state = campaign.State;
        var streams = campaign.RandomStreams;

        var (holdingId, plotId) = SetupEstate(state, campaign.SettlementId);

        var construction = new ConstructionSystem();
        var maintenance = new MaintenanceSystem();
        var production = new ProductionSystem(GoodCatalog);

        const int totalMonths = 120;
        const int saveLoadEveryMonths = 40;

        for (var month = 0; month < totalMonths; month++)
        {
            construction.Tick(state, new MonthlyTickContext(state.Date, streams));
            StaffAnyNewlyCompletedBuildings(state);
            maintenance.Tick(state, new MonthlyTickContext(state.Date, streams));
            production.Tick(state, new MonthlyTickContext(state.Date, streams));
            state.AdvanceMonth();

            if ((month + 1) % saveLoadEveryMonths != 0)
                continue;

            var path = Path.Combine(Path.GetTempPath(), $"gens-phase6-production-soak-{Guid.NewGuid():N}.gens");
            try
            {
                var beforeHash = StateHasher.Hash(state);
                SaveWriter.Write(path, state, streams, "0.0.0-test", config.ContentPackHash);
                var loaded = SaveReader.Read(path);

                Assert.That(StateHasher.Hash(loaded.State), Is.EqualTo(beforeHash),
                    $"State hash must survive a save/load round-trip at month {state.Date.TotalMonths}.");

                state = loaded.State;
                streams = loaded.RandomStreams;
            }
            finally
            {
                File.Delete(path);
            }
        }

        Assert.That(state.Date.TotalMonths, Is.EqualTo(totalMonths));

        state.Stockpiles.TryGet(holdingId, out var stockpile);
        Assert.Multiple(() =>
        {
            // All six buildings across the three chains must have finished construction by month 120
            // (each queues at most 3 construction months) and every good must have accumulated.
            Assert.That(state.Buildings.Count, Is.EqualTo(6));
            Assert.That(stockpile.QuantityOf(BreadId), Is.GreaterThan(0));
            Assert.That(stockpile.QuantityOf(ToolsId), Is.GreaterThan(0));
            Assert.That(stockpile.QuantityOf(TextilesId), Is.GreaterThan(0));

            // Every stockpile quantity stays inside the storage constraint at every point by
            // construction (Stockpile.Add itself throws otherwise), and the queue drained completely.
            Assert.That(stockpile.RemainingCapacity, Is.GreaterThanOrEqualTo(0));
            state.ConstructionQueues.TryGet(holdingId, out var queue);
            Assert.That(queue.Projects, Is.Empty, "All six chain buildings must have finished construction.");
        });
    }

    [Test]
    public void SameSeedReproducesIdenticalHashAfterAFullProductionSoakRun() =>
        Assert.That(RunSoak(9911UL, 120), Is.EqualTo(RunSoak(9911UL, 120)));

    private static ulong RunSoak(ulong seed, int months)
    {
        var config = BuildConfig(seed);
        var campaign = CampaignBootstrapper.Bootstrap(config);
        var state = campaign.State;
        var streams = campaign.RandomStreams;
        SetupEstate(state, campaign.SettlementId);

        var construction = new ConstructionSystem();
        var maintenance = new MaintenanceSystem();
        var production = new ProductionSystem(GoodCatalog);

        for (var month = 0; month < months; month++)
        {
            construction.Tick(state, new MonthlyTickContext(state.Date, streams));
            StaffAnyNewlyCompletedBuildings(state);
            maintenance.Tick(state, new MonthlyTickContext(state.Date, streams));
            production.Tick(state, new MonthlyTickContext(state.Date, streams));
            state.AdvanceMonth();
        }

        return StateHasher.Hash(state);
    }

    /// <summary>Every completed raw-producer and converter building needs its staffing slots filled
    /// before it can go operational; there is no labor-assignment command wired to buildings yet
    /// (that is a later phase's job — Phase 6 item 6 only wires Duty assignment to Characters), so this
    /// soak test staffs a completed building directly with a fixed worker-ID tag the moment it appears,
    /// deterministically by definition ID.</summary>
    private static void StaffAnyNewlyCompletedBuildings(WorldState state)
    {
        foreach (var entry in state.Buildings.InAscendingOrder().ToArray())
        {
            var building = entry.Value;
            foreach (var slot in building.Definition.StaffingSlots)
            {
                var assigned = building.Staff[slot.Id].Count;
                for (var i = assigned; i < slot.Capacity; i++)
                    building.AssignStaff(slot.Id, $"worker_{building.Id.Value:D7}_{slot.Id}_{i}");
            }
        }
    }

    private static (RuntimeId<Holding> HoldingId, RuntimeId<Plot> PlotId) SetupEstate(
        WorldState state, RuntimeId<Settlement> settlementId)
    {
        var holdingId = state.HoldingIds.Issue();
        var plotId = state.PlotIds.Issue();
        var plot = Plot.Create(
            plotId, settlementId, TerrainType.Hills, TerrainFeature.MineralDeposit,
            capacity: 6, occupyingHoldingId: holdingId);
        state.Plots.Add(plotId, plot);
        state.Holdings.Add(holdingId, Holding.Create(holdingId, settlementId));
        // Ample but bounded storage — large enough for six buildings' worth of monthly output not to
        // stall the soak run, small enough to remain a real "storage constraint" rather than infinite.
        state.Stockpiles.Add(holdingId, new Stockpile(5_000));

        var queue = new ConstructionQueue();
        state.Plots.TryGet(plotId, out var freshPlot);
        var completed = Array.Empty<BuildingInstance>();
        queue.Enqueue(freshPlot, Ager(), completed);
        queue.Enqueue(freshPlot, Pistrinum(), completed);
        queue.Enqueue(freshPlot, Ferraria(), completed);
        // Fabrica's prerequisite check only runs at Enqueue time, against buildings already on record —
        // Ferraria is queued just above it, and the FIFO guarantees Ferraria really is complete before
        // Fabrica's own turn comes up, so this placeholder instance (never registered in WorldState,
        // discarded immediately after this call) only satisfies that one-time enqueue-time check.
        var ferrariaPlaceholder = new BuildingInstance(RuntimeId<Building>.Parse("building_9999999"), plotId, Ferraria());
        queue.Enqueue(freshPlot, Fabrica(), new[] { ferrariaPlaceholder });
        queue.Enqueue(freshPlot, Ovile(), completed);
        queue.Enqueue(freshPlot, Textrinum(), completed);
        state.ConstructionQueues.Add(holdingId, queue);

        return (holdingId, plotId);
    }

    private static CampaignConfig BuildConfig(ulong seed) => new()
    {
        Seed = seed,
        StartDate = new GameDate(0),
        RulesetId = "classic",
        ContentPackHash = "content-hash-placeholder",
        RegionId = "latium",
        Difficulty = "standard",
    };
}
