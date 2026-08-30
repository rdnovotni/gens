using Gens.Simulation.Ledger;

namespace Gens.Simulation.Edicts;

/// <summary>Versioned constants for Phase 12 item 9's real, reachable Edicts (<c>gens-policies-edicts-
/// design.md</c> §5), matching every other catalog's "unsized against real playtest data, but named in
/// one place" convention — §10's own "all numeric sizing... Edict costs, Reception curves" Open
/// Question names this directly.</summary>
public static class EdictCatalog
{
    // ---- §5.1: "every Edict costs real Influence and Dignitas to issue" ------------------------

    public const int ManumissionEdictInfluenceCost = 15;
    public const int ManumissionEdictDignitasCost = 5;
    public const int ManumissionEdictDignitasGain = 25;
    public const int ManumissionEdictFavorGain = 15;

    public const int CitizenshipGrantInfluenceCost = 10;
    public const int CitizenshipGrantDignitasCost = 5;
    public const int CitizenshipGrantDignitasGain = 15;

    public const int ProscriptionInfluenceCost = 25;
    public const int ProscriptionDignitasCost = 10;

    /// <summary>§5.7's "seizing assets in one stroke" — the cap on how much of the target Actor's own
    /// <see cref="LedgerAccountKey.ForActor"/> balance a <see cref="IssueProscriptionCommand"/> seizes,
    /// mirroring <see cref="Legal.LegalCatalog.MaxBriberyWeight"/>'s own "cap, don't take everything"
    /// shape for a resource this codebase already tracks. A target with less than this amount on hand
    /// simply has its full balance seized, never more than it holds.</summary>
    public static readonly Money ProscriptionMaxSeizure = Money.FromDenarii(150);

    // ---- Reception (§5.1) — every real Edict's own backlash is a real Scandal (see
    // ScandalSourceType.EdictBacklash's own doc comment) at a severity this catalog names per type. ---

    public const Scandal.ScandalSeverity ManumissionEdictReceptionSeverity = Scandal.ScandalSeverity.PublicDisgrace;
    public const Scandal.ScandalSeverity CitizenshipGrantReceptionSeverity = Scandal.ScandalSeverity.MinorEmbarrassment;

    /// <summary>§5.7's own "the single darkest Edict available" gets this item's harshest real Reception
    /// tier — matching <see cref="Scandal.DiscoverFabricationCommand"/>'s own precedent for reaching <see
    /// cref="Scandal.ScandalSeverity.NotaCensoriaEligible"/> without the formal Nota Censoria consequence
    /// itself ever firing (still gated on an unbuilt "sitting Senator" concept per that severity's own
    /// doc comment).</summary>
    public const Scandal.ScandalSeverity ProscriptionReceptionSeverity = Scandal.ScandalSeverity.NotaCensoriaEligible;
}
