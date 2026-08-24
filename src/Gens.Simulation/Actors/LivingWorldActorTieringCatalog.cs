namespace Gens.Simulation.Actors;

/// <summary>Numeric constants for <see cref="LivingWorldActorTieringService"/> (Phase 10 item 3).
/// <c>gens-rival-houses-design.md</c> §10's Open Questions explicitly leaves every promotion/demotion
/// threshold unspecified ("How long without an active thread before... demotion happens?"); this
/// catalog is where that original engineering choice lives, versioned and named rather than an inline
/// literal, per rule 10 ("content is data, rules are code").</summary>
public static class LivingWorldActorTieringCatalog
{
    /// <summary>How many consecutive quiet months (no active thread, per §2.4: no live Feud, marriage
    /// negotiation, or contested claim) a <see cref="LivingWorldActorTier.Noteworthy"/> actor tolerates
    /// before <see cref="LivingWorldActorTieringService.DemoteIfQuiet"/> freezes it back to <see
    /// cref="LivingWorldActorTier.Background"/>. Two in-game years: long enough that an ordinary lull
    /// between interactions does not thrash tier, short enough that the "of Note" set stays bounded per
    /// §2.4's own stated goal.</summary>
    public const int DemotionQuietPeriodMonths = 24;
}
