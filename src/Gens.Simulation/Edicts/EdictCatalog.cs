using System.Collections.Generic;
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

    /// <summary>§5.7's "seizing assets in one stroke," sized off the target <see
    /// cref="Actors.LivingWorldActor"/>'s own <see cref="Actors.LivingWorldActorNetWorth.Band"/> — the
    /// only wealth figure a Gens actor actually carries. <see cref="Actors.LivingWorldActorNetWorth.Figure"/>
    /// is never populated anywhere in this codebase (confirmed by direct search), and <see
    /// cref="LedgerAccountKey.ForActor"/> is only ever funded for <see
    /// cref="Actors.LivingWorldActorType.Collegium"/> actors (<see
    /// cref="Collegia.FundCollegiumArcaCommand"/>) — a rival Gens actor's own ledger account is always
    /// empty, so seizing from it (this item's original, incorrect implementation) reported and
    /// transferred zero for every real rival house. <see cref="ProscriptionSeizureByBand"/> is the
    /// corrected, real sizing.</summary>
    public static readonly Money ProscriptionMaxSeizure = Money.FromDenarii(150);

    /// <summary>The real seizure amount per <see cref="Actors.LivingWorldActorNetWorth.Band"/> — a
    /// <see cref="Economy.HouseholdWealthBand.Ruined"/> target has nothing left to seize, and <see
    /// cref="Economy.HouseholdWealthBand.Wealthy"/> caps at <see cref="ProscriptionMaxSeizure"/> itself.
    /// </summary>
    public static readonly IReadOnlyDictionary<Economy.HouseholdWealthBand, Money> ProscriptionSeizureByBand =
        new Dictionary<Economy.HouseholdWealthBand, Money>
        {
            [Economy.HouseholdWealthBand.Ruined] = Money.Zero,
            [Economy.HouseholdWealthBand.Modest] = Money.FromDenarii(50),
            [Economy.HouseholdWealthBand.Comfortable] = Money.FromDenarii(100),
            [Economy.HouseholdWealthBand.Wealthy] = ProscriptionMaxSeizure,
        };

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
