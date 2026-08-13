namespace Gens.Simulation.Characters;

/// <summary>Current hairstyle (<c>gens-familia-design.md</c> §2.4 / Core §7.11). A minimal, generated
/// starting value — the full hairstyle catalog and its player-chosen changes belong to the later Hair,
/// Facial Hair & Body Marking design's own scope, not this item.</summary>
public enum HairStyle
{
    Cropped,
    ShoulderLength,
    // Not "Long": CA1720 flags an identifier that collides with the `long` (Int64) keyword alias.
    Flowing,
    BoundUp,
}
