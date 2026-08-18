#nullable enable

using System;
using Gens.Simulation.Queries;
using UnityEngine.UIElements;

namespace Gens.Presentation.Adapters;

/// <summary>Display-ready fields for the persistent ink bar (Phase 9 item 6;
/// <c>gens-core-design.md</c> §7.4's "INK BAR — gens name · date/season · treasury · dignitas"),
/// present at the top of every screen. Built from <see cref="InkBarProjection"/> rather than exposing
/// its raw <see cref="Ledger.Money"/>/int fields to UXML data binding directly.</summary>
public readonly record struct InkBarViewModel(
    string GensNameLabel,
    string DateLabel,
    string TreasuryLabel,
    string DignitasLabel);

/// <summary>The only code allowed to turn an <see cref="InkBarProjection"/> into ink-bar text,
/// mirroring <see cref="CampaignClockAdapter"/>'s identical role for the narrower campaign-clock
/// projection. No <c>UnityEngine</c> dependency, so this translation is testable without a <see
/// cref="VisualElement"/> tree; see <see cref="InkBarBinding"/> for the part that touches UI
/// Toolkit.</summary>
public sealed class InkBarAdapter : IProjectionAdapter<InkBarProjection, InkBarViewModel>
{
    public InkBarViewModel Adapt(InkBarProjection projection) =>
        new(
            GensNameLabel: projection.GensName,
            DateLabel: $"{projection.MonthOfYear:D2}/{projection.DisplayYear} {projection.Era}",
            TreasuryLabel: $"{projection.Treasury.ToDisplayString()} denarii",
            DignitasLabel: $"{projection.Dignitas} dignitas");
}

/// <summary>Applies an <see cref="InkBarViewModel"/> to the named <see cref="Label"/>s inside an
/// already-loaded ink-bar UXML hierarchy (<c>Assets/UI/InkBar.uxml</c>) — the concrete "UXML/USS-bound
/// view model" translation ADR 0013 reserves for Unity adapters.</summary>
public static class InkBarBinding
{
    public const string GensNameLabelName = "ink-bar-gens-name";
    public const string DateLabelName = "ink-bar-date";
    public const string TreasuryLabelName = "ink-bar-treasury";
    public const string DignitasLabelName = "ink-bar-dignitas";

    public static void Apply(VisualElement root, InkBarViewModel viewModel)
    {
        if (root is null)
            throw new ArgumentNullException(nameof(root));

        SetLabel(root, GensNameLabelName, viewModel.GensNameLabel);
        SetLabel(root, DateLabelName, viewModel.DateLabel);
        SetLabel(root, TreasuryLabelName, viewModel.TreasuryLabel);
        SetLabel(root, DignitasLabelName, viewModel.DignitasLabel);
    }

    private static void SetLabel(VisualElement root, string name, string text)
    {
        var label = root.Q<Label>(name);
        if (label is not null)
            label.text = text;
    }
}
