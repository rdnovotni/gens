using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Numerics;
using Gens.Simulation.Regions;

namespace Gens.Simulation.RealEstate;

/// <summary>
/// §4's District — a settlement subdivision resolving "urban geography matters" at the district tier
/// rather than the individual-building tier (Phase 15 item 1; <c>gens-land-ownership-real-estate-
/// design.md</c> §4). A small number of named Districts per settlement, scaling with <see
/// cref="SettlementStage"/> (<see cref="RealEstateCatalog.MaxDistrictsForStage"/>) — a Vicus might
/// have just one, a full City four or five, matching §4's own worked example directly.
/// </summary>
public sealed record District
{
    private District()
    {
    }

    public required RuntimeId<District> Id { get; init; }
    public required RuntimeId<Settlement> SettlementId { get; init; }
    public required string Name { get; init; }

    /// <summary>§4's Property Value trend — a <see cref="Fixed64"/> index around <see
    /// cref="Fixed64.One"/> (the baseline "nothing pulling it up or down yet" reading), moved monthly
    /// by <see cref="DistrictPropertyValueSystem"/> from real, already-tracked inputs: Settlement
    /// Demographics' population/Contentment, Natural Disaster damage, and (where <see
    /// cref="LinkedGazetteerLocationId"/> resolves) a region's Gazetteer Prominence Tier. Monuments
    /// (§4's fourth named input) is not wired — Monuments &amp; Legacy Building is Phase 17, unbuilt
    /// (confirmed by direct search); see <see cref="DistrictPropertyValueSystem"/>'s own doc comment
    /// for the honest accounting of this gap.</summary>
    public required Fixed64 PropertyValue { get; init; }

    /// <summary>§4/§12's "a real input into District Property Value wherever a settlement coincides
    /// with a named Gazetteer entry" — null for the (overwhelmingly common) settlement that is not
    /// itself a region's own curated Gazetteer entry.</summary>
    public DefinitionId<GazetteerLocationDefinition>? LinkedGazetteerLocationId { get; init; }

    /// <summary>The settlement's own total population the last time <see
    /// cref="DistrictPropertyValueSystem"/> ran — this item's own invented state for computing a
    /// month-over-month population-growth reading (§4's "population... trends") without needing a
    /// separate population-history partition. Zero until the system's own first tick.</summary>
    public int PreviousSettlementPopulation { get; init; }

    public static District Create(
        RuntimeId<District> id,
        RuntimeId<Settlement> settlementId,
        string name,
        DefinitionId<GazetteerLocationDefinition>? linkedGazetteerLocationId = null,
        Fixed64? propertyValue = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A District requires a non-empty name.", nameof(name));

        return new District
        {
            Id = id,
            SettlementId = settlementId,
            Name = name,
            LinkedGazetteerLocationId = linkedGazetteerLocationId,
            PropertyValue = propertyValue ?? Fixed64.One,
        };
    }

    /// <summary>Reconstructs a <see cref="District"/> from persisted save data (ADR 0010).</summary>
    public static District Restore(
        RuntimeId<District> id,
        RuntimeId<Settlement> settlementId,
        string name,
        Fixed64 propertyValue,
        DefinitionId<GazetteerLocationDefinition>? linkedGazetteerLocationId,
        int previousSettlementPopulation) =>
        new()
        {
            Id = id,
            SettlementId = settlementId,
            Name = name,
            PropertyValue = propertyValue,
            LinkedGazetteerLocationId = linkedGazetteerLocationId,
            PreviousSettlementPopulation = previousSettlementPopulation,
        };
}
