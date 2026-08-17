using Gens.Simulation.Actions;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Policies;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Actions;

public sealed class ActionCatalogTests
{
    private static readonly DefinitionId<ActionDefinition> AlwaysEligibleId = new("always-eligible");
    private static readonly ValidationErrorCode NeverEligibleError = new("test.neverEligible");

    [Test]
    public void GetReturnsARegisteredDefinition()
    {
        var catalog = new ActionCatalog(new[] { MakeDefinition(AlwaysEligibleId, eligible: true) });

        var definition = catalog.Get(AlwaysEligibleId);

        Assert.That(definition.Id, Is.EqualTo(AlwaysEligibleId));
    }

    [Test]
    public void GetThrowsForAnUnregisteredId()
    {
        var catalog = new ActionCatalog(Array.Empty<ActionDefinition>());

        Assert.Throws<KeyNotFoundException>(() => catalog.Get(AlwaysEligibleId));
    }

    [Test]
    public void ConstructorRejectsDuplicateIds()
    {
        var duplicate = MakeDefinition(AlwaysEligibleId, eligible: true);

        Assert.Throws<ArgumentException>(() => new ActionCatalog(new[] { duplicate, duplicate }));
    }

    [Test]
    public void EligibilityHookReturnsNullWhenEligible()
    {
        var definition = MakeDefinition(AlwaysEligibleId, eligible: true);
        var state = new WorldState(new GameDate(0));
        var invocation = new ActionInvocation("household_0000000", null, new GameDate(0));

        Assert.That(definition.Eligibility(state, invocation), Is.Null);
    }

    [Test]
    public void EligibilityHookReturnsTheDeclaredErrorWhenIneligible()
    {
        var definition = MakeDefinition(AlwaysEligibleId, eligible: false);
        var state = new WorldState(new GameDate(0));
        var invocation = new ActionInvocation("household_0000000", null, new GameDate(0));

        Assert.That(definition.Eligibility(state, invocation), Is.EqualTo(NeverEligibleError));
    }

    [Test]
    public void PolicyActionDefinitionsCatalogRegistersBothActions()
    {
        var catalog = PolicyActionDefinitions.BuildCatalog();

        Assert.Multiple(() =>
        {
            Assert.That(catalog.TryGet(PolicyActionDefinitions.ChangeRitesBudget, out var ritesBudget), Is.True);
            Assert.That(ritesBudget!.Confirmation, Is.EqualTo(ActionConfirmationSeverity.Ordinary));

            Assert.That(catalog.TryGet(PolicyActionDefinitions.FundFestival, out var festival), Is.True);
            Assert.That(festival!.Confirmation, Is.EqualTo(ActionConfirmationSeverity.WaxSeal));
            Assert.That(festival!.Cost.Treasury, Is.EqualTo(PolicyActionDefinitions.DefaultFestivalAmount));
        });
    }

    [Test]
    public void FundFestivalEligibilityReflectsTheHouseholdsActualTreasuryBalance()
    {
        var catalog = PolicyActionDefinitions.BuildCatalog();
        var definition = catalog.Get(PolicyActionDefinitions.FundFestival);
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var invocation = new ActionInvocation(householdId.ToTaggedString(), null, new GameDate(0));

        var beforeFunding = definition.Eligibility(state, invocation);

        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), PolicyActionDefinitions.DefaultFestivalAmount),
                new LedgerPosting(LedgerAccountKey.Mint, -PolicyActionDefinitions.DefaultFestivalAmount),
            });

        var afterFunding = definition.Eligibility(state, invocation);

        Assert.Multiple(() =>
        {
            Assert.That(beforeFunding, Is.EqualTo(FundFestivalCommands.InsufficientTreasury));
            Assert.That(afterFunding, Is.Null);
        });
    }

    private static ActionDefinition MakeDefinition(DefinitionId<ActionDefinition> id, bool eligible) => new(
        id: id,
        nameKey: "test.name",
        descriptionKey: "test.description",
        targetKind: ActionTargetKind.None,
        cost: ActionCost.None,
        duration: ActionDuration.Instant,
        confirmation: ActionConfirmationSeverity.Ordinary,
        eligibility: (_, _) => eligible ? null : NeverEligibleError,
        scoreForAi: (_, _) => eligible ? 1.0 : 0.0,
        projectResult: (_, _) => ActionResultProjection.Of("test"));
}
