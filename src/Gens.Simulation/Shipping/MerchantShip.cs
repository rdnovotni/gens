using Gens.Simulation.Characters;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Societates;
using Gens.Simulation.State;

namespace Gens.Simulation.Shipping;

/// <summary>§2's full Ship Registry (Phase 15 item 8; <c>gens-private-ships-shipping-ventures-design.md</c>
/// §2), every real vessel class the design doc names — matching <see cref="RealEstate.PropertyOwnerRef"/>'s
/// and <see cref="Legal.LegalCase.CaseType"/>'s own identical "every real category represented"
/// precedent. All fourteen are reachable through <see cref="Shipping.ShippingCatalog.CapacityTierFor"/>
/// and <see cref="CommissionShipCommand"/> — unlike some of those precedents, this item does not narrow
/// the registry itself, only how much distinct mechanical behavior sits behind each entry (see <see
/// cref="Shipping.ShippingCatalog"/>'s own doc comment for exactly which per-class effects are real:
/// capacity tier and Storm resistance, not a bespoke stat block per class).</summary>
public enum ShipVesselClass
{
    // §2.1 general cargo classes.
    NavisCaudicaria,
    Corbita,
    GrainCarrier,
    PunicTrader,
    AegeanMerchantman,
    GallicBritannicCoaster,
    RedSeaNabataeanTrader,

    // §2.2 specialized classes, new this pass.
    Liburnian,
    Actuaria,
    Ponto,
    Hippago,
    NileRiverboat,
    PonticGrainTrader,
    PersonalPleasureBarge,
}

/// <summary>§11's data model <c>capacityTier</c>, "read from vesselClass" rather than chosen
/// independently — see <see cref="Shipping.ShippingCatalog.CapacityTierFor"/>.</summary>
public enum ShipCapacityTier
{
    None,
    Low,
    Standard,
    High,
}

/// <summary>§5's three real ownership shapes. <see cref="MerchantShip.ActualOwnerHouseholdId"/> is
/// always populated regardless of which of these applies, per §11's own "the real beneficial owner —
/// always tracked, regardless of ownerType."</summary>
public enum ShipOwnershipMode
{
    Sole,
    Societas,
    Fronted,
}

/// <summary>§11's data model <c>status</c>, every real terminal and non-terminal state the design doc
/// names. <see cref="LostToPiracy"/> and <see cref="Captured"/> are real, reachable values no system in
/// this item ever produces — Piracy &amp; Banditry is Phase 16, confirmed unbuilt by direct search (see
/// <see cref="Shipping.ShipVoyageRiskSystem"/>'s own doc comment) — named here so a save or a future
/// item can already store one rather than needing a save-breaking enum change once that system
/// ships.</summary>
public enum ShipStatus
{
    Active,
    Damaged,
    PresumedLost,
    LostToStorm,
    LostToPiracy,
    Captured,
    Retired,
    Sold,
}

/// <summary>§9's two real, distinct reputation sources, kept separate from <see
/// cref="MerchantShip.BlessedLaunch"/> per that section's own "worth keeping separate rather than
/// merging into one generic score." Sticky once set — this item builds no reversal path from either
/// tier back to <see cref="None"/> or to each other, the same "name the gap, don't fabricate the
/// unspecified branch" discipline <see cref="PrivateInfrastructure.LandReclamationProject"/>'s own
/// no-continuation-past-resolution precedent already established.</summary>
public enum ShipReputationTier
{
    None,
    LuckyShip,
    BadReputation,
}

/// <summary>
/// §11's <c>MerchantShip</c> data model (Phase 15 item 8) — a real, persistent, ownable vessel record,
/// the concrete asset class §1 says "no individual merchant vessel has ever actually existed as" until
/// this item. Reuses <see cref="Land.LandCondition"/>'s own 0-100 scale directly on the record (§7:
/// "ages and accumulates wear the same way an Estate &amp; Settlement building does"), matching <see
/// cref="RealEstate.PropertyRecord.Condition"/>'s identical reuse rather than <see
/// cref="PrivateInfrastructure.InfrastructureCondition"/>'s separate keyed partition — a Ship, like a
/// Property Record, is already a single owned record with nowhere else its own condition could live.
/// </summary>
public sealed record MerchantShip
{
    private MerchantShip()
    {
    }

    public required RuntimeId<MerchantShip> Id { get; init; }
    public required string Name { get; init; }
    public required ShipVesselClass VesselClass { get; init; }
    public required GoodQuality BuildQuality { get; init; }
    public required ShipOwnershipMode OwnershipMode { get; init; }
    public required RuntimeId<Household> ActualOwnerHouseholdId { get; init; }

    /// <summary>Set only while <see cref="OwnershipMode"/> is <see cref="ShipOwnershipMode.Societas"/>.
    /// A real, validated tie to <see cref="Societates.Societas"/> (item 2's own real record, not a
    /// placeholder) — this item does not additionally extend <see
    /// cref="Societates.PropertySubjectRef"/>/<see cref="Societas.LinkedPropertySubject"/> to point
    /// back at a Ship; that reverse link is Societates' own domain and this item's Ship-to-Societas
    /// direction is real without it.</summary>
    public RuntimeId<Societas>? OwningSocietasId { get; init; }

    public required bool IsFlagship { get; init; }
    public required bool BlessedLaunch { get; init; }
    public required ShipReputationTier ReputationTier { get; init; }
    public required int VoyagesCompleted { get; init; }

