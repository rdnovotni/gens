using Gens.Simulation.Identity;
using Gens.Simulation.Villas;

namespace Gens.Simulation.Land;

/// <summary>
/// The boundary record for a holding — the physical property/estate a household occupies (Phase 6
/// item 1; <c>gens-villa-design.md</c> §1). A holding is the parent layer for villa stage and room
/// instances (Phase 6 item 5). Ownership and occupancy are deliberately distinct so leased or
/// delegated property does not lose its legal owner.
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

    public string? OwnerId { get; init; }
    public string? OccupantId { get; init; }

    /// <summary>Maximum number of residents supported before later housing systems apply pressure.</summary>
    public required int ResidentCapacity { get; init; }

    /// <summary>The holding's optional residence. Production buildings remain plot-level objects;
    /// villa rooms are owned by this specialized interior layer.</summary>
    public Villa? Villa { get; init; }

    /// <summary>The only supported way to construct a <see cref="Holding"/>.</summary>
    public static Holding Create(
        RuntimeId<Holding> id,
        RuntimeId<Settlement> settlementId,
        string? ownerId = null,
        string? occupantId = null,
        int residentCapacity = 1,
        Villa? villa = null)
    {
        if (residentCapacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(residentCapacity), residentCapacity, "Resident capacity must be positive.");
        if (ownerId is not null && string.IsNullOrWhiteSpace(ownerId))
            throw new ArgumentException("An owner ID cannot be empty.", nameof(ownerId));
        if (occupantId is not null && string.IsNullOrWhiteSpace(occupantId))
            throw new ArgumentException("An occupant ID cannot be empty.", nameof(occupantId));

        return new Holding
        {
            Id = id,
            SettlementId = settlementId,
            OwnerId = ownerId,
            OccupantId = occupantId,
            ResidentCapacity = residentCapacity,
            Villa = villa,
        };
    }
}
