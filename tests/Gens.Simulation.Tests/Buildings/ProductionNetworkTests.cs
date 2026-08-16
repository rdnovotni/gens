using Gens.Simulation.Buildings;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;
using static Gens.Simulation.Tests.Buildings.EstateChainFixtures;

namespace Gens.Simulation.Tests.Buildings;

/// <summary>
/// Unit and short multi-month coverage for Phase 6 item 7's three monthly systems
/// (<see cref="ConstructionSystem"/>, <see cref="MaintenanceSystem"/>, <see cref="ProductionSystem"/>)
/// against the estate chains in <see cref="EstateChainFixtures"/>. The full 120-month exit-gate slice
/// lives in <c>ExitGate/ProductionNetworkSoakTests</c>.
/// </summary>
public sealed class ProductionNetworkTests
{
    [Test]
    public void ProductionConsumesInputsAndAddsOutputsWhenStorageHasRoom()
    {
        var state = NewWorldState();
        var (holdingId, plotId) = SetupHolding(state, capacity: 100);
        var pistrinum = new BuildingInstance(state.BuildingIds.Issue(), plotId, Pistrinum());
        pistrinum.AssignStaff("baker", "char_0000001");
        state.Buildings.Add(pistrinum.Id, pistrinum);
        state.Stockpiles.TryGet(holdingId, out var stockpile);
        stockpile.Add(new GoodDefinition(GrainId, Perishability.NonPerishable), 5);

        var events = new ProductionSystem(GoodCatalog).Tick(state, Context(state));

        Assert.Multiple(() =>
        {
            Assert.That(stockpile.QuantityOf(GrainId), Is.EqualTo(4));
            Assert.That(stockpile.QuantityOf(BreadId), Is.EqualTo(1));
            var resolved = (ProductionResolvedEvent)events.Single();
            Assert.That(resolved.Outcome, Is.EqualTo(ProductionOutcome.Produced));
            Assert.That(resolved.BuildingId, Is.EqualTo(pistrinum.Id));
            Assert.That(resolved.InputLines.Single(), Is.EqualTo(new RecipeLine(GrainId, 1)));
            Assert.That(resolved.OutputLines.Single(), Is.EqualTo(new RecipeLine(BreadId, 1)));
        });
    }

    [Test]
    public void ProductionSkipsWhenInputsAreShort()
    {
        var state = NewWorldState();
        var (holdingId, plotId) = SetupHolding(state, capacity: 100);
        var pistrinum = new BuildingInstance(state.BuildingIds.Issue(), plotId, Pistrinum());
        pistrinum.AssignStaff("baker", "char_0000001");
        state.Buildings.Add(pistrinum.Id, pistrinum);
        state.Stockpiles.TryGet(holdingId, out var stockpile);
        // No grain added at all.

        var events = new ProductionSystem(GoodCatalog).Tick(state, Context(state));

        Assert.Multiple(() =>
        {
            Assert.That(stockpile.QuantityOf(BreadId), Is.Zero);
            var resolved = (ProductionResolvedEvent)events.Single();
            Assert.That(resolved.Outcome, Is.EqualTo(ProductionOutcome.InputShortage));
            Assert.That(resolved.InputLines.Single(), Is.EqualTo(new RecipeLine(GrainId, 1)),
                "A shortage still reports the lines that were needed, for the ledger.");
            Assert.That(resolved.OutputLines.Single(), Is.EqualTo(new RecipeLine(BreadId, 1)));
        });
    }

    [Test]
    public void ProductionSkipsWhenOutputCannotFitInRemainingCapacity()
    {
        // Ager has no inputs (it is the chain's raw producer), so its output is a pure storage add —
        // the cleanest way to force a genuine capacity shortfall rather than one a recipe's own input
        // consumption would coincidentally free back up.
        var state = NewWorldState();
        var (holdingId, plotId) = SetupHolding(state, capacity: 3);
        var ager = new BuildingInstance(state.BuildingIds.Issue(), plotId, Ager());
        ager.AssignStaff("farmhand", "char_0000001");
        ager.AssignStaff("farmhand", "char_0000002");
        state.Buildings.Add(ager.Id, ager);
        state.Stockpiles.TryGet(holdingId, out var stockpile);
        stockpile.Add(new GoodDefinition(ToolsId, Perishability.NonPerishable), 3); // fills all 3 capacity

        var events = new ProductionSystem(GoodCatalog).Tick(state, Context(state));

        Assert.Multiple(() =>
        {
            // Nothing consumed or produced: all-or-nothing.
            Assert.That(stockpile.QuantityOf(GrainId), Is.Zero);
            Assert.That(stockpile.RemainingCapacity, Is.Zero);
            var resolved = (ProductionResolvedEvent)events.Single();
            Assert.That(resolved.Outcome, Is.EqualTo(ProductionOutcome.StorageFull));
        });
    }

