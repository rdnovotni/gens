using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;

namespace Gens.Simulation.Legal;

/// <summary>Versioned constants for Phase 12 item 4's Law &amp; Court mechanics
/// (<c>gens-legal-court-design.md</c>), matching <see cref="Magistracies.MagistracyCatalog"/>'s identical
/// "unsized against real playtest data, but named in one place" convention — §12's Open Questions
/// explicitly leaves the Quick Resolution weighting formula, a Major case's typical duration,
/// verdict-probability curves, the filing-cost scale, and the Dismissed-verdict Dignitas penalty all
/// unsized.</summary>
public static class LegalCatalog
{
    /// <summary>§4: "filing isn't free... a real, if modest, [cost] attaches to bringing any case,
    /// scaling with depth."</summary>
    public static readonly Money QuickFilingCost = Money.FromDenarii(5);
    public static readonly Money MajorFilingCost = Money.FromDenarii(20);

    /// <summary>§4: "a Dismissed verdict carries a further, sharper cost specifically for the filer... a
    /// small Dignitas cost distinct from — and worse than — honestly losing a well-argued case."</summary>
    public const int DismissalDignitasPenalty = 8;

    /// <summary>§6's extra severity on top of <see cref="DismissalDignitasPenalty"/> for the household
    /// head who exercised a politically-weaponized Patria Potestas — the case is still always Dismissed
    /// (§6: "no court can formally override"), but the social cost of it having been brought at all is
    /// real and worse than an ordinary meritless filing.</summary>
    public const int PatriaPotestasCaseDignitasPenalty = 15;

    /// <summary>§5: a Major case runs "over real time, not an instant" — the number of months <see
    /// cref="LegalCaseAdvancementSystem"/> lets Evidence &amp; Testimony gathering continue before moving
    /// to the Hearing.</summary>
    public const int MajorCaseEvidenceGatheringMonths = 3;

    /// <summary>§8's case-strength gains from a submitted <see cref="TestimonySubmittedEvent"/> or <see
    /// cref="EvidenceGatheredEvent"/> — "a Legal Scholar Trait gives real argument-construction weight
    /// beyond raw Learning," applied as a flat bonus on top of the base gain (a direct trait-id
    /// membership check, matching <see cref="Religion.ReligionCatalog"/>'s own "no compiled TraitCatalog
    /// reachable here" precedent — see <see cref="LegalScholarTraitId"/>).</summary>
    public const int TestimonyCaseStrengthGain = 5;
    public const int EvidenceCaseStrengthGain = 5;
    public const int LegalScholarCaseStrengthBonus = 8;

    /// <summary>§4: "each party's existing Dignitas [is] a real thumb on the scale." Dignitas divided by
    /// this factor is added directly to that party's case score.</summary>
    public const int DignitasThumbDivisor = 5;

    /// <summary>§7: a bribe's Denarii amount converts to case-score weight at this rate, capped at <see
    /// cref="MaxBriberyWeight"/> so a sufficiently wealthy household can't simply buy an unlosable
    /// case.</summary>
    public const int BriberyWeightPerTenDenarii = 1;
    public const int MaxBriberyWeight = 25;

    /// <summary>§9's verdict-roll shape: the flat chance any case (Quick or Major) comes back Dismissed
    /// regardless of either side's score ("insufficient case either way"), and — for <see
    /// cref="LegalCaseType.PropertyLand"/>/<see cref="LegalCaseType.Contract"/> cases with a close
    /// score — the chance of a <see cref="LegalCaseVerdict.SplitCompromise"/> instead ("a real middle
    /// outcome, common for property and contract disputes specifically").</summary>
    public const int DismissalChancePercent = 8;
    public const int SplitCompromiseChancePercent = 12;
    public const int SplitCompromiseMarginThreshold = 5;

    /// <summary>The plaintiff's win-share floor/ceiling once <see cref="DismissalChancePercent"/> and (if
    /// eligible) <see cref="SplitCompromiseChancePercent"/> are already spent — the score margin shifts a
    /// 50/50 split within this band rather than ever reaching a guaranteed or impossible outcome.</summary>
    public const int MinVerdictChancePercent = 10;
    public const int MaxVerdictChancePercent = 90;

    /// <summary>§9's capital-case sentence split: a <see cref="LegalCaseVerdict.Convicted"/> verdict with
    /// a score margin at or beyond this threshold rolls <see cref="LegalSentence.Exile"/> instead of the
    /// default <see cref="LegalSentence.Fine"/> — a stronger case against the defendant reads as a more
    /// severe, better-proven charge.</summary>
    public const int SevereConvictionMarginThreshold = 15;

    public static readonly Money FineSentenceAmount = Money.FromDenarii(40);

    /// <summary>§9: "a Dignitas shift for both parties" — the ordinary civil win/loss swing.</summary>
    public const int WinnerDignitasGain = 6;
    public const int LoserDignitasLoss = 6;

    /// <summary>A compromise costs and pays less than a clean win/loss — real, but "a real middle
    /// outcome" per §5's own framing extends to its consequences too.</summary>
    public const int SplitCompromiseDignitasSwing = 2;

    /// <summary>§9's capital-case swing — a conviction is a sharper Dignitas loss than an ordinary civil
    /// defeat, and an acquittal a real vindication.</summary>
    public const int ConvictedDignitasLoss = 15;
    public const int AcquittedDignitasGain = 5;

    /// <summary>§9: "a relationship-web scar" — the <see cref="RecordInteractionCommand"/> opinion swing
    /// this item applies between the two household heads on a clean win/loss or capital verdict (not a
    /// Dismissal or a Split Compromise, per that verdict's own milder framing).</summary>
    public const int RelationshipScarOpinionDelta = -20;

    /// <summary>§6.2/§10's Piety-tier precedent applied to Law &amp; Court: content-authored trait ids
    /// (<c>content/source/traits/legal.json</c>) read directly off <see cref="Character.Traits"/>, since
    /// no compiled <c>TraitCatalog</c> is reachable from a command or monthly system in this
    /// codebase.</summary>
    public static readonly DefinitionId<Trait> LegalScholarTraitId = new("legal-scholar");
    public static readonly DefinitionId<Trait> LitigiousTraitId = new("litigious");
    public static readonly DefinitionId<Trait> ScandalMarkedTraitId = new("scandal-marked");
}
