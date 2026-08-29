namespace Gens.Simulation.Reputation;

/// <summary>Versioned constants for Phase 12 item 1's reputation/favor primitive, matching every other
/// catalog's "numeric sizing is unsized/tunable, but must live in one named place" convention.</summary>
public static class ReputationCatalog
{
    /// <summary>How long an <see cref="FavorStatus.Outstanding"/> <see cref="FavorObligation"/> can sit
    /// uncollected before <see cref="FavorExpirationSystem"/> lapses it (10 years). Unsized against any
    /// real playtest data — a placeholder consistent with this project's "all numeric sizing" Open
    /// Questions convention (e.g. <c>gens-politics-patronage-design.md</c> §12's identical disclaimer for
    /// Dignitas thresholds and Influence rates).</summary>
    public const int FavorExpirationAfterMonths = 120;
}
