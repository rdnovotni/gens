namespace Gens.Simulation.Stewardship;

/// <summary>
/// Untuned baseline constants for the steward/Council competence and Loyalty-risk rolls (Phase 10
/// package 13; <c>gens-steward-council-auto-management-design.md</c> §5-6, whose own §11 "Open
/// Questions" names "competence/Loyalty roll formulas... unsized" explicitly) — this implementation's
/// own invented numbers, matching <see cref="Policies.RitesBudgetCatalog"/>'s and <see
/// cref="Characters.PunishCommands"/>'s identical disclaimer for their own untuned constants. A
/// dedicated static class rather than private consts on the system itself because <see
/// cref="StewardAutonomousDecisionSystemTests"/> needs to reference these exact same thresholds
/// rather than duplicating magic numbers.
/// </summary>
public static class StewardIncidentCatalog
{
    /// <summary>Below this Condition.Loyalty value, a steward/Council carries a real, standing risk of
    /// an incident each month (§6). At or above it, no incident can occur at all — a loyal appointee is
    /// never at risk, matching §6's "a fiercely loyal... one is safer" framing.</summary>
    public const int LoyaltyRiskThreshold = 40;

    /// <summary>The percent chance, each month an active assignment's Loyalty is below <see
    /// cref="LoyaltyRiskThreshold"/>, that some incident occurs at all (before rolling which one).</summary>
    public const int IncidentChancePercent = 8;

    /// <summary>Incident-type distribution once an incident occurs, in the severity order §6 itself
    /// names ("Skimming... rarer [Embezzlement]... rarest and darkest [Active sabotage]") — the three
    /// weights sum to 100.</summary>
    public const int SkimmingWeightPercent = 65;

    public const int EmbezzlementWeightPercent = 30;

    public const int ActiveSabotageWeightPercent = 100 - SkimmingWeightPercent - EmbezzlementWeightPercent;

    /// <summary>A "modest sum" (§6) quietly diverted by a single Skimming incident.</summary>
    public const int SkimmingAmountDenarii = 3;

    /// <summary>A "substantial Treasury loss" (§6) inflicted by a single Embezzlement incident — an
    /// order of magnitude above Skimming, matching §6's own "rarer, and far more severe" framing.</summary>
    public const int EmbezzlementAmountDenarii = 30;

    /// <summary>A single incident's Treasury impact large enough, on its own, to mark a <see
    /// cref="ReturnReport.ChronicleWorthy"/> even before folding in every other entry — Embezzlement
    /// clears this on its own, Skimming does not.</summary>
    public const int ChronicleWorthyTreasuryImpactDenarii = 20;
}
