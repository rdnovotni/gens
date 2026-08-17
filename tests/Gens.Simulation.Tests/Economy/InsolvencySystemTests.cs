using Gens.Simulation.Economy;
using Gens.Simulation.Goods;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.Time;
using Gens.Simulation.Villas;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Economy;

public sealed class InsolvencySystemTests
{
    [Test]
    public void ANetWorthAssessmentIsRecordedForEveryHouseholdWithAHolding()
    {
        var (state, settlementId) = EconomyTestFixtures.NewSettlement();
        var (holdingId, householdId) = EconomyTestFixtures.AddHouseholdHolding(state, settlementId);
        state.Stockpiles.TryGet(holdingId, out var stockpile);
        stockpile!.Add(EconomyTestFixtures.Grain(), 10);
        EconomyTestFixtures.SetGrainPrice(state, settlementId, Money.FromDenarii(5));

        new InsolvencySystem().Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        state.NetWorthAssessments.TryGet(householdId, out var netWorth);
        Assert.Multiple(() =>
        {
            Assert.That(netWorth, Is.Not.Null);
            Assert.That(netWorth!.StoredGoodsValue, Is.EqualTo(Money.FromDenarii(50)));
            Assert.That(netWorth.Total, Is.EqualTo(Money.FromDenarii(50)));
            Assert.That(netWorth.Band, Is.EqualTo(HouseholdWealthBand.Modest));
        });
    }

    [Test]
    public void SustainedNegativeNetWorthEscalatesToInsolventAndForciblyLiquidatesStock()
    {
        var (state, settlementId) = EconomyTestFixtures.NewSettlement();
        var (holdingId, householdId) = EconomyTestFixtures.AddHouseholdHolding(state, settlementId);
        state.Stockpiles.TryGet(holdingId, out var stockpile);
        stockpile!.Add(EconomyTestFixtures.Grain(), 1_000);
        EconomyTestFixtures.SetGrainPrice(state, settlementId, Money.FromDenarii(1));

        // Push the household's own treasury deeply negative so total Net Worth (goods value minus this)
        // still sits below the InsolvencyNetWorthFloor despite the stock on hand.
        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Debt,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(-5_000)),
                new LedgerPosting(LedgerAccountKey.ForSettlementTreasury(settlementId), Money.FromDenarii(5_000)),
            });

        var system = new InsolvencySystem();
        for (var month = 0; month < HouseholdEconomyCalculator.InsolvencyInsolventMonths; month++)
        {
            system.Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));
            state.AdvanceMonth();
        }

        state.InsolvencyStates.TryGet(householdId, out var insolvency);
        Assert.Multiple(() =>
        {
            Assert.That(insolvency!.Stage, Is.EqualTo(InsolvencyStage.Insolvent));
            Assert.That(insolvency.ConsequencesApplied, Does.Contain(InsolvencyConsequence.ForcedLiquidation));
            Assert.That(stockpile.Quantity, Is.LessThan(1_000));
        });
    }

    [Test]
    public void RuinedStageDemotesAVillaAboveItsMinimumStage()
    {
        var (state, settlementId) = EconomyTestFixtures.NewSettlement();
        var householdId = state.HouseholdIds.Issue();
        var holdingId = state.HoldingIds.Issue();
        var villa = new Villa(VillaStage.Urbana);
        state.Holdings.Add(holdingId, Holding.Create(
            holdingId, settlementId, ownerId: householdId.ToTaggedString(), residentCapacity: 1, villa: villa));
        state.Stockpiles.Add(holdingId, new Stockpile(100));

        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Debt,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(-1_000_000)),
                new LedgerPosting(LedgerAccountKey.ForSettlementTreasury(settlementId), Money.FromDenarii(1_000_000)),
            });

        var system = new InsolvencySystem();
        for (var month = 0; month < HouseholdEconomyCalculator.InsolvencyRuinedMonths; month++)
        {
            system.Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));
            state.AdvanceMonth();
        }

        state.InsolvencyStates.TryGet(householdId, out var insolvency);
        Assert.Multiple(() =>
        {
            Assert.That(insolvency!.Stage, Is.EqualTo(InsolvencyStage.Ruined));
            Assert.That(villa.Stage, Is.EqualTo(VillaStage.Rustica));
            Assert.That(insolvency.ConsequencesApplied, Does.Contain(InsolvencyConsequence.VillaStageDemotion));
        });
    }
}
