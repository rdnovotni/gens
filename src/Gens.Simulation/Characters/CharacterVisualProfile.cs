namespace Gens.Simulation.Characters;

/// <summary>
/// The fixed-genetics "Detailed/Body Attributes" every Character carries (<c>gens-familia-design.md</c>
/// §2.4, Core §7.11) — "the same dataset the Appearance system already defines... height and build,
/// facial structure, complexion, hair/eye color and style, notable features" — plus the <see
/// cref="PortraitRecipe"/> generated from them. Roadmap Phase 5 item 2. Status-appropriate
/// dress/grooming, the player-facing Description text, and the actual rendered image are all later
/// work (Fashion & Dress, the Paper Doll design) layered on top of this fixed base.
/// </summary>
public sealed record CharacterVisualProfile
{
    public required Height Height { get; init; }
    public required Build Build { get; init; }
    public required FacialStructure FacialStructure { get; init; }
    public required Complexion Complexion { get; init; }
    public required HairColor HairColor { get; init; }
    public required HairStyle HairStyle { get; init; }
    public required EyeColor EyeColor { get; init; }
    public required IReadOnlyList<NotableFeature> NotableFeatures { get; init; }
    public required PortraitRecipe Portrait { get; init; }

    /// <summary>Hand-written for the same reason as <see cref="PortraitRecipe.Equals(PortraitRecipe?)"/>:
    /// the record-synthesized <c>Equals</c> would compare <see cref="NotableFeatures"/> by list
    /// reference rather than by content.</summary>
    public bool Equals(CharacterVisualProfile? other) =>
        other is not null &&
        Height == other.Height &&
        Build == other.Build &&
        FacialStructure == other.FacialStructure &&
        Complexion == other.Complexion &&
        HairColor == other.HairColor &&
        HairStyle == other.HairStyle &&
        EyeColor == other.EyeColor &&
        NotableFeatures.SequenceEqual(other.NotableFeatures) &&
        Portrait.Equals(other.Portrait);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Height);
        hash.Add(Build);
        hash.Add(FacialStructure);
        hash.Add(Complexion);
        hash.Add(HairColor);
        hash.Add(HairStyle);
        hash.Add(EyeColor);
        foreach (var feature in NotableFeatures)
            hash.Add(feature);
        hash.Add(Portrait);
        return hash.ToHashCode();
    }
}
