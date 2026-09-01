using Gens.Simulation.Numerics;

namespace Gens.Simulation.Health;

/// <summary>Pure math for §4's Quarantine spread-reduction effect — the mechanical payoff behind both
/// §4.1 Personal Quarantine and §4.2 Settlement-Wide Quarantine, plus §4.3's Imperial-scale reduced
/// effectiveness. No effectiveness figure exists in the design corpus (§12's own "quarantine
/// effectiveness (including its Imperial-scale reduction)" open item) — every multiplier here is this
/// implementation's own invented number, chosen only so that Quarantine measurably reduces spread
/// without reducing it to exactly zero (isolation is never perfect — a household still shares meals,
/// a besieged settlement still has cracks in its cordon), and so §4.3's Imperial-scale case is
/// meaningfully — not marginally — weaker than an ordinary local Quarantine.</summary>
public static class QuarantineEffectCalculator
{
    /// <summary>§4.1: how much a Quarantined Character's own case contributes to household-contact
    /// spread this month, relative to an unquarantined case's 1.0.</summary>
    public static double PersonalSpreadMultiplier(bool quarantined) => quarantined ? 0.2 : 1.0;

    /// <summary>§4.2/§4.3: the settlement-wide multiplier applied on top of every individual spread
    /// roll at a settlement under an active Settlement-Wide Quarantine. §4.3's <paramref
    /// name="imperialScale"/> case ("meaningfully less effective... against something already moving
    /// through the whole province") only partially restores the multiplier back toward 1.0 rather than
    /// disabling Quarantine's effect outright — a settlement gate still closes, it just can't hold
    /// against a pandemic the way it holds against a contained local case.</summary>
    public static double SettlementSpreadMultiplier(bool settlementQuarantineActive, bool imperialScale)
    {
        if (!settlementQuarantineActive)
            return 1.0;
        return imperialScale ? 0.75 : 0.35;
    }

    /// <summary>§4.2's own "at a real Contentment... cost" — Phase 14 item 5's closed gap (<see
    /// cref="SetSettlementQuarantineCommand"/>'s own doc comment named this exact hook as absent). A
    /// felt, same-month shock applied every month the settlement-wide Quarantine stays active, the same
    /// "recomputed from its own formula next month regardless" shape
    /// <see cref="Hazards.DisasterDamageCalculator.ContentmentImpact"/> already established for a Disaster
    /// Event's own Contentment hit — closing the gates for public health is a real, felt imposition on a
    /// population, not a one-off. No figure for this exists in the design corpus (§12's own "quarantine
    /// effectiveness" open item, generalized here to its cost side too); this implementation's own
    /// invented constant, sized well below <see cref="Hazards.DisasterDamageCalculator.ContentmentImpact"/>'s
    /// own Catastrophic-tier figure — a standing policy decision reads as a lesser, chronic irritant next
    /// to a single violent disaster, not an equally sharp shock.</summary>
    public static Fixed64 ContentmentImpact => Fixed64.FromRaw(-30_000); // -0.03.

    /// <summary>§4.2's own "at a real Commerce cost" — the other half of this item 5's closed gap: an
    /// active settlement-wide Quarantine measurably restricts how much of a settlement's own production
    /// actually reaches its market this month (movement restrictions choking off supply reaching the
    /// square), read by <see cref="Markets.MarketClearingSystem"/> as a multiplier on total supply before
    /// clearing. This implementation's own invented figure, chosen only so that Quarantine is a real,
    /// felt trade-off against its own spread-reduction benefit rather than a free lunch, without erasing
    /// a quarantined settlement's market outright (a besieged settlement still trades in secret, at the
    /// gate, and in whatever a household already has on hand).</summary>
    public static double CommerceSupplyMultiplier(bool settlementQuarantineActive) =>
        settlementQuarantineActive ? 0.6 : 1.0;
}
