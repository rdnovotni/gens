namespace Gens.Simulation.Health;

/// <summary>§6's Sanitation Investment standing policy tier — "Minimal / Standard / Comprehensive,
/// trading an ongoing Treasury cost against a real, settlement-wide reduction across every Endemic
/// Exposure score and every Epidemic's severity/spread rate simultaneously." Built here in full and
/// forward-flagged to Policies &amp; Edicts' own Standing Policy roster (§6's own "explicitly flagged
/// as belonging in Policies &amp; Edicts... on its own next revisit"), the identical precedent Religion's
/// Rites Budget and Natural Disasters' Disaster Relief already established per §6.</summary>
public enum SanitationInvestmentTier
{
    /// <summary>The default for any settlement with no explicit investment on record (see <see
    /// cref="SanitationQueries.EffectiveTier"/>) — no ongoing cost, no exposure/spread
    /// reduction.</summary>
    Minimal,
    Standard,
    Comprehensive,
}
