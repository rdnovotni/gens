using Gens.Simulation.Identity;

namespace Gens.Simulation.Regions;

/// <summary>One entry in a region's Gazetteer (<c>gens-starting-regions-design.md</c> §8.2, §12): a
/// real, historically-grounded place — Narbo Martius, Tarraco, Ephesus — with enough identity that
/// Travel, Politics &amp; Patronage, Religion, Diplomacy, and Espionage can transact with it. Not a
/// settlement the player builds in or owns (§8.4) — that is <see cref="Gens.Simulation.Land.Settlement"/>'s
/// own, entirely separate, entirely abstract track.</summary>
public sealed record GazetteerLocationDefinition
{
    public GazetteerLocationDefinition(
        DefinitionId<GazetteerLocationDefinition> id,
        DefinitionId<RegionProfileDefinition> regionId,
        string name,
        IReadOnlyList<GazetteerRole> roles,
        ProminenceTier prominenceTier,
        string groundingNote,
        string? rivalSeatHouseId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A gazetteer location requires a non-empty name.", nameof(name));
        if (string.IsNullOrWhiteSpace(groundingNote))
            throw new ArgumentException("A gazetteer location requires a non-empty grounding note.", nameof(groundingNote));
        if (roles is null || roles.Count == 0)
            throw new ArgumentException("A gazetteer location requires at least one role.", nameof(roles));
        if (roles.Distinct().Count() != roles.Count)
            throw new ArgumentException("A gazetteer location's roles must not repeat.", nameof(roles));

        Id = id;
        RegionId = regionId;
        Name = name;
        Roles = roles;
        ProminenceTier = prominenceTier;
        GroundingNote = groundingNote;
        RivalSeatHouseId = rivalSeatHouseId;
    }

    public DefinitionId<GazetteerLocationDefinition> Id { get; }

    /// <summary>The region this entry belongs to — validated against the owning <see
    /// cref="RegionProfileDefinition.Id"/> at region-construction time (§12's <c>regionId</c> field).</summary>
    public DefinitionId<RegionProfileDefinition> RegionId { get; }

    public string Name { get; }
    public IReadOnlyList<GazetteerRole> Roles { get; }
    public ProminenceTier ProminenceTier { get; }
    public string GroundingNote { get; }

    /// <summary>Nullable — §9.1's pre-filled or procedural rival house seated here, if any. A free-form
    /// content-authored tag rather than a typed reference: Rival Houses' own seeding (§9) is item 6/9
    /// territory, not this schema's.</summary>
    public string? RivalSeatHouseId { get; }
}
