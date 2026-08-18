#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Gens.Simulation.Campaign;
using Gens.Simulation.Queries;
using UnityEngine.UIElements;

namespace Gens.Presentation.Adapters;

/// <summary>One display-ready line in the Monthly Report screen's headline list, built from a <see
/// cref="MonthlyReportEntry"/> rather than the source <see cref="IDomainEvent"/> itself.</summary>
public readonly record struct MonthlyReportHeadlineViewModel(string EventId, string Label, string? SourceInstanceId);

/// <summary>Display-ready fields for the Monthly Report screen (Phase 9 item 6). Combines the
/// household's financial summary (<see cref="HouseholdFinancialsQuery"/>) with the event-derived
/// headlines <see cref="MonthlyReportProjector"/> already builds (Phase 9 item 4) — the two live in
/// separate query/read-model shapes (see <see cref="HouseholdFinancialsQuery"/>'s own doc comment for
/// why), so this is where the screen's one view model is actually assembled.</summary>
public readonly record struct MonthlyReportViewModel(
    string IncomeLabel,
    string ExpensesLabel,
    string NetLabel,
    IReadOnlyList<MonthlyReportHeadlineViewModel> Headlines);

/// <summary>The only code allowed to turn a <see cref="HouseholdFinancialsProjection"/> plus a <see
/// cref="MonthlyReportProjection"/> into Monthly Report screen text, mirroring <see
/// cref="CampaignClockAdapter"/>'s "pure translation, no UnityEngine" role. Takes both projections
/// directly rather than fitting <see cref="IProjectionAdapter{TProjection,TViewModel}"/>'s
/// single-projection shape, since this screen alone draws from two distinct read paths.</summary>
public static class MonthlyReportAdapter
{
    public static MonthlyReportViewModel Adapt(HouseholdFinancialsProjection financials, MonthlyReportProjection report) =>
        new(
            IncomeLabel: $"{financials.Income.ToDisplayString()} denarii",
            ExpensesLabel: $"{financials.Expenses.ToDisplayString()} denarii",
            NetLabel: $"{financials.Net.ToDisplayString()} denarii",
            Headlines: report.Entries.Select(Adapt).ToArray());

    private static MonthlyReportHeadlineViewModel Adapt(MonthlyReportEntry entry) =>
        new(entry.EventId, $"[{entry.Group}] {entry.EventType} — {entry.Importance}", entry.SourceInstanceId);
}

/// <summary>Populates the Monthly Report screen's summary labels and headline container
/// (<c>Assets/UI/MonthlyReportScreen.uxml</c>) from a <see cref="MonthlyReportViewModel"/> — the
/// concrete "UXML/USS-bound view model" translation ADR 0013 reserves for Unity adapters.</summary>
public static class MonthlyReportBinding
{
    public const string IncomeLabelName = "monthly-report-income";
    public const string ExpensesLabelName = "monthly-report-expenses";
    public const string NetLabelName = "monthly-report-net";
    public const string HeadlineContainerName = "monthly-report-headlines";
    private const string HeadlineRowClass = "monthly-report-headline";

    public static void Apply(VisualElement root, MonthlyReportViewModel viewModel)
    {
        if (root is null)
            throw new ArgumentNullException(nameof(root));

        SetLabel(root, IncomeLabelName, viewModel.IncomeLabel);
        SetLabel(root, ExpensesLabelName, viewModel.ExpensesLabel);
        SetLabel(root, NetLabelName, viewModel.NetLabel);

        var container = root.Q<VisualElement>(HeadlineContainerName);
        if (container is null)
            return;

        container.Clear();
        foreach (var headline in viewModel.Headlines)
        {
            var label = new Label(headline.Label) { name = $"monthly-report-headline-{headline.EventId}" };
            label.AddToClassList(HeadlineRowClass);
            container.Add(label);
        }
    }

    private static void SetLabel(VisualElement root, string name, string text)
    {
        var label = root.Q<Label>(name);
        if (label is not null)
            label.text = text;
    }
}
