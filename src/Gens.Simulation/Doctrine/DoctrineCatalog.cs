using Gens.Simulation.Ledger;

namespace Gens.Simulation.Doctrine;

/// <summary>Versioned constants for Phase 12 item 9's Household Doctrine mechanics
/// (<c>gens-policies-edicts-design.md</c> §3), matching every other catalog's "unsized against real
/// playtest data, but named in one place" convention — §10's own "all numeric sizing" Open Question
/// names Doctrine Affinity gain/decay rates directly.</summary>
public static class DoctrineCatalog
{
    /// <summary>§3.1's first threshold — visible flavor, no capstone yet.</summary>
    public const int EmergingThreshold = 40;

    /// <summary>§3.1's second threshold — unlocks the Doctrine's own Defining capstone.</summary>
    public const int DefiningThreshold = 75;

    /// <summary>How many Affinity points a month whose real, checkable signals match a Doctrine's own
    /// pattern (<see cref="DoctrineResolutionSystem"/>) adds — "matching choices raise Affinity."</summary>
    public const int MatchGainPerMonth = 4;

    /// <summary>How many Affinity points a month whose signals actively contradict a Doctrine's pattern
    /// subtracts — "contradicting choices lower it."</summary>
    public const int MismatchLossPerMonth = 6;

    /// <summary>How many Affinity points a month with neither a matching nor a contradicting signal
    /// erodes on its own — "unfed Affinity decays slowly on its own."</summary>
    public const int UnfedDecayPerMonth = 1;

    // ---- Mos Maiorum — Ancestral Sanction (§3.2's Defining capstone) --------------------------

    /// <summary>How much of the original conviction's Dignitas penalty <see
    /// cref="InvokeAncestralSanctionCommand"/> restores when it overturns a ruling — a real, if partial,
    /// "without the usual political cost" per §3.2, not a full erasure of the case having happened at
    /// all.</summary>
    public const int AncestralSanctionDignitasRestored = 20;

    // ---- Domus Pia — The Great Rite (§3.2's Defining capstone) --------------------------------

    public static readonly Money GreatRiteCost = Money.FromDenarii(200);
    public const int GreatRiteFavorGain = 40;
    public const int GreatRiteDignitasGain = 30;

    // ---- Domus Dura — Iron Hand (§3.2's Defining capstone) --------------------------------------

    /// <summary>The Iron Hand's own real, projected labor-output-ceiling bonus (§3.2: "the single
    /// highest sustained labor-output multiplier in the project"), read by <see
    /// cref="DoctrineLaborModifierQuery"/> — matching <see
    /// cref="Policies.RitesBudgetCatalog"/>'s own "the projection exists before its consumer does"
    /// precedent: <see cref="Characters.LaborOutputSystem"/> is an already-shipped, already-tested
    /// system this item does not reopen to actually add this bonus into its own ceiling formula.</summary>
    public const int IronHandOutputCeilingBonus = 15;

    /// <summary>The permanent Unrest/flight-risk/Legal-scrutiny baseline increase Iron Hand carries
    /// "regardless of whether the household's policies later soften" (§3.2) — projected here for the
    /// same future-consumer reason as <see cref="IronHandOutputCeilingBonus"/>, and never actually
    /// applied to <see cref="Characters.LaborFlightSystem"/>'s or <see
    /// cref="Crime.DetentionResolver"/>'s own risk formulas by this item.</summary>
    public const int IronHandFlightRiskBaselineIncrease = 10;
}
