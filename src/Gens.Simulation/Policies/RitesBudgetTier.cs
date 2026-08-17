namespace Gens.Simulation.Policies;

/// <summary>The three tiers of <c>gens-policies-edicts-design.md</c> §2.3's Rites Budget Standing
/// Policy: "Frugal / Standard / Lavish, trading Incense/Treasury draw against Divine Favor stability."
/// Religion (§6, future) does not exist yet to actually spend Incense or track Divine Favor, so this
/// pass's own <see cref="RitesBudgetCatalog"/> projects the modifiers that system will eventually
/// consume without yet wiring them into a monthly tick — matching <see
/// cref="Gens.Simulation.Characters.RegimenCatalog"/>'s identical "the projection exists before its consumer does"
/// precedent.</summary>
public enum RitesBudgetTier
{
    Frugal,
    Standard,
    Lavish,
}
