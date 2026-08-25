using Gens.Simulation.Time;

namespace Gens.Simulation.Actors;

/// <summary>Numeric constants for <see cref="AncestralGrudge"/> (Phase 10 item 5;
/// <c>gens-rival-houses-design.md</c> §5.2: "decaying far slower than ordinary opinion... can keep
/// houses Rivalrous for generations"). §10's Open Questions explicitly leaves the decay rate
/// unspecified; this catalog is where that original engineering choice lives, matching <see
/// cref="LivingWorldActorTieringCatalog"/>'s identical convention.</summary>
public static class AncestralGrudgeCatalog
{
    /// <summary>How many months after <see cref="AncestralGrudge.OriginDate"/> a grudge remains active
    /// — roughly a human generation (25 years), matching §5.2's own "for generations" framing, far
    /// longer than any ordinary opinion decay elsewhere in this codebase (e.g. <see
    /// cref="Characters.RelationshipDecaySystem"/>'s single-point-per-month drift).</summary>
    public const int DecayMonths = 300;

    /// <summary>Whether <paramref name="grudge"/> is still active as of <paramref name="now"/> — i.e.
    /// fewer than <see cref="DecayMonths"/> have elapsed since it formed.</summary>
    public static bool IsActive(GameDate now, AncestralGrudge grudge) =>
        now.TotalMonths - grudge.OriginDate.TotalMonths < DecayMonths;
}
