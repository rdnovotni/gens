using Gens.Simulation.Characters;
using Gens.Simulation.Ledger;
using Gens.Simulation.Queries;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Queries;

public sealed class InkBarQueryTests
{
    [Test]
    public void ProjectsGensNameFromTheEldestLivingMemberAndTheHouseholdTreasury()
    {
        var state = new WorldState(new GameDate(754 * 12));
        var householdId = state.HouseholdIds.Issue();

        var youngerId = state.CharacterIds.Issue();
        state.Characters.Add(youngerId, CharacterTestFixtures.Minimal(
            youngerId, nomen: "Cornelius", household: householdId, birthDate: new GameDate(754 * 12 - 200)));
        var elderId = state.CharacterIds.Issue();
        state.Characters.Add(elderId, CharacterTestFixtures.Minimal(
            elderId, nomen: "Aurelius", household: householdId, birthDate: new GameDate(754 * 12 - 400)));

        state.LedgerAccounts.Add(
            LedgerAccountKey.ForHousehold(householdId),
            new LedgerAccount(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(500)));

        var projection = new InkBarQuery(householdId).Execute(state, "player");

        Assert.Multiple(() =>
        {
            Assert.That(projection.GensName, Is.EqualTo("Aurelius"));
            Assert.That(projection.Era, Is.EqualTo("CE"));
            Assert.That(projection.DisplayYear, Is.EqualTo(1));
            Assert.That(projection.Treasury, Is.EqualTo(Money.FromDenarii(500)));
            Assert.That(projection.Dignitas, Is.EqualTo(0));
        });
    }

    [Test]
    public void FallsBackToAPlaceholderNameWhenTheHouseholdHasNoLivingMembers()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();

        var projection = new InkBarQuery(householdId).Execute(state, "player");

        Assert.That(projection.GensName, Is.EqualTo("Unknown Gens"));
        Assert.That(projection.Treasury, Is.EqualTo(Money.Zero));
    }
}