    /// <summary>How many consecutive discrete Voyage Events this Ship has resolved as <see
    /// cref="VoyageOutcome.Damaged"/> — this item's own real, live counter driving §9's "conversely, a
    /// Ship that's suffered repeated bad voyages can just as easily earn the opposite reputation," not
    /// itself named in §11's own data model sketch but needed to make that half of §9 a real, testable
    /// mechanism rather than a stored, unconsumed flag.</summary>
    public required int ConsecutiveBadOutcomes { get; init; }

    public required ShipStatus Status { get; init; }
    public required LandCondition Condition { get; init; }
    public RuntimeId<Character>? NavarchusId { get; init; }
    public required RuntimeId<Settlement> HomeSettlementId { get; init; }
    public RuntimeId<Economy.StandingContract>? AssignedTradeRouteId { get; init; }
    public RuntimeId<Economy.DebtRecord>? FenusNauticumRecordId { get; init; }

    public static MerchantShip Create(
        RuntimeId<MerchantShip> id,
        string name,
        ShipVesselClass vesselClass,
        GoodQuality buildQuality,
        ShipOwnershipMode ownershipMode,
        RuntimeId<Household> actualOwnerHouseholdId,
        RuntimeId<Settlement> homeSettlementId,
        RuntimeId<Societas>? owningSocietasId,
        bool blessedLaunch) => new()
        {
            Id = id,
            Name = name,
            VesselClass = vesselClass,
            BuildQuality = buildQuality,
            OwnershipMode = ownershipMode,
            ActualOwnerHouseholdId = actualOwnerHouseholdId,
            OwningSocietasId = owningSocietasId,
            IsFlagship = false,
            BlessedLaunch = blessedLaunch,
            ReputationTier = ShipReputationTier.None,
            VoyagesCompleted = 0,
            ConsecutiveBadOutcomes = 0,
            Status = ShipStatus.Active,
            Condition = new LandCondition(ShippingCatalog.StartingCondition(buildQuality)),
            NavarchusId = null,
            HomeSettlementId = homeSettlementId,
            AssignedTradeRouteId = null,
            FenusNauticumRecordId = null,
        };

    /// <summary>Reconstructs a <see cref="MerchantShip"/> from persisted save data (ADR 0010).</summary>
    public static MerchantShip Restore(
        RuntimeId<MerchantShip> id,
        string name,
        ShipVesselClass vesselClass,
        GoodQuality buildQuality,
        ShipOwnershipMode ownershipMode,
        RuntimeId<Household> actualOwnerHouseholdId,
        RuntimeId<Societas>? owningSocietasId,
        bool isFlagship,
        bool blessedLaunch,
        ShipReputationTier reputationTier,
        int voyagesCompleted,
        int consecutiveBadOutcomes,
        ShipStatus status,
        LandCondition condition,
        RuntimeId<Character>? navarchusId,
        RuntimeId<Settlement> homeSettlementId,
        RuntimeId<Economy.StandingContract>? assignedTradeRouteId,
        RuntimeId<Economy.DebtRecord>? fenusNauticumRecordId) => new()
        {
            Id = id,
            Name = name,
            VesselClass = vesselClass,
            BuildQuality = buildQuality,
            OwnershipMode = ownershipMode,
            ActualOwnerHouseholdId = actualOwnerHouseholdId,
            OwningSocietasId = owningSocietasId,
            IsFlagship = isFlagship,
            BlessedLaunch = blessedLaunch,
            ReputationTier = reputationTier,
            VoyagesCompleted = voyagesCompleted,
            ConsecutiveBadOutcomes = consecutiveBadOutcomes,
            Status = status,
            Condition = condition,
            NavarchusId = navarchusId,
            HomeSettlementId = homeSettlementId,
            AssignedTradeRouteId = assignedTradeRouteId,
            FenusNauticumRecordId = fenusNauticumRecordId,
        };
}

/// <summary>Read/write helpers over <see cref="WorldState.MerchantShips"/>, matching <see
/// cref="RealEstate.PlotPropertyResolver"/>'s identical "remove then re-add" convention.</summary>
public static class MerchantShipResolver
{
    public static bool TryGetCurrent(WorldState state, RuntimeId<MerchantShip> shipId, out MerchantShip ship)
    {
        if (state.MerchantShips.TryGet(shipId, out var entry))
        {
            ship = entry!;
            return true;
        }

        ship = null!;
        return false;
    }

    public static void Set(WorldState state, MerchantShip ship)
    {
        if (state.MerchantShips.TryGet(ship.Id, out _))
            state.MerchantShips.Remove(ship.Id);
        state.MerchantShips.Add(ship.Id, ship);
    }
}

/// <summary>§1's <c>MerchantMarine</c> — "a household's own collection of owned Ships," computed rather
/// than stored, matching <see cref="PrivateInfrastructure.RoadClusterQuery"/>'s and <see
/// cref="MerchantFamilies.EquestrianStatusQuery"/>'s own identical "computed, not stored" precedent
/// rather than a redundantly-maintained <c>shipIds</c>/<c>flagshipId</c> list that could drift from the
/// real <see cref="MerchantShip.ActualOwnerHouseholdId"/>/<see cref="MerchantShip.IsFlagship"/> fields
/// it would only restate.</summary>
public static class MerchantMarineQuery
{
    public static IEnumerable<MerchantShip> ShipsOwnedBy(WorldState state, RuntimeId<Household> householdId) =>
        state.MerchantShips.InAscendingOrder()
            .Select(entry => entry.Value)
            .Where(ship => ship.ActualOwnerHouseholdId == householdId);

    public static MerchantShip? FlagshipOf(WorldState state, RuntimeId<Household> householdId) =>
        ShipsOwnedBy(state, householdId).FirstOrDefault(ship => ship.IsFlagship);
}
