using Gens.Simulation.Characters;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;

namespace Gens.Simulation.Actors;

/// <summary>The kind of living-world entity a <see cref="LivingWorldActor"/> represents
/// (<c>gens-rival-houses-design.md</c> §6: the framework generalizes past rival gentes). Phase 10
/// only creates <see cref="Gens"/> actors; the rest exist here so the framework itself does not need
/// to change shape once Diplomacy, Piracy, and Religion (Phases 13/16 and later) start creating their
/// own actors.</summary>
public enum LivingWorldActorType
{
    Gens,
    Collegium,
    ForeignPeople,
    BanditConfederation,
    ReligiousInstitution,
}

/// <summary>The two simulation-fidelity tiers every <see cref="LivingWorldActor"/> lives at
/// (<c>gens-rival-houses-design.md</c> §2): <see cref="Background"/> is a lightweight band-tracked
/// record with no full parallel economy/politics tick; <see cref="Noteworthy"/> is promoted the
/// instant real player contact occurs and gets a full Character-backed head, tracked Holdings, and
/// Events/Chronicle texture. Mirrors <c>Characters.PopGroup</c> vs. <c>Characters.Character</c>'s
/// identical Background/Named fidelity split (ADR 0009) one social tier up.</summary>
public enum LivingWorldActorTier
{
    Background,
    Noteworthy,
}

/// <summary>An actor's current fortune trajectory (<c>gens-rival-houses-design.md</c> §2.1): drives
/// background-tier behavior (Rising is more likely to initiate contact; Declining is a target for
/// absorption or a contested claim) and is itself what periodic background rolls (Phase 10 item 3)
/// drift over time.</summary>
public enum LivingWorldActorStandingTrend
{
    Rising,
    Established,
    Declining,
}

/// <summary>How this actor came to exist (<c>gens-rival-houses-design.md</c> §2.2): <see
/// cref="Ancient"/> actors are seeded at campaign start; <see cref="NovusHomo"/> and <see
/// cref="CadetBranch"/> are the two paths that replenish the roster during play. <see
/// cref="ParentActorId"/> on the owning record is populated only for <see cref="CadetBranch"/>.</summary>
public enum LivingWorldActorOrigin
{
    Ancient,
    NovusHomo,
    CadetBranch,
}

/// <summary>The household's Economic Identity (<c>gens-estate-settlement-design.md</c> §6, referenced
/// by <c>gens-rival-houses-design.md</c> §3.3): describes the institution, not its current head —
/// a Martial house can still be led by a Peace-Loving pater, a deliberate source of narrative
/// tension per that section.</summary>
public enum EconomicIdentityTag
{
    Agrarian,
    Mercantile,
    Industrial,
    Martial,
}

/// <summary>The household's political Faction (<c>gens-politics-patronage-design.md</c> §3.1,
/// referenced by <c>gens-rival-houses-design.md</c> §3.3).</summary>
public enum FactionTag
{
    Traditionalist,
    Popularist,
}

/// <summary>An actor's Economic Identity and Faction tags (<c>gens-rival-houses-design.md</c> §3.3).
/// Either may be unset: a freshly created <see cref="LivingWorldActorOrigin.NovusHomo"/> house, for
/// example, may not yet have a settled Faction leaning.</summary>
public readonly record struct LivingWorldActorIdentity(EconomicIdentityTag? Economic, FactionTag? Faction)
{
    public static readonly LivingWorldActorIdentity None = new(null, null);
}

/// <summary>An actor's wealth, at whatever fidelity its <see cref="LivingWorldActorTier"/> supports
/// (<c>gens-rival-houses-design.md</c> §3.4): a <see cref="LivingWorldActorTier.Background"/> actor
/// only ever has <see cref="Band"/> populated; a <see cref="LivingWorldActorTier.Noteworthy"/> actor
/// is calculated like the player's own household and also carries an exact <see cref="Figure"/>.
/// Reuses <see cref="HouseholdWealthBand"/> (Phase 8 item 6) rather than introducing a parallel band
/// enum, per rule 10 ("content is data, rules are code") and this framework's stated goal of never
/// creating a parallel economy (<c>gens-rival-houses-design.md</c> §8).</summary>
public readonly record struct LivingWorldActorNetWorth(HouseholdWealthBand Band, Money? Figure);

