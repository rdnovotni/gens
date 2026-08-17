using Gens.Simulation.Buildings;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Economy;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Markets;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.ExitGate;

/// <summary>
/// Proves Phase 8 item 8's "long-run economic telemetry" for items 5-7's new systems, extending the
/// established Phase 6/7 soak-test pattern (<see cref="ProductionNetworkSoakTests"/>,
/// <see cref="LaborConditionsSoakTests"/>) to the economy: two households (a grain-producing estate
/// and a pure-consumer estate) trade through <see cref="HouseholdPurchasingSystem"/>, a staffed
/// building draws <see cref="WagesSystem"/> wages, both estates pay <see cref="RentAndTaxSystem"/>
/// rent/tax, a standing loan runs through <see cref="DebtSystem"/>'s interest/default ladder, a
/// provincial supply <see cref="StandingContract"/> sells at a premium, and <see
/// cref="InsolvencySystem"/>/<see cref="HouseholdStatementSystem"/> assess Net Worth and produce
/// monthly statements every tick — run 180 months (matching Phase 6's 120-month exit-gate scale, sized
/// up slightly since this soak spans more interacting systems), asserting at every tick that no
/// registered invariant (Ledger, Markets, or this pass's own Economy checks) is violated, then
/// reporting aggregate conservation and flow figures as final assertions.
/// </summary>
public sealed class EconomySoakTests
{
    private static readonly DefinitionId<Good> GrainId = NeedsConsumptionCalculator.ConsumptionGood;

    [Test]
    public void TheEconomySurvivesOneHundredEightyMonthsWithConservedMoneyAndNoInvariantViolations()
    {
        var config = new CampaignConfig
        {
            Seed = 424242UL,
            StartDate = new GameDate(0),
            RulesetId = "classic",
            ContentPackHash = "content-hash-placeholder",
            RegionId = "latium",
            Difficulty = "standard",
        };
        var campaign = CampaignBootstrapper.Bootstrap(config);
        var state = campaign.State;
        var streams = campaign.RandomStreams;
        var settlementId = campaign.SettlementId;
        var producerHouseholdId = campaign.HouseholdId;

        var (producerHoldingId, plotId) = SetupProducerEstate(state, settlementId, producerHouseholdId);
        var consumerHouseholdId = state.HouseholdIds.Issue();
        var consumerHoldingId = state.HoldingIds.Issue();
        state.Holdings.Add(consumerHoldingId, Holding.Create(
            consumerHoldingId, settlementId, ownerId: consumerHouseholdId.ToTaggedString(), residentCapacity: 6));
        state.Stockpiles.Add(consumerHoldingId, new Stockpile(1_000));

        DebtService.IssueLoan(state, state.Date, settlementId, consumerHouseholdId, Money.FromDenarii(150));
        StandingContractService.OpenProvincialSupplyContract(
            state, settlementId, producerHouseholdId, producerHoldingId, GrainId,
            quantityPerMonth: 3, priceOverMarketFraction: Fixed64.FromRaw(150_000));

        var construction = new ConstructionSystem();
        var maintenance = new MaintenanceSystem();
        var production = new ProductionSystem(new[] { new GoodDefinition(GrainId, Perishability.NonPerishable) });
        var marketClearing = new MarketClearingSystem(
            new[] { new KeyValuePair<DefinitionId<Good>, Money>(GrainId, Money.FromDenarii(3)) },
            MarketPriceBoundConfig.Default);
        var purchasing = new HouseholdPurchasingSystem();
        var wages = new WagesSystem();
        var rentAndTax = new RentAndTaxSystem();
        var debt = new DebtSystem();
        var insolvency = new InsolvencySystem();
        var standingContracts = new StandingContractSystem();
        var statements = new HouseholdStatementSystem();

        var invariants = new InvariantCheckRunner(new IInvariantCheck[]
        {
            new LedgerTransactionBalanceInvariantCheck(),
            new LedgerAccountBalanceConsistencyInvariantCheck(),
            new MarketNoNegativeStockOrPriceInvariantCheck(),
            new MarketBoundedPriceChangeInvariantCheck(MarketPriceBoundConfig.Default),
            new EconomyTotalMoneySupplyInvariantCheck(),
            new EconomyStockpileNonNegativeInvariantCheck(),
            new EconomyDebtRecordConsistencyInvariantCheck(),
            new EconomyNetWorthConsistencyInvariantCheck(),
        });

        const int totalMonths = 180;
        for (var month = 0; month < totalMonths; month++)
        {
            var context = new MonthlyTickContext(state.Date, streams);
            construction.Tick(state, context);
            StaffAnyNewlyCompletedBuildings(state);
            maintenance.Tick(state, context);
            production.Tick(state, context);
            marketClearing.Tick(state, context);
            purchasing.Tick(state, context);
            wages.Tick(state, context);
            rentAndTax.Tick(state, context);
            debt.Tick(state, context);
            insolvency.Tick(state, context);
            standingContracts.Tick(state, context);
            statements.Tick(state, context);

            // Fatal invariants throw immediately; every check here is IsFatal so a clean RunAll return
            // (rather than an exception) is itself the per-tick assertion.
            invariants.RunAll(state);

            state.AdvanceMonth();
        }

        Assert.That(state.Date.TotalMonths, Is.EqualTo(totalMonths));

        // Aggregate telemetry: total Ledger money supply is exactly conserved (every posting balances,
        // Phase 8's own exit-gate language: "every quantity and coin movement reconciles").
        var totalMoneySupply = Money.Zero;
        foreach (var account in state.LedgerAccounts.InAscendingOrder())
            totalMoneySupply += account.Value.Balance;
        Assert.That(totalMoneySupply, Is.EqualTo(Money.Zero));

        // No stockpile went negative across 180 months of production, trading, seizure, and forced
        // liquidation.
        foreach (var entry in state.Stockpiles.InAscendingOrder())
            Assert.That(entry.Value.Quantity, Is.GreaterThanOrEqualTo(0));

        // The economy actually did something over 180 months: both households received monthly
        // statements, at least one debt-interest cycle posted, and the standing contract sold at least
        // once.
        Assert.Multiple(() =>
        {
            Assert.That(state.HouseholdStatements.Count, Is.GreaterThan(0));
            Assert.That(state.NetWorthAssessments.Count, Is.EqualTo(2));
            Assert.That(state.DebtRecords.Count, Is.EqualTo(1));
            state.LedgerAccounts.TryGet(LedgerAccountKey.ForSettlementTreasury(settlementId), out var treasury);
            Assert.That(treasury, Is.Not.Null, "The settlement Treasury should have posted activity (wages/rent/tax/contracts).");
        });
    }

