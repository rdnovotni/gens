using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;

namespace Gens.Simulation.RealEstate;

/// <summary>§3's two genuinely new asset types Estate &amp; Settlement's Plot-based model doesn't
/// cover (Phase 15 item 1): a mobile <see cref="Ship"/>, not tied to any fixed Plot, and a lightweight
/// <see cref="NamedHolding"/> for a rival gens, temple, or collegium — sized for narrative and
/// negotiation purposes without needing Estate &amp; Settlement's own full building-chain simulation
/// behind it.</summary>
public enum PropertyAssetType
{
    Ship,
    NamedHolding,
}

/// <summary>
/// §3's Property Record, narrowed to exactly the two asset kinds this item actually needs a new
/// record for (<see cref="PropertyAssetType"/>'s own doc comment): a player-owned, Estate &amp;
/// Settlement-tracked villa/insula/tabernae/workshop/warehouse/farm continues to live entirely on its
/// own <see cref="Plot"/> (extended only by <see cref="PropertyManagementState"/>, a parallel sparse
/// partition, not a duplicate schema) — this record exists for the two asset kinds that have no Plot
/// to attach to at all.
/// </summary>
public sealed record PropertyRecord
{
    private PropertyRecord()
    {
    }

    public required RuntimeId<PropertyRecord> Id { get; init; }
    public required PropertyAssetType AssetType { get; init; }
    public required string Name { get; init; }
    public required PropertyOwnerRef Owner { get; init; }

    /// <summary>Null for a <see cref="PropertyAssetType.Ship"/> (§3: "mobile, not tied to a fixed
    /// plot"); required in practice for a <see cref="PropertyAssetType.NamedHolding"/>, which is
    /// always sited somewhere even though it has no Plot of its own.</summary>
    public RuntimeId<Settlement>? SettlementId { get; init; }

    public RuntimeId<District>? DistrictId { get; init; }

    public required PropertyManagementStatus ManagementStatus { get; init; }

    /// <summary>§6's assigned Operator — non-null only while <see cref="ManagementStatus"/> is <see
    /// cref="PropertyManagementStatus.LeasedOut"/>.</summary>
    public RuntimeId<Character>? OperatorCharacterId { get; init; }

    /// <summary>§6.1's skim state, resolved monthly by <see cref="OperatorLifecycleSystem"/> from the
    /// Operator's own Core Attributes/Loyalty — set only while an Operator is assigned.</summary>
    public bool OperatorIsSkimming { get; init; }

    /// <summary>How many consecutive months the current Operator has held this assignment — §6.1's "a
    /// decade" of steady tenure before a real buyout becomes plausible; reset to zero whenever the
    /// Operator changes.</summary>
    public int OperatorTenureMonths { get; init; }

    /// <summary>§6.1's "enough saved to offer a real buyout" — true once <see
    /// cref="OperatorLifecycleSystem"/> judges the current Operator both loyal and ambitious enough,
    /// over a long enough tenure, on a Property whose District Value is genuinely climbing. Cleared by
    /// <see cref="ResolveOperatorBuyoutCommand"/> either way (accepted or declined).</summary>
    public bool OperatorBuyoutOffered { get; init; }

    /// <summary>§5's <c>ager publicus</c> lease — the household actually using state-owned land without
    /// owning it (<see cref="PropertyTransferMethod.AgerPublicusLease"/>'s own doc comment). Mirrors
    /// <see cref="Plot.OccupyingHoldingId"/>'s identical "who's using it, independently of ownership"
    /// shape for the one owner kind (<see cref="PropertyOwnerKind.RomanState"/>) that is never
    /// bought outright.</summary>
    public RuntimeId<Household>? LesseeId { get; init; }

    public required Money Value { get; init; }

    /// <summary>§9's Property Value inputs read Condition alongside income history — 0-100, matching
    /// <see cref="LandCondition"/>'s identical scale so the two read the same way across this item's
    /// domain.</summary>
    public required LandCondition Condition { get; init; }

    public static PropertyRecord Create(
        RuntimeId<PropertyRecord> id,
        PropertyAssetType assetType,
        string name,
        PropertyOwnerRef owner,
        Money value,
        RuntimeId<Settlement>? settlementId = null,
        RuntimeId<District>? districtId = null,
        LandCondition? condition = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A PropertyRecord requires a non-empty name.", nameof(name));
        if (assetType == PropertyAssetType.NamedHolding && settlementId is null)
            throw new ArgumentException(
                "A Named Holding must be sited at a settlement even though it has no Plot of its own.", nameof(settlementId));
        if (assetType == PropertyAssetType.Ship && settlementId is not null)
            throw new ArgumentException("A Ship is mobile and cannot be sited at a fixed settlement.", nameof(settlementId));

        return new PropertyRecord
        {
            Id = id,
            AssetType = assetType,
            Name = name,
            Owner = owner,
            SettlementId = settlementId,
            DistrictId = districtId,
            ManagementStatus = PropertyManagementStatus.DirectlyManaged,
            OperatorCharacterId = null,
            OperatorIsSkimming = false,
            OperatorTenureMonths = 0,
            OperatorBuyoutOffered = false,
            LesseeId = null,
            Value = value,
            Condition = condition ?? LandCondition.Pristine,
        };
    }

    /// <summary>Reconstructs a <see cref="PropertyRecord"/> from persisted save data (ADR 0010).</summary>
    public static PropertyRecord Restore(
        RuntimeId<PropertyRecord> id,
        PropertyAssetType assetType,
        string name,
        PropertyOwnerRef owner,
        RuntimeId<Settlement>? settlementId,
        RuntimeId<District>? districtId,
        PropertyManagementStatus managementStatus,
        RuntimeId<Character>? operatorCharacterId,
        bool operatorIsSkimming,
        int operatorTenureMonths,
        bool operatorBuyoutOffered,
        RuntimeId<Household>? lesseeId,
        Money value,
        LandCondition condition) =>
        new()
        {
            Id = id,
            AssetType = assetType,
            Name = name,
            Owner = owner,
            SettlementId = settlementId,
            DistrictId = districtId,
            ManagementStatus = managementStatus,
            OperatorCharacterId = operatorCharacterId,
            OperatorIsSkimming = operatorIsSkimming,
            OperatorTenureMonths = operatorTenureMonths,
            OperatorBuyoutOffered = operatorBuyoutOffered,
            LesseeId = lesseeId,
            Value = value,
            Condition = condition,
        };
}