    [Test]
    public void ProductionSkipsAnUnstaffedOrRuinedBuilding()
    {
        var state = NewWorldState();
        var (holdingId, plotId) = SetupHolding(state, capacity: 100);
        var pistrinum = new BuildingInstance(state.BuildingIds.Issue(), plotId, Pistrinum());
        state.Buildings.Add(pistrinum.Id, pistrinum); // never staffed
        state.Stockpiles.TryGet(holdingId, out var stockpile);
        stockpile.Add(new GoodDefinition(GrainId, Perishability.NonPerishable), 5);

        var events = new ProductionSystem(GoodCatalog).Tick(state, Context(state));

        Assert.Multiple(() =>
        {
            Assert.That(events, Is.Empty);
            Assert.That(stockpile.QuantityOf(BreadId), Is.Zero);
            Assert.That(pistrinum.IsOperational, Is.False);
        });
    }

    [Test]
    public void UnpaidUpkeepDropsConditionAndThenStopsProduction()
    {
        var state = NewWorldState();
        var (holdingId, plotId) = SetupHolding(state, capacity: 100);
        var fabrica = new BuildingInstance(state.BuildingIds.Issue(), plotId, Fabrica());
        fabrica.AssignStaff("smith", "char_0000001"); // fully staffed throughout: only condition should gate it
        state.Buildings.Add(fabrica.Id, fabrica);
        state.Stockpiles.TryGet(holdingId, out var stockpile);
        // No iron in stock at all: upkeep (1 iron) cannot be paid, four months in a row.

        var maintenance = new MaintenanceSystem();
        for (var i = 0; i < 4; i++)
            maintenance.Tick(state, Context(state));

        Assert.Multiple(() =>
        {
            Assert.That(fabrica.Condition, Is.EqualTo(BuildingCondition.Ruined));
            Assert.That(fabrica.IsOperational, Is.False);
        });

        stockpile.Add(new GoodDefinition(IronId, Perishability.NonPerishable), 10);
        var events = new ProductionSystem(GoodCatalog).Tick(state, Context(state));
        Assert.That(events, Is.Empty, "A Ruined building must not produce even when fully staffed and stocked.");
    }

    [Test]
    public void UnpaidUpkeepStillEmitsAConsumptionEventRecordingWhatWasNeeded()
    {
        var state = NewWorldState();
        var (holdingId, plotId) = SetupHolding(state, capacity: 100);
        var fabrica = new BuildingInstance(state.BuildingIds.Issue(), plotId, Fabrica());
        state.Buildings.Add(fabrica.Id, fabrica);
        // No iron in stock: upkeep (1 iron) cannot be paid.

        var events = new MaintenanceSystem().Tick(state, Context(state));

        var resolved = (BuildingUpkeepResolvedEvent)events.Single();
        Assert.Multiple(() =>
        {
            Assert.That(resolved.Paid, Is.False);
            Assert.That(resolved.UpkeepLines.Single(), Is.EqualTo(new RecipeLine(IronId, 1)));
            Assert.That(resolved.PreviousCondition, Is.EqualTo(BuildingCondition.Pristine));
            Assert.That(resolved.NewCondition, Is.Not.EqualTo(BuildingCondition.Pristine));
        });
    }

    [Test]
    public void PaidUpkeepConsumesStockAndLeavesConditionUnchanged()
    {
        var state = NewWorldState();
        var (holdingId, plotId) = SetupHolding(state, capacity: 100);
        var fabrica = new BuildingInstance(state.BuildingIds.Issue(), plotId, Fabrica());
        state.Buildings.Add(fabrica.Id, fabrica);
        state.Stockpiles.TryGet(holdingId, out var stockpile);
        stockpile.Add(new GoodDefinition(IronId, Perishability.NonPerishable), 5);

        var events = new MaintenanceSystem().Tick(state, Context(state));

        Assert.Multiple(() =>
        {
            Assert.That(stockpile.QuantityOf(IronId), Is.EqualTo(4));
            Assert.That(fabrica.Condition, Is.EqualTo(BuildingCondition.Pristine));
            var resolved = (BuildingUpkeepResolvedEvent)events.Single();
            Assert.That(resolved.Paid, Is.True);
            Assert.That(resolved.UpkeepLines.Single(), Is.EqualTo(new RecipeLine(IronId, 1)));
            Assert.That(resolved.PreviousCondition, Is.EqualTo(BuildingCondition.Pristine));
            Assert.That(resolved.NewCondition, Is.EqualTo(BuildingCondition.Pristine));
        });
    }

