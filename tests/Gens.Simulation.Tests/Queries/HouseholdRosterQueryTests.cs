using Gens.Simulation.Characters;
using Gens.Simulation.Queries;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Queries;

public sealed class HouseholdRosterQueryTests
{
    [Test]
    public void ListsOnlyLivingMembersOfTheRequestedHousehold()
    {
        var state = new WorldState(new GameDate(400));
        var householdId = state.HouseholdIds.Issue();
        var otherHouseholdId = state.HouseholdIds.Issue();

        var memberId = state.CharacterIds.Issue();
        state.Characters.Add(memberId, CharacterTestFixtures.Minimal(
            memberId, household: householdId, birthDate: new GameDate(0),
            duty: new DutyAssignment(householdId, DutySlot.FieldHand, new GameDate(0))));

        var deadMemberId = state.CharacterIds.Issue();
        state.Characters.Add(deadMemberId, CharacterTestFixtures.Minimal(
            deadMemberId, household: householdId,
            deathRecord: new DeathRecord(new GameDate(10), DeathCause.Disease, 40)));

        var otherHouseholdMemberId = state.CharacterIds.Issue();
        state.Characters.Add(otherHouseholdMemberId, CharacterTestFixtures.Minimal(
            otherHouseholdMemberId, household: otherHouseholdId));

        var projection = new HouseholdRosterQuery(householdId).Execute(state, "player");

        Assert.That(projection.Members, Has.Count.EqualTo(1));
        var row = projection.Members[0];
        Assert.Multiple(() =>
        {
            Assert.That(row.CharacterId, Is.EqualTo(memberId.ToTaggedString()));
            Assert.That(row.FullName, Is.EqualTo("Marcus Aurelius"));
            Assert.That(row.AgeInYears, Is.EqualTo(33));
            Assert.That(row.DutySlot, Is.EqualTo(DutySlot.FieldHand));
        });
    }
}
