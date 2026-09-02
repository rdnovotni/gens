using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;
using Gens.Simulation.RealEstate;

namespace Gens.Simulation.Societates;

/// <summary>Versioned constants for Phase 15 item 2's Societates &amp; Business Partnerships mechanics
/// (<c>gens-societates-business-partnerships-design.md</c>), matching <see
/// cref="RealEstateCatalog"/>'s and <see cref="Legal.LegalCatalog"/>'s identical "unsized against real
/// playtest data, but named in one place" convention — §11's Open Questions explicitly leaves "all
/// numeric sizing... profit-split defaults, unlimited-liability exposure curves, and dispute-resolution
/// probabilities" unsized.</summary>
public static class SocietatesCatalog
{
    /// <summary>§7's "detectable the same way" as an Operator's own skimming risk — reused directly
    /// from <see cref="RealEstateCatalog.SkimmingLoyaltyThreshold"/> rather than inventing a second
    /// threshold for the identical "is this person running unsupervised money a real liability"
    /// question, itself reused from <see cref="Stewardship.StewardIncidentCatalog.LoyaltyRiskThreshold"/>.</summary>
    public const int SkimmingLoyaltyThreshold = RealEstateCatalog.SkimmingLoyaltyThreshold;

    /// <summary>§7/§9's "a partner's own Reactive Traits (Ambition, Greed)... directly determine a
    /// partner's own likelihood of triggering §7's own dispute types" — the content-authored "greedy"
    /// trait id (<c>content/source/traits/congenital.json</c>), read directly off <see
    /// cref="Character.Traits"/> rather than through a compiled <c>TraitCatalog</c>'s axis score,
    /// matching <see cref="Legal.LegalCatalog.LitigiousTraitId"/>'s and <see
    /// cref="Legal.LegalCatalog.LegalScholarTraitId"/>'s own identical "no compiled TraitCatalog
    /// reachable from a command or monthly system in this codebase" precedent.</summary>
    public static readonly DefinitionId<Trait> GreedyTraitId = new("greedy");

    /// <summary>§6's audit consequence for a false accusation against a partner, mirroring <see
    /// cref="RealEstateCatalog.FalseAuditAccusationLoyaltyPenalty"/>'s own identical "a relationship-web
    /// hit if the [accused] turns out to have been honest all along."</summary>
    public const int FalseAuditAccusationLoyaltyPenalty = RealEstateCatalog.FalseAuditAccusationLoyaltyPenalty;

    /// <summary>§3's unlimited liability, this item's own invented sizing (§11's own "unlimited-
    /// liability exposure curves... unsized"): the flat Denarii amount debited from a failing partner's
    /// own household Ledger account when no <see cref="Societas.LinkedPropertySubject"/> resolves a
    /// real tracked Value to scale from.</summary>
    public static readonly Money BaseUnlimitedLiabilityAmount = Money.FromDenarii(500);

    /// <summary>§3's own worked contrast — "a household entering a Societas Unius Rei... risks a real,
    /// contained loss... a household entering a Societas Omnium Bonorum... risks genuine, complete
    /// ruin" — read as a flat multiplier on <see cref="BaseUnlimitedLiabilityAmount"/> (or the linked
    /// asset's own tracked Value) for a Societas Omnium Bonorum specifically, since that partnership
    /// type pools "essentially their entire property," not just venture-earmarked capital.</summary>
    public static readonly Fixed64 OmniumBonorumLiabilityMultiplier = Fixed64.FromRaw(3_000_000); // 3.0.

    /// <summary>The linked asset's own tracked Value, when one resolves, scales the liability call
    /// instead of the flat <see cref="BaseUnlimitedLiabilityAmount"/> — a bigger venture failing costs
    /// its exposed partners more. This is the fraction of that Value called in for a Societas Unius Rei
    /// (multiplied further by <see cref="OmniumBonorumLiabilityMultiplier"/> for an Omnium Bonorum).</summary>
    public static readonly Fixed64 LinkedAssetLiabilityFraction = Fixed64.FromRaw(500_000); // 0.5.

    /// <summary>§9's "particularly relevant for a Societas Omnium Bonorum, where unwinding one
    /// partner's own stake... is real, complicated work" — the Ambition floor (Core Condition) this
    /// item's own <see cref="PartnerDisputeRiskQuery"/> reads as a real, if purely descriptive,
    /// early-exit-likelihood signal (no autonomous filer exists yet to act on it — see that query's own
    /// doc comment).</summary>
    public const int EarlyExitAmbitionThreshold = 65;
}
