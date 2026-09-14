using System.Linq;
#nullable enable
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Numerics;

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

    // ---- Slice 7: Legitimacy acknowledgment cost (§10) --------------------------------------------

    /// <summary>§10's real, visible Dignitas cost for <see
    /// cref="Succession.AcknowledgeIllegitimateChildCommand"/> — sized on the same order as <see
    /// cref="Scandal.ScandalCatalog.MinorEmbarrassmentDignitasPenalty"/>, this implementation's own
    /// reading of "a deliberate, visible choice... not a quiet toggle" as a real but not
    /// disgrace-grade household Dignitas hit.</summary>
    public const int IllegitimateChildAcknowledgmentDignitasPenalty = 10;

    /// <summary>§10's betrayed-spouse opinion swing — applied only when the acknowledging parent
    /// currently holds an open marriage to someone other than the child's other parent (see <see
    /// cref="Succession.AcknowledgeIllegitimateChildCommand"/>'s own doc comment for the exact
    /// betrayal test). Sized on the same order as <see
    /// cref="Scandal.ScandalCatalog.RelationshipScarOpinionDelta"/>, this codebase's own comparable
    /// "a specific other party learns of a specific betrayal" magnitude. Already negative, passed
    /// directly, never negated a second time.</summary>
    public const int IllegitimateChildAcknowledgmentBetrayedSpouseOpinionDelta = -20;

    // ---- Slice 8: Affairs & Discovery (§11) ---------------------------------------------------------

    /// <summary>How many points <see cref="AffairDiscoverySystem"/> adds to an undiscovered <see
    /// cref="RomanticBond.DiscoveryRisk"/> each month it remains undiscovered, before any Intrigue
    /// term — mirrors <see cref="Interactions.SchemeProgressCatalog.BaseDiscoveryRiskPerMonthPercent"/>'s
    /// identical "flat base, then an Intrigue-scaled addition" shape.</summary>
    public const int AffairMonthlyDiscoveryRiskGain = 4;

    /// <summary>The additional monthly <see cref="RomanticBond.DiscoveryRisk"/> points <see
    /// cref="AffairDiscoverySystem"/> adds, scaled by the most vigilant real wronged spouse's own <see
    /// cref="Characters.CoreAttributes.Intrigue"/> (0-100) where one is resolvable — zero when no
    /// wronged spouse exists to be vigilant at all (see that system's own doc comment for the
    /// zero-wronged-spouse defensive case). Mirrors <see
    /// cref="Interactions.SchemeProgressCatalog.MaxTargetIntrigueRiskBonusPercent"/>'s identical role,
    /// scaled down to this mechanic's own smaller base gain above.</summary>
    public const int AffairMonthlyDiscoveryRiskWrongedSpouseIntrigueWeightPercent = 4;

    /// <summary>The percent chance, once <see cref="RomanticBond.DiscoveryRisk"/> clears this
    /// threshold, that <see cref="AffairDiscoverySystem"/> rolls the affair as discovered rather than
    /// foiled that month.</summary>
    public const int AffairDiscoveryThresholdPercent = 70;

    /// <summary>The base Escalated-vs-Foiled chance (0-100) <see cref="AffairDiscoverySystem"/> rolls
    /// with once <see cref="AffairDiscoveryThresholdPercent"/> is crossed, before either side's
    /// Intrigue is weighed — a coin-flip default, matching <see
    /// cref="Interactions.SchemeProgressCatalog.BaseCounterPlayFoilChancePercent"/>'s identical
    /// unsized baseline.</summary>
    public const int AffairDiscoveryBaseEscalateChancePercent = 50;

    /// <summary>How many percentage points the Escalate chance shifts per point of Intrigue
    /// difference between the most vigilant real wronged spouse and the concealing pair's own average
    /// Intrigue (wronged-spouse Intrigue minus the pair's average, then multiplied by this and divided
    /// by 100) — a more Intrigue-capable wronged spouse is likelier to actually catch the affair, and a
    /// more Intrigue-capable concealing pair is likelier to keep covering it up. This is the deliberate
    /// mirror image of <see cref="Interactions.SchemeProgressCatalog.CounterPlayIntrigueDifferenceWeightPercent"/>'s
    /// own sign: there, high target Intrigue raises the FOIL chance (good for the target); here, high
    /// wronged-spouse Intrigue raises the ESCALATE chance (good for the wronged spouse, i.e. the
    /// discovery actually lands) — the two mechanics assign the "benefits from being perceptive" role
    /// to opposite sides of their own roll.</summary>
    public const int AffairDiscoveryIntrigueDifferenceWeightPercent = 50;

    /// <summary>What percent of an undiscovered affair's <see cref="RomanticBond.DiscoveryRisk"/>
    /// survives a Foiled roll (the plan's own "reduce partway back down, e.g. to half") — this
    /// implementation's own untuned choice of exactly half, rather than resetting to zero (a foiled
    /// scare still leaves some residual suspicion behind) or leaving it unchanged (a foiled roll should
    /// cost the concealing pair something, or the threshold would just be re-rolled unchanged next
    /// month).</summary>
    public const int AffairDiscoveryFoiledRiskRetentionPercent = 50;

    // ---- Trait references for Slice 8's reactive grants (§11) — content not yet authored -----------

    /// <summary>§11's reactive trait for a discovered affair's offending party. Not yet authored in
    /// <c>content/source/traits/</c> (a later content-authoring slice adds it) — referenced here
    /// anyway, unlike this file's own earlier "don't reference an undefined content ID" caution,
    /// because granting an unrecognized <see cref="DefinitionId{T}"/> at runtime is confirmed safe by
    /// direct inspection: <see cref="Character.Traits"/> is a bare list of definition-reference
    /// strings with no catalog-membership check anywhere in the grant path (<see
    /// cref="Scandal.RecordScandalCommand.ApplyScandalMarkedTrait"/>'s own remove-then-readd idiom,
    /// which this module's <see cref="AffairDiscoverySystem"/> reuses, never consults a <see
    /// cref="TraitCatalog"/> at all) — only <see cref="TraitCatalog.CheckExclusivity"/>/<see
    /// cref="TraitCatalog.GetAxisScore"/>, and the offline content compiler's own authored-definition
    /// validation, ever resolve a trait ID against a loaded catalog, and neither sits in this grant's
    /// path.</summary>
    public static readonly DefinitionId<Trait> AdulterousTraitId = new("adulterous");

    /// <summary>§11's reactive trait for a discovered affair's real wronged spouse(s) — see <see
    /// cref="AdulterousTraitId"/>'s own doc comment for why granting this unauthored ID at runtime is
    /// safe.</summary>
    public static readonly DefinitionId<Trait> HeartbrokenTraitId = new("heartbroken");

    /// <summary>§11's second reactive trait for a discovered affair's real wronged spouse(s),
    /// granted alongside <see cref="HeartbrokenTraitId"/> — see <see cref="AdulterousTraitId"/>'s own
    /// doc comment for why granting this unauthored ID at runtime is safe.</summary>
    public static readonly DefinitionId<Trait> GuardedTraitId = new("guarded");

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

    // ---- Slice 8 (cont'd): ResolveAffairCommand consequences (§11) ---------------------------------

    /// <summary>The small positive Affection bump <see cref="ResolveAffairCommand"/>'s <see
    /// cref="AffairResolution.Forgiven"/> branch applies, via <see
    /// cref="RecordRomanticInteractionCommand"/>, between the wronged spouse and the offender — sized on
    /// the same small order as <see cref="ConfessFeelingsAffectionDelta"/>, this implementation's own
    /// "a real, if partial, mending" magnitude rather than a full reset to trust.</summary>
    public const int AffairForgivenessAffectionDelta = 15;

    // ---- Slice 8 (cont'd): ExerciseExtremeLegalRemedyCommand (§12, §17) -----------------------------

    /// <summary>§12's "regardless of the legal justification, this carries a real, guaranteed cost" —
    /// the severe household Dignitas penalty <see cref="ExerciseExtremeLegalRemedyCommand"/> always
    /// applies to the ACTOR's (the wronged spouse's) own household, deliberately sized above every other
    /// Dignitas penalty this module or <see cref="Scandal.ScandalCatalog"/> applies (larger than <see
    /// cref="Scandal.ScandalCatalog.PublicDisgraceDignitasPenalty"/>) — killing a fellow citizen outside
    /// the courts is a graver act than an ordinary Scandal or Legal conviction, even one this
    /// implementation's own narrow gate treats as "justified."</summary>
    public const int ExtremeLegalRemedyActorHouseholdDignitasPenalty = 40;

    /// <summary>The relationship-web hit <see cref="ExerciseExtremeLegalRemedyCommand"/> applies between
    /// the actor and their own surviving spouse (the offender, when <c>AlsoOffender</c> is <c>false</c>
    /// and that spouse is therefore still alive to record a scar against) — sized the same as <see
    /// cref="Legal.LegalCatalog.RelationshipScarOpinionDelta"/>, this codebase's own comparable
    /// "a household-shattering act leaves a lasting mark" magnitude. Already negative, passed directly.</summary>
    public const int ExtremeLegalRemedyRelationshipScarOpinionDelta = -20;

    // ---- Slice 9: Adultery legal case + Infamia (§12, §13) ------------------------------------------

    /// <summary>§12's "partial property confiscation" on an Adultery conviction — <see
    /// cref="AdulteryResolutionHook"/>'s own confiscation-posting fraction of the convicted household's
    /// current Treasury balance, deliberately smaller than <see
    /// cref="PublicContracts.PublicContractsCatalog.RestitutionFraction"/>'s own 0.5: §12 itself calls
    /// this "partial," distinctly less than a full restitution.</summary>
    public static readonly Fixed64 AdulteryConfiscationFraction = Fixed64.FromRaw(300_000); // 0.3

    /// <summary>§13's per-rank-point magnitude <see cref="StatusRoleDignitasModifier.Calculate"/>
    /// multiplies its two parties' <see cref="Characters.LegalStatus"/> rank gap by — this
    /// implementation's own untuned baseline, unsized per §17, on the same small order as this module's
    /// other per-point Dignitas weights.</summary>
    public const int StatusRoleDignitasModifierMagnitude = 4;

    // ---- Content-authoring pass: remaining Romance trait references (§3, §11) -----------------------

    /// <summary>§11's opposite of <see cref="AdulterousTraitId"/> — now authored in
    /// <c>content/source/traits/romance.json</c> as an opposed reactive pair. Not yet granted by any
    /// system this pass (no code path currently marks a spouse "faithful" as a positive counterpart to
    /// a discovered affair); reserved here for whichever later item adds that grant, matching this
    /// file's own established "name the constant before the first caller exists" precedent.</summary>
    public static readonly DefinitionId<Trait> FaithfulTraitId = new("faithful");

    /// <summary>§4/§8's romantic-infatuation reactive trait — authored in
    /// <c>content/source/traits/romance.json</c> as a genuine opposed pair with <see
    /// cref="DisillusionedTraitId"/> (unlike <see cref="HeartbrokenTraitId"/>/<see
    /// cref="GuardedTraitId"/>, which are granted together and so are deliberately NOT authored as
    /// opposed). Not yet granted by any system this pass — reserved for a later Courtship/Autonomous
    /// Romance refinement.</summary>
    public static readonly DefinitionId<Trait> InfatuatedTraitId = new("infatuated");

    /// <summary>The reactive counterpart to <see cref="InfatuatedTraitId"/> — see that constant's own
    /// doc comment.</summary>
    public static readonly DefinitionId<Trait> DisillusionedTraitId = new("disillusioned");

    /// <summary>§3's "a character's Beauty tier... weight[s] Attraction directly" — the 4-tier
    /// <c>beauty</c> spectrum authored in <c>content/source/traits/congenital.json</c>
    /// (<c>plain</c>/<c>common</c>/<c>fair</c>/<c>striking</c>, tier positions 0-3). Not yet consumed
    /// by <see cref="RecordRomanticInteractionCommand"/> or any other Attraction math this pass — §3's
    /// own Beauty-weighting is content-authored here but left unwired, matching this document's
    /// "reserve the constant, defer the caller" precedent elsewhere in this file.</summary>
    public static readonly DefinitionId<Trait> PlainBeautyTraitId = new("plain");

    public static readonly DefinitionId<Trait> CommonBeautyTraitId = new("common");

    public static readonly DefinitionId<Trait> FairBeautyTraitId = new("fair");

    public static readonly DefinitionId<Trait> StrikingBeautyTraitId = new("striking");
}
