using Gens.Simulation.Ledger;

namespace Gens.Simulation.MerchantFamilies;

/// <summary>Versioned constants for Phase 15 item 3's Merchant Families &amp; the Equestrian Order
/// mechanics (<c>gens-merchant-families-design.md</c>), matching <see
/// cref="Societates.SocietatesCatalog"/>'s and <see cref="RealEstate.RealEstateCatalog"/>'s identical
/// "unsized against real playtest data, but named in one place" convention — §10's Open Questions
/// explicitly leaves "all numeric sizing — the equestrian Net Worth threshold itself, wholesale-vs-
/// retail respectability weighting, and merchant-house volatility curves" unsized.</summary>
public static class MerchantFamiliesCatalog
{
    /// <summary>§2's "a real wealth threshold, distinct from and lower than the Senate's own property
    /// census" — read directly against <see cref="Economy.NetWorth.Total"/> (player) or <see
    /// cref="Actors.LivingWorldActorNetWorth.Figure"/> (a Noteworthy rival), per <see
    /// cref="EquestrianStatusQuery"/>. This item's own invented figure, deliberately below <see
    /// cref="SenateNetWorthThreshold"/> per §2's own "distinct from and lower than."</summary>
    public static readonly Money EquestrianNetWorthThreshold = Money.FromDenarii(20_000);

    /// <summary>§6's Senate property census gate — "a merchant family clears the Senate's own Net Worth
    /// gate comparatively early and comparatively easily." This item's own invented figure, read by <see
    /// cref="SenateEntryProgressQuery"/>; no Politics &amp; Patronage system sizes this threshold
    /// anywhere else in this codebase (confirmed by direct search), so this is the first real number
    /// given to that document's own §6 gate.</summary>
    public static readonly Money SenateNetWorthThreshold = Money.FromDenarii(50_000);

    /// <summary>§6's Dignitas gate — "stalls at the Dignitas gate specifically... has to close that
    /// second gap through deliberate, visible investment." This item's own invented figure, read against
    /// <see cref="Reputation.DignitasResolver.Current"/>.</summary>
    public const int SenateDignitasThreshold = 100;

    /// <summary>§6's three named Dignitas-investment actions ("funding a Games &amp; Spectacle event or a
    /// Public Works Funded Action... rather than direct profit," "pursuing a strategic marriage into an
    /// old, prestige-rich but cash-poor house," "holding a local magistracy... as a visible,
    /// respectability-building stepping stone") — this item's own invented Dignitas award per action,
    /// scaled to §6's own text ("a strategic marriage" reads as the single largest deliberate move,
    /// "funding" and "a local magistracy" as smaller, repeatable ones).</summary>
    public static int DignitasEffectFor(DignitasInvestmentActionType actionType) => actionType switch
    {
        DignitasInvestmentActionType.FundedGamesOrPublicWorks => 15,
        DignitasInvestmentActionType.LocalMagistracy => 20,
        DignitasInvestmentActionType.StrategicMarriage => 25,
        _ => throw new ArgumentOutOfRangeException(nameof(actionType), actionType, "Unrecognized Dignitas investment action type."),
    };
}
