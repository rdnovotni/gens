using Gens.Simulation.Economy;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Economy;

public sealed class RentAndTaxSystemTests
{
    [Test]
    public void RentAndTaxBothPostFromTheHouseholdToTheSettlementTreasury()
    {
        var (state, settlementId) = EconomyTestFixtures.NewSettlement();
        var (_, householdId) = EconomyTestFixtures.AddHouseholdHolding(state, settlementId, residentCapacity: 5);

        var events = new RentAndTaxSystem().Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        Assert.That(events, Has.Count.EqualTo(2));
        var rent = HouseholdEconomyCalculator.MultiplyByQuantity(HouseholdEconomyCalculator.RentPerResidentCapacity, 5);
        var tax = rent.Scale(HouseholdEconomyCalculator.DecumaTaxRate);

        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var household);
        state.LedgerAccounts.TryGet(LedgerAccountKey.ForSettlementTreasury(settlementId), out var treasury);

        Assert.Multiple(() =>
        {
            Assert.That(household!.Balance, Is.EqualTo(-(rent + tax)));
            Assert.That(treasury!.Balance, Is.EqualTo(rent + tax));
        });
    }
}
