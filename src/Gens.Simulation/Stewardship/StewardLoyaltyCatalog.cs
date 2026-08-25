using Gens.Simulation.Ledger;

namespace Gens.Simulation.Stewardship;

/// <summary>Numeric constants for the Loyalty &amp; Betrayal risk roll (§6) a steward with unsupervised
/// Treasury access carries every month. §11's Open Questions leaves incident severity/frequency
/// unspecified; this catalog is where that original engineering choice lives, matching <see
/// cref="StewardCompetenceCatalog"/>'s identical convention.</summary>
public static class StewardLoyaltyCatalog
{
    /// <summary>Percent chance (0-100) of an incident at zero Loyalty — even a fully disloyal steward
    /// does not skim every single month.</summary>
    public const int MaxIncidentChancePercent = 15;

    /// <summary>Below this Loyalty score (0-100), an incident that occurs escalates from Skimming to
    /// Embezzlement (§6: "rarer/severe, substantial Treasury loss").</summary>
    public const int EmbezzlementLoyaltyThreshold = 15;

    /// <summary>Below this Loyalty score, an incident escalates all the way to Active Sabotage (§6:
    /// "rarest; Loyalty collapsed to hostility").</summary>
    public const int ActiveSabotageLoyaltyThreshold = 5;

    public static Money AmountFor(StewardIncidentType type) => type switch
    {
        StewardIncidentType.Skimming => Money.FromDenarii(5),
        StewardIncidentType.Embezzlement => Money.FromDenarii(40),
        StewardIncidentType.ActiveSabotage => Money.FromDenarii(100),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown steward incident type."),
    };
}
