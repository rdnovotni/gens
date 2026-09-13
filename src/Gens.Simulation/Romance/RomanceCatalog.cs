using System.Linq;
#nullable enable
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;

namespace Gens.Simulation.Romance;

/// <summary>
/// Untuned baseline constants for Phase 17 item 3's Romance, Sexuality &amp; Lineage mechanics
/// (<c>gens-romance-sexuality-lineage-design.md</c>, whose own §17 "Open Questions" explicitly leaves
/// every numeric size unset — "untuned <c>const</c>/<c>readonly</c> placeholders... same disclaimer
/// convention every prior phase used") — this implementation's own invented figures, matching <see
/// cref="Companions.CompanionsCatalog"/>'s and <see cref="Scandal.ScandalCatalog"/>'s identical
/// "unsized against real playtest data, but named in one place" disclaimer. None of these are load-bearing
/// game balance yet; they exist so every later slice reads one shared, named constant instead of a
/// scattered magic number.
///
/// Also records one deliberate non-integration, so it reads as a conscious decision rather than a gap:
/// this document's own §15 cross-system integration list does not mention Education &amp; Culture (Phase
/// 17 item 2) at all, even though it now exists — the older, superseded <c>gens-romance-seduction-design.md</c>
/// once linked Education to courtship, but the FINAL document dropped that linkage, and this Romance
/// module deliberately does not add it back. No constant here reads an Education-related stat, and none
/// should be added without a fresh design decision to do so.
/// </summary>
public static class RomanceCatalog
{
    // ---- Slice 2: Affection & Attraction (§3, §16) -------------------------------------------------

    /// <summary>Affection/Attraction points moved toward zero per month a <see cref="RomanticBond"/>
    /// goes without a meaningful interaction — <see cref="RomanticBondDecaySystem"/>'s own drift rate,
    /// matching <see cref="Characters.RelationshipDecaySystem"/>'s identical invented baseline.</summary>
    public const int MonthlyAffectionAttractionDecay = 1;

    // ---- Slice 3 (forward-declared): Courtship interaction deltas (§4) -----------------------------

    public const int FlirtAffectionDelta = 4;
    public const int FlirtAttractionDelta = 6;
    public const int CourtWooAffectionDelta = 6;
    public const int CourtWooAttractionDelta = 4;
    public const int ConfessFeelingsAffectionDelta = 10;
    public const int ConfessFeelingsAttractionDelta = 8;
    public const int RebukeAffectionDelta = -12;
    public const int RebukeAttractionDelta = -15;

    /// <summary>The Affection AND Attraction threshold a bond must clear on <i>both</i> scores before
    /// a later slice's <c>ProposeMarriageCommand</c> (§4) will accept a proposal — a genuine love match,
    /// not merely a warm-enough one.</summary>
    public const int LoveMatchThreshold = 70;

    // ---- Slice 5 (forward-declared): Autonomous Romance (§8, §8.1) ---------------------------------

    /// <summary>The minimum Affection or Attraction a later slice's <c>AutonomousRomanceSystem</c>
    /// requires on an eligible pair before it will even roll for a spontaneous advance (§8.1).</summary>
    public const int AutonomousRomanceMinimumScore = 55;

    /// <summary>The flat per-pair monthly percent chance a later slice's <c>AutonomousRomanceSystem</c>
    /// rolls against for an eligible pair — §17's Open Questions settles the sizing question as "a flat
    /// per-pair monthly percent chance, invented baseline, flagged for future balancing."</summary>
    public const int AutonomousRomanceMonthlyChancePercent = 3;

    // ---- Slice 8 (forward-declared): Affairs & Discovery (§11) --------------------------------------

    /// <summary>How many points a later slice's <c>AffairDiscoverySystem</c> adds to an undiscovered
    /// <see cref="RomanticBond.DiscoveryRisk"/> each month it remains undiscovered.</summary>
    public const int AffairMonthlyDiscoveryRiskGain = 4;

    /// <summary>The percent chance, once <see cref="RomanticBond.DiscoveryRisk"/> clears this
    /// threshold, that a later slice's <c>AffairDiscoverySystem</c> rolls the affair as discovered
    /// rather than foiled that month.</summary>
    public const int AffairDiscoveryThresholdPercent = 70;

    // ---- Trait references (only traits that already exist in content today) ------------------------

    /// <summary>Congenital Lustful/Chaste (§3: "Congenital Lustful/Chaste and a character's Beauty
    /// tier... weight Attraction directly") — already authored in
    /// <c>content/source/traits/congenital.json</c>.</summary>
    public static readonly DefinitionId<Trait> LustfulTraitId = new("lustful");

    public static readonly DefinitionId<Trait> ChasteTraitId = new("chaste");

    /// <summary>§8's Rehabilitation payoff trait, already authored in
    /// <c>content/source/traits/legal.json</c> and already granted by <see
    /// cref="Scandal.ScandalCatalog.RehabilitatedTraitId"/> for the unrelated Scandal mechanic — reused
    /// here by reference rather than re-declared, matching how <see
    /// cref="Scandal.ScandalCatalog.ScandalMarkedTraitId"/> itself reuses <see
    /// cref="Legal.LegalCatalog.ScandalMarkedTraitId"/>.</summary>
    public static readonly DefinitionId<Trait> RehabilitatedTraitId = Scandal.ScandalCatalog.RehabilitatedTraitId;

    // TODO(next slice): trait DefinitionId<Trait> references for faithful/adulterous,
    // infatuated/disillusioned, heartbroken/guarded, and the beauty spectrum (plain/common/fair/striking)
    // once content/source/traits authors them (see the approved plan's "Content authoring" section) —
    // deliberately not added yet since referencing an undefined content ID would be a silent content-load
    // failure waiting to happen, not a real forward declaration.
}
