using Gens.Simulation.Identity;

namespace Gens.Simulation.Land;

/// <summary>
/// The boundary record for a geographic region (Phase 6 item 1; <c>gens-estate-settlement-design.md</c>
/// §6). A region provides fixed bonuses and gates availability of certain building chains and terrain
/// types — those fields belong to a later phase pass. This record carries only what other entities
/// need to reference: a stable <see cref="RuntimeId{T}"/> and a human-readable name. Terrain bonuses,
/// penalty tables, and gated-chain availability are additive fields deferred per ADR 0011's
/// "additive only until v1 ships" policy.
/// </summary>
public sealed record Region
{
    private Region()
    {
    }

    public required RuntimeId<Region> Id { get; init; }

    /// <summary>The human-readable name for this region (e.g. "Italian Heartland", "Gallic Frontier").
    /// Content-authored and never empty.</summary>
    public required string Name { get; init; }

    /// <summary>The only supported way to construct a <see cref="Region"/>.</summary>
    public static Region Create(RuntimeId<Region> id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A Region requires a non-empty name.", nameof(name));

        return new Region { Id = id, Name = name };
    }
}
