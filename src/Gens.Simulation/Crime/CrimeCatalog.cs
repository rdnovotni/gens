using Gens.Simulation.Ledger;

namespace Gens.Simulation.Crime;

/// <summary>Versioned constants for Phase 12 item 5's Crime, Detention &amp; Punishment mechanics
/// (<c>gens-crime-punishment-imprisonment-design.md</c>), matching <see
/// cref="Legal.LegalCatalog"/>'s identical "unsized against real playtest data, but named in one
/// place" convention — §13's Open Questions explicitly leaves "all numeric sizing... the
/// Justified/Unjust Dignitas and relationship-web deltas, escape-risk thresholds, Ransom pricing, and
/// every severity tier's own exact weighting" unsized.</summary>
public static class CrimeCatalog
{
    /// <summary>§4: "a small, expected relationship-web cost from the target's own family" — the
    /// opinion swing a Justified Imprison applies directly between actor and target.</summary>
    public const int JustifiedImprisonOpinionPenalty = 10;

    /// <summary>§4: "a genuinely severe consequence, scaled to the target's own Dignitas and
    /// standing" — an Unjust Imprison's Dignitas hit to the actor's own household and its own,
    /// sharper relationship-web scar.</summary>
    public const int UnjustImprisonDignitasPenalty = 20;
    public const int UnjustImprisonOpinionPenalty = 30;

    /// <summary>§8: execution/sentencing scales §4's own Justified/Unjust math "up to its natural
    /// maximum severity." A Justified sentence still costs the sentenced household some Dignitas (a
    /// real, if contained, fact of record); an Unjust one is this document's own single most severe
    /// consequence-generating event.</summary>
    public const int JustifiedSentenceDignitasPenalty = 10;
    public const int UnjustSentenceDignitasPenalty = 40;
    public const int UnjustSentenceOpinionPenalty = 40;

    /// <summary>§7's Fine sentence amount — distinct from, and set independent of, <see
    /// cref="Legal.LegalCatalog.FineSentenceAmount"/>, since this command can apply a Fine directly
    /// (e.g. after a private Imprison-and-sentence sequence) without a <see
    /// cref="Legal.LegalCase"/> ever having existed to roll one.</summary>
    public static readonly Money FineSentenceAmount = Money.FromDenarii(60);

    /// <summary>§7: Relegatio/Deportatio both remove the sentenced household head from productive
    /// standing; Deportatio's own "substantial property confiscation" is modeled as a real, if flat,
    /// Ledger seizure distinct from (and harsher than) Relegatio's "citizenship and most property
    /// retained."</summary>
    public static readonly Money DeportatioPropertyConfiscation = Money.FromDenarii(200);

    /// <summary>§5's escape-risk placeholder for a Detained free character with no <see
    /// cref="Characters.RegimenSettings"/> to read (only an enslaved Character under Labor &amp;
    /// Slavery's own Regimen system has one) — see <see cref="DetentionResolver.ComputeRiskScore"/>'s
    /// own doc comment for why this is a narrower reuse of <see
    /// cref="Characters.FlightRiskCalculator"/> than the full Regimen-driven formula.</summary>
    public const int MaxFreeDetaineeRiskScore = 100;

    /// <summary>§5: a failed escape attempt still costs the detainee some Loyalty (mirroring <see
    /// cref="Characters.LaborFlightSystem"/>'s own <c>RecaptureLoyaltyPenalty</c> for the identical
    /// "caught trying" beat), even though this item does not build a full dispatched-pursuit
    /// countdown the way that system's enslaved-specific flight/recapture engine does.</summary>
    public const int FailedEscapeAttemptLoyaltyPenalty = 10;

    /// <summary>§10: Ransom pricing scales with the captive's own household Dignitas (a stand-in for
    /// "sufficient standing") — this factor converts Dignitas points directly into a suggested
    /// opening demand in Denarii.</summary>
    public const int RansomDenariiPerDignitasPoint = 5;
    public static readonly Money MinimumRansomDemand = Money.FromDenarii(50);

    /// <summary>§10: "a successfully ransomed captive returning home is a real, concrete goodwill
    /// gesture" — the Dignitas swing a Paid or a Mercy release grants the releasing household, and the
    /// opinion repair it grants directly between the two household heads.</summary>
    public const int RansomPaidOrMercyDignitasGain = 8;
    public const int RansomPaidOrMercyOpinionGain = 15;

    /// <summary>§10: "a refused or excessively harsh demand reads as its own kind of provocation" —
    /// the opinion cost a Refused ransom negotiation applies between the two household heads.</summary>
    public const int RansomRefusedOpinionPenalty = 15;
}