/// <summary>How dangerous an actor's military capacity is (<c>gens-rival-houses-design.md</c> §3.1/
/// §3.4): abstracted for every actor by default; a <see cref="LivingWorldActorTier.Noteworthy"/> actor
/// whose Standing with the player reaches Feuding can resolve this to a real Force via the future
/// Military &amp; Combat system (Phase 16), tracked by <see cref="ResolvedForceId"/>. No Force/Squad
/// record exists in this codebase yet, so that reference is a bare string (matching <see
/// cref="Characters.Relationship.ProvenanceEventId"/>'s identical "reference an entity kind that does
/// not exist yet as a plain string" convention) rather than a typed <see cref="RuntimeId{T}"/>.</summary>
public readonly record struct LivingWorldActorMilitaryStrength(
    MilitaryStrengthBand Band,
    string? ResolvedForceId = null);

/// <summary>A coarse, code-defined military-capacity band (<c>gens-rival-houses-design.md</c> §3.1's
/// "abstracted Military Strength"). Not sized against any real combat system yet — Military &amp;
/// Combat (Phase 16) is unbuilt — so this exists only to give <see
/// cref="LivingWorldActorMilitaryStrength"/> something to compare across actors until then.</summary>
public enum MilitaryStrengthBand
{
    Negligible,
    Modest,
    Notable,
    Formidable,
}

/// <summary>
/// One entry in the living world: a rival gens, or (per <c>gens-rival-houses-design.md</c> §6) any
/// other autonomous actor the same tiered-simulation framework later covers. Phase 10 item 3
/// ("<c>LivingWorldActor</c> framework and background/noteworthy fidelity tiers") — this file is the
/// data-model foundation only; no promotion/demotion, tick, or command behavior lands here (that is
/// packages 3-8 of the Phase 10 plan). Immutable like every other <c>WorldState</c> record — a future
/// system replaces the entry in <see cref="Gens.Simulation.State.WorldState.Actors"/> rather than
/// mutating one in place, matching <c>Policies.HouseholdPolicyState</c>'s identical
/// remove-then-re-add convention.
/// </summary>
/// <param name="HeadCharacterId">Null until the head is actually needed as a full <see
/// cref="Character"/> — lazy instantiation (<c>gens-characters-design.md</c> §11), applied one level
/// up per <c>gens-rival-houses-design.md</c> §3.2.</param>
/// <param name="ParentActorId">Populated only when <paramref name="OriginStory"/> is <see
/// cref="LivingWorldActorOrigin.CadetBranch"/> (§2.2) — the house this one split off from.</param>
public sealed record LivingWorldActor(
    RuntimeId<Actor> ActorId,
    LivingWorldActorType ActorType,
    string Name,
    LivingWorldActorTier Tier,
    LivingWorldActorStandingTrend StandingTrend,
    LivingWorldActorOrigin OriginStory,
    RuntimeId<Actor>? ParentActorId,
    LivingWorldActorIdentity IdentityTags,
    RuntimeId<Character>? HeadCharacterId,
    int Dignitas,
    LivingWorldActorNetWorth NetWorth,
    LivingWorldActorMilitaryStrength MilitaryStrength,
    RuntimeId<Region> RegionId,
    RuntimeId<Settlement> HomeSettlementId)
{
    /// <summary>The only supported way to construct a <see cref="LivingWorldActor"/>. Enforces the
    /// invariants the design doc states but a bare positional-record constructor cannot: a non-empty
    /// name, and <see cref="ParentActorId"/> set if and only if <paramref name="originStory"/> is
    /// <see cref="LivingWorldActorOrigin.CadetBranch"/> (§2.2).</summary>
    public static LivingWorldActor Create(
        RuntimeId<Actor> actorId,
        LivingWorldActorType actorType,
        string name,
        LivingWorldActorTier tier,
        LivingWorldActorStandingTrend standingTrend,
        LivingWorldActorOrigin originStory,
        RuntimeId<Actor>? parentActorId,
        LivingWorldActorIdentity identityTags,
        int dignitas,
        LivingWorldActorNetWorth netWorth,
        LivingWorldActorMilitaryStrength militaryStrength,
        RuntimeId<Region> regionId,
        RuntimeId<Settlement> homeSettlementId,
        RuntimeId<Character>? headCharacterId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A LivingWorldActor requires a non-empty name.", nameof(name));

        if (originStory == LivingWorldActorOrigin.CadetBranch && parentActorId is null)
            throw new ArgumentException(
                "A cadet-branch actor must record the parent house it split from.", nameof(parentActorId));

        if (originStory != LivingWorldActorOrigin.CadetBranch && parentActorId is not null)
            throw new ArgumentException(
                "Only a cadet-branch actor records a parent house.", nameof(parentActorId));

        return new LivingWorldActor(
            actorId, actorType, name, tier, standingTrend, originStory, parentActorId, identityTags,
            headCharacterId, dignitas, netWorth, militaryStrength, regionId, homeSettlementId);
    }
}
