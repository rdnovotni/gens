using Gens.Simulation.Hazards;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Policies;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Policies;

public sealed class FundDisasterReliefCommandTests
{
    [Test]
    public void AcceptedReliefDebitsTheHouseholdAndGrantsDignitas()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, settlementId) = SeedFundedHousehold(state, Money.FromDenarii(100));
        var disasterEventId = SeedDisasterEvent(state, settlementId, DisasterSeverity.Severe);

        var result = FundDisasterReliefCommands.Pipeline.Execute(
            state, new FundDisasterReliefCommand(
                state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, disasterEventId, Money.FromDenarii(30)));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account), Is.True);
            Assert.That(account!.Balance, Is.EqualTo(Money.FromDenarii(70)));
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(FundDisasterReliefCommands.DignitasGain));
        });
    }

    [Test]
    public void AcceptedReliefStampsTheDisasterEventReliefFunded()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, settlementId) = SeedFundedHousehold(state, Money.FromDenarii(100));
        var disasterEventId = SeedDisasterEvent(state, settlementId, DisasterSeverity.Catastrophic);

        FundDisasterReliefCommands.Pipeline.Execute(
            state, new FundDisasterReliefCommand(
                state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, disasterEventId, Money.FromDenarii(30)));

        Assert.That(state.DisasterEvents.TryGet(disasterEventId, out var disasterEvent), Is.True);
        Assert.That(disasterEvent!.ReliefFunded, Is.True);
    }

    [Test]
    public void AMinorOrModerateEventIsRejectedAsTooLowSeverity()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, settlementId) = SeedFundedHousehold(state, Money.FromDenarii(100));
        var disasterEventId = SeedDisasterEvent(state, settlementId, DisasterSeverity.Moderate);

        var result = FundDisasterReliefCommands.Pipeline.Execute(
            state, new FundDisasterReliefCommand(
                state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, disasterEventId, Money.FromDenarii(10)));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(FundDisasterReliefCommands.SeverityTooLow));
    }

    [Test]
    public void ASecondReliefForTheSameEventIsRejected()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, settlementId) = SeedFundedHousehold(state, Money.FromDenarii(100));
        var disasterEventId = SeedDisasterEvent(state, settlementId, DisasterSeverity.Severe);

        FundDisasterReliefCommands.Pipeline.Execute(
            state, new FundDisasterReliefCommand(
                state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, disasterEventId, Money.FromDenarii(10)));
        var result = FundDisasterReliefCommands.Pipeline.Execute(
            state, new FundDisasterReliefCommand(
                state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, disasterEventId, Money.FromDenarii(10)));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(FundDisasterReliefCommands.AlreadyFunded));
    }

    [Test]
    public void AnUnknownDisasterEventIsRejected()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, _) = SeedFundedHousehold(state, Money.FromDenarii(100));

        var result = FundDisasterReliefCommands.Pipeline.Execute(
            state, new FundDisasterReliefCommand(
                state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, state.DisasterEventIds.Issue(), Money.FromDenarii(10)));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(FundDisasterReliefCommands.DisasterEventNotFound));
    }

    [Test]
    public void FundingBeyondTheTreasuryBalanceIsRejected()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, settlementId) = SeedFundedHousehold(state, Money.FromDenarii(5));
        var disasterEventId = SeedDisasterEvent(state, settlementId, DisasterSeverity.Severe);

        var result = FundDisasterReliefCommands.Pipeline.Execute(
            state, new FundDisasterReliefCommand(
                state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, disasterEventId, Money.FromDenarii(30)));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(FundDisasterReliefCommands.InsufficientTreasury));
    }

    [Test]
    public void ZeroOrNegativeAmountIsRejected()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, settlementId) = SeedFundedHousehold(state, Money.FromDenarii(100));
        var disasterEventId = SeedDisasterEvent(state, settlementId, DisasterSeverity.Severe);

        var result = FundDisasterReliefCommands.Pipeline.Execute(
            state, new FundDisasterReliefCommand(
                state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, disasterEventId, Money.Zero));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(FundDisasterReliefCommands.AmountMustBePositive));
    }

    private static RuntimeId<DisasterEvent> SeedDisasterEvent(WorldState state, RuntimeId<Settlement> settlementId, DisasterSeverity severity)
    {
        var id = state.DisasterEventIds.Issue();
        state.DisasterEvents.Add(id, DisasterEvent.Create(id, settlementId, state.Date, HazardType.Fire, severity));
        return id;
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
