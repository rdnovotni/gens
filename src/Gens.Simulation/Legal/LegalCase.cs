using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Magistracies;
using Gens.Simulation.Random;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Legal;

/// <summary>§2's case-type vocabulary. Every real design-doc category is represented, but this item does
/// not build a resolution path from any other domain into most of them (Debt's own "Legal exposure" step,
/// Succession's Declaration/Disownment challenges, Slave Market warranty claims, a Military captive's
/// legal disposition — none of those callers exist yet, matching <see
/// cref="Reputation.AdjustDignitasCommand"/>'s own "no such caller exists yet" precedent applied to an
/// enum instead of a command). <see cref="Criminal"/> and <see cref="Political"/> are the two types this
/// item's own <see cref="LegalCaseResolver"/> treats as capital-shaped (§9: "Acquitted, or Convicted with
/// an actual sentence") rather than the ordinary Plaintiff/Defendant/Split shape every other type
/// resolves to.</summary>
public enum LegalCaseType
{
    PropertyLand,
    Contract,
    Debt,
    SlaveOwnership,
    Succession,
    Criminal,
    Political,
    Family,
    Military,
}

/// <summary>§4 vs §5: a Quick case resolves in the single <see cref="FileLawsuitCommand"/> submission
/// that opens it; a Major case runs the full <see cref="LegalCaseStage"/> progression over real
/// time.</summary>
public enum LegalCaseDepth
{
    Quick,
    Major,
}

/// <summary>§5's four-stage Major process (§11's own data model). A Quick case skips straight from
/// nonexistence to <see cref="Ruled"/> inside <see cref="FileLawsuitCommand"/> itself and never occupies
/// <see cref="Filed"/>/<see cref="EvidenceGathering"/>/<see cref="Hearing"/> at all.</summary>
public enum LegalCaseStage
{
    Filed,
    EvidenceGathering,
    Hearing,
    Ruled,
}

/// <summary>§9's real outcome diversity — "never a flat binary." <see cref="Plaintiff"/>/<see
/// cref="Defendant"/>/<see cref="SplitCompromise"/> are the ordinary civil shape; <see cref="Acquitted"/>/
/// <see cref="Convicted"/> replace them for <see cref="LegalCaseType.Criminal"/>/<see
/// cref="LegalCaseType.Political"/> cases. <see cref="Dismissed"/> is common to both shapes.</summary>
public enum LegalCaseVerdict
{
    Dismissed,
    Plaintiff,
    Defendant,
    SplitCompromise,
    Acquitted,
    Convicted,
}

/// <summary>§9's sentence vocabulary for a <see cref="LegalCaseVerdict.Convicted"/> verdict. Only <see
/// cref="Fine"/> and <see cref="Exile"/> are ever actually rolled by <see
/// cref="LegalCaseResolver.RollVerdict"/> — <see cref="DebtBondage"/> needs Economy &amp; Finance's own
/// debt-bondage mechanism to receive it meaningfully (that mechanism exists for a defaulted loan, not for
/// an arbitrary criminal sentence) and <see cref="Execution"/> needs a real, deliberate "this item ends a
/// Character's life" decision this pass does not make casually; both are kept in the enum for schema
/// completeness, matching <see cref="MagistracyLossReason.LegalConviction"/>'s own precedent for a
/// design-doc value kept modeled-but-unreached until a later pass deliberately wires it.</summary>
public enum LegalSentence
{
    Fine,
    Exile,
    DebtBondage,
    Execution,
}

/// <summary>Which side of a <see cref="LegalCase"/> a <see cref="SubmitTestimonyCommand"/> or <see
/// cref="GatherEvidenceCommand"/> supports (§8).</summary>
public enum LegalCaseSide
{
    Plaintiff,
    Defendant,
}

