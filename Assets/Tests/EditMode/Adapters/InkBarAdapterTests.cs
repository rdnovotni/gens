#nullable enable

using Gens.Presentation.Adapters;
using Gens.Presentation.Tests.Support;
using Gens.Simulation.Queries;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Gens.Presentation.Tests.EditMode.Adapters;

public sealed class InkBarAdapterTests
{
    [Test]
    public void AdaptFormatsEveryProjectionFieldThroughTheShellsRealQuery()
    {
        var shell = CampaignTestFixtures.Bootstrap();
        CampaignTestFixtures.AddAdultHouseholdMember(shell, praenomen: "Gaius", nomen: "Julius");

        var projection = shell.Query(new InkBarQuery(shell.HouseholdId), "player");
        var viewModel = new InkBarAdapter().Adapt(projection);

        Assert.That(viewModel.GensNameLabel, Is.EqualTo("Julius"));
        Assert.That(viewModel.DateLabel, Does.Match(@"^\d{2}/\d+ (BCE|CE)$"));
        Assert.That(viewModel.TreasuryLabel, Does.EndWith(" denarii"));
        Assert.That(viewModel.DignitasLabel, Is.EqualTo("0 dignitas"));
    }

    [Test]
    public void AdaptFallsBackToUnknownGensWhenNoLivingMemberExists()
    {
        var projection = new InkBarProjection("household_0000000", "Unknown Gens", 100, "CE", 3, default, 0);

        var viewModel = new InkBarAdapter().Adapt(projection);

        Assert.That(viewModel.GensNameLabel, Is.EqualTo("Unknown Gens"));
    }

    [Test]
    public void BindingAppliesEveryLabelByName()
    {
        var root = new VisualElement();
        root.Add(new Label { name = InkBarBinding.GensNameLabelName });
        root.Add(new Label { name = InkBarBinding.DateLabelName });
        root.Add(new Label { name = InkBarBinding.TreasuryLabelName });
        root.Add(new Label { name = InkBarBinding.DignitasLabelName });
        var viewModel = new InkBarViewModel("Aurelii", "01/1 CE", "100 denarii", "0 dignitas");

        InkBarBinding.Apply(root, viewModel);

        Assert.That(root.Q<Label>(InkBarBinding.GensNameLabelName).text, Is.EqualTo("Aurelii"));
        Assert.That(root.Q<Label>(InkBarBinding.DateLabelName).text, Is.EqualTo("01/1 CE"));
        Assert.That(root.Q<Label>(InkBarBinding.TreasuryLabelName).text, Is.EqualTo("100 denarii"));
        Assert.That(root.Q<Label>(InkBarBinding.DignitasLabelName).text, Is.EqualTo("0 dignitas"));
    }

    [Test]
    public void BindingToleratesAMissingLabelInsteadOfThrowing()
    {
        var root = new VisualElement();
        var viewModel = new InkBarViewModel("Aurelii", "01/1 CE", "100 denarii", "0 dignitas");

        Assert.That(() => InkBarBinding.Apply(root, viewModel), Throws.Nothing);
    }
}
