#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Gens.Simulation.Queries;
using UnityEngine.UIElements;

namespace Gens.Presentation.Adapters;

/// <summary>One building's display-ready row within its parent holding (Phase 9 item 6's
/// estate/settlement screen).</summary>
public readonly record struct EstateBuildingRowViewModel(string BuildingId, string Label, string ConditionLabel);

/// <summary>One holding's display-ready summary, grouping its buildings the way <c>gens-core-design.md</c>
/// §7.4's "grouped building chains" right leaf calls for.</summary>
public readonly record struct EstateHoldingRowViewModel(
    string HoldingId,
    string Label,
    IReadOnlyList<EstateBuildingRowViewModel> Buildings);

/// <summary>Display-ready fields for the estate/settlement screen.</summary>
public readonly record struct EstateSettlementViewModel(
    string SettlementStageLabel,
    IReadOnlyList<EstateHoldingRowViewModel> Holdings);

/// <summary>The only code allowed to turn an <see cref="EstateSettlementProjection"/> into
/// estate-screen text, mirroring <see cref="CampaignClockAdapter"/>'s "pure translation, no
/// UnityEngine" role.</summary>
public sealed class EstateSettlementAdapter : IProjectionAdapter<EstateSettlementProjection, EstateSettlementViewModel>
{
    public EstateSettlementViewModel Adapt(EstateSettlementProjection projection) =>
        new(
            SettlementStageLabel: projection.SettlementStage.ToString(),
            Holdings: projection.Holdings.Select(Adapt).ToArray());

    private static EstateHoldingRowViewModel Adapt(EstateHoldingRow holding) =>
        new(
            HoldingId: holding.HoldingId,
            Label: holding.VillaStage is { } stage
                ? $"Villa ({stage}) · {holding.ResidentCapacity} residents"
                : $"Holding · {holding.ResidentCapacity} residents",
            Buildings: holding.Plots.SelectMany(plot => plot.Buildings).Select(Adapt).ToArray());

    private static EstateBuildingRowViewModel Adapt(EstateBuildingRow building) =>
        new(building.BuildingId, $"{building.DefinitionKey} ({building.Tier})", building.Condition.ToString());
}

/// <summary>Populates the estate/settlement screen's holding container
/// (<c>Assets/UI/EstateSettlementScreen.uxml</c>) from an <see cref="EstateSettlementViewModel"/> —
/// the concrete "UXML/USS-bound view model" translation ADR 0013 reserves for Unity adapters.</summary>
public static class EstateSettlementBinding
{
    public const string StageLabelName = "estate-settlement-stage";
    public const string HoldingContainerName = "estate-holdings";
    public const string ChangeRitesBudgetButtonName = "estate-settlement-change-rites-budget";
    public const string FundFestivalButtonName = "estate-settlement-fund-festival";
    private const string HoldingClass = "estate-holding";
    private const string HoldingLabelClass = "estate-holding__label";
    private const string BuildingListClass = "estate-holding__buildings";
    private const string BuildingRowClass = "estate-building-row";

    /// <summary>Populates the estate/settlement screen and wires its two household-action buttons
    /// (Phase 9 item 7's wax-seal/ordinary confirmation flow) to the caller-supplied handlers — the
    /// same "callback per interactive element" shape as <see cref="HouseholdRosterBinding.Apply"/>'s
    /// <c>onSelectCharacter</c>, since neither action actually submits anything itself here; the caller
    /// (Phase 9 item 8's <c>GensUIController</c>) owns running the confirmation dialog first.</summary>
    public static void Apply(
        VisualElement root, EstateSettlementViewModel viewModel, Action onChangeRitesBudget, Action onFundFestival)
    {
        if (root is null)
            throw new ArgumentNullException(nameof(root));
        if (onChangeRitesBudget is null)
            throw new ArgumentNullException(nameof(onChangeRitesBudget));
        if (onFundFestival is null)
            throw new ArgumentNullException(nameof(onFundFestival));

        var changeRitesBudgetButton = root.Q<Button>(ChangeRitesBudgetButtonName);
        if (changeRitesBudgetButton is not null)
            changeRitesBudgetButton.clicked += onChangeRitesBudget;

        var fundFestivalButton = root.Q<Button>(FundFestivalButtonName);
        if (fundFestivalButton is not null)
            fundFestivalButton.clicked += onFundFestival;

        var stageLabel = root.Q<Label>(StageLabelName);
        if (stageLabel is not null)
            stageLabel.text = viewModel.SettlementStageLabel;

        var container = root.Q<VisualElement>(HoldingContainerName);
        if (container is null)
            return;

        container.Clear();
        foreach (var holding in viewModel.Holdings)
        {
            var holdingElement = new VisualElement { name = $"estate-holding-{holding.HoldingId}" };
            holdingElement.AddToClassList(HoldingClass);

            var label = new Label(holding.Label);
            label.AddToClassList(HoldingLabelClass);
            holdingElement.Add(label);

            var buildingList = new VisualElement();
            buildingList.AddToClassList(BuildingListClass);
            foreach (var building in holding.Buildings)
            {
                var buildingLabel = new Label($"{building.Label} — {building.ConditionLabel}");
                buildingLabel.AddToClassList(BuildingRowClass);
                buildingList.Add(buildingLabel);
            }

            holdingElement.Add(buildingList);
            container.Add(holdingElement);
        }
    }
}
