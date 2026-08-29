using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Identity;
using Gens.Simulation.Succession;
using Gens.Simulation.Time;

namespace Gens.Simulation.Epithets;

/// <summary>The four real historical categories <c>gens-epithets-nicknames-titles-design.md</c> §2
/// names. This item's own <see cref="EpithetGenerationSystem"/> only ever mints <see
/// cref="VirtueOrAchievement"/> agnomina: <see cref="Conquest"/> needs Military &amp; Combat and
/// Diplomacy with Non-Roman Peoples (§3) to actually resolve a campaign outcome, <see
/// cref="CrowdGivenNickname"/> needs Fame (§4, Games &amp; Spectacle/Celebrities, neither built), and
/// <see cref="MockingNickname"/> needs Scandal (§7) — none of those three source systems exist in this
/// codebase yet (all Phase 12/16, not started). The type is still modeled in full so a later phase's
/// award logic only has to add cases to <see cref="EpithetGenerationSystem"/>, not redesign the data
/// model underneath it.</summary>
public enum AgnomenType
{
    Conquest,
    VirtueOrAchievement,
    CrowdGivenNickname,
    MockingNickname,
}

/// <summary>§4's two sources for an Agnomen. <see cref="FormalSenateOrCuriaGrant"/> is modeled but never
/// produced by this item's own award logic: a real Senate/Curia vote needs Politics &amp; Patronage
/// (Phase 12, not yet built) to actually convene one, so every Agnomen this codebase currently mints
/// carries <see cref="OrganicCrowdOrigin"/> instead — the household's own reputation earning the name
/// rather than any body formally conferring it.</summary>
public enum AgnomenGrantMethod
{
    FormalSenateOrCuriaGrant,
    OrganicCrowdOrigin,
}

/// <summary>
/// One Character's real, permanent, earned name (Phase 11 item 5; <c>gens-epithets-nicknames-titles-
/// design.md</c> §9's <c>Agnomen</c> data model) — distinct from Traits' Combo Title and Policies &amp;
/// Edicts' Hybrid Doctrine naming (§1's own three-way taxonomy): a documented deed, not a fluid
/// description. Kept forever once granted, matching <see cref="SuccessionDispute"/>'s identical
/// "resolved or not, kept for the campaign's lifetime" convention — an Agnomen is never revoked by this
/// item (§10's own "formal revocation" Open Question is left unaddressed).
/// </summary>
/// <param name="SourceChronicleEntryIds">The Dynasty Chronicle entries whose accumulated pattern
/// actually justified the grant (§9's own provenance requirement — "rules and provenance rather than
/// free text") — empty when the grant instead traces to a single dated event, in which case <see
/// cref="SourceSuccessionDisputeId"/> carries that provenance instead.</param>
/// <param name="SourceSuccessionDisputeId">Set only when this Agnomen was earned by prevailing in a
/// contested succession (<see cref="Succession.SuccessionDisputeResolvedEvent"/>) — <c>null</c> for an
/// achievement Agnomen earned through sustained Chronicle accumulation instead.</param>
/// <param name="DignitasEffect">§9's field, always <c>null</c> in this codebase: no personal or
/// household Dignitas stat exists yet on <see cref="Character"/> — only <see
/// cref="Actors.LivingWorldActor.Dignitas"/> tracks a bare int for rival houses (see <see
/// cref="Succession.DeclareHeirCommand"/>'s own doc comment for the same gap). Kept as a field so a
/// later phase that adds personal Dignitas only has to start writing it, not add it.</param>
/// <param name="FameEffect">§9's field, always <c>null</c> in this codebase: Fame does not exist
/// anywhere in this codebase yet (Games &amp; Spectacle/Celebrities, Phase 17, not started).</param>
/// <param name="IsSuppressible">§7's field: whether Damage Control tools could realistically remove
/// this Agnomen. Always <c>false</c> here, since every Agnomen this item mints is a positive achievement
/// grant, not a mocking nickname — nothing to suppress.</param>
public sealed record Agnomen(
    RuntimeId<Agnomen> AgnomenId,
    RuntimeId<Character> CharacterId,
    AgnomenType AgnomenType,
    string Name,
    AgnomenGrantMethod GrantMethod,
    GameDate GrantedDate,
    IReadOnlyList<RuntimeId<ChronicleEntry>> SourceChronicleEntryIds,
    RuntimeId<SuccessionDispute>? SourceSuccessionDisputeId,
    int? DignitasEffect,
    int? FameEffect,
    bool IsSuppressible);

/// <summary>This item's own invented sizing for §10's "all numeric sizing... unsized" Open Question —
/// the design doc names no thresholds, so these are a scoped baseline, not a value pulled from the
/// design corpus.</summary>
public static class AgnomenCatalog
{
    /// <summary>§2's real virtue agnomen awarded to a household head whose own personally-linked
    /// Dynasty Chronicle record has accumulated real, sustained achievement — "the great one," matching
    /// the real historical <c>Magnus</c> honorific's own "won through sustained accomplishment rather
    /// than a single deed" texture.</summary>
    public const string AchievementAgnomenName = "Magnus";

    /// <summary>How many of a Character's own personally-linked <see
    /// cref="ChronicleTier.Major"/>/<see cref="ChronicleTier.Legendary"/> entries it takes to qualify
    /// for <see cref="AchievementAgnomenName"/>.</summary>
    public const int AchievementChronicleEntryThreshold = 3;

    /// <summary>§2's real virtue agnomen — "fortunate" — awarded to the Character who actually prevails
    /// in a contested <see cref="SuccessionDispute"/> (<see cref="Succession.SuccessionDisputeResolvedEvent"/>),
    /// a real, concrete, single dated deed rather than an accumulated pattern.</summary>
    public const string SuccessionVictoryAgnomenName = "Felix";
}
