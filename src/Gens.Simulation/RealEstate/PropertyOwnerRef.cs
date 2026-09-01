using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;

namespace Gens.Simulation.RealEstate;

/// <summary>
/// §2's ownership roster (Phase 15 item 1; <c>gens-land-ownership-real-estate-design.md</c> §2): every
/// kind of body this project can support actually owning a Property Record. Every real category §2
/// names is represented, matching <see cref="Legal.LegalCase.CaseType"/>'s and <see
/// cref="Scandal.ScandalRecordType"/>'s own identical "every real category represented" precedent —
/// see <see cref="PropertyOwnerRef"/>'s own doc comment for which kinds this item actually resolves
/// against a real runtime entity versus a lightweight, narrative-only reference.
/// </summary>
public enum PropertyOwnerKind
{
    /// <summary>The player's own household — <see cref="PropertyOwnerRef.OwnerId"/> is a <see
    /// cref="RuntimeId{Household}"/>'s tagged string.</summary>
    PlayerHousehold,

    /// <summary>A Rival Gens (Rival Houses' own <see cref="Actors.LivingWorldActor"/>) — <see
    /// cref="PropertyOwnerRef.OwnerId"/> is a <see cref="RuntimeId{Actor}"/>'s tagged string.</summary>
    RivalGens,

    /// <summary>A named individual Character who owns property in their own right rather than as head
    /// of a tracked gens (§2's "a freedman shopkeeper... a Companion with personal property") — <see
    /// cref="PropertyOwnerRef.OwnerId"/> is a <see cref="RuntimeId{Character}"/>'s tagged string.</summary>
    IndividualCharacter,

    /// <summary>A Temple (§2's Anatolia/Diodoros precedent). No Temple runtime entity exists anywhere
    /// in this codebase — <see cref="PropertyOwnerRef.OwnerId"/> is a free-form display name (e.g.
    /// "Temple of Diana Nemorensis") rather than a resolvable ID, matching §3's own "lightweight Named
    /// Holdings... sized for narrative and negotiation purposes" framing applied to the owner itself,
    /// not just the asset.</summary>
    Temple,

    /// <summary>A Collegium (§2, resolving Rival Houses' own open question just enough to let a
    /// Collegium own property) — <see cref="Collegia.CollegiumDetails"/> is already keyed by <see
    /// cref="RuntimeId{Actor}"/>, so <see cref="PropertyOwnerRef.OwnerId"/> is that same tagged
    /// string.</summary>
    Collegium,

    /// <summary>The Roman state's <c>ager publicus</c> (§2, §5) — never fully "bought," only ever
    /// leased. A fixed sentinel with no per-instance ID: there is exactly one Roman state, matching
    /// <see cref="Ledger.LedgerAccountKey.Mint"/>'s identical "one named external account, no per-
    /// instance ID" shape. <see cref="PropertyOwnerRef.OwnerId"/> is always <c>null</c> for this
    /// kind.</summary>
    RomanState,

    /// <summary>The settlement itself, as a civic body (§2's Forum/Baths/Carcer precedent) — <see
    /// cref="PropertyOwnerRef.OwnerId"/> is a <see cref="RuntimeId{Settlement}"/>'s tagged
    /// string.</summary>
    Municipal,

    /// <summary>A Societas business partnership (§2, §7). Full Societas mechanics (partner shares,
    /// governance, dissolution) are explicitly Phase 15 item 2, not this item's scope — this value
    /// exists purely as a schema placeholder so a Property Record can already point at "a Societas
    /// owns this" honestly, per this item's own "Societas placeholder... NOT full Societas mechanics"
    /// scope note. <see cref="PropertyOwnerRef.OwnerId"/> is a free-form, uninterpreted name; nothing
    /// in this item resolves it against a real partnership record, since none exists yet.</summary>
    Societas,

