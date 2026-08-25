namespace Gens.Simulation.Stewardship;

/// <summary>Numeric constants for how a steward's Stewardship Core Attribute (+ Learning, §5:
/// "Learning for complex cases") gates decision quality (Phase 10 item 11). §11's Open Questions
/// leaves the roll formula unspecified; this catalog is where that original engineering choice lives,
/// matching <see cref="LivingWorldActorTieringCatalog"/>'s identical convention from the actor
/// framework.</summary>
public static class StewardCompetenceCatalog
{
    /// <summary>Percent chance (0-100) a steward with attribute scores of zero still executes its
    /// chosen decision competently — quality is never purely a coin flip.</summary>
    public const int BaseExecutionChancePercent = 40;

    /// <summary>How much of Stewardship's 0-100 range folds into the execution-chance roll, in
    /// percentage points at Stewardship's maximum.</summary>
    public const int StewardshipWeightPercent = 45;

    /// <summary>How much of Learning's 0-100 range folds in, in the same units as <see
    /// cref="StewardshipWeightPercent"/> — smaller, per §5's "for complex cases" framing this as a
    /// secondary contributor.</summary>
    public const int LearningWeightPercent = 15;
}
