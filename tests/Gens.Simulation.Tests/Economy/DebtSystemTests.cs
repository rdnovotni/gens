using Gens.Simulation.Economy;
using Gens.Simulation.Ledger;
using Gens.Simulation.Markets;
using Gens.Simulation.Numerics;
using Gens.Simulation.Random;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Economy;

public sealed class DebtSystemTests
{
    [Test]
    public void IssueLoanCreditsTheDebtorAndDebitsTheSettlementTreasury()
    {
        var (state, settlementId) = EconomyTestFixtures.NewSettlement();
        var householdId = state.HouseholdIds.Issue();

        var debt = DebtService.IssueLoan(state, state.Date, settlementId, householdId, Money.FromDenarii(100));

        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var household);
        state.LedgerAccounts.TryGet(LedgerAccountKey.ForSettlementTreasury(settlementId), out var treasury);

        Assert.Multiple(() =>
        {
            Assert.That(household!.Balance, Is.EqualTo(Money.FromDenarii(100)));
            Assert.That(treasury!.Balance, Is.EqualTo(Money.FromDenarii(-100)));
            Assert.That(debt.Status, Is.EqualTo(DebtStatus.Current));
            Assert.That(state.DebtRecords.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public void APaidInterestMonthResetsMonthsOverdueAndStaysCurrent()
    {
        var (state, settlementId) = EconomyTestFixtures.NewSettlement();
        var householdId = state.HouseholdIds.Issue();
        var debt = DebtService.IssueLoan(state, state.Date, settlementId, householdId, Money.FromDenarii(1_000));
        // The household already holds the loan proceeds (1,000), comfortably covering the interest.

        new DebtSystem().Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        state.DebtRecords.TryGet(debt.Id, out var updated);
        Assert.Multiple(() =>
        {
            Assert.That(updated!.Status, Is.EqualTo(DebtStatus.Current));
            Assert.That(updated.MonthsOverdue, Is.EqualTo(0));
        });
    }

    [Test]
    public void SustainedMissedPaymentsEscalateToDefaultAndSeizeAssets()
    {
        var (state, settlementId) = EconomyTestFixtures.NewSettlement();
        var (holdingId, householdId) = EconomyTestFixtures.AddHouseholdHolding(state, settlementId, residentCapacity: 1);
        state.Stockpiles.TryGet(holdingId, out var stockpile);
        stockpile!.Add(EconomyTestFixtures.Grain(), 100);
        EconomyTestFixtures.SetGrainPrice(state, settlementId, Money.FromDenarii(1));

        // Open a large loan the household's own (empty, pre-loan) ledger balance cannot service —
        // post it directly rather than via IssueLoan so the household never actually receives the cash.
        var debtId = state.DebtRecordIds.Issue();
        var debt = new DebtRecord(
            debtId, settlementId, householdId, Money.FromDenarii(10_000),
            HouseholdEconomyCalculator.InitialDebtInterestRate, DebtOrigin.Loan, isFenusNauticum: false,
            monthsOverdue: 0, status: DebtStatus.Current, resolution: DebtResolution.None);
        state.DebtRecords.Add(debtId, debt);

        var system = new DebtSystem();
        for (var month = 0; month < HouseholdEconomyCalculator.DebtDefaultThresholdMonths; month++)
        {
            system.Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));
            state.AdvanceMonth();
        }

        state.DebtRecords.TryGet(debtId, out var final);
        Assert.Multiple(() =>
        {
            Assert.That(final!.Status, Is.EqualTo(DebtStatus.Defaulted));
            Assert.That(final.Resolution, Is.EqualTo(DebtResolution.AssetSeizure));
            Assert.That(stockpile.Quantity, Is.LessThan(100), "Seizure should have removed some stock.");
        });
    }

    [Test]
    public void ForgivenDebtsAreNeverTouchedByTheMonthlyTick()
    {
        var (state, settlementId) = EconomyTestFixtures.NewSettlement();
        var householdId = state.HouseholdIds.Issue();
        var debtId = state.DebtRecordIds.Issue();
        var debt = new DebtRecord(
            debtId, settlementId, householdId, Money.FromDenarii(50), Fixed64.Zero,
            DebtOrigin.Loan, isFenusNauticum: true, monthsOverdue: 0, status: DebtStatus.Forgiven, resolution: DebtResolution.None);
        state.DebtRecords.Add(debtId, debt);

        var events = new DebtSystem().Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        Assert.That(events, Is.Empty);
        state.DebtRecords.TryGet(debtId, out var unchanged);
        Assert.That(unchanged, Is.EqualTo(debt));
    }
}
