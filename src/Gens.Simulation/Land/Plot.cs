using Gens.Simulation.Identity;

namespace Gens.Simulation.Land;

/// <summary>
/// The boundary record for an individual parcel of land within a settlement (Phase 6 item 1;
/// <c>gens-estate-settlement-design.md</c> §2). Each plot has a fixed terrain type that gates which
/// buildings can occupy it. Terrain type, contested state, building instance, ownership, and land
/// condition are additive fields deferred to Phase 6 item 2+, per ADR 0011's "additive only until
/// v1 ships" policy.
/// </summary>
public sealed record Plot
{
    private Plot()
    {
    }

    public required RuntimeId<Plot> Id { get; init; }

    /// <summary>The settlement this plot belongs to. Fixed at creation — a plot does not move
    /// between settlements.</summary>
    public required RuntimeId<Settlement> SettlementId { get; init; }

    /// <summary>The only supported way to construct a <see cref="Plot"/>.</summary>
    public static Plot Create(RuntimeId<Plot> id, RuntimeId<Settlement> settlementId)
    {
        return new Plot { Id = id, SettlementId = settlementId };
    }
}
