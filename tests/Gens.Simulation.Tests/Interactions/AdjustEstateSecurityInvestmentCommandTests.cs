using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Interactions;

/// <summary>Phase 16 item 2 coverage for <see cref="AdjustEstateSecurityInvestmentCommand"/>.</summary>
public sealed class AdjustEstateSecurityInvestmentCommandTests
{
    private static WorldState StateWithFundedSponsor(out RuntimeId<Character> sponsorId, out RuntimeId<Household> householdId)
    {
        var state = new WorldState(new GameDate(0));
        householdId = state.HouseholdIds.Issue();
        sponsorId = state.CharacterIds.Issue();
        state.Characters.Add(sponsorId, CharacterTestFixtures.Minimal(sponsorId, household: householdId));
        state.LedgerAccounts.Add(
            LedgerAccountKey.ForHousehold(householdId),
            new LedgerAccount(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(10_000)));
        return state;
    }

    [Test]
    public void RejectsWhenTheSponsorIsUnknown()
    {
        var state = StateWithFundedSponsor(out _, out var householdId);
        var unknownId = state.CharacterIds.Issue();

        var command = new AdjustEstateSecurityInvestmentCommand(
            state.CommandIds.Issue(), unknownId.ToTaggedString(), new GameDate(0), CausationId: null,
            householdId, unknownId, TargetSecurityLevel: 20);

        var result = AdjustEstateSecurityInvestmentCommands.Pipeline.Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(AdjustEstateSecurityInvestmentCommands.SponsorNotFound));
    }

    [Test]
    public void RejectsWhenTheSponsorDoesNotBelongToTheHousehold()
    {
        var state = StateWithFundedSponsor(out var sponsorId, out _);
        var otherHouseholdId = state.HouseholdIds.Issue();

        var command = new AdjustEstateSecurityInvestmentCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), new GameDate(0), CausationId: null,
            otherHouseholdId, sponsorId, TargetSecurityLevel: 20);

        var result = AdjustEstateSecurityInvestmentCommands.Pipeline.Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(AdjustEstateSecurityInvestmentCommands.SponsorNotOfHousehold));
    }

    [Test]
    public void RejectsATargetLevelOutsideZeroToOneHundred()
    {
        var state = StateWithFundedSponsor(out var sponsorId, out var householdId);

        var command = new AdjustEstateSecurityInvestmentCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), new GameDate(0), CausationId: null,
            householdId, sponsorId, TargetSecurityLevel: 101);

        var result = AdjustEstateSecurityInvestmentCommands.Pipeline.Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(AdjustEstateSecurityInvestmentCommands.SecurityLevelOutOfRange));
    }

    [Test]
    public void RejectsWhenTheHouseholdCannotAffordTheRaise()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var sponsorId = state.CharacterIds.Issue();
        state.Characters.Add(sponsorId, CharacterTestFixtures.Minimal(sponsorId, household: householdId));

        var command = new AdjustEstateSecurityInvestmentCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), new GameDate(0), CausationId: null,
            householdId, sponsorId, TargetSecurityLevel: 20);

        var result = AdjustEstateSecurityInvestmentCommands.Pipeline.Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(AdjustEstateSecurityInvestmentCommands.InsufficientFunds));
    }

    [Test]
    public void RaisingTheLevelFromZeroChargesExactlyTheCostPerPointAndCreatesTheEntry()
    {
        var state = StateWithFundedSponsor(out var sponsorId, out var householdId);
        var account = LedgerAccountKey.ForHousehold(householdId);
        state.LedgerAccounts.TryGet(account, out var before);

        var command = new AdjustEstateSecurityInvestmentCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), new GameDate(0), CausationId: null,
            householdId, sponsorId, TargetSecurityLevel: 20);
        var result = AdjustEstateSecurityInvestmentCommands.Pipeline.Execute(state, command);

        state.LedgerAccounts.TryGet(account, out var after);
        state.EstateSecurityInvestments.TryGet(householdId, out var investment);

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(investment!.SecurityLevel, Is.EqualTo(20));
            Assert.That(after!.Balance, Is.EqualTo(before!.Balance - Money.FromDenarii(20 * RaidThreatCatalog.SecurityLevelCostPerPointDenarii)));
        });
    }

    [Test]
    public void LoweringTheLevelIsFreeAndDoesNotTouchTheLedger()
    {
        var state = StateWithFundedSponsor(out var sponsorId, out var householdId);
        state.EstateSecurityInvestments.Add(householdId, EstateSecurityInvestment.CreateUnguarded(householdId, new GameDate(0)).WithLevel(50, new GameDate(0)));
        var account = LedgerAccountKey.ForHousehold(householdId);
        state.LedgerAccounts.TryGet(account, out var before);

        var command = new AdjustEstateSecurityInvestmentCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), new GameDate(1), CausationId: null,
            householdId, sponsorId, TargetSecurityLevel: 10);
        AdjustEstateSecurityInvestmentCommands.Pipeline.Execute(state, command);

        state.LedgerAccounts.TryGet(account, out var after);
        state.EstateSecurityInvestments.TryGet(householdId, out var investment);

        Assert.Multiple(() =>
        {
            Assert.That(investment!.SecurityLevel, Is.EqualTo(10));
            Assert.That(after!.Balance, Is.EqualTo(before!.Balance));
        });
    }
}
