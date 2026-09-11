using System.Linq;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Magistracies;
using Gens.Simulation.Policies;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Policies;

/// <summary>Phase 16 item 6 coverage for the Magistracies Funded Action: <see
/// cref="FundLocalDefenseCommand"/> raises <see cref="EstateSecurityInvestment.SecurityLevel"/> and pays
/// the standard Funded-Action Dignitas gain, with a modest bonus when the funding household's own head
/// holds an active <see cref="MagistracyOffice.Aedile"/> seat at the target settlement.</summary>
public sealed class FundLocalDefenseCommandTests
{
    [Test]
    public void AcceptedFundingRaisesSecurityAndPaysTheOrdinaryDignitasGain()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, settlementId) = SeedFundedHousehold(state, Money.FromDenarii(1000));
        var dignitasBefore = DignitasResolver.Current(state, householdId);

        var result = FundLocalDefenseCommands.Pipeline.Execute(
            state, new FundLocalDefenseCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, settlementId, Money.FromDenarii(200)));

        state.EstateSecurityInvestments.TryGet(householdId, out var investment);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account), Is.True);
            Assert.That(account!.Balance, Is.EqualTo(Money.FromDenarii(800)));
            Assert.That(investment!.SecurityLevel, Is.EqualTo(200 / (int)RaidThreatCatalog.SecurityLevelCostPerPointDenarii));
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(dignitasBefore + FundLocalDefenseCommands.DignitasGain));
            Assert.That(result.Events, Has.Some.InstanceOf<LocalDefenseFundedEvent>());
            var evt = result.Events.OfType<LocalDefenseFundedEvent>().Single();
            Assert.That(evt.AedileBonusApplied, Is.False);
        });
    }

    [Test]
    public void AnActiveAedileAtTheTargetSettlementGetsTheBonusDignitasMultiplier()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, settlementId) = SeedFundedHousehold(state, Money.FromDenarii(1000));
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, household: householdId, location: settlementId));
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0)));

        var recordId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(recordId, new MagistracyRecord(recordId, headId, MagistracyOffice.Aedile, settlementId, new GameDate(0)));
        var dignitasBefore = DignitasResolver.Current(state, householdId);

        var result = FundLocalDefenseCommands.Pipeline.Execute(
            state, new FundLocalDefenseCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, settlementId, Money.FromDenarii(200)));

        var expectedGain = FundLocalDefenseCommands.DignitasGain * (100 + FundLocalDefenseCommands.AedileBonusPercent) / 100;
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(dignitasBefore + expectedGain));
            var evt = result.Events.OfType<LocalDefenseFundedEvent>().Single();
            Assert.That(evt.AedileBonusApplied, Is.True);
        });
    }

    [Test]
    public void AnAedileSeatAtADifferentSettlementGetsNoBonus()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, settlementId) = SeedFundedHousehold(state, Money.FromDenarii(1000));
        var otherSettlementId = state.SettlementIds.Issue();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, household: householdId, location: settlementId));
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0)));

        var recordId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(recordId, new MagistracyRecord(recordId, headId, MagistracyOffice.Aedile, otherSettlementId, new GameDate(0)));

        var result = FundLocalDefenseCommands.Pipeline.Execute(
            state, new FundLocalDefenseCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, settlementId, Money.FromDenarii(200)));

        var evt = result.Events.OfType<LocalDefenseFundedEvent>().Single();
        Assert.That(evt.AedileBonusApplied, Is.False);
    }

    [Test]
    public void ZeroOrNegativeAmountIsRejected()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, settlementId) = SeedFundedHousehold(state, Money.FromDenarii(100));

        var result = FundLocalDefenseCommands.Pipeline.Execute(
            state, new FundLocalDefenseCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, settlementId, Money.Zero));

        Assert.That(result.Error, Is.EqualTo(FundLocalDefenseCommands.AmountMustBePositive));
    }

    [Test]
    public void FundingBeyondTheTreasuryBalanceIsRejected()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, settlementId) = SeedFundedHousehold(state, Money.FromDenarii(10));

        var result = FundLocalDefenseCommands.Pipeline.Execute(
            state, new FundLocalDefenseCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, settlementId, Money.FromDenarii(50)));

        Assert.That(result.Error, Is.EqualTo(FundLocalDefenseCommands.InsufficientTreasury));
    }

    private static (RuntimeId<Household> HouseholdId, RuntimeId<Settlement> SettlementId) SeedFundedHousehold(WorldState state, Money startingBalance)
    {
        var householdId = state.HouseholdIds.Issue();
        var settlementId = state.SettlementIds.Issue();

        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), startingBalance),
                new LedgerPosting(LedgerAccountKey.ForSettlementTreasury(settlementId), -startingBalance),
            });

        return (householdId, settlementId);
    }
}
