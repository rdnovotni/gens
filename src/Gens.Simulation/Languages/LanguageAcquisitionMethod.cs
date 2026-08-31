namespace Gens.Simulation.Languages;

/// <summary>§5's real, concrete acquisition paths and §10's <c>acquisitionMethod</c> field — a fact
/// recorded about how a <see cref="LanguageProficiency"/> was reached, not a simulated acquisition
/// system: per this item's own scope discipline, Education &amp; Culture's Learning-investment math,
/// Distant Holding/Travel sustained-exposure accrual, and a Wanderer teacher's own recruitment/hosting
/// mechanic are named prerequisites this enum records the *result* of, not systems this item
/// simulates.</summary>
public enum LanguageAcquisitionMethod
{
    /// <summary>A Character's own origin culture, read from their region's Population &amp; Culture
    /// Distribution table — grants Fluent/Native automatically at creation (§5).</summary>
    NativeOrigin,

    /// <summary>Education &amp; Culture's own Learning investment, especially at a named Institution of
    /// Renown (§5) — that system's own acquisition math is out of this item's scope.</summary>
    FormalEducation,

    /// <summary>A Distant Holding in a region with a different dominant language, or an extended Travel
    /// stay (§5) — the growth curve itself stays unsized (§11's own open question).</summary>
    SustainedExposure,

    /// <summary>A Philosopher/Rhetorician-type Wanderer, Hosted or Recruited (§5) — Wandering
    /// Populations' own teacher mechanic (Phase 14 item 4) is not simulated here.</summary>
    WandererInstruction,
}
