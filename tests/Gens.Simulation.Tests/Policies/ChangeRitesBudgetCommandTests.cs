using Gens.Simulation.Identity;
using Gens.Simulation.Policies;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Policies;

public sealed class ChangeRitesBudgetCommandTests
{
    [Test]
    public void FirstChangeForAHouseholdIsAcceptedRegardlessOfCooldown()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();

        var result = ChangeRitesBudgetCommands.Pipeline.Execute(
            state, MakeCommand(state, householdId, RitesBudgetTier.Lavish, new GameDate(0)));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(state.HouseholdPolicies.TryGet(householdId, out var policy), Is.True);
            Assert.That(policy!.RitesBudget, Is.EqualTo(RitesBudgetTier.Lavish));
            Assert.That(policy!.LastChangedDate, Is.EqualTo(new GameDate(0)));
        });
    }

    [Test]
    public void ChangingToTheSameTierIsRejected()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        ChangeRitesBudgetCommands.Pipeline.Execute(state, MakeCommand(state, householdId, RitesBudgetTier.Lavish, new GameDate(0)));

        var result = ChangeRitesBudgetCommands.Pipeline.Execute(
            state, MakeCommand(state, householdId, RitesBudgetTier.Lavish, new GameDate(RitesBudgetCatalog.CooldownMonths)));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ChangeRitesBudgetCommands.TierUnchanged));
    }

    [Test]
    public void ChangingBeforeTheCooldownElapsesIsRejected()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        ChangeRitesBudgetCommands.Pipeline.Execute(state, MakeCommand(state, householdId, RitesBudgetTier.Frugal, new GameDate(0)));

        var result = ChangeRitesBudgetCommands.Pipeline.Execute(
            state, MakeCommand(state, householdId, RitesBudgetTier.Lavish, new GameDate(RitesBudgetCatalog.CooldownMonths - 1)));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ChangeRitesBudgetCommands.OnCooldown));
    }

    [Test]
    public void ChangingExactlyAtTheCooldownBoundaryIsAccepted()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        ChangeRitesBudgetCommands.Pipeline.Execute(state, MakeCommand(state, householdId, RitesBudgetTier.Frugal, new GameDate(0)));

        var result = ChangeRitesBudgetCommands.Pipeline.Execute(
            state, MakeCommand(state, householdId, RitesBudgetTier.Lavish, new GameDate(RitesBudgetCatalog.CooldownMonths)));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(state.HouseholdPolicies.TryGet(householdId, out var policy), Is.True);
            Assert.That(policy!.RitesBudget, Is.EqualTo(RitesBudgetTier.Lavish));
        });
    }

    [Test]
    public void AcceptedChangeEmitsARitesBudgetChangedEventWithThePreviousTier()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        ChangeRitesBudgetCommands.Pipeline.Execute(state, MakeCommand(state, householdId, RitesBudgetTier.Frugal, new GameDate(0)));

        var result = ChangeRitesBudgetCommands.Pipeline.Execute(
            state, MakeCommand(state, householdId, RitesBudgetTier.Lavish, new GameDate(RitesBudgetCatalog.CooldownMonths)));

        var domainEvent = (RitesBudgetChangedEvent)result.Events.Single();
        Assert.Multiple(() =>
        {
            Assert.That(domainEvent.HouseholdId, Is.EqualTo(householdId));
            Assert.That(domainEvent.PreviousTier, Is.EqualTo(RitesBudgetTier.Frugal));
            Assert.That(domainEvent.NewTier, Is.EqualTo(RitesBudgetTier.Lavish));
        });
    }

    [Test]
    public void HouseholdPolicyResolverFallsBackToTheCatalogDefaultWhenNoEntryExists()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();

        var effective = HouseholdPolicyResolver.GetEffectiveRitesBudget(state, householdId);

        Assert.That(effective, Is.EqualTo(RitesBudgetCatalog.Default));
    }

    private static ChangeRitesBudgetCommand MakeCommand(
        WorldState state, RuntimeId<Household> householdId, RitesBudgetTier tier, GameDate date) =>
        new(state.CommandIds.Issue(), "system", date, null, householdId, tier);
}