/// <summary>
/// One formal dispute (Phase 12 item 4; §11's own data-model sketch). Kept forever once filed, ruled or
/// not, matching <see cref="MagistracyRecord"/>'s identical "kept for the campaign's lifetime" convention
/// — a case's own history (who sued whom, over what, and how it came out) is exactly the kind of record
/// a future Dynasty Chronicle/Legal-history query needs the full log for, not just the live docket.
///
/// <b>Household-level parties, by deliberate scope decision:</b> §11's own sketch leaves
/// <c>plaintiffId</c>/<c>defendantId</c> untyped. This item resolves every case at <see
/// cref="Household"/> granularity — the same unit <see cref="Reputation.AdjustDignitasCommand"/> already
/// moves and <see cref="Chronicle.ChronicleProjector"/> already links entries to — rather than at
/// individual <see cref="Character"/> granularity. §6's Patria Potestas case (brought against a specific
/// dependent) is the one design-doc scenario this simplification visibly flattens: no Character-level
/// standing/reputation primitive exists anywhere in this codebase to move for a single dependent instead
/// of their whole household, so <see cref="IsPatriaPotestasCase"/> is tracked as a flag on an otherwise
/// ordinary household-vs-household case rather than this item inventing a second, Character-scoped
/// version of Dignitas just to name the exact victim.
/// </summary>
/// <param name="PresidingCharacterId">Null until §3's presiding assignment succeeds — see <see
/// cref="LegalCaseResolver.SelectPresidingMagistrate"/>. Stays null when no eligible, non-recused
/// Decurion sits at <see cref="SettlementId"/> (§12's own "small-settlement recusal chain... isn't
/// specified" open question; this item leaves such a case presider-less rather than inventing a
/// generated-NPC-magistrate fallback Characters §11's "generate on demand" principle has no concrete
/// entry point for in this domain).</param>
/// <param name="PresidingCharacterScouted">§3: whether an Intrigue-driven inquiry or a Legal Scholar's
/// own professional knowledge has already revealed the presider's real leanings before the Hearing. A
/// flag rather than a stored copy of the presider's Axes/Traits — those are already directly readable off
/// the live <see cref="Character"/> record once <see cref="PresidingCharacterId"/> is known; this item's
/// own contribution is only the "has this actually been scouted yet" gate a query layer can read before
/// deciding whether to show them, matching <see cref="Epithets.Agnomen"/>'s own "the flag is the
/// documented hook" precedent for a consequence this pass names but does not build the presentation
/// layer for.</param>
/// <param name="PlaintiffBriberyWeight">§7's Bribery input, accumulated across every <see
/// cref="OfferBribeCommand"/> a party has submitted, capped at <see
/// cref="LegalCatalog.MaxBriberyWeight"/> — the weighted-check "thumb on the scale" <see
/// cref="LegalCaseResolver.RollVerdict"/> reads directly alongside case strength and Dignitas.</param>
/// <param name="IsPatriaPotestasCase">§6: flags "a case that can't win but can still hurt" — <see
/// cref="LegalCaseResolver.RollVerdict"/> forces this to <see cref="LegalCaseVerdict.Dismissed"/>
/// unconditionally regardless of any rolled score ("no court can formally override an exercise of this
/// authority"), while <see cref="LegalCaseAdvancementSystem"/> still applies §6's own real social cost —
/// a harsher Dignitas penalty and a Scandal-Marked <see cref="Trait"/> for the defendant's household
/// head — on top of the ordinary dismissal penalty every other dismissed filer pays.</param>
public sealed record LegalCase(
    RuntimeId<LegalCase> CaseId,
    LegalCaseType CaseType,
    RuntimeId<Household> PlaintiffId,
    RuntimeId<Household> DefendantId,
    RuntimeId<Settlement> SettlementId,
    LegalCaseDepth Depth,
    LegalCaseStage Stage,
    GameDate FiledDate,
    RuntimeId<Character>? PresidingCharacterId = null,
    bool PresidingCharacterScouted = false,
    int PlaintiffCaseStrength = 0,
    int DefendantCaseStrength = 0,
    int PlaintiffBriberyWeight = 0,
    int DefendantBriberyWeight = 0,
    bool IsPatriaPotestasCase = false,
    LegalCaseVerdict? Verdict = null,
    LegalSentence? Sentence = null,
    GameDate? RuledDate = null);

