using Gens.Simulation.Ledger;

namespace Gens.Simulation.Collegia;

/// <summary>Versioned constants for Phase 12 item 6's Collegia mechanics, matching <see
/// cref="Clientela.ClientelaCatalog"/>'s identical "unsized against real playtest data, but named in
/// one place" convention — §12's Open Questions explicitly leaves every figure below unsized.</summary>
public static class CollegiumCatalog
{
    /// <summary>§4's sponsorship payoff: real Dignitas for the patron, sized flat rather than by
    /// membership count — no design-doc formula scales it, matching <see
    /// cref="Magistracies.MagistracyCatalog"/>'s own "one flat constant... rather than inventing a
    /// scaling rule the design doc never specifies" precedent.</summary>
    public const int SponsorshipDignitasGrant = 8;

    /// <summary>§4's sponsorship payoff, Influence half — written directly via <see
    /// cref="Clientela.InfluenceResolver.Apply"/>, matching <see
    /// cref="Magistracies.SalutatioSystem"/>'s own "no shared Influence-moving command exists yet"
    /// precedent.</summary>
    public const int SponsorshipInfluenceGrant = 5;

    /// <summary>§7's real Dignitas risk a patron carries for public association with an Illicit
    /// collegium, applied when that patron's own sponsored collegium is dissolved while Illicit.</summary>
    public const int IllicitPatronDignitasPenalty = 12;

    /// <summary>§6's darker political tool: the real relationship-web cost when a Justified use (a real
    /// Punishable Offense on the target household's own recorded head) still disrupts a rival, matching
    /// <see cref="Crime.CrimeCatalog.JustifiedImprisonOpinionPenalty"/>'s own "even a legitimate exercise
    /// of power costs something" precedent.</summary>
    public const int JustifiedDisruptionOpinionPenalty = 5;

    /// <summary>§6's darker political tool, Unjust path: "every bit as Unjust as an individual
    /// imprisonment would be, just at a much larger, more visible scale" — sized above <see
    /// cref="Crime.CrimeCatalog.UnjustImprisonDignitasPenalty"/> for exactly that reason.</summary>
    public const int UnjustDisruptionDignitasPenalty = 15;

    /// <summary>The Unjust path's relationship-web scar, matching <see
    /// cref="Crime.CrimeCatalog.UnjustImprisonOpinionPenalty"/>'s own sizing for the same kind of
    /// naked-power exercise.</summary>
    public const int UnjustDisruptionOpinionPenalty = 20;

    /// <summary>The named Ledger sink category dues and patron funding both post through — a plain
    /// balance move into the collegium's own per-Actor account, not a gift with nothing behind it,
    /// matching <see cref="LedgerAccount"/>'s own "moving funds from an Actor's personal account into
    /// their household's" example for <see cref="LedgerTransactionCategory.Transfers"/>.</summary>
    public const LedgerTransactionCategory ArcaFundingCategory = LedgerTransactionCategory.Transfers;
}
