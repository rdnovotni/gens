#nullable enable

using System;
using Gens.Simulation.Queries;
using UnityEngine.UIElements;

namespace Gens.Presentation.Adapters;

/// <summary>Display-ready fields for the persistent ink bar's date readout (Phase 9 item 6), built
/// from <see cref="CampaignClockProjection"/> rather than exposing its raw int/enum fields to UXML
/// data binding directly.</summary>
public readonly record struct CampaignClockViewModel(string DateLabel);

/// <summary>Worked example (mirrors <see cref="CampaignClockQuery"/>'s own "prove the pattern
/// compiles" role in the Queries layer): the only code allowed to turn a
/// <see cref="CampaignClockProjection"/> into UI Toolkit-facing text. Contains no <c>UnityEngine</c>
/// dependency, so the projection-to-view-model translation itself is testable without a
/// <see cref="VisualElement"/> tree; see <see cref="CampaignClockBinding"/> for the part that
/// actually touches UI Toolkit.</summary>
public sealed class CampaignClockAdapter : IProjectionAdapter<CampaignClockProjection, CampaignClockViewModel>
{
    public CampaignClockViewModel Adapt(CampaignClockProjection projection) =>
        new($"{projection.MonthOfYear:D2}/{projection.DisplayYear} {projection.Era}");
}

/// <summary>Applies a <see cref="CampaignClockViewModel"/> to a named <see cref="Label"/> inside an
/// already-loaded UXML hierarchy — the concrete "UXML/USS-bound view model" translation ADR 0013
/// reserves for Unity adapters, kept separate from <see cref="CampaignClockAdapter"/> so the pure
/// projection-to-view-model step stays engine-free.</summary>
public static class CampaignClockBinding
{
    public const string DateLabelName = "campaign-clock-date";

    public static void Apply(VisualElement root, CampaignClockViewModel viewModel)
    {
        if (root is null)
            throw new ArgumentNullException(nameof(root));

        var label = root.Q<Label>(DateLabelName);
        if (label is not null)
            label.text = viewModel.DateLabel;
    }
}
