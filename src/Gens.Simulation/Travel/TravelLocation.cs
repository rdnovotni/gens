using Gens.Simulation.Actors;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Regions;

namespace Gens.Simulation.Travel;

/// <summary>
/// <c>gens-travel-design.md</c> §10's <c>Location{}</c> shape: a real, persistent place a Character
/// can be at or travel to. A value type rather than its own <see cref="RuntimeId{T}"/>-keyed WorldState
/// partition — every kind's identity is already fully determined by whichever entity it's anchored to
/// (a Settlement, a Region Profile, a Rival House's Actor), so a separate Location registry would just
/// duplicate an identity that already exists elsewhere rather than adding one.
///
/// <see cref="RegionId"/> is this item's own scoping decision, not part of §10's literal data model: it
/// carries the authored <see cref="RegionProfileDefinition"/> (Phase 13 item 1) this place's Distance
/// Tier (<c>gens-starting-regions-design.md</c> §7.1) resolves against — <see cref="SettlementId"/>
/// alone can't answer that, since nothing yet links a runtime <see cref="Settlement"/> back to the
/// content region profile it sits in (Phase 13 item 1's own explicit boundary: it "does not change"
/// <see cref="Region"/>'s callers). Null only for <see cref="LocationKind.Home"/> and <see
/// cref="LocationKind.Rome"/> — a Home Location's Distance Tier is always <see
/// cref="DistanceTier.Near"/> by the Home Anchor rule (§8.1) regardless of region, and Rome's own
/// region is instead resolved from whichever <see cref="RegionProfileDefinition"/>'s Gazetteer seats it
/// as <see cref="GazetteerRole.Capital"/> (§8.3), not authored on this struct.
/// </summary>
public readonly record struct TravelLocation
{
    private TravelLocation(
        LocationKind kind,
        DefinitionId<RegionProfileDefinition>? regionId,
        RuntimeId<Settlement>? settlementId,
        RuntimeId<Actor>? actorId)
    {
        Kind = kind;
        RegionId = regionId;
        SettlementId = settlementId;
        ActorId = actorId;
    }

    public LocationKind Kind { get; }

    /// <summary>The content <see cref="RegionProfileDefinition"/> this place's Distance Tier resolves
    /// against — see this type's own doc comment. Null only for <see cref="LocationKind.Home"/> and
    /// <see cref="LocationKind.Rome"/>.</summary>
    public DefinitionId<RegionProfileDefinition>? RegionId { get; }

    /// <summary>The concrete runtime settlement this place is — set for every kind except <see
    /// cref="LocationKind.Rome"/> (a single always-available named location, §2) and <see
    /// cref="LocationKind.FrontierRegion"/> (a region rather than a single settlement, §2).</summary>
    public RuntimeId<Settlement>? SettlementId { get; }

    /// <summary>The specific Rival Houses <see cref="LivingWorldActor"/> whose home settlement this
    /// is — set only for <see cref="LocationKind.RivalEstate"/>, matching §10's <c>linkedActorId</c>
    /// field exactly.</summary>
    public RuntimeId<Actor>? ActorId { get; }

    /// <summary>A Character's own household settlement (§2; the implicit default every Character
    /// starts at, §10).</summary>
    public static TravelLocation Home(RuntimeId<Settlement> settlementId) =>
        new(LocationKind.Home, regionId: null, settlementId, actorId: null);

    /// <summary>The capital — a single always-available named location (§2), carrying no region of its
    /// own; see this type's own doc comment for how its Distance Tier is resolved instead.</summary>
    public static TravelLocation Rome() =>
        new(LocationKind.Rome, regionId: null, settlementId: null, actorId: null);

    /// <summary>The seat of <paramref name="regionId"/>'s Provincial Governor (§2).</summary>
    public static TravelLocation ProvincialCapital(
        DefinitionId<RegionProfileDefinition> regionId, RuntimeId<Settlement> settlementId) =>
        new(LocationKind.ProvincialCapital, regionId, settlementId, actorId: null);

    /// <summary><paramref name="actorId"/>'s home settlement (Rival Houses §3.1), in the region the
    /// caller judges that house currently sits in (§2's "literally the home settlement already tracked
    /// on that house's own <see cref="LivingWorldActor"/> record").</summary>
    public static TravelLocation RivalEstate(
        RuntimeId<Actor> actorId, RuntimeId<Settlement> settlementId, DefinitionId<RegionProfileDefinition> regionId) =>
        new(LocationKind.RivalEstate, regionId, settlementId, actorId);

    /// <summary><paramref name="regionId"/> itself as a destination (§2) — a region rather than a
    /// single settlement, so this kind carries no <see cref="SettlementId"/>.</summary>
    public static TravelLocation FrontierRegion(DefinitionId<RegionProfileDefinition> regionId) =>
        new(LocationKind.FrontierRegion, regionId, settlementId: null, actorId: null);

    /// <summary>A second holding held via the Procurator mechanic (§2).</summary>
    public static TravelLocation SecondSettlement(
        RuntimeId<Settlement> settlementId, DefinitionId<RegionProfileDefinition> regionId) =>
        new(LocationKind.SecondSettlement, regionId, settlementId, actorId: null);
}
