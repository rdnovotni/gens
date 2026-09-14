using System.Linq;
#nullable enable
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Romance;

/// <summary>How much is genuinely at stake in a discovered <see cref="AffairRecord"/> (§11) — this
/// implementation's own decisive reading of "Legitimacy genuinely contested," "a rival house
/// involved," and "a politically important marriage threatened" (§17's own guidance to make a simple,
/// stated, decisive call rather than leaving this open-ended). See <see
/// cref="AffairDiscoverySystem"/>'s own doc comment for the exact three checks that promote an
/// escalation from <see cref="Minor"/> to <see cref="HighStakes"/>.</summary>
public enum AffairStakesLevel
{
    Minor,
    HighStakes,
}

/// <summary>How a discovered affair was ultimately resolved (§11, §12). Only <see
/// cref="QuietlyResolved"/> is ever produced by this item's own <see cref="AffairDiscoverySystem"/> —
/// every other value is modeled for schema completeness, matching <see
/// cref="Legal.LegalCase"/>'s own "every real category represented, only some reachable" precedent,
/// and is left for a later slice's own <c>ResolveAffairCommand</c> (<see cref="Forgiven"/>, <see
/// cref="Divorced"/>, <see cref="Challenged"/>) and Adultery legal case (<see
/// cref="ProsecutedAdultery"/>), plus the narrowly-gated <see
/// cref="ExtremeLegalRemedyExercised"/> (§12, §17).</summary>
public enum AffairResolution
{
    QuietlyResolved,
    Forgiven,
    Divorced,
    Challenged,
    ProsecutedAdultery,
    ExtremeLegalRemedyExercised,
}

/// <summary>
/// One discovered affair (<c>gens-romance-sexuality-lineage-design.md</c> §11) — created <i>only at
/// discovery</i>, by <see cref="AffairDiscoverySystem"/>, never before: an undiscovered affair is just
/// a private, un-<see cref="RomanticBond.IsKnownPublicly"/> <see cref="RomanticBond"/>, and gets no
/// <see cref="AffairRecord"/> of its own until that changes. Kept forever once created, matching <see
/// cref="Scandal.ScandalRecord"/>'s and <see cref="Succession.SuccessionDispute"/>'s identical
/// "resolved or not, kept for the campaign's lifetime" convention.
///
/// <see cref="OffenderCharacterId"/> is whichever bond participant actually held the open outside
/// marriage this affair betrays; <see cref="ThirdPartyCharacterId"/> is the other bond participant;
/// <see cref="WrongedSpouseId"/> is that marriage's other side. When BOTH bond participants turn out to
/// hold an open marriage to someone else (each cheating on their own spouse with the other), this
/// record still only names one side as "the" offender/wronged-spouse pair — the canonically smaller
/// <see cref="RomanticBondKey.CharacterAId"/>'s own marriage, deterministically, per <see
/// cref="AffairDiscoverySystem"/>'s own resolution helper — while the OTHER real wronged spouse still
/// gets the same reactive trait grant as this record's own named one (see that system's own doc
/// comment); this record's fixed, single-<see cref="WrongedSpouseId"/> shape is simply not rich enough
/// to name a second one without inventing a plural field the approved plan's own schema does not ask
/// for. <c>LegalCaseId</c> (a later slice's own field, once Adultery legal cases exist) is deliberately
/// not modeled here yet — adding a field nobody sets would be a forward declaration this item has no
/// need to make.
/// </summary>
/// <param name="StakesLevel">§11 — see <see cref="AffairStakesLevel"/>'s own doc comment.</param>
/// <param name="InvolvesRivalHouse">Whether either bond participant is a tracked Rival House's own
/// head (§11's first <see cref="AffairStakesLevel.HighStakes"/> trigger) — see <see
/// cref="AffairDiscoverySystem"/>'s own doc comment for exactly how "tracked Rival House" is
/// resolved.</param>
/// <param name="LegitimacyContested">Whether either bond participant has an unresolved <see
/// cref="PregnancyRecord"/> conceived via this exact <see cref="RomanticBondType.Affair"/> bond at the
/// moment of discovery (§11's second trigger — "a real child is at stake").</param>
/// <param name="ThreatensPoliticalMarriage">Whether a real wronged spouse's own household currently
/// holds an active Magistracy, or that spouse carries a <see cref="BondTag.Patron"/>/<see
/// cref="BondTag.Client"/> tie of note (§11's third trigger — this implementation's own decisive,
/// stated reading of "a politically important marriage" per §17's guidance).</param>
/// <param name="Resolution"><c>null</c> while unresolved (every <see
/// cref="AffairStakesLevel.HighStakes"/> record this item creates); see <see
/// cref="AffairResolution"/>'s own doc comment for which values a later slice actually produces.</param>
/// <param name="StatusRoleDignitasModifier">§13's <see cref="LegalStatus"/>-comparison Dignitas
/// modifier — a later slice's own shared pure calculator populates this; always <c>0</c> for every
/// record this item creates (see that later slice's own <c>StatusRoleDignitasModifier</c> calculator,
/// not yet built).</param>
/// <param name="DiscoveredDate">When <see cref="AffairDiscoverySystem"/> rolled this affair as
/// Escalated rather than Foiled.</param>
public sealed record AffairRecord(
    RuntimeId<AffairRecord> AffairId,
    RuntimeId<Character> OffenderCharacterId,
    RuntimeId<Character> ThirdPartyCharacterId,
    RuntimeId<Character> WrongedSpouseId,
    AffairStakesLevel StakesLevel,
    bool InvolvesRivalHouse,
    bool LegitimacyContested,
    bool ThreatensPoliticalMarriage,
    AffairResolution? Resolution,
    int StatusRoleDignitasModifier,
    GameDate DiscoveredDate);
