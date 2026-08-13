namespace Gens.Simulation.Characters;

/// <summary>An ordered list of semantic layer tokens (e.g. <c>"body/build/slight"</c>,
/// <c>"feature/scar"</c>) a future rendering layer resolves against content-authored art (Core §7.11's
/// "rendered composite... so dozens of household members can each have a distinct, consistent likeness
/// without hand-drawing every one"). This is the "procedural portrait recipe" the roadmap's Phase 5
/// item 2 calls for — deliberately just data, with no raster art required to exist yet.</summary>
public sealed record PortraitRecipe
{
    public required IReadOnlyList<string> Layers { get; init; }

    /// <summary>Hand-written rather than record-synthesized: the default field-wise <c>Equals</c>
    /// would compare <see cref="Layers"/> by reference, which breaks the moment a save round trip (or
    /// any other rebuild) produces an equal-but-distinct list instance.</summary>
    public bool Equals(PortraitRecipe? other) =>
        other is not null && Layers.SequenceEqual(other.Layers, StringComparer.Ordinal);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var layer in Layers)
            hash.Add(layer, StringComparer.Ordinal);
        return hash.ToHashCode();
    }
}
