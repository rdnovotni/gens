namespace Gens.Simulation.Regions;

/// <summary>The five Reputation Duality applicability shapes a region can carry
/// (<c>gens-starting-regions-design.md</c> §6/§12): whether, and how, a region uses Politics &amp;
/// Patronage's local-standing-vs-Rome-standing split.</summary>
public enum ReputationDualityMode
{
    /// <summary>No live "local, non-Roman" populace to hold a second axis of standing with (§6:
    /// Italian Heartland, Greek East, Anatolia, Sicily, the Alpine Provinces).</summary>
    None,

    /// <summary>The mechanic's original, full-intensity home (§6: Gallic Frontier, Britannia).</summary>
    Full,

    /// <summary>Intensity modulates by campaign date as a region's own real conquest arc closes (§6:
    /// Iberian Colony, North African Colony) — the region document should let start year/scenario
    /// modulate how "live" the split still is, per §6's own framing. This is the shape a <see
    /// cref="DatedRule{TValue}"/> override on <see cref="RegionProfileDefinition.ReputationDuality"/>
    /// exists to express.</summary>
    Tapering,

    /// <summary>A structural rather than a temporal tension: conquest was sudden and total, but native
    /// culture/religion/administration were kept deliberately separate for the entire range by design
    /// (§6: Egypt).</summary>
    PermanentStructural,

    /// <summary>Applicability depends on which specific sub-area of the region a household's own life
    /// actually touches, not one regional dial (§6: Syria/The Levant, The Balkans).</summary>
    Localized,
}
