namespace Gens.Simulation.Cultures;

/// <summary>§17's <c>encounterRarityTier</c> — "only meaningful for the six Trade Contact Only
/// cultures" (§10). §10 never sizes an actual rarity scale (it names Chinese as "the most indirect,
/// rarest possible" contact and every other Trade-Contact-Only entry as some flavor of "real, rare"),
/// so this two-step scale is this item's own disclosed, invented reading — <see
/// cref="ExceptionallyRare"/> reserved for the one entry (<c>chinese</c>) §10.2 itself singles out as
/// rarer than its five siblings, every other Trade-Contact-Only culture at the ordinary <see
/// cref="Rare"/> tier.</summary>
public enum EncounterRarityTier
{
    /// <summary>Not meaningful outside <see cref="CultureCategory.TradeContactOnly"/> — every
    /// Provincial/Frontier/Great Power/Contested Buffer culture's own default.</summary>
    NotApplicable,

    /// <summary>Indian, Garamantian, Aksumite, Taprobane, Sogdian (§10.1, §10.3-§10.6).</summary>
    Rare,

    /// <summary>Chinese specifically — "the most indirect, rarest possible Silk Road contact" (§10.2).</summary>
    ExceptionallyRare,
}
