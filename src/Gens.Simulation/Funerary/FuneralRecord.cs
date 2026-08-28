using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Time;

namespace Gens.Simulation.Funerary;

/// <summary>A <see cref="FuneralRecord"/>'s lifecycle (Phase 11 item 4; <c>gens-ancestor-veneration-
/// funerary-customs-design.md</c> §2).</summary>
public enum FuneralStatus
{
    /// <summary>Opened at death (§2.1's automatic, cost-free <c>collocatio</c>), awaiting a tier
    /// choice — either <see cref="ChooseFuneralTierCommand"/> or, after <see
    /// cref="FuneraryCatalog.FuneralAutoResolutionAfterMonths"/>, <see
    /// cref="FuneralAutoResolutionSystem"/>.</summary>
    Pending,

    /// <summary>The <c>pompa funebris</c> (§2.2) has been held at a chosen tier and interment (§2.4)
    /// is complete.</summary>
    Held,
}

/// <summary>The three-tier shape of §2.2's <c>pompa funebris</c> — "the same three-tier shape as
/// Religion's Rites Budget", matching <see cref="Policies.RitesBudgetTier"/>'s identical
/// Frugal/Standard/Lavish structure.</summary>
public enum FuneralTier
{
    Modest,
    Proper,
    Grand,
}

/// <summary>§3's burial method. Per direction quoted in that section, the design doc wants this
/// primarily culture/faith-tenet driven (a "soft cultural drift" from cremation toward inhumation
/// across the 2nd century AD, hardened by a strict faith tenet where one exists) — but Cultures of the
/// Known World and Religions of the Known World (Phase 13, not yet built) are what would actually own
/// that tenet system. This implementation's own documented simplification: every funeral in this pass
/// defaults to <see cref="Cremation"/>, the dominant Republic-through-early-Empire practice for the
/// bulk of this project's own 133 BCE-AD 235 span, with the timeline-driven drift toward <see
/// cref="Inhumation"/> and any faith-tenet override left for that future phase to actually implement —
/// not a mechanic this item builds a stand-in for.</summary>
public enum BurialMethod
{
    Cremation,
    Inhumation,
}

/// <summary>Where the remains are actually housed (§3.3) — a flavor variant of whichever of the
/// Family Tomb, Mausoleum, or public Necropolis already exists for this household (Monuments &amp;
/// Legacy Building, not yet built in this codebase); the Columbarium §3.3 describes for a household of
/// modest means is likewise a flavor variant of the Necropolis, not a distinct Building entry (§10's
/// own "Columbarium's status as a distinct Building entry" Open Question). This implementation always
/// records <see cref="FamilyTomb"/> as a placeholder destination — no Buildings-partition lookup
/// exists yet to resolve a household's actual tomb, matching this record's own <see
/// cref="BurialMethod"/> simplification above.</summary>
public enum IntermentDestination
{
    FamilyTomb,
}

/// <summary>
/// One Character's funeral, from death through interment (Phase 11 item 4; §2, §9's <c>FuneralRecord</c>
/// data model). Kept once <see cref="FuneralStatus.Held"/> rather than removed, matching <see
/// cref="Succession.SuccessionDispute"/>'s identical "resolved or not, kept for the campaign's
/// lifetime" convention. Deliberately omits §7's <c>laudatioDeliveredBy</c>/<c>laudatioOutcome</c>
/// fields: the <c>laudatio funebris</c> needs Rhetoric/orator mechanics (Politics &amp; Patronage,
/// Phase 12, not yet built) this item does not attempt to stand in for — see this namespace's own
/// roadmap progress note for the full list of what §7's political-instrument framing leaves out here.
/// </summary>
public sealed record FuneralRecord(
    RuntimeId<FuneralRecord> FuneralId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> DeceasedCharacterId,
    GameDate DeathDate,
    FuneralStatus Status,
    FuneralTier? Tier,
    BurialMethod? BurialMethod,
    IntermentDestination? InterredAt,
    GameDate? HeldDate,
    Money? Cost,
    int? MemoriaGained,
    bool ImaginesDisplayed = false);
