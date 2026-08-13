using Gens.Simulation.Identity;

namespace Gens.Simulation.Land;

/// <summary>
/// The boundary record for a settlement within a region (Phase 6 item 1;
/// <c>gens-estate-settlement-design.md</c> §5). A settlement grows through four stages (Villa →
/// Vicus → Town → City) and contains a set of plots. Ownership, terrain features, goods storage,
/// economic identity, and population are additive fields deferred to later Phase 6 passes, per ADR
/// 0011's "additive only until v1 ships" policy.
/// </summary>
public sealed record Settlement
{
    private Settlement()
    {
    }

    public required RuntimeId<Settlement> Id { get; init; }

    /// <summary>The region this settlement belongs to. Fixed at creation — a settlement does not
    /// move between regions.</summary>
    public required RuntimeId<Region> RegionId { get; init; }

    /// <summary>Current growth stage (<c>gens-estate-settlement-design.md</c> §5). Starts at
    /// <see cref="SettlementStage.Villa"/> and advances only through deliberate stage-transition
    /// actions gated on population and construction — never incremented automatically.</summary>
    public required SettlementStage Stage { get; init; }

    /// <summary>The only supported way to construct a <see cref="Settlement"/>.</summary>
    public static Settlement Create(
        RuntimeId<Settlement> id,
        RuntimeId<Region> regionId,
        SettlementStage stage = SettlementStage.Villa)
    {
        return new Settlement { Id = id, RegionId = regionId, Stage = stage };
    }
}
