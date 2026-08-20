#nullable enable

using System.Linq;
using Gens.Presentation.Adapters;
using Gens.Presentation.Tests.Support;
using Gens.Simulation.Campaign;
using Gens.Simulation.Policies;
using Gens.Simulation.Queries;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Gens.Presentation.Tests.EditMode.Adapters;

public sealed class MonthlyReportAdapterTests
{
    [Test]
    public void AdaptCombinesFinancialsAndEventDerivedHeadlinesFromTheShellsRealQueryAndSubmit()
    {
        var shell = CampaignTestFixtures.Bootstrap();
        var result = shell.Submit(ChangeRitesBudgetCommands.Pipeline, new ChangeRitesBudgetCommand(
            shell.State.CommandIds.Issue(), shell.HouseholdId.ToTaggedString(), shell.State.Date, null,
            shell.HouseholdId, RitesBudgetTier.Lavish));
        Assert.That(result.Accepted, Is.True);

        var financials = shell.Query(new HouseholdFinancialsQuery(shell.HouseholdId), "player");
        var report = MonthlyReportProjector.Project(shell.State.Date, result.Events);
        var viewModel = MonthlyReportAdapter.Adapt(financials, report);

        Assert.That(viewModel.IncomeLabel, Does.EndWith(" denarii"));
        Assert.That(viewModel.Headlines.Select(h => h.Label), Has.Some.Contains("policies.ritesBudgetChanged"));
    }

    [Test]
    public void BindingReplacesHeadlinesOnReapplyRatherThanAppending()
    {
        var root = new VisualElement();
        root.Add(new Label { name = MonthlyReportBinding.IncomeLabelName });
        root.Add(new Label { name = MonthlyReportBinding.ExpensesLabelName });
        root.Add(new Label { name = MonthlyReportBinding.NetLabelName });
        var container = new VisualElement { name = MonthlyReportBinding.HeadlineContainerName };
        root.Add(container);
        var first = new MonthlyReportViewModel("1", "2", "3", new[] { new MonthlyReportHeadlineViewModel("e1", "one", null) });
        var second = new MonthlyReportViewModel("1", "2", "3", System.Array.Empty<MonthlyReportHeadlineViewModel>());

        MonthlyReportBinding.Apply(root, first);
        Assert.That(container.childCount, Is.EqualTo(1));

        MonthlyReportBinding.Apply(root, second);
        Assert.That(container.childCount, Is.EqualTo(0));
    }
}
