using Gens.Simulation.Actors;
using Gens.Simulation.Ledger;

namespace Gens.Simulation.NotableBusinesses;

/// <summary>Versioned constants for Phase 15 item 4's Notable Businesses mechanics
/// (<c>gens-notable-businesses-design.md</c>), matching <see cref="MerchantFamilies.MerchantFamiliesCatalog"/>'s
/// and <see cref="RealEstate.RealEstateCatalog"/>'s identical "unsized against real playtest data, but
/// named in one place" convention — §11's Open Questions explicitly leaves "all numeric sizing —
/// Reputation growth/decay, the Notable Business count per settlement, and Merge/Move's own real costs"
/// unsized.</summary>
public static class NotableBusinessesCatalog
{
    /// <summary>§4/§10's 0-100 Reputation scale.</summary>
    public const int MinReputation = 0;
    public const int MaxReputation = 100;

    /// <summary>§3's newly-promoted business starts with no established reputation either way — the
    /// midpoint of the 0-100 scale, not a self-serving default in either direction.</summary>
    public const int DefaultReputation = 50;

    /// <summary>§3's "demote back to the aggregate pool" quiet window — this item's own invented
    /// figure, matching <see cref="Actors.LivingWorldActorTieringCatalog.DemotionQuietPeriodMonths"/>'s
    /// identical two-in-game-years reasoning ("long enough that an ordinary lull... does not thrash
    /// tier"), reused verbatim rather than re-derived, since §3 explicitly names Rival Houses' own
    /// promotion/demotion pattern as the one this document mirrors.</summary>
    public const int DemotionQuietPeriodMonths = LivingWorldActorTieringCatalog.DemotionQuietPeriodMonths;

    /// <summary>§4's named reputation movers — "Reputation rises through consistent Quality output and
    /// falls through supply failures, price gouging, or a genuine business-specific Scandal." This
    /// item's own invented per-event magnitudes.</summary>
    public const int QualityOutputReputationGain = 5;
    public const int SupplyFailureReputationLoss = 10;
    public const int PriceGougingReputationLoss = 8;
    public const int BusinessScandalReputationLoss = 20;

    /// <summary>§5's worked example — "his own Reputation and income both take a real, felt hit" —
    /// this item's own invented per-action-type magnitude, escalating from a mild competitive pressure
    /// (undercutting) through the sharper, Coercive-Interaction-backed end (Sabotage, a damaging
    /// rumor).</summary>
    public static int RivalryActionReputationEffectFor(BusinessRivalryActionType actionType) => actionType switch
    {
        BusinessRivalryActionType.PriceUndercut => -4,
        BusinessRivalryActionType.WorkerPoach => -6,
        BusinessRivalryActionType.Sabotage => -12,
        BusinessRivalryActionType.DamagingRumor => -10,
        _ => throw new ArgumentOutOfRangeException(nameof(actionType), actionType, "Unrecognized rivalry action type."),
    };

    /// <summary>§7's "carrying real, steady income" — this item's own invented monthly figure for a
    /// municipal-scale contract, deliberately modest against <see
    /// cref="MerchantFamilies.MerchantFamiliesCatalog.EquestrianNetWorthThreshold"/>'s own much larger
    /// wealth-tier figure.</summary>
    public static readonly Money GovernmentContractDefaultMonthlyStipend = Money.FromDenarii(15);

    /// <summary>§7's "a genuine obligation the business can't simply walk away from without real
    /// Reputation... consequences if it fails to deliver."</summary>
    public const int ContractFailureReputationLoss = 15;

    /// <summary>§8's Specialize — "trading Reputation-building potential... for reduced resilience" —
    /// this item's own invented one-time quality-premium bump.</summary>
    public const int SpecializeReputationBonus = 8;

    /// <summary>§8's Move — "a real, one-time cost." This item's own invented figure.</summary>
    public static readonly Money MoveRelocationCost = Money.FromDenarii(200);

    /// <summary>§8's Lobby — "spending Influence or a direct payment." This item's own invented
    /// figures for each of the two named spend types.</summary>
    public const int LobbyInfluenceCost = 15;
    public static readonly Money LobbyDirectPaymentCost = Money.FromDenarii(100);

    /// <summary>§6's supply-disruption penalty — "a supplier's own... bankruptcy... genuinely disrupts
    /// the dependent business's own Output," realized here as the same <see
    /// cref="SupplyFailureReputationLoss"/> magnitude §4 already gives an ordinary supply failure.</summary>
    public const int SupplierDisruptionReputationLoss = SupplyFailureReputationLoss;
}
