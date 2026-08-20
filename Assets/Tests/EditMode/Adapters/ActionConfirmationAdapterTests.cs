#nullable enable

using Gens.Presentation.Adapters;
using Gens.Presentation.Tests.Support;
using Gens.Simulation.Actions;
using Gens.Simulation.Policies;
using NUnit.Framework;

namespace Gens.Presentation.Tests.EditMode.Adapters;

/// <summary>Covers the severity logic (Phase 9 item 7's wax-seal vs. ordinary dialog choice) plus the
/// name-key humanization, both against the two real <see cref="ActionDefinition"/>s Phase 9 items 1-2
/// registered (<see cref="PolicyActionDefinitions"/>) rather than a hand-rolled stand-in.</summary>
public sealed class ActionConfirmationAdapterTests
{
    private readonly ActionCatalog _catalog = PolicyActionDefinitions.BuildCatalog();

    [Test]
    public void OrdinaryActionIsNotAWaxSeal()
    {
        var shell = CampaignTestFixtures.Bootstrap();
        var definition = _catalog.Get(PolicyActionDefinitions.ChangeRitesBudget);
        var invocation = new ActionInvocation(shell.HouseholdId.ToTaggedString(), null, shell.State.Date);
        var projection = definition.ProjectResult(shell.State, invocation);

        var viewModel = ActionConfirmationAdapter.Adapt(definition, projection);

        Assert.That(viewModel.IsWaxSeal, Is.False);
        Assert.That(viewModel.TitleLabel, Is.EqualTo("Change Rites Budget"));
        Assert.That(viewModel.BodyLabel, Is.EqualTo(projection.Summary));
    }

    [Test]
    public void ConsequentialActionIsAWaxSeal()
    {
        var shell = CampaignTestFixtures.Bootstrap();
        var definition = _catalog.Get(PolicyActionDefinitions.FundFestival);
        var invocation = new ActionInvocation(shell.HouseholdId.ToTaggedString(), null, shell.State.Date);
        var projection = definition.ProjectResult(shell.State, invocation);

        var viewModel = ActionConfirmationAdapter.Adapt(definition, projection);

        Assert.That(viewModel.IsWaxSeal, Is.True);
        Assert.That(viewModel.TitleLabel, Is.EqualTo("Fund Festival"));
    }

    [Test]
    public void HumanizeNameKeyTitleCasesEveryHyphenSegment()
    {
        var definition = new ActionDefinition(
            id: new(value: "probe"),
            nameKey: "actions.some-multi-word-action.name",
            descriptionKey: "actions.some-multi-word-action.description",
            targetKind: ActionTargetKind.None,
            cost: ActionCost.None,
            duration: ActionDuration.Instant,
            confirmation: ActionConfirmationSeverity.Ordinary,
            eligibility: (_, _) => null,
            scoreForAi: (_, _) => 0,
            projectResult: (_, _) => ActionResultProjection.Of("preview"));

        var viewModel = ActionConfirmationAdapter.Adapt(definition, ActionResultProjection.Of("preview"));

        Assert.That(viewModel.TitleLabel, Is.EqualTo("Some Multi Word Action"));
    }

    [Test]
    public void AdaptThrowsOnNullDefinition()
    {
        Assert.That(
            () => ActionConfirmationAdapter.Adapt(null!, ActionResultProjection.Of("x")),
            Throws.TypeOf<System.ArgumentNullException>());
    }
}
