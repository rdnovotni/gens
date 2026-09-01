using Gens.Simulation.Health;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Health;

public sealed class SanitationInvestmentSystemTests
{
    [Test]
    public void AComprehensiveSettlementPaysItsMonthlyCostFromItsOwnTreasury()
    {
        var state = new WorldState(new GameDate(10));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        state.SettlementSanitationInvestments.Add(
            settlementId, SettlementSanitationInvestment.Create(settlementId, SanitationInvestmentTier.Comprehensive));

        var system = new SanitationInvestmentSystem();
        var events = system.Tick(state, new MonthlyTickContext(new GameDate(10), new RandomStreamSet()));

        Assert.That(events, Is.Not.Empty);
        var cost = SanitationInvestmentCalculator.MonthlyTreasuryCost(SanitationInvestmentTier.Comprehensive);
        state.LedgerAccounts.TryGet(LedgerAccountKey.ForSettlementTreasury(settlementId), out var treasury);
        Assert.That(treasury.Balance, Is.EqualTo(-cost));
    }

    [Test]
    public void AMinimalSettlementIsSkippedAndPaysNothing()
    {
        var state = new WorldState(new GameDate(10));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        state.SettlementSanitationInvestments.Add(
            settlementId, SettlementSanitationInvestment.Create(settlementId, SanitationInvestmentTier.Minimal));

        var system = new SanitationInvestmentSystem();
        var events = system.Tick(state, new MonthlyTickContext(new GameDate(10), new RandomStreamSet()));

        Assert.That(events, Is.Empty);
        Assert.That(state.LedgerAccounts.TryGet(LedgerAccountKey.ForSettlementTreasury(settlementId), out _), Is.False);
    }

    [Test]
    public void ASettlementWithNoInvestmentEntryAtAllIsSkipped()
    {
        var state = new WorldState(new GameDate(10));
        var system = new SanitationInvestmentSystem();
        var events = system.Tick(state, new MonthlyTickContext(new GameDate(10), new RandomStreamSet()));
        Assert.That(events, Is.Empty);
    }
}