    /// <summary>The Emperor's own Imperial Patrimonium (§2) — rare, flavor-weighted, no runtime entity
    /// backs an Emperor in this codebase. Same fixed-sentinel shape as <see cref="RomanState"/>; <see
    /// cref="PropertyOwnerRef.OwnerId"/> is always <c>null</c>.</summary>
    ImperialPatrimonium,
}

/// <summary>
/// §3's "ownership pointer... resolv[ing] to any of §2's owner types" — a tagged owner reference, kept
/// as a plain <see cref="Kind"/> + string pair rather than a single narrow <c>RuntimeId&lt;T&gt;</c>
/// for the identical reason <see cref="Ledger.LedgerAccountKey"/> already gives (an owner can be a
/// Household, an Actor, a Character, a Settlement, or one of several entities this codebase has never
/// built a runtime record for at all — no single phantom type could name them all). <see
/// cref="Land.Plot.OwnerId"/> already stores a bare owner tag this same way ("households, characters,
/// civic bodies, temples, and partnerships may all own land") — this type is that same convention,
/// made real and interpretable rather than left as an unparsed string, and it round-trips through <see
/// cref="Plot.OwnerId"/> directly via <see cref="ToTaggedOwnerId"/>/<see cref="Parse"/> so this item
/// does not need to change <see cref="Plot"/>'s own schema to give its ownership pointer real
/// structure.
/// </summary>
public readonly record struct PropertyOwnerRef(PropertyOwnerKind Kind, string? OwnerId)
{
    private const string Separator = ":";

    public static PropertyOwnerRef ForPlayerHousehold(RuntimeId<Household> householdId) =>
        new(PropertyOwnerKind.PlayerHousehold, householdId.ToTaggedString());

    public static PropertyOwnerRef ForRivalGens(RuntimeId<Actor> actorId) =>
        new(PropertyOwnerKind.RivalGens, actorId.ToTaggedString());

    public static PropertyOwnerRef ForIndividualCharacter(RuntimeId<Character> characterId) =>
        new(PropertyOwnerKind.IndividualCharacter, characterId.ToTaggedString());

    public static PropertyOwnerRef ForTemple(string displayName) =>
        new(PropertyOwnerKind.Temple, RequireName(displayName, nameof(displayName)));

    public static PropertyOwnerRef ForCollegium(RuntimeId<Actor> actorId) =>
        new(PropertyOwnerKind.Collegium, actorId.ToTaggedString());

    public static readonly PropertyOwnerRef RomanState = new(PropertyOwnerKind.RomanState, null);

    public static PropertyOwnerRef ForMunicipal(RuntimeId<Settlement> settlementId) =>
        new(PropertyOwnerKind.Municipal, settlementId.ToTaggedString());

    public static PropertyOwnerRef ForSocietasPlaceholder(string displayName) =>
        new(PropertyOwnerKind.Societas, RequireName(displayName, nameof(displayName)));

    public static readonly PropertyOwnerRef ImperialPatrimonium = new(PropertyOwnerKind.ImperialPatrimonium, null);

    /// <summary>Whether this owner kind resolves against a real, campaign-tracked entity this item can
    /// validate a command against (<see cref="PlayerHousehold"/>-shaped kinds excepted — household
    /// existence has no dedicated registry anywhere in this codebase; see callers for how each real
    /// kind is actually checked). <see cref="PropertyOwnerKind.Temple"/>, <see
    /// cref="PropertyOwnerKind.Societas"/>, <see cref="PropertyOwnerKind.RomanState"/>, and <see
    /// cref="PropertyOwnerKind.ImperialPatrimonium"/> are always narrative-only.</summary>
    public bool IsNarrativeOnly =>
        Kind is PropertyOwnerKind.Temple or PropertyOwnerKind.Societas
            or PropertyOwnerKind.RomanState or PropertyOwnerKind.ImperialPatrimonium;

    /// <summary>Round-trips this reference through <see cref="Plot.OwnerId"/>'s own bare-string
    /// convention: <c>"{kind}:{ownerId}"</c>, or just <c>"{kind}"</c> for the two ID-less sentinel
    /// kinds.</summary>
    public string ToTaggedOwnerId() => OwnerId is null ? Kind.ToString() : $"{Kind}{Separator}{OwnerId}";

    /// <summary>Legacy bare <see cref="RuntimeId{T}"/> tagged-string prefixes predating this item, each
    /// mapped to the owner kind that convention always implicitly meant: <see
    /// cref="Land.HouseholdEconomyCalculator.TryGetOwningHousehold"/> (Phase 6/8) already stores <see
    /// cref="Plot.OwnerId"/> as a bare household tag with no owner-kind prefix, and <see
    /// cref="Land.Plot.OwnerId"/>'s own doc comment ("households, characters, civic bodies, temples,
    /// and partnerships may all own land") names three further bare-tagged forms a pre-upgrade Plot can
    /// legally carry: a bare Character tag, a bare Settlement tag (the civic-body case, §2's
    /// Municipal), and a bare Actor tag. A bare Actor tag is inherently ambiguous between <see
    /// cref="PropertyOwnerKind.RivalGens"/> and <see cref="PropertyOwnerKind.Collegium"/> — both key
    /// off the same <see cref="RuntimeId{Actor}"/> — so it resolves to RivalGens, the more general of
    /// the two, on the same "best-effort legacy read, not a lossless round trip" basis <see
    /// cref="PropertyOwnerKind.Temple"/>'s and <see cref="PropertyOwnerKind.Societas"/>'s own free-form
    /// (never previously tagged at all) legacy forms simply cannot recover.</summary>
    private static readonly (string Prefix, PropertyOwnerKind Kind)[] LegacyBarePrefixes =
    {
        ("household_", PropertyOwnerKind.PlayerHousehold),
        ("char_", PropertyOwnerKind.IndividualCharacter),
        ("settlement_", PropertyOwnerKind.Municipal),
        ("actor_", PropertyOwnerKind.RivalGens),
    };

    /// <summary>Non-throwing form of <see cref="Parse"/> — used by callers (e.g. <see
    /// cref="PropertyResolver.TryResolve"/>'s own monthly-tick callers) that must not abort an entire
    /// tick over one property carrying an owner tag genuinely outside every recognized legacy or
    /// current form (a free-form pre-upgrade Temple or Societas display name, chiefly).</summary>
    public static bool TryParse(string taggedOwnerId, out PropertyOwnerRef ownerRef)
    {
        if (string.IsNullOrWhiteSpace(taggedOwnerId))
        {
            ownerRef = default;
            return false;
        }

        var splitIndex = taggedOwnerId.IndexOf(Separator, StringComparison.Ordinal);
        var kindText = splitIndex < 0 ? taggedOwnerId : taggedOwnerId[..splitIndex];
        if (Enum.TryParse<PropertyOwnerKind>(kindText, out var kind))
        {
            var ownerId = splitIndex < 0 ? null : taggedOwnerId[(splitIndex + Separator.Length)..];
            ownerRef = new PropertyOwnerRef(kind, ownerId);
            return true;
        }

        foreach (var (prefix, legacyKind) in LegacyBarePrefixes)
        {
            if (taggedOwnerId.StartsWith(prefix, StringComparison.Ordinal))
            {
                ownerRef = new PropertyOwnerRef(legacyKind, taggedOwnerId);
                return true;
            }
        }

        ownerRef = default;
        return false;
    }

    public static PropertyOwnerRef Parse(string taggedOwnerId)
    {
        if (TryParse(taggedOwnerId, out var ownerRef))
            return ownerRef;

        throw new FormatException($"'{taggedOwnerId}' is not a recognized {nameof(PropertyOwnerRef)}.");
    }

    private static string RequireName(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A display name is required.", paramName);
        return value;
    }
}
