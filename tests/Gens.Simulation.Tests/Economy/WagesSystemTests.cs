using Gens.Simulation.Buildings;
using Gens.Simulation.Economy;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Economy;

public sealed class WagesSystemTests
{
    [Test]
    public void EveryStaffedWorkerIsPaidFromTheSettlementTreasury()
    {
        var (state, settlementId) = EconomyTestFixtures.NewSettlement();
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId, TerrainType.FertilePlain, TerrainFeature.None, capacity: 1));

        var definition = new BuildingDefinition(
            new DefinitionId<Building>("ager"), BuildingTier.Tier1, constructionMonths: 1, plotCapacity: 1,
            staffingSlots: new[] { new StaffingSlot("farmhand", 2) },
            recipe: new ProductionRecipe(Array.Empty<RecipeLine>(), new[] { new RecipeLine(new DefinitionId<Good>("grain"), 1) }));
        var buildingId = state.BuildingIds.Issue();
        var building = new BuildingInstance(buildingId, plotId, definition);
        building.AssignStaff("farmhand", "char_0000001");
        building.AssignStaff("farmhand", "char_0000002");
        state.Buildings.Add(buildingId, building);

        var events = new WagesSystem().Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        Assert.That(events, Has.Count.EqualTo(1));
        state.LedgerAccounts.TryGet(new LedgerAccountKey(LedgerAccountKind.Actor, "char_0000001"), out var worker1);
        state.LedgerAccounts.TryGet(new LedgerAccountKey(LedgerAccountKind.Actor, "char_0000002"), out var worker2);
        state.LedgerAccounts.TryGet(LedgerAccountKey.ForSettlementTreasury(settlementId), out var treasury);

        Assert.Multiple(() =>
        {
            Assert.That(worker1!.Balance, Is.EqualTo(HouseholdEconomyCalculator.WagePerWorkerPerMonth));
            Assert.That(worker2!.Balance, Is.EqualTo(HouseholdEconomyCalculator.WagePerWorkerPerMonth));
            Assert.That(treasury!.Balance, Is.EqualTo(-(HouseholdEconomyCalculator.WagePerWorkerPerMonth + HouseholdEconomyCalculator.WagePerWorkerPerMonth)));
        });
    }

    [Test]
    public void NoWagesArePostedWhenNoBuildingHasAnyStaff()
    {
        var (state, _) = EconomyTestFixtures.NewSettlement();
        var events = new WagesSystem().Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));
        Assert.That(events, Is.Empty);
    }
}
