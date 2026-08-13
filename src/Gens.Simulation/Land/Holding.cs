using Gens.Simulation.Identity;

namespace Gens.Simulation.Land;

/// <summary>
/// The boundary record for a holding — the physical property/estate a household occupies (Phase 6
/// item 1; <c>gens-villa-design.md</c> §1). A holding is the parent layer for villa stage and room
/// instances (Phase 6 item 5). Ownership, villa stage, rooms, grandeur score, and the outpost flag
/// are additive fields deferred to Phase 6 item 5, per ADR 0011's "additive only until v1 ships"
/// policy.
/// </summary>
public sealed record Holding
{
    private Holding()
    {
    }

    public required RuntimeId<Holding> Id { get; init; }

    /// <summary>The settlement this holding is located in. Fixed at creation — a holding does not
    /// relocate between settlements.</summary>
    public required RuntimeId<Settlement> SettlementId { get; init; }

    /// <summary>The only supported way to construct a <see cref="Holding"/>.</summary>
    public static Holding Create(RuntimeId<Holding> id, RuntimeId<Settlement> settlementId)
    {
        return new Holding { Id = id, SettlementId = settlementId };
    }
}
