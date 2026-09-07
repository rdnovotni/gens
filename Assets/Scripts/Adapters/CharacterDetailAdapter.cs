#nullable enable

using System;
using System.Linq;
using Gens.Simulation.Queries;
using UnityEngine.UIElements;

namespace Gens.Presentation.Adapters;

/// <summary>Display-ready fields for the character detail screen (Phase 9 item 6;
/// <c>gens-core-design.md</c> §7.4's "Familia individual record" diptych: identity leaf plus
/// stat-gauge leaf).</summary>
public readonly record struct CharacterDetailViewModel(
    string NameLabel,
    string SubtitleLabel,
    string AttributesLabel,
    string SkillsLabel,
    string ConditionLabel);

/// <summary>The only code allowed to turn a <see cref="CharacterDetailProjection"/> into
/// character-sheet text, mirroring <see cref="CampaignClockAdapter"/>'s "pure translation, no
/// UnityEngine" role.</summary>
public sealed class CharacterDetailAdapter : IProjectionAdapter<CharacterDetailProjection, CharacterDetailViewModel>
{
    public CharacterDetailViewModel Adapt(CharacterDetailProjection projection)
    {
        var shared = global::Gens.Presentation.ProjectionMappers.CharacterDetail(projection);
        return new CharacterDetailViewModel(
            shared.Name, shared.Subtitle,
            string.Join(" · ", shared.Attributes.Select(static x => $"{x.Label} {x.Value}")),
            string.Join(" · ", shared.Skills.Select(static x => $"{x.Label} {x.Value}")),
            string.Join(" · ", shared.Condition.Select(static x => $"{x.Label} {x.Value}")));
    }
}

/// <summary>Applies a <see cref="CharacterDetailViewModel"/> to the named <see cref="Label"/>s inside
/// an already-loaded character-detail UXML hierarchy (<c>Assets/UI/CharacterDetailScreen.uxml</c>) —
/// the concrete "UXML/USS-bound view model" translation ADR 0013 reserves for Unity adapters.</summary>
public static class CharacterDetailBinding
{
    public const string NameLabelName = "character-detail-name";
    public const string SubtitleLabelName = "character-detail-subtitle";
    public const string AttributesLabelName = "character-detail-attributes";
    public const string SkillsLabelName = "character-detail-skills";
    public const string ConditionLabelName = "character-detail-condition";

    public static void Apply(VisualElement root, CharacterDetailViewModel viewModel)
    {
        if (root is null)
            throw new ArgumentNullException(nameof(root));

        SetLabel(root, NameLabelName, viewModel.NameLabel);
        SetLabel(root, SubtitleLabelName, viewModel.SubtitleLabel);
        SetLabel(root, AttributesLabelName, viewModel.AttributesLabel);
        SetLabel(root, SkillsLabelName, viewModel.SkillsLabel);
        SetLabel(root, ConditionLabelName, viewModel.ConditionLabel);
    }

    private static void SetLabel(VisualElement root, string name, string text)
    {
        var label = root.Q<Label>(name);
        if (label is not null)
            label.text = text;
    }
}
