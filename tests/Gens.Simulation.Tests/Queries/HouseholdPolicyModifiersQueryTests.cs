using Gens.Simulation.Policies;
using Gens.Simulation.Queries;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Queries;

public sealed class HouseholdPolicyModifiersQueryTests
{
    [Test]
    public void ProjectsTheCatalogDefaultWhenNoPolicyHasBeenSet()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();

        var projection = new HouseholdPolicyModifiersQuery(householdId).Execute(state, "player");

        Assert.Multiple(() =>
        {
            Assert.That(projection.RitesBudget, Is.EqualTo(RitesBudgetCatalog.Default));
            Assert.That(projection.TreasuryDrawPerMonth, Is.EqualTo(RitesBudgetCatalog.TreasuryDrawPerMonth(RitesBudgetCatalog.Default)));
            Assert.That(projection.DivineFavorStabilityModifier, Is.EqualTo(RitesBudgetCatalog.DivineFavorStabilityModifier(RitesBudgetCatalog.Default)));
        });
    }

    [Test]
    public void ProjectsTheModifiersForTheHouseholdsCurrentTierAfterAChange()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        ChangeRitesBudgetCommands.Pipeline.Execute(
            state, new ChangeRitesBudgetCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, RitesBudgetTier.Lavish));

        var projection = new HouseholdPolicyModifiersQuery(householdId).Execute(state, "player");

        Assert.Multiple(() =>
        {
            Assert.That(projection.RitesBudget, Is.EqualTo(RitesBudgetTier.Lavish));
            Assert.That(projection.TreasuryDrawPerMonth, Is.EqualTo(RitesBudgetCatalog.TreasuryDrawPerMonth(RitesBudgetTier.Lavish)));
            Assert.That(projection.DivineFavorStabilityModifier, Is.EqualTo(RitesBudgetCatalog.DivineFavorStabilityModifier(RitesBudgetTier.Lavish)));
        });
    }
}