/// <summary>Read-side helpers over <see cref="WorldState.LegalCases"/>, matching <see
/// cref="MagistracyResolver"/>'s identical "a small, hand-curated collection doesn't need a maintained
/// secondary index yet" linear-scan convention, plus the shared verdict-rolling logic both <see
/// cref="FileLawsuitCommand"/> (Quick) and <see cref="LegalCaseAdvancementSystem"/> (Major) call rather
/// than each reimplementing §4/§9's weighted-check formula.</summary>
public static class LegalCaseResolver
{
    /// <summary>§3's presiding assignment: the first active Decurion at <paramref name="settlementId"/>
    /// whose own household is neither party to the case ("automatic recusal applies the instant the
    /// player's own household is a party"). Returns null when every active Decurion is conflicted, or
    /// none exists — see <see cref="LegalCase.PresidingCharacterId"/>'s own doc comment for why this item
    /// leaves that gap open rather than generating a fallback NPC magistrate.</summary>
    public static RuntimeId<Character>? SelectPresidingMagistrate(
        WorldState state, RuntimeId<Settlement> settlementId, RuntimeId<Household> plaintiffId, RuntimeId<Household> defendantId)
    {
        foreach (var seat in MagistracyResolver.ActiveCuriaSeats(state, settlementId))
        {
            if (!state.Characters.TryGet(seat.HolderId, out var holder) || holder is null)
                continue;
            if (holder.Household == plaintiffId || holder.Household == defendantId)
                continue;

            return seat.HolderId;
        }

        return null;
    }

    /// <summary>§4/§9's single weighted check, shared by Quick Resolution and a Major Hearing alike:
    /// each party's own case strength (testimony/evidence gathered), each party's existing Dignitas as a
    /// real "thumb on the scale" (§4: "shaped by reputation... a deliberate feature of this setting"),
    /// and any Bribery weight already offered against the presider's Greed (§7 — read here as a flat
    /// score contribution rather than a Greed-axis-gated check, since no per-Character axis score is
    /// reachable from this domain; see <see cref="Religion.ReligionCatalog"/>'s own identical "no
    /// compiled catalog reachable here" precedent for reading personality through trait ids instead).
    /// <see cref="LegalCase.IsPatriaPotestasCase"/> short-circuits straight to <see
    /// cref="LegalCaseVerdict.Dismissed"/> before any of that is even computed (§6: "no court can
    /// formally override").</summary>
    public static (LegalCaseVerdict Verdict, LegalSentence? Sentence) RollVerdict(
        WorldState state, LegalCase legalCase, RandomStreamSet randomStreams, string streamName)
    {
        if (legalCase.IsPatriaPotestasCase)
            return (LegalCaseVerdict.Dismissed, null);

        var plaintiffScore = legalCase.PlaintiffCaseStrength
            + DignitasResolver.Current(state, legalCase.PlaintiffId) / LegalCatalog.DignitasThumbDivisor
            + legalCase.PlaintiffBriberyWeight;
        var defendantScore = legalCase.DefendantCaseStrength
            + DignitasResolver.Current(state, legalCase.DefendantId) / LegalCatalog.DignitasThumbDivisor
            + legalCase.DefendantBriberyWeight;
        var margin = plaintiffScore - defendantScore;

        var roll = (int)randomStreams.NextUInt(streamName, 100);
        var isCapital = legalCase.CaseType is LegalCaseType.Criminal or LegalCaseType.Political;

        if (roll < LegalCatalog.DismissalChancePercent)
            return (LegalCaseVerdict.Dismissed, null);

        var cursor = LegalCatalog.DismissalChancePercent;
        var isSplitEligible = !isCapital && legalCase.CaseType is LegalCaseType.PropertyLand or LegalCaseType.Contract;
        if (isSplitEligible && Math.Abs(margin) <= LegalCatalog.SplitCompromiseMarginThreshold)
        {
            if (roll < cursor + LegalCatalog.SplitCompromiseChancePercent)
                return (LegalCaseVerdict.SplitCompromise, null);
            cursor += LegalCatalog.SplitCompromiseChancePercent;
        }

        var remaining = 100 - cursor;
        var plaintiffShare = Math.Clamp(50 + margin, LegalCatalog.MinVerdictChancePercent, LegalCatalog.MaxVerdictChancePercent);
        var plaintiffThreshold = cursor + (remaining * plaintiffShare / 100);
        var plaintiffPrevails = roll < plaintiffThreshold;

        if (!isCapital)
            return (plaintiffPrevails ? LegalCaseVerdict.Plaintiff : LegalCaseVerdict.Defendant, null);

        if (!plaintiffPrevails)
            return (LegalCaseVerdict.Acquitted, null);

        var sentence = margin >= LegalCatalog.SevereConvictionMarginThreshold ? LegalSentence.Exile : LegalSentence.Fine;
        return (LegalCaseVerdict.Convicted, sentence);
    }
}
