using System.Linq;
#nullable enable
using System;
namespace Gens.Simulation.Travel;

/// <summary>The eight destination kinds <c>gens-travel-design.md</c> §2 names, matching that
/// document's own <c>Location.type</c> field (§10): the original seven (<c>"home" | "rome" |
/// "provincialCapital" | "rivalEstate" | "frontierRegion" | "campaign" | "secondSettlement"</c>) plus
/// <see cref="InstitutionOfRenown"/>, added by Phase 17 item 2 (<c>gens-education-culture-design.md</c>
/// §4's Study Abroad Journey) as a real, intentional extension — see §2's own updated entry and this
/// enum value's own doc comment for why it is documented rather than merely bolted on. Real, persistent
/// places (§5) rather than generic types, per that section's own framing.</summary>
public enum LocationKind
{
    /// <summary>A Character's own household settlement — the implicit default every Character starts
    /// at and returns to (§10's Characters-schema addition: "defaults to a 'home' Location").</summary>
    Home,

    /// <summary>The capital — a single always-available named location, not tied to any one region
    /// (§2). Its own Distance Tier is resolved via whichever region's Gazetteer seats it as <see
    /// cref="Regions.GazetteerRole.Capital"/> (<c>gens-starting-regions-design.md</c> §8.3), not a
    /// fixed field on this location itself.</summary>
    Rome,

    /// <summary>The seat of a region's Provincial Governor (§2) — one or more per starting region.</summary>
    ProvincialCapital,

    /// <summary>A specific Rival House's home settlement (§2), tracked on that house's own <see
    /// cref="Actors.LivingWorldActor.HomeSettlementId"/> (Rival Houses §3.1).</summary>
    RivalEstate,

    /// <summary>A region, rather than a single settlement (§2) — the natural venue for Diplomacy with
    /// Non-Roman Peoples and elevated Piracy &amp; Banditry/Natural Disaster exposure (§4, future
    /// phases).</summary>
    FrontierRegion,

    /// <summary>A genuinely mobile destination that moves with wherever Military &amp; Combat's active
    /// deployment currently is (§2). Military &amp; Combat (Phase 16) does not exist yet, so <see
    /// cref="TravelLocation"/> deliberately offers no factory for this kind this pass — the enum value
    /// is reserved so the type's own shape doesn't need to change once that phase lands.</summary>
    Campaign,

    /// <summary>A second holding the player has come to hold via the Procurator mechanic (§2;
    /// Companions &amp; Court Positions §5.3, not yet built) — a real destination for checking in
    /// personally.</summary>
    SecondSettlement,

    /// <summary>One of the five fixed Institutions of Renown (<c>gens-education-culture-design.md</c>
    /// §4's Study Abroad Journey, §12's <c>InstitutionOfRenown</c> model) — Athens, Rhodes, Alexandria,
    /// Pergamon, or Massilia. Added by Phase 17 item 2 as an 8th <see cref="LocationKind"/> rather than
    /// forced through the region/Distance-Tier system: each institution authors its own fixed <see
    /// cref="DistanceTier"/>/<see cref="RouteRiskLevel"/> in content (<see
    /// cref="Education.InstitutionOfRenown"/>), bypassing the still-mostly-unbuilt region-pair distance
    /// system — see <see cref="TravelRoute.Resolve"/>'s own dedicated branch for this kind, and this
    /// item's amendment to <c>gens-travel-design.md</c> §2 documenting it as a real, intentional
    /// extension rather than an undocumented special case.</summary>
    InstitutionOfRenown,
}
