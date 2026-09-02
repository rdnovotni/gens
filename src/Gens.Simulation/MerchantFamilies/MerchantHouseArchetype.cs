using Gens.Simulation.RealEstate;
using Gens.Simulation.State;

namespace Gens.Simulation.MerchantFamilies;

/// <summary>§4's five real, distinct Roman commercial archetypes (Phase 15 item 3;
/// <c>gens-merchant-families-design.md</c> §4), matching <see cref="RealEstate.PropertyOwnerRef"/>'s and
/// <see cref="Societates.PartnershipType"/>'s own identical "every real category represented" precedent.
/// <see cref="ShippingMagnate"/> and <see cref="TaxFarmer"/> name the concrete mechanic behind their own
/// wealth (a Societas per §7 of <c>gens-societates-business-partnerships-design.md</c>, a Publicanus
/// Contract per §8 of that same document) but this item does not require either to actually resolve
/// against one — a household may be flavored a Shipping Magnate before its first Societas exists, the
/// same "the label describes a household's own character, not a hard mechanical prerequisite" reading
/// §4's own prose supports (it describes what these households <i>are</i>, not a checklist to
/// satisfy).</summary>
public enum MerchantHouseType
{
    /// <summary>§4's "the broader, more prestigious term: financiers, wholesale businessmen, and
    /// large-scale commercial investors" — the real Latin root behind Settlement Demographics' own
    /// existing Negotiatores pop group.</summary>
    Negotiator,

    /// <summary>§4's "the narrower term for the traders and shippers actually moving goods."</summary>
    Mercator,

    /// <summary>§4's "a household whose wealth runs through Societas partnerships... spreading real
    /// maritime risk across investors precisely because the <i>lex Claudia</i> barred senators from
    /// holding ships directly."</summary>
    ShippingMagnate,

    /// <summary>§4's "a household holding one or more Publicanus Contracts... carrying that system's own
    /// real profit-and-scandal-risk dial."</summary>
    TaxFarmer,

    /// <summary>§4's "the real, historically plausible and already-modeled upward path" — a freedman
    /// Operator's own successful property buyout (Land Ownership &amp; Real Estate §6.1) is, in
    /// miniature, exactly how a real merchant dynasty's own founding generation often began.</summary>
    FreedmanDynasty,
}

/// <summary>§3's Cicero Distinction (Phase 15 item 3): "petty retail trade... judged beneath a
/// respectable man's dignity" versus "large-scale wholesale or import commerce... nearly honorable if
/// conducted on a sufficiently grand scale" — the same underlying activity, judged by scale rather than
/// by kind.</summary>
public enum TradeScaleTier
{
    Retail,
    WholesaleOrImport,
}

/// <summary>
/// §7/§9's <c>MerchantHouseArchetype</c> data model (Phase 15 item 3): a household's own real merchant
/// character, per §7's "a formal Merchant House archetype for Rival Houses' own Background/Notable
/// framework." §9's own sketch keys this by a bare <c>householdId</c>; this item reads that generically
/// enough to cover both halves of §7's own text — a Rival House extending <see
/// cref="Actors.LivingWorldActor"/>'s Background/Notable framework directly, and the player's own
/// household, which §8's cross-integration ties to Policies &amp; Edicts' own player-facing Domus
/// Mercatoria Household Doctrine — so this item reuses <see cref="RealEstate.PropertyOwnerRef"/> rather
/// than inventing a second, narrower reference type, the same reuse <see
/// cref="Societates.Societas.Partners"/> already established for "any owner kind this codebase already
/// has a tagged reference for." Only <see cref="PropertyOwnerKind.PlayerHousehold"/> and <see
/// cref="PropertyOwnerKind.RivalGens"/> ever carry one — see <see
/// cref="DesignateMerchantHouseCommands.InvalidOwnerKind"/>.
///
/// §9's own sketch also carries a <c>wealthVolatilityTier</c> field, always <c>"high"</c> — "the
/// defining trait distinguishing this from an old landed gens" per §7's own text, with no further
/// variation §7 or §9 ever describes. This record deliberately omits that field entirely rather than
/// keep it as a redundant, always-<c>"high"</c> string on every archetype: the identical judgment call
/// <see cref="Societates.SocietasPartner"/>'s own doc comment already made for unlimited liability
/// (documented as a fact of what this record <i>means</i>, not a value that varies instance to
/// instance). §7's own "exact mechanical trigger for a merchant house's own sudden collapse" is a named
/// open question (§10) this item does not resolve — no combined probability model across Piracy,
/// Insolvency, and a failed Societas is built here; that volatility already lives in those three
/// existing systems on their own terms.
/// </summary>
public sealed record MerchantHouseArchetype(PropertyOwnerRef Owner, MerchantHouseType MerchantType, TradeScaleTier WholesaleOrRetailTier);

/// <summary>Read-side lookup for a household's own <see cref="MerchantHouseArchetype"/>, matching <see
/// cref="RealEstate.PropertyResolver"/>'s and <see cref="Societates.SocietasResolver"/>'s own identical
/// "one shared resolver, not each caller re-scanning state" convention.</summary>
public static class MerchantHouseArchetypeResolver
{
    public static bool TryGetCurrent(WorldState state, PropertyOwnerRef owner, out MerchantHouseArchetype archetype) =>
        state.MerchantHouseArchetypes.TryGet(owner.ToTaggedOwnerId(), out archetype!);
}
