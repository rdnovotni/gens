using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;
using Gens.Simulation.Societates;

namespace Gens.Simulation.PublicContracts;

/// <summary>Versioned constants for Phase 15 item 6's Public Contracts &amp; Competitive Bidding
/// mechanics (<c>gens-public-contracts-competitive-bidding-design.md</c>), matching <see
/// cref="BusinessCompetition.BusinessCompetitionCatalog"/>'s and every other Phase 15 item's identical
/// "unsized against real playtest data, but named in one place" convention — §9's Open Questions
/// explicitly leaves "all numeric sizing beyond the Lustrum's own real 60-month historical interval —
/// bid-weighting formulas, reliability scoring, and disqualification duration" unsized.</summary>
public static class PublicContractsCatalog
{
    // ---- §3 The Lustrum ---------------------------------------------------------------------------

    /// <summary>§3's one real, non-negotiated figure: "every 60 months, a Lustrum fires" — the
    /// historical five-year census interval, not a number this item invents.</summary>
    public const int LustrumIntervalMonths = 60;

    // ---- §5 The Bidding Process --------------------------------------------------------------------

    /// <summary>§5.1's Price input, converted to bid score: every this-many-denarii offered lowers a
    /// bid's total score by one point (a lower price scores higher) — this item's own invented
    /// conversion rate, matching <see cref="Legal.LegalCatalog.DignitasThumbDivisor"/>'s identical
    /// "divide a real figure down to comparable score-point scale" shape.</summary>
    public const int PriceScoreDivisorDenarii = 5;

    /// <summary>§5's Influence input: a bribe's Denarii amount converts to bid-score weight at this
    /// rate, capped at <see cref="MaxBribeScoreWeight"/> — reusing <see
    /// cref="Legal.LegalCatalog.BriberyWeightPerTenDenarii"/>'s own identical conversion directly rather
    /// than inventing a second "how much is a bribe worth" formula for the same underlying act (§5:
    /// "the existing Bribes category... aimed at the sitting Censor personally").</summary>
    public const int BribeScoreWeightPerTenDenarii = Legal.LegalCatalog.BriberyWeightPerTenDenarii;
    public const int MaxBribeScoreWeight = Legal.LegalCatalog.MaxBriberyWeight;

    /// <summary>§5's "a Censor sympathetic to a bidder's Faction... can and does weight the decision" —
    /// this item's own flat thumb-on-the-scale bonus, matching <see
    /// cref="Magistracies.MagistracyCatalog.FactionAlignmentBonus"/>'s identical magnitude and reasoning
    /// for the same Faction-alignment shape applied to a different award.</summary>
    public const int FactionAlignmentScoreBonus = Magistracies.MagistracyCatalog.FactionAlignmentBonus;

    /// <summary>§5's "already personally connected via Clientela" — a real bonus when the awarding
    /// Censor and the bidder's own resolved Character already carry a directed <see
    /// cref="Characters.BondTag.Patron"/>/<see cref="Characters.BondTag.Client"/> bond, either
    /// direction.</summary>
    public const int ClientelaBondScoreBonus = 10;

    /// <summary>§5's "or simply corrupt" — reuses <see cref="SocietatesCatalog.GreedyTraitId"/> directly
    /// as the awarding Censor's own corruptibility tell (cross-domain reuse, matching <see
    /// cref="BusinessCompetition.BusinessCompetitionCatalog.GreedyTraitId"/>'s identical precedent): a
    /// Greedy Censor applies this bonus to whichever bidder offered the largest bribe, deterministically
    /// rather than by a further, unneeded dice roll — the corruption is the Censor's own standing trait,
    /// not a fresh chance each award.</summary>
    public static readonly DefinitionId<Trait> CorruptCensorTraitId = SocietatesCatalog.GreedyTraitId;
    public const int CorruptCensorBribeFavoritismBonus = 15;

    // ---- §6 Contract Fraud --------------------------------------------------------------------------

    /// <summary>§6.1's "a real, quiet margin gain" — the fraction of a contract's own awarded value
    /// quietly diverted to the holder each month while cutting corners goes undiscovered.</summary>
    public static readonly Fixed64 CuttingCornersMonthlyMarginGainFraction = Fixed64.FromRaw(20_000); // 0.02

    /// <summary>§6.1's "a Discovery risk that rises the longer it continues" — this item's own invented
    /// monthly increment and 0-100 threshold, matching <see
    /// cref="Interactions.SchemeProgressCatalog"/>'s identical progress/discovery-race shape (see this
    /// domain's own doc comment on why it mirrors that engine's numbers rather than literally reusing
    /// its Character-vs-Character <see cref="Interactions.Scheme"/> record).</summary>
    public const int FraudDiscoveryRiskGainPerMonth = 15;
    public const int FraudDiscoveryRiskThreshold = 100;

    // ---- §6.2 Prosecution and Consequences -----------------------------------------------------------

    /// <summary>§6.2's "restitution" — the fraction of the defrauded contract's own awarded value a
    /// conviction orders repaid.</summary>
    public static readonly Fixed64 RestitutionFraction = Fixed64.FromRaw(500_000); // 0.5

    /// <summary>§6.2's "permanent or long-term disqualification from future contract bidding" — §9
    /// leaves the actual duration unsized ("left open, in the same spirit as Monuments &amp; Legacy
    /// Building's own rare Damnatio Memoriae reversal"); this item anchors it to one full <see
    /// cref="LustrumIntervalMonths"/> cycle — a real, concrete "long-term" reading tied to this
    /// document's own one genuinely fixed interval, rather than an arbitrary second number.</summary>
    public const int DisqualificationMonths = LustrumIntervalMonths;

    private static readonly LedgerAccountKey CuttingCornersMarginSink = new(LedgerAccountKind.System, "publicContracts:cuttingCornersMargin");
    private static readonly LedgerAccountKey RestitutionSink = new(LedgerAccountKind.System, "publicContracts:restitution");

    /// <summary>The Settlement Treasury's own quiet loss when a contract holder cuts corners — routed
    /// through a named System sink rather than directly out of thin air, matching <see
    /// cref="Legal.OfferBribeCommand"/>'s own <c>BriberySink</c> precedent for a deliberately concealed
    /// transfer.</summary>
    public static LedgerAccountKey CuttingCornersSink => CuttingCornersMarginSink;

    public static LedgerAccountKey ConvictionRestitutionSink => RestitutionSink;
}