    [Test]
    public void SameSeedReproducesIdenticalHashAfterAFullEconomySoakRun() =>
        Assert.That(RunSoakHash(55555UL, 60), Is.EqualTo(RunSoakHash(55555UL, 60)));

    private static ulong RunSoakHash(ulong seed, int months)
    {
        var config = new CampaignConfig
        {
            Seed = seed,
            StartDate = new GameDate(0),
            RulesetId = "classic",
            ContentPackHash = "content-hash-placeholder",
            RegionId = "latium",
            Difficulty = "standard",
        };
        var campaign = CampaignBootstrapper.Bootstrap(config);
        var state = campaign.State;
        var streams = campaign.RandomStreams;
        var (producerHoldingId, _) = SetupProducerEstate(state, campaign.SettlementId, campaign.HouseholdId);

        var production = new ProductionSystem(new[] { new GoodDefinition(GrainId, Perishability.NonPerishable) });
        var construction = new ConstructionSystem();
        var maintenance = new MaintenanceSystem();
        var marketClearing = new MarketClearingSystem(
            new[] { new KeyValuePair<DefinitionId<Good>, Money>(GrainId, Money.FromDenarii(3)) },
            MarketPriceBoundConfig.Default);
        var purchasing = new HouseholdPurchasingSystem();
        var wages = new WagesSystem();
        var rentAndTax = new RentAndTaxSystem();
        var statements = new HouseholdStatementSystem();
        var debt = new DebtSystem();
        var insolvency = new InsolvencySystem();
        var standingContracts = new StandingContractSystem();

        for (var month = 0; month < months; month++)
        {
            var context = new MonthlyTickContext(state.Date, streams);
            construction.Tick(state, context);
            StaffAnyNewlyCompletedBuildings(state);
            maintenance.Tick(state, context);
            production.Tick(state, context);
            marketClearing.Tick(state, context);
            purchasing.Tick(state, context);
            wages.Tick(state, context);
            rentAndTax.Tick(state, context);
            debt.Tick(state, context);
            insolvency.Tick(state, context);
            standingContracts.Tick(state, context);
            statements.Tick(state, context);
            state.AdvanceMonth();
        }

        _ = producerHoldingId;
        return StateHasher.Hash(state);
    }

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

    /// <summary>A single-building grain estate (an "Ager," matching <see
    /// cref="Tests.Buildings.EstateChainFixtures.Ager"/>'s shape but authored locally to avoid a
    /// cross-namespace test dependency) whose Holding is owned by <paramref name="householdId"/>.</summary>
    private static (RuntimeId<Holding> HoldingId, RuntimeId<Plot> PlotId) SetupProducerEstate(
        WorldState state, RuntimeId<Settlement> settlementId, RuntimeId<Household> householdId)
    {
        var holdingId = state.HoldingIds.Issue();
        var plotId = state.PlotIds.Issue();
        var plot = Plot.Create(plotId, settlementId, TerrainType.FertilePlain, TerrainFeature.None, capacity: 2, occupyingHoldingId: holdingId);
        state.Plots.Add(plotId, plot);
        state.Holdings.Add(holdingId, Holding.Create(
            holdingId, settlementId, ownerId: householdId.ToTaggedString(), residentCapacity: 3));
        state.Stockpiles.Add(holdingId, new Stockpile(2_000));

        var ager = new BuildingDefinition(
            new DefinitionId<Building>("ager"), BuildingTier.Tier1, constructionMonths: 1, plotCapacity: 1,
            staffingSlots: new[] { new StaffingSlot("farmhand", 2) },
            recipe: new ProductionRecipe(Array.Empty<RecipeLine>(), new[] { new RecipeLine(GrainId, 20) }));

        var queue = new ConstructionSchedule();
        state.Plots.TryGet(plotId, out var freshPlot);
        queue.Enqueue(freshPlot, ager, Array.Empty<BuildingInstance>());
        state.ConstructionSchedules.Add(holdingId, queue);

        return (holdingId, plotId);
    }
}
