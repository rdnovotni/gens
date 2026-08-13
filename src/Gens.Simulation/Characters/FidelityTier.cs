namespace Gens.Simulation.Characters;

/// <summary>
/// The three fidelity tiers ADR 0009 declares. Not a field stored anywhere — which tier a person sits
/// at is implicit in which partition holds them: a count inside a <see cref="PopGroup"/> is <see
/// cref="Background"/>, a full <see cref="Character"/> record is <see cref="Named"/>. This enum exists
/// so code that reasons about the tier boundary (promotion eligibility, future <c>LivingWorldActor</c>
/// work) has one shared vocabulary rather than each caller inventing its own two-state flag.
/// </summary>
public enum FidelityTier
{
    /// <summary>Aggregate only, per <see cref="PopGroup"/>'s shape. No <see
    /// cref="Identity.RuntimeId{T}"/> exists for any individual at this tier.</summary>
    Background,

    /// <summary>Reserved for the future <c>LivingWorldActor</c> bounded-aggregate tier (Roadmap Phase
    /// 10). Not populated by anything in Phase 5-7 — declared now so the enum is stable across that
    /// phase boundary (ADR 0009).</summary>
    Noteworthy,

    /// <summary>The full <see cref="Character"/> record. Promotion into this tier is one-directional:
    /// once a person reaches <see cref="Named"/>, they never demote back to <see cref="Background"/>.</summary>
    Named,
}
