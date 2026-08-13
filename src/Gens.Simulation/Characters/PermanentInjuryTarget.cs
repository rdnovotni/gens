namespace Gens.Simulation.Characters;

/// <summary>What a <see cref="PermanentInjury"/> applies its standing penalty to
/// (<c>gens-familia-design.md</c> §3.1: "a standing penalty to a relevant Core Attribute, Labor
/// Skill, or Fertility rather than to Health itself"). <see cref="Condition.Health"/> is deliberately
/// excluded — per §3.1, Health "continues to recover normally around" a permanent injury.</summary>
public enum PermanentInjuryTarget
{
    // Core Attributes (§2.1).
    Diplomacy,
    Martial,
    Stewardship,
    Intrigue,
    Learning,

    // Labor Skills (§2.2).
    Fieldwork,
    DomesticService,
    Craft,
    Culinary,
    Medicine,

    // Condition (§2.3) — Fertility only; Health is explicitly excluded by §3.1.
    Fertility,
}
