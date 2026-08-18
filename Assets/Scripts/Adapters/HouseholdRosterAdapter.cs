#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Gens.Simulation.Queries;
using UnityEngine.UIElements;

namespace Gens.Presentation.Adapters;

/// <summary>One display-ready roster row (Phase 9 item 6's household roster screen).</summary>
public readonly record struct HouseholdRosterRowViewModel(string CharacterId, string DisplayName, string DisplaySubtitle);

/// <summary>Display-ready fields for the household roster screen's right leaf (<c>gens-core-design.md</c>
/// §7.4: "Familia individual record... navigable list").</summary>
public readonly record struct HouseholdRosterViewModel(IReadOnlyList<HouseholdRosterRowViewModel> Rows);

/// <summary>The only code allowed to turn a <see cref="HouseholdRosterProjection"/> into roster row
/// text, mirroring <see cref="CampaignClockAdapter"/>'s "pure translation, no UnityEngine" role.</summary>
public sealed class HouseholdRosterAdapter : IProjectionAdapter<HouseholdRosterProjection, HouseholdRosterViewModel>
{
    public HouseholdRosterViewModel Adapt(HouseholdRosterProjection projection) =>
        new(projection.Members.Select(Adapt).ToArray());

    private static HouseholdRosterRowViewModel Adapt(HouseholdRosterRow row) =>
        new(
            CharacterId: row.CharacterId,
            DisplayName: row.FullName,
            DisplaySubtitle: row.DutySlot is { } duty
                ? $"{row.AgeInYears} · {row.LegalStatus} · {duty}"
                : $"{row.AgeInYears} · {row.LegalStatus}");
}

/// <summary>Populates the roster screen's row container (<c>Assets/UI/HouseholdRosterScreen.uxml</c>)
/// from a <see cref="HouseholdRosterViewModel"/> — the concrete "UXML/USS-bound view model"
/// translation ADR 0013 reserves for Unity adapters. Each row is a clickable <see cref="Button"/> so
/// selecting a member is the roster screen's own path into the character detail screen (Phase 9 item
/// 6's four screens are meant to be navigable, not four static panels).</summary>
public static class HouseholdRosterBinding
{
    public const string RowContainerName = "roster-rows";
    private const string RowClass = "roster-row";
    private const string NameClass = "roster-row__name";
    private const string SubtitleClass = "roster-row__subtitle";

    public static void Apply(VisualElement root, HouseholdRosterViewModel viewModel, Action<string> onSelectCharacter)
    {
        if (root is null)
            throw new ArgumentNullException(nameof(root));
        if (onSelectCharacter is null)
            throw new ArgumentNullException(nameof(onSelectCharacter));

        var container = root.Q<VisualElement>(RowContainerName);
        if (container is null)
            return;

        container.Clear();
        foreach (var row in viewModel.Rows)
        {
            var button = new Button { name = $"roster-row-{row.CharacterId}" };
            button.AddToClassList(RowClass);
            var name = new Label(row.DisplayName);
            name.AddToClassList(NameClass);
            var subtitle = new Label(row.DisplaySubtitle);
            subtitle.AddToClassList(SubtitleClass);
            button.Add(name);
            button.Add(subtitle);
            button.clicked += () => onSelectCharacter(row.CharacterId);
            container.Add(button);
        }
    }
}
