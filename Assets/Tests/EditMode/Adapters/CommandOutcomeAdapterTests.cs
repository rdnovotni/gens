#nullable enable

using Gens.Presentation.Adapters;
using Gens.Presentation.Tests.Support;
using Gens.Simulation.Policies;
using NUnit.Framework;

namespace Gens.Presentation.Tests.EditMode.Adapters;

public sealed class CommandOutcomeAdapterTests
{
    [Test]
    public void AdaptReportsAcceptedWithNoErrorCodeForAnAcceptedCommand()
    {
        var shell = CampaignTestFixtures.Bootstrap();
        var result = shell.Submit(ChangeRitesBudgetCommands.Pipeline, new ChangeRitesBudgetCommand(
            shell.State.CommandIds.Issue(), shell.HouseholdId.ToTaggedString(), shell.State.Date, null,
            shell.HouseholdId, RitesBudgetTier.Lavish));

        var viewModel = new CommandOutcomeAdapter().Adapt(result);

        Assert.That(viewModel.Accepted, Is.True);
        Assert.That(viewModel.ErrorCode, Is.Null);
    }

    [Test]
    public void AdaptReportsRejectedWithTheStableErrorCodeForARejectedCommand()
    {
        var shell = CampaignTestFixtures.Bootstrap();
        shell.Submit(ChangeRitesBudgetCommands.Pipeline, new ChangeRitesBudgetCommand(
            shell.State.CommandIds.Issue(), shell.HouseholdId.ToTaggedString(), shell.State.Date, null,
            shell.HouseholdId, RitesBudgetTier.Standard));
        var rejected = shell.Submit(ChangeRitesBudgetCommands.Pipeline, new ChangeRitesBudgetCommand(
            shell.State.CommandIds.Issue(), shell.HouseholdId.ToTaggedString(), shell.State.Date, null,
            shell.HouseholdId, RitesBudgetTier.Standard));

        var viewModel = new CommandOutcomeAdapter().Adapt(rejected);

        Assert.That(viewModel.Accepted, Is.False);
        Assert.That(viewModel.ErrorCode, Is.EqualTo(ChangeRitesBudgetCommands.TierUnchanged.ToString()));
    }
}
