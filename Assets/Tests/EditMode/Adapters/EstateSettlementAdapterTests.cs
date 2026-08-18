#nullable enable

using System.Linq;
using Gens.Presentation.Adapters;
using Gens.Presentation.Tests.Support;
using Gens.Simulation.Buildings;
using Gens.Simulation.Queries;
using NUnit.Framework;
using UnityEngine.UIElements;
using static Gens.Presentation.Tests.Support.UiTestEvents;

namespace Gens.Presentation.Tests.EditMode.Adapters;

public sealed class EstateSettlementAdapterTests
{
    [Test]
    public void AdaptShapesHoldingsAndBuildingsFromTheShellsRealQuery()
    {
        var shell = CampaignTestFixtures.Bootstrap();
        CampaignTestFixtures.AddAdultHouseholdMember(shell); // EstateLookup.HasLaborAvailable needs a living resident.
        var holdingId = CampaignTestFixtures.SeedHouseholdHolding(shell);
        var plotId = CampaignTestFixtures.FirstPlotOf(shell, holdingId);
        shell.State.ConstructionSchedules.Add(holdingId, new ConstructionSchedule());
        shell.State.ConstructionSchedules.TryGet(holdingId, out var schedule);
        shell.State.Plots.TryGet(plotId, out var plot);
        schedule!.Enqueue(plot!, CampaignTestFixtures.GrainFieldDefinition(), System.Array.Empty<BuildingInstance>());
        shell.AdvanceMonth(new Gens.Simulation.Time.IMonthlySystem<Gens.Simulation.State.WorldState>[] { new ConstructionSystem() });

        var projection = shell.Query(new EstateSettlementQuery(shell.SettlementId, shell.HouseholdId), "player");
        var viewModel = new EstateSettlementAdapter().Adapt(projection);

        var holding = viewModel.Holdings.Single();
        Assert.That(holding.HoldingId, Is.EqualTo(holdingId.ToTaggedString()));
        var building = holding.Buildings.Single();
        Assert.That(building.Label, Does.Contain("ager"));
    }

    [Test]
    public void AdaptLabelsAHoldingWithoutAVillaAsABareHolding()
    {
        var projection = new EstateSettlementProjection("settlement_0000000", Gens.Simulation.Land.SettlementStage.Villa, new[]
        {
            new EstateHoldingRow("holding_0000000", VillaStage: null, ResidentCapacity: 5,
                Plots: System.Array.Empty<EstatePlotRow>()),
        });

        var viewModel = new EstateSettlementAdapter().Adapt(projection);

        Assert.That(viewModel.Holdings.Single().Label, Is.EqualTo("Holding · 5 residents"));
    }

    [Test]
    public void BindingWiresBothActionButtonsAndPopulatesHoldings()
    {
        var root = new VisualElement();
        root.Add(new Label { name = EstateSettlementBinding.StageLabelName });
        root.Add(new VisualElement { name = EstateSettlementBinding.HoldingContainerName });
        root.Add(new Button { name = EstateSettlementBinding.ChangeRitesBudgetButtonName });
        root.Add(new Button { name = EstateSettlementBinding.FundFestivalButtonName });
        var viewModel = new EstateSettlementViewModel("Villa", new[]
        {
            new EstateHoldingRowViewModel("holding_0000000", "Holding · 1 residents", new[]
            {
                new EstateBuildingRowViewModel("building_0000000", "ager (Tier1)", "Sound"),
            }),
        });
        var ritesClicks = 0;
        var festivalClicks = 0;

        EstateSettlementBinding.Apply(root, viewModel, () => ritesClicks++, () => festivalClicks++);
        SimulateClick(root.Q<Button>(EstateSettlementBinding.ChangeRitesBudgetButtonName));
        SimulateClick(root.Q<Button>(EstateSettlementBinding.FundFestivalButtonName));

        Assert.That(ritesClicks, Is.EqualTo(1));
        Assert.That(festivalClicks, Is.EqualTo(1));
        Assert.That(root.Q<Label>(EstateSettlementBinding.StageLabelName).text, Is.EqualTo("Villa"));
        Assert.That(root.Q<VisualElement>(EstateSettlementBinding.HoldingContainerName).childCount, Is.EqualTo(1));
    }

    [Test]
    public void ApplyThrowsOnNullCallbacks()
    {
        var root = new VisualElement();
        var viewModel = new EstateSettlementViewModel("Villa", System.Array.Empty<EstateHoldingRowViewModel>());

        Assert.That(
            () => EstateSettlementBinding.Apply(root, viewModel, null!, () => { }),
            Throws.TypeOf<System.ArgumentNullException>());
    }
}
