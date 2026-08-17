using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Policies;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Policies;

public sealed class FundFestivalCommandTests
{
    [Test]
    public void AcceptedFundingDebitsTheHouseholdAndCreditsTheFestivalSink()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, settlementId) = SeedFundedHousehold(state, Money.FromDenarii(100));

        var result = FundFestivalCommands.Pipeline.Execute(
            state, new FundFestivalCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, settlementId, Money.FromDenarii(50)));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account), Is.True);
            Assert.That(account!.Balance, Is.EqualTo(Money.FromDenarii(50)));
        });
    }

    [Test]
    public void AcceptedFundingEmitsBothTheLedgerPostingAndTheFestivalFundedEvent()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, settlementId) = SeedFundedHousehold(state, Money.FromDenarii(100));

        var result = FundFestivalCommands.Pipeline.Execute(
            state, new FundFestivalCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, settlementId, Money.FromDenarii(50)));

        Assert.That(result.Events, Has.Count.EqualTo(2));
        Assert.That(result.Events.OfType<LedgerTransactionPostedEvent>().Count(), Is.EqualTo(1));
        Assert.That(result.Events.OfType<FestivalFundedEvent>().Count(), Is.EqualTo(1));
    }

    [Test]
    public void ZeroOrNegativeAmountIsRejected()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, settlementId) = SeedFundedHousehold(state, Money.FromDenarii(100));

        var result = FundFestivalCommands.Pipeline.Execute(
            state, new FundFestivalCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, settlementId, Money.Zero));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(FundFestivalCommands.AmountMustBePositive));
    }

    [Test]
    public void FundingBeyondTheTreasuryBalanceIsRejected()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, settlementId) = SeedFundedHousehold(state, Money.FromDenarii(10));

        var result = FundFestivalCommands.Pipeline.Execute(
            state, new FundFestivalCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, settlementId, Money.FromDenarii(50)));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(FundFestivalCommands.InsufficientTreasury));
    }

    [Test]
    public void FundingWithNoLedgerAccountYetIsRejectedAsInsufficientTreasury()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var settlementId = state.SettlementIds.Issue();

        var result = FundFestivalCommands.Pipeline.Execute(
            state, new FundFestivalCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, settlementId, Money.FromDenarii(1)));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(FundFestivalCommands.InsufficientTreasury));
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
