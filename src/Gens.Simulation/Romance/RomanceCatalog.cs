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

    /// <summary>The Affection/Attraction bump <see cref="AutonomousRomanceSystem"/> applies via <see
    /// cref="RecordRomanticInteractionCommand"/> on a successful spontaneous-advance roll for a pair not
    /// yet clearing <see cref="AutonomousRomanceMinimumScore"/> on both scores (§8.1) — sized on the same
    /// small order as <see cref="FlirtAffectionDelta"/>/<see cref="FlirtAttractionDelta"/>, this
    /// implementation's own "quiet, unprompted spark" magnitude rather than a full Courtship-interaction
    /// investment.</summary>
    public const int AutonomousRomanceNudgeAffectionDelta = 4;

    public const int AutonomousRomanceNudgeAttractionDelta = 4;

    // ---- Slice 6: Pregnancy, Fertility & Childbirth (§9) ------------------------------------------

    /// <summary>Months from conception to due date <see cref="PregnancyRecord.Create"/> stamps —
    /// §9's own gestation length, kept as a named constant rather than a magic <c>9</c> scattered at
    /// each call site.</summary>
    public const int PregnancyTermMonths = 9;

    /// <summary>The floor maternal-death-risk percent <see cref="ChildbirthResolutionSystem"/> never
    /// rolls below, regardless of how healthy the mother is — §9's own "genuine, non-trivial risk"
    /// framing: real childbirth risk in this setting is never actually zero, so a maximally healthy
    /// mother still clears this floor rather than a computed value reaching (or crossing) zero.</summary>
    public const int ChildbirthMaternalRiskFloorPercent = 1;

    /// <summary>The base maternal-death-risk percent <see cref="ChildbirthResolutionSystem"/> starts
    /// from before <see cref="Characters.Condition.Health"/> reduces it — this implementation's own
    /// untuned baseline, unsized per §17.</summary>
    public const int ChildbirthBaseMaternalRiskPercent = 8;

    /// <summary>How many maternal-risk percentage points <see cref="Characters.Condition.Health"/>
    /// shaves off <see cref="ChildbirthBaseMaternalRiskPercent"/> per 100 Health points — i.e. a mother
    /// at full Health reduces the base risk by this many points outright, scaled linearly below that.</summary>
    public const int ChildbirthMaternalRiskHealthWeightPercent = 6;

    /// <summary>How many further maternal-risk percentage points a filled <see
    /// cref="Companions.SeniorPositionTitle.CourtPhysician"/> shaves off, on top of the Health-based
    /// reduction — §9's "attended by someone skilled" mitigation, applied only when that Senior Position
    /// (Phase 17 item 1) is actually filled and present at the mother's household.</summary>
    public const int ChildbirthPhysicianRiskReductionPercent = 3;

    /// <summary>The flat, untuned infant-death-risk percent <see cref="ChildbirthResolutionSystem"/>
    /// rolls independently of the maternal roll (§9) — this implementation's own invented baseline,
    /// unsized per §17.</summary>
    public const int ChildbirthInfantRiskPercent = 5;

    // ---- Slice 6 (cont'd): newborn naming -----------------------------------------------------------

    /// <summary>Placeholder newborn <see cref="Characters.NamePool"/> <see
    /// cref="ChildbirthResolutionSystem"/> passes to <see cref="Characters.BirthCharacterCommands"/>.
    /// No production system in this codebase resolves a real content-driven NamePool from a Character's
    /// culture yet — confirmed by inspection: today <see cref="Characters.BirthCharacterCommand"/>, <see
    /// cref="Characters.PromoteToNamedCommand"/>, and <c>InstantiateWandererCommand</c> are all exercised
    /// only by tests, each constructing its own small literal <see cref="Characters.NamePool"/> inline
    /// (see e.g. <c>tests/.../Characters/NamePoolTestFixtures.cs</c>). This is the first production call
    /// site that actually needs one, so it gets its own small, explicitly-flagged placeholder here rather
    /// than inventing a fake "resolve from content" mechanism this item was never scoped to build. A later
    /// item wiring real <c>names</c> content into a per-culture NamePool should replace this constant's
    /// single use site in <see cref="ChildbirthResolutionSystem"/>.</summary>
    public static readonly NamePool PlaceholderNewbornNamePool = new()
    {
        Praenomina = new[] { "Marcus", "Gaius", "Lucius", "Quintus" },
        Nomina = new[] { "Aurelius", "Fabius", "Julius", "Cornelius" },
        Cognomina = new[] { "Maximus", "Rufus", "Longus" },
        GivenNames = new[] { "Bato", "Dagan", "Vercingetorix" },
    };

    // ---- Slice 4: Seduce Scheme (§7) ----------------------------------------------------------------

    /// <summary>The weight <see cref="Interactions.SchemeProgressSystem"/> applies to the target's
    /// existing <see cref="RomanticBond.Attraction"/> toward the initiator when computing a <see
    /// cref="Interactions.SchemeType.Seduce"/> Scheme's success chance, on top of that system's ordinary
    /// Intrigue-weighted formula — every other <see cref="Interactions.SchemeType"/> is unaffected (§7,
    /// §2/§3's "never overrides genuine unwillingness": a cold-start seduction against someone with no
    /// existing spark, Attraction 0, gets none of this bonus).</summary>
    public const int SeduceAttractionSuccessWeightPercent = 50;

    /// <summary>The Affection/Attraction bump a successful Seduce Scheme applies to the resulting <see
    /// cref="RomanticBond"/> via <see cref="Romance.SeduceSchemeResolutionHook"/> — sized on the same
    /// order as <see cref="ConfessFeelingsAffectionDelta"/>/<see cref="ConfessFeelingsAttractionDelta"/>,
    /// this codebase's other "high-investment, high-payoff" Romance interaction.</summary>
    public const int SeduceSuccessAffectionDelta = 10;

    public const int SeduceSuccessAttractionDelta = 8;

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
