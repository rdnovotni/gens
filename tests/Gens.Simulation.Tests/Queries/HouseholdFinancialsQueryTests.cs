using Gens.Simulation.Economy;
using Gens.Simulation.Ledger;
using Gens.Simulation.Queries;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Queries;

public sealed class HouseholdFinancialsQueryTests
{
    [Test]
    public void ProjectsTheCurrentMonthsPreparedStatement()
    {
        var state = new WorldState(new GameDate(50));
        var householdId = state.HouseholdIds.Issue();
        state.HouseholdStatements.Add(householdId, new HouseholdMonthlyStatement(
            householdId, state.Date, Money.FromDenarii(100), Money.FromDenarii(40), Money.FromDenarii(60)));

        var projection = new HouseholdFinancialsQuery(householdId).Execute(state, "player");

        Assert.Multiple(() =>
        {
            Assert.That(projection.Income, Is.EqualTo(Money.FromDenarii(100)));
            Assert.That(projection.Expenses, Is.EqualTo(Money.FromDenarii(40)));
            Assert.That(projection.Net, Is.EqualTo(Money.FromDenarii(60)));
        });
    }

    [Test]
    public void DefaultsToZeroWhenNoStatementWasPreparedThisMonth()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();

        var projection = new HouseholdFinancialsQuery(householdId).Execute(state, "player");

        Assert.That(projection.Income, Is.EqualTo(Money.Zero));
        Assert.That(projection.Expenses, Is.EqualTo(Money.Zero));
        Assert.That(projection.Net, Is.EqualTo(Money.Zero));
    }
}
