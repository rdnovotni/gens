#nullable enable

using System;
using System.Text;
using Gens.Simulation.Characters;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gens.Presentation.Adapters;

/// <summary>Display-ready fields for a character's placeholder/procedural portrait (Phase 9 item 8):
/// no raster or composited art exists yet (<see cref="PortraitRecipe"/> is deliberately just data), so
/// this is a deterministic stand-in — a medallion tint derived from the character's own <see
/// cref="PortraitRecipe.Layers"/> tokens, plus a two-letter monogram — rather than a placeholder shared
/// by every character alike.</summary>
public readonly record struct PortraitViewModel(string MonogramLabel, string BackgroundColorHex);

/// <summary>The only code allowed to turn a <see cref="CharacterVisualProfile"/> into portrait-medallion
/// display fields, mirroring <see cref="CampaignClockAdapter"/>'s "pure translation, no UnityEngine"
/// role.</summary>
public static class PortraitAdapter
{
    /// <summary>A small, curated set of muted medallion tints (kept within the ink-bar palette's own
    /// register, <c>gens-core-design.md</c> §7.2) rather than an arbitrary hue wheel, so a deterministic
    /// but data-driven choice among them still always looks intentional.</summary>
    private static readonly string[] MedallionPalette =
    {
        "#5C3350", // Tyrian Purple
        "#9C4B2E", // Terracotta Oxide
        "#6E8272", // Verdigris Bronze
        "#B9922E", // Gold Leaf
        "#7A6A9C", // muted violet, palette-adjacent
        "#4E6B57", // muted green, palette-adjacent
        "#8C5A3C", // muted clay, palette-adjacent
        "#5E5140", // muted olive-brown, palette-adjacent
    };

    public static PortraitViewModel Adapt(string fullName, CharacterVisualProfile visualProfile)
    {
        if (visualProfile is null)
            throw new ArgumentNullException(nameof(visualProfile));

        var seed = string.Join('|', visualProfile.Portrait.Layers);
        var tint = MedallionPalette[(int)(DeterministicHash(seed) % (uint)MedallionPalette.Length)];
        return new PortraitViewModel(MonogramLabel: Monogram(fullName), BackgroundColorHex: tint);
    }

    private static string Monogram(string fullName)
    {
        var words = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length switch
        {
            0 => "—",
            1 => words[0][..1].ToUpperInvariant(),
            _ => (words[0][..1] + words[^1][..1]).ToUpperInvariant(),
        };
    }

    /// <summary>A plain FNV-1a hash over UTF-8 bytes — deterministic across processes and .NET
    /// versions, unlike <see cref="string.GetHashCode()"/>, which is randomized per process by design.
    /// </summary>
    private static uint DeterministicHash(string value)
    {
        const uint offsetBasis = 2166136261;
        const uint prime = 16777619;

        var hash = offsetBasis;
        foreach (var b in Encoding.UTF8.GetBytes(value))
        {
            hash ^= b;
            hash *= prime;
        }

        return hash;
    }
}

/// <summary>Applies a <see cref="PortraitViewModel"/> to the character detail screen's portrait
/// medallion (<c>Assets/UI/CharacterDetailScreen.uxml</c>) — the concrete "UXML/USS-bound view model"
/// translation ADR 0013 reserves for Unity adapters.</summary>
public static class PortraitBinding
{
    public const string FrameElementName = "character-detail-portrait";
    public const string MonogramLabelName = "character-detail-portrait-monogram";

    public static void Apply(VisualElement root, PortraitViewModel viewModel)
    {
        if (root is null)
            throw new ArgumentNullException(nameof(root));

        var frame = root.Q<VisualElement>(FrameElementName);
        if (frame is not null && ColorUtility.TryParseHtmlString(viewModel.BackgroundColorHex, out var color))
            frame.style.backgroundColor = color;

        var monogram = root.Q<Label>(MonogramLabelName);
        if (monogram is not null)
            monogram.text = viewModel.MonogramLabel;
    }
}
