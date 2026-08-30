using Gens.Simulation.Clientela;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.InterestGroups;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.InterestGroups;

/// <summary>Phase 12 item 6 coverage: the one real, checkable Interest Group membership (Creditors vs.
/// Debtors' Debtor half, §2) and Collective Lobbying's real Influence-pooling payoff (§5).</summary>
public sealed class InterestGroupsTests
{
    private static RuntimeId<Household> AddHouseholdWithHead(WorldState state, string nomen)
    {
        var householdId = state.HouseholdIds.Issue();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, nomen: nomen, household: householdId));
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0)));
        return householdId;
    }

    [Test]
    public void IsMemberReadsCreditorsVsDebtorsFromARealActiveDebtRecord()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = AddHouseholdWithHead(state, "Debtor");
        var settlementId = state.SettlementIds.Issue();

        Assert.That(InterestGroupResolver.IsMember(state, householdId, InterestGroupType.CreditorsVsDebtors), Is.False);

        DebtService.IssueLoan(state, new GameDate(0), settlementId, householdId, Money.FromDenarii(100));

        Assert.That(InterestGroupResolver.IsMember(state, householdId, InterestGroupType.CreditorsVsDebtors), Is.True);
    }

    [Test]
    public void IsMemberThrowsForEveryTypeWithNoRealCheckableData()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = AddHouseholdWithHead(state, "Someone");

        Assert.Multiple(() =>
        {
            Assert.Throws<NotSupportedException>(() =>
                InterestGroupResolver.IsMember(state, householdId, InterestGroupType.LandownersVsLandless));
            Assert.Throws<NotSupportedException>(() =>
                InterestGroupResolver.IsMember(state, householdId, InterestGroupType.PublicaniEquestrian));
            Assert.Throws<NotSupportedException>(() =>
                InterestGroupResolver.IsMember(state, householdId, InterestGroupType.Veterans));
            Assert.Throws<NotSupportedException>(() =>
                InterestGroupResolver.IsMember(state, householdId, InterestGroupType.ProvincialInterest));
        });
    }

    [Test]
    public void CollectiveLobbyingCommandPoolsInfluenceFromContributorsIntoTheBeneficiary()
    {
        var state = new WorldState(new GameDate(0));
        var beneficiaryHouseholdId = AddHouseholdWithHead(state, "Beneficiary");
        var contributorAId = AddHouseholdWithHead(state, "ContributorA");
        var contributorBId = AddHouseholdWithHead(state, "ContributorB");
        InfluenceResolver.Apply(state, contributorAId, 20);
        InfluenceResolver.Apply(state, contributorBId, 20);

        var result = CollectiveLobbyingCommands.Pipeline.Execute(
            state,
            new CollectiveLobbyingCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, new[] { contributorAId, contributorBId },
                beneficiaryHouseholdId, InfluencePerContributor: 15));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(InfluenceResolver.Current(state, contributorAId), Is.EqualTo(5));
            Assert.That(InfluenceResolver.Current(state, contributorBId), Is.EqualTo(5));
            Assert.That(InfluenceResolver.Current(state, beneficiaryHouseholdId), Is.EqualTo(30));
        });
    }

    [Test]
    public void CollectiveLobbyingCommandRejectsAnUnderfundedContributor()
    {
        var state = new WorldState(new GameDate(0));
        var beneficiaryHouseholdId = AddHouseholdWithHead(state, "Beneficiary");
        var contributorHouseholdId = AddHouseholdWithHead(state, "Contributor");

        var result = CollectiveLobbyingCommands.Pipeline.Execute(
            state,
            new CollectiveLobbyingCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, new[] { contributorHouseholdId },
                beneficiaryHouseholdId, InfluencePerContributor: 10));

        Assert.That(result.Error, Is.EqualTo(CollectiveLobbyingCommands.InsufficientInfluence));
    }
}
