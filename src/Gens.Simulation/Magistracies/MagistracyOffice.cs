namespace Gens.Simulation.Magistracies;

/// <summary>
/// The four achievable local offices <c>gens-politics-patronage-design.md</c> §5.1-§5.4 builds — "a
/// single settlement's own Curia," per that section's own scoping line. §11's <c>MagistracyRecord.office</c>
/// sketch also lists five Rome-track values (<c>quaestorRoman</c>, <c>aedileRoman</c>, <c>praetor</c>,
/// <c>consul</c>, <c>provincialGovernor</c>) belonging to §6's Cursus Honorum and §7's Provincial
/// Administration — both explicitly out of this item's scope (see the roadmap's own Phase 12 item
/// numbering). Those five are deliberately omitted from this enum rather than included-but-unreachable
/// dead code: nothing in this item can construct one, and adding a value with no code path to reach it
/// would misrepresent what's actually built. §6/§7's own future item is free to extend this enum (or
/// introduce its own) when it actually implements them.
/// </summary>
public enum MagistracyOffice
{
    /// <summary>§5.1 — the base Curia seat; passive Dignitas only, and the gate every office below
    /// requires holding first (§5.5: "a contested election for any office above Decurion").</summary>
    Decurion,

    /// <summary>§5.2 — public works/games funding; the ladder's one "occasional real duty" office (see
    /// <see cref="FundAedileWorksCommand"/>).</summary>
    Aedile,

    /// <summary>§5.3 — financial oversight; satisfies Economy &amp; Finance's Tax Policy
    /// "requiresOffice" gate (see <see cref="MagistracyResolver.ActiveOfficeCountForHousehold"/>'s own
    /// doc comment for why that gate isn't itself wired here).</summary>
    QuaestorLocal,

    /// <summary>§5.4 — the paired chief-magistrate office; two colleagues (see <see
    /// cref="PairDuumvirsCommand"/>), the ladder's largest passive Dignitas, and the Mint's
    /// "political milestone" gate (unwired — see that command's own doc comment).</summary>
    Duumvir,
}

/// <summary>Why a <see cref="MagistracyRecord"/>'s term ended before its next anniversary renewal
/// (§5.7's <c>lostEarlyReason</c>) — <c>null</c> on an active record, or on one that simply wasn't
/// renewed/re-won at a normal term boundary (an unchallenged non-renewal has no "early" reason to
/// record; see <see cref="MagistracyTermSystem"/>'s own doc comment for why that path isn't modeled
/// as a loss at all).</summary>
public enum MagistracyLossReason
{
    /// <summary>Lost a contested re-election (§5.5) to a challenger.</summary>
    LostReelection,

    /// <summary>Stripped by Economy &amp; Finance's Insolvency ladder (§5.7; that document's §9) —
    /// wired directly against <see cref="Economy.InsolvencyState.Stage"/> by <see
    /// cref="MagistracyTermSystem"/>, since <see cref="Economy.InsolvencySystem"/> itself doesn't yet
    /// apply its own flagged-but-unimplemented <c>officeOrCensusLoss</c> consequence (see that system's
    /// doc comment) — this item reads the Insolvency ladder's stage directly rather than waiting on
    /// that gap to close.</summary>
    Insolvency,

    /// <summary>A Legal &amp; Court conviction (§5.7's second, sharper route) — kept in this enum for
    /// schema completeness with §11's own <c>lostEarlyReason</c> vocabulary, but genuinely unreachable
    /// in this codebase today: Legal &amp; Court (§6.16) doesn't exist yet, the same unbuilt-dependency
    /// shape <see cref="Reputation.HouseholdReputation"/>'s own doc comment already used for Fame. No
    /// code path in this item can ever produce this value; wiring it is that future phase's job.</summary>
    LegalConviction,
}
