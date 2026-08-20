#nullable enable

using Gens.Presentation.Adapters;
using Gens.Presentation.Tests.Support;
using Gens.Simulation.Queries;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Gens.Presentation.Tests.EditMode.Adapters;

public sealed class CampaignClockAdapterTests
{
    [Test]
    public void AdaptFormatsTheProjectionFromTheShellsRealQuery()
    {
        var shell = CampaignTestFixtures.Bootstrap(startMonths: 0);

        var projection = shell.Query(new CampaignClockQuery(), "player");
        var viewModel = new CampaignClockAdapter().Adapt(projection);

        Assert.That(viewModel.DateLabel, Does.Match(@"^\d{2}/\d+ (BCE|CE)$"));
    }

    [Test]
    public void AdaptPadsSingleDigitMonths()
    {
        var projection = new CampaignClockProjection(754, "BCE", 3);

        var viewModel = new CampaignClockAdapter().Adapt(projection);

        Assert.That(viewModel.DateLabel, Is.EqualTo("03/754 BCE"));
    }

    [Test]
    public void BindingAppliesTheDateLabelByName()
    {
        var root = new VisualElement();
        root.Add(new Label { name = CampaignClockBinding.DateLabelName });

        CampaignClockBinding.Apply(root, new CampaignClockViewModel("03/754 BCE"));

        Assert.That(root.Q<Label>(CampaignClockBinding.DateLabelName).text, Is.EqualTo("03/754 BCE"));
    }

    [Test]
    public void BindingThrowsOnNullRoot()
    {
        Assert.That(
            () => CampaignClockBinding.Apply(null!, new CampaignClockViewModel("x")),
            Throws.TypeOf<System.ArgumentNullException>());
    }
}
