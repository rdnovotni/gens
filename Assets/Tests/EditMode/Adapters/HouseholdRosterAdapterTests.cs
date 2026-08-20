#nullable enable

using System.Linq;
using Gens.Presentation.Adapters;
using Gens.Presentation.Tests.Support;
using Gens.Simulation.Characters;
using Gens.Simulation.Queries;
using NUnit.Framework;
using UnityEngine.UIElements;
using static Gens.Presentation.Tests.Support.UiTestEvents;

namespace Gens.Presentation.Tests.EditMode.Adapters;

public sealed class HouseholdRosterAdapterTests
{
    [Test]
    public void AdaptShapesOneRowPerLivingMemberFromTheShellsRealQuery()
    {
        var shell = CampaignTestFixtures.Bootstrap();
        var characterId = CampaignTestFixtures.AddAdultHouseholdMember(shell, praenomen: "Marcus", nomen: "Aurelius");
        shell.Submit(AssignDutyCommands.Pipeline, new AssignDutyCommand(
            shell.State.CommandIds.Issue(), shell.HouseholdId.ToTaggedString(), shell.State.Date, null,
            characterId, shell.HouseholdId, DutySlot.FieldHand));

        var projection = shell.Query(new HouseholdRosterQuery(shell.HouseholdId), "player");
        var viewModel = new HouseholdRosterAdapter().Adapt(projection);

        var row = viewModel.Rows.Single();
        Assert.That(row.CharacterId, Is.EqualTo(characterId.ToTaggedString()));
        Assert.That(row.DisplayName, Is.EqualTo("Marcus Aurelius"));
        Assert.That(row.DisplaySubtitle, Does.Contain("FieldHand"));
    }

    [Test]
    public void AdaptOmitsTheDutySlotSegmentWhenUnassigned()
    {
        var projection = new HouseholdRosterProjection("household_0000000", new[]
        {
            new HouseholdRosterRow("character_0000000", "Marcus Aurelius", Sex.Male, 25, LifecycleStage.Adult,
                LegalStatus.RomanCitizen, SocialClass.Plebeian, DutySlot: null),
        });

        var viewModel = new HouseholdRosterAdapter().Adapt(projection);

        Assert.That(viewModel.Rows.Single().DisplaySubtitle, Is.EqualTo("25 · RomanCitizen"));
    }

    [Test]
    public void BindingBuildsOneClickableRowPerMemberAndInvokesTheSelectionCallback()
    {
        var root = new VisualElement();
        root.Add(new VisualElement { name = HouseholdRosterBinding.RowContainerName });
        var viewModel = new HouseholdRosterViewModel(new[]
        {
            new HouseholdRosterRowViewModel("character_0000000", "Marcus Aurelius", "25 · RomanCitizen"),
        });
        string? selected = null;

        HouseholdRosterBinding.Apply(root, viewModel, id => selected = id);
        var button = root.Q<Button>("roster-row-character_0000000");
        Assert.That(button, Is.Not.Null);

        // Pointer down+up (rather than a bare ClickEvent) is the simulation Clickable's manipulator
        // actually listens for regardless of Unity/UI Toolkit version — a synthetic ClickEvent alone
        // is not guaranteed to reach Button.clicked across every version this project might build
        // against.
        SimulateClick(button);

        Assert.That(selected, Is.EqualTo("character_0000000"));
    }

    [Test]
    public void BindingClearsPreviousRowsOnReapply()
    {
        var root = new VisualElement();
        var container = new VisualElement { name = HouseholdRosterBinding.RowContainerName };
        root.Add(container);
        var first = new HouseholdRosterViewModel(new[]
        {
            new HouseholdRosterRowViewModel("character_0000000", "A", "x"),
        });
        var second = new HouseholdRosterViewModel(System.Array.Empty<HouseholdRosterRowViewModel>());

        HouseholdRosterBinding.Apply(root, first, _ => { });
        HouseholdRosterBinding.Apply(root, second, _ => { });

        Assert.That(container.childCount, Is.EqualTo(0));
    }
}