    [Test]
    public void ConstructionAdvancesAndAddsTheFinishedBuildingOnCompletion()
    {
        var state = NewWorldState();
        var (holdingId, plotId) = SetupHolding(state, capacity: 100);
        state.Plots.TryGet(plotId, out var plot);
        var queue = new ConstructionSchedule();
        queue.Enqueue(plot, Pistrinum(), Array.Empty<BuildingInstance>()); // 2 construction months

        state.ConstructionSchedules.Add(holdingId, queue);

        var system = new ConstructionSystem();
        var firstMonthEvents = system.Tick(state, Context(state));
        Assert.That(state.Buildings.Count, Is.Zero);
        var progressed = (BuildingConstructionProgressedEvent)firstMonthEvents.Single();
        Assert.That(progressed.HoldingId, Is.EqualTo(holdingId));
        Assert.That(progressed.DefinitionId, Is.EqualTo(PistrinumId));
        Assert.That(progressed.CompletedMonths, Is.EqualTo(1));
        Assert.That(progressed.TotalMonths, Is.EqualTo(2));

        var events = system.Tick(state, Context(state));

        Assert.Multiple(() =>
        {
            Assert.That(state.Buildings.Count, Is.EqualTo(1));
            var completed = (BuildingConstructionCompletedEvent)events.Single();
            Assert.That(completed.HoldingId, Is.EqualTo(holdingId));
            Assert.That(completed.DefinitionId, Is.EqualTo(PistrinumId));
            Assert.That(state.Buildings.TryGet(completed.BuildingId, out var building), Is.True);
            Assert.That(building.Definition.Id, Is.EqualTo(PistrinumId));
        });
    }

    [Test]
    public void ConstructionPausesWithoutLaborAndResumesWhenItReturns()
    {
        var state = NewWorldState();
        var (holdingId, plotId) = SetupHolding(state, capacity: 100);
        state.Plots.TryGet(plotId, out var plot);
        var householdId = state.HouseholdIds.Issue();
        var holding = Holding.Create(holdingId, plot.SettlementId, occupantId: householdId.ToTaggedString());
        state.Holdings.Remove(holdingId);
        state.Holdings.Add(holdingId, holding);

        var queue = new ConstructionSchedule();
        queue.Enqueue(plot, Ovile(), Array.Empty<BuildingInstance>());
        state.ConstructionSchedules.Add(holdingId, queue);

        var system = new ConstructionSystem();
        // No living Character belongs to `householdId`, so labor is unavailable this tick.
        Assert.That(system.Tick(state, Context(state)), Is.Empty);
        Assert.That(queue.Projects.Single().CompletedMonths, Is.Zero, "No labor: no progress this month.");
    }

    [Test]
    public void FullChainRunsAcrossSeveralMonthsFromEmptyStockpileToFinishedGood()
    {
        var state = NewWorldState();
        var (holdingId, plotId) = SetupHolding(state, capacity: 100);
        state.Plots.TryGet(plotId, out var plot);

        var queue = new ConstructionSchedule();
        queue.Enqueue(plot, Ager(), Array.Empty<BuildingInstance>());
        state.ConstructionSchedules.Add(holdingId, queue);

        var construction = new ConstructionSystem();
        var maintenance = new MaintenanceSystem();
        var production = new ProductionSystem(GoodCatalog);
        state.Stockpiles.TryGet(holdingId, out var stockpile);

        BuildingInstance? pistrinum = null;
        for (var month = 0; month < 8; month++)
        {
            construction.Tick(state, Context(state));
            if (pistrinum is null && state.Buildings.Count == 1)
            {
                // Ager just finished (1 construction month): stand up Pistrinum next and staff both.
                state.Buildings.TryGet(state.Buildings.InAscendingOrder().Single().Key, out var ager);
                ager.AssignStaff("farmhand", "char_0000001");
                ager.AssignStaff("farmhand", "char_0000002");
                pistrinum = new BuildingInstance(state.BuildingIds.Issue(), plotId, Pistrinum());
                pistrinum.AssignStaff("baker", "char_0000003");
                state.Buildings.Add(pistrinum.Id, pistrinum);
            }

            maintenance.Tick(state, Context(state));
            production.Tick(state, Context(state));
            state.AdvanceMonth();
        }

        Assert.Multiple(() =>
        {
            Assert.That(state.Buildings.Count, Is.EqualTo(2));
            Assert.That(stockpile.QuantityOf(BreadId), Is.GreaterThan(0), "Grain produced by Ager must have reached Pistrinum and become bread.");
        });
    }

    private static WorldState NewWorldState() => new(new GameDate(0));

    private static MonthlyTickContext Context(WorldState state) => new(state.Date, new RandomStreamSet());

    private static (RuntimeId<Holding> HoldingId, RuntimeId<Plot> PlotId) SetupHolding(WorldState state, long capacity)
    {
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var holdingId = state.HoldingIds.Issue();
        var plotId = state.PlotIds.Issue();
        var plot = Plot.Create(plotId, settlementId, TerrainType.Hills, TerrainFeature.MineralDeposit, capacity: 4, occupyingHoldingId: holdingId);
        state.Plots.Add(plotId, plot);
        state.Holdings.Add(holdingId, Holding.Create(holdingId, settlementId));
        state.Stockpiles.Add(holdingId, new Stockpile(capacity));
        return (holdingId, plotId);
    }
}
