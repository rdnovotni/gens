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
}
