using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Chronicle;

/// <summary>Which broad section of the household record an entry belongs to (Phase 11 item 3;
/// <c>gens-dynasty-chronicle-design.md</c> §2/§4) — the category filter <see
/// cref="Queries.ChronicleQuery"/> can cut across every <see cref="GenerationalChapter"/> at
/// once.</summary>
public enum ChronicleCategory
{
    BirthsAndDeaths,
    MarriagesAndFamily,
    PoliticsAndOffice,
    WarAndCombat,
    WealthAndBuilding,
    FaithAndScandal,
    Other,
}

/// <summary>How much narrative weight an entry carries (§3), assigned once at generation time (§6)
/// per <see cref="ChronicleProjector"/>'s default mapping and never recomputed afterward — the player
/// reads whichever tiers they want via <see cref="Queries.ChronicleQuery"/>'s own filtering rather
/// than the Chronicle deciding for them what is worth seeing.</summary>
public enum ChronicleTier
{
    Minor,
    Notable,
    Major,
    Legendary,
}

/// <summary>Distinguishes a system-generated entry from a player-authored diary note (§7) — the two
/// never get confused with each other even though both live in the same <see
/// cref="State.WorldState.ChronicleEntries"/> partition.</summary>
public enum ChronicleEntrySource
{
    System,
    PlayerNote,
}

/// <summary>
/// One line of a household's own record (Phase 11 item 3; §2's entry anatomy). Immutable: <see
/// cref="SetChronicleEntryPinnedCommand"/>/<see cref="AnnotateChronicleEntryCommand"/> replace an
/// entry (remove then re-add under the same <see cref="EntryId"/>) to change <see cref="Pinned"/> or
/// <see cref="PlayerAnnotation"/>, matching <see cref="Succession.HouseholdHeadship"/>'s identical
/// convention.
/// </summary>
/// <param name="HouseholdId">Which household's record this entry belongs to — <c>null</c> only for a
/// rival-house-only fact (e.g. <c>actors.extinguished</c>) that never touched a player-tracked
/// household at all; §9's own Dossier posting is what carries that case, via <see
/// cref="Actors.RivalDossierRefresh"/>, rather than this field.</param>
/// <param name="LinkedCharacterIds">Whoever the entry is actually about (§2) — every Character named
/// in the source event's own <c>SubjectIds</c>, in that same order.</param>
/// <param name="SourceSystem">Which event <see cref="Commands.IDomainEvent.Type"/> tag actually
/// generated this entry (§2's "source reference"), for traceability.</param>
/// <param name="CrossHouseLinkedEntryId">§9's cross-house linking hook. Always <c>null</c> in this
/// pass: a shared <see cref="State.WorldState.ChronicleEntries"/> partition already lets one entry
/// serve both the player's and a rival's Dossier (see <see cref="ChronicleGenerationSystem"/>'s own
/// doc comment) without needing a second, divergent-prose entry — §12's own "cross-house prose
/// divergence depth... isn't decided" leaves that duplication for whichever later pass resolves it.</param>
/// <remarks>Deliberately omits §5's Milestone fields (<c>isMilestone</c>/<c>milestoneStatus</c>): the
/// roadmap names this item as "Chronicle entries from domain events, significance tiers, chapters,
/// filters, pins, annotations, and rival entries" — the milestone-as-goal-tracker mechanism is its own
/// later item.</remarks>
public sealed record ChronicleEntry(
    RuntimeId<ChronicleEntry> EntryId,
    RuntimeId<Household>? HouseholdId,
    GameDate Month,
    ChronicleCategory Category,
    ChronicleTier Tier,
    string Prose,
    IReadOnlyList<RuntimeId<Character>> LinkedCharacterIds,
    string SourceSystem,
    ChronicleEntrySource Source,
    bool Pinned = false,
    string? PlayerAnnotation = null,
    RuntimeId<ChronicleEntry>? CrossHouseLinkedEntryId = null);
