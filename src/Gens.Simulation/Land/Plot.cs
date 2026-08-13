using Gens.Simulation.Identity;

namespace Gens.Simulation.Land;

/// <summary>
/// The boundary record for an individual parcel of land within a settlement (Phase 6 item 1;
/// <c>gens-estate-settlement-design.md</c> §2). Each plot has a fixed terrain type that gates which
/// buildings can occupy it. Terrain type, contested state, building instance, ownership, and land
/// condition are explicit state; later building work consumes these gates without replacing them.
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

    public required TerrainType Terrain { get; init; }
    public required TerrainFeature Features { get; init; }
    public required LandCondition Condition { get; init; }

    /// <summary>Number of building-space units available on this parcel. Buildings consume these
    /// units in Phase 6 item 4.</summary>
    public required int Capacity { get; init; }

    /// <summary>A tagged runtime/content owner ID. Kept polymorphic because households, characters,
    /// civic bodies, temples, and partnerships may all own land.</summary>
    public string? OwnerId { get; init; }

    /// <summary>The holding currently occupying/managing this parcel, independently of ownership.</summary>
    public RuntimeId<Holding>? OccupyingHoldingId { get; init; }

    public bool IsContested { get; init; }
    public LandAcquisition? Acquisition { get; init; }

    /// <summary>The only supported way to construct a <see cref="Plot"/>.</summary>
    public static Plot Create(
        RuntimeId<Plot> id,
        RuntimeId<Settlement> settlementId,
        TerrainType terrain = TerrainType.FertilePlain,
        TerrainFeature features = TerrainFeature.None,
        LandCondition? condition = null,
        int capacity = 1,
        bool isContested = false,
        string? ownerId = null,
        RuntimeId<Holding>? occupyingHoldingId = null,
        LandAcquisition? acquisition = null)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Plot capacity must be positive.");
        if (ownerId is not null && string.IsNullOrWhiteSpace(ownerId))
            throw new ArgumentException("An owner ID cannot be empty.", nameof(ownerId));
        if (ownerId is null && acquisition is not null)
            throw new ArgumentException("An unowned plot cannot have acquisition provenance.", nameof(acquisition));

        return new Plot
        {
            Id = id,
            SettlementId = settlementId,
            Terrain = terrain,
            Features = features,
            Condition = condition ?? LandCondition.Pristine,
            Capacity = capacity,
            IsContested = isContested,
            OwnerId = ownerId,
            OccupyingHoldingId = occupyingHoldingId,
            Acquisition = acquisition,
        };
    }
}
