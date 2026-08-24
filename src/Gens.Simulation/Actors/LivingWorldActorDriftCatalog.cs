namespace Gens.Simulation.Actors;

/// <summary>Numeric constants for <see cref="BackgroundHouseDriftSystem"/> (Phase 10 item 3's
/// background-tier abstract tick). <c>gens-rival-houses-design.md</c> §10's Open Questions explicitly
/// leaves "how often do background houses roll for fortune/standing shifts?" unspecified; this
/// catalog is where that original engineering choice lives, versioned and named rather than an inline
/// literal, matching <see cref="LivingWorldActorTieringCatalog"/>'s identical convention.</summary>
public static class LivingWorldActorDriftCatalog
{
    /// <summary>The hard per-tick cap on how many Background-tier actors <see
    /// cref="BackgroundHouseDriftSystem"/> processes (Phase 10 item 7's "simulation budgets so
    /// background actors cannot expand work linearly without bounds"). A campaign with more Background
    /// houses than this simply spreads their drift rolls across more ticks — see that system's own doc
    /// comment for the rotating-window mechanism that keeps every house's odds equal over time despite
    /// the cap.</summary>
    public const int MaxBackgroundActorsProcessedPerTick = 500;

    /// <summary>Percent chance (0-100), per processed tick, that a Background actor's <see
    /// cref="LivingWorldActorNetWorth.Band"/> shifts one step in the direction its current <see
    /// cref="LivingWorldActorStandingTrend"/> implies (Rising ⇒ up, Declining ⇒ down, Established ⇒ no
    /// drift). This is the whole of what "periodic... fortune shifts" (§2.1) means for a Background
    /// house: individual births/deaths/marriages within it are never simulated at all — only its
    /// aggregate wealth band and standing trend move.</summary>
    public const int NetWorthDriftChancePercent = 10;

    /// <summary>Percent chance (0-100), per processed tick, that a Background actor's <see
    /// cref="LivingWorldActorStandingTrend"/> itself randomly moves to a neighboring trend — representing
    /// a house's fortunes turning without requiring a specific triggering event.</summary>
    public const int StandingTrendDriftChancePercent = 5;
}
