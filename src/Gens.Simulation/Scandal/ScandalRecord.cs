using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Scandal;

/// <summary>§4's source vocabulary — every source named in the design doc is represented, matching
/// <see cref="Legal.LegalCase.CaseType"/>'s own "every real category represented, only some reachable"
/// precedent. Only <see cref="UnjustAction"/>, <see cref="DiscoveredFabrication"/>, <see
/// cref="WeaponizedLegalCase"/>, and <see cref="IllicitCollegiumExposure"/> are ever actually recorded
/// by this item — see <see cref="RecordScandalCommand"/>'s own doc comment and each real call site
/// (<see cref="Crime.ImprisonCommand"/>, <see cref="Crime.ApplySentenceCommand"/>, <see
/// cref="DiscoverFabricationCommand"/>, <see cref="Legal.LegalCaseRuling"/>, <see
/// cref="Collegia.DissolveCollegiumCommand"/>). <see cref="AffairDiscovery"/> needs Romance, Sexuality
/// &amp; Lineage's own affair-discovery mechanic (Phase 17, unbuilt); <see
/// cref="ScandalousPerformance"/> and <see cref="FameCollapse"/> both need Games &amp; Spectacle/
/// Celebrities &amp; Influential Figures (Phase 17, unbuilt — Fame itself does not exist anywhere in
/// this codebase, confirmed by direct search, matching Phase 12 item 1's own identical finding); <see
/// cref="PublicanusCorruption"/> needs Land Ownership &amp; Real Estate's Publicanus Contract (Phase
/// 15, unbuilt); <see cref="DeliberateRumor"/> needs the "Spread a Damaging Rumor" Interaction —
/// confirmed by direct search to exist only as a named row in <c>gens-characters-design.md</c> §9.4's
/// own table, with no code anywhere in <c>Gens.Simulation.Interactions</c> implementing it.</summary>
public enum ScandalSourceType
{
    AffairDiscovery,
    UnjustAction,
    DiscoveredFabrication,
    ScandalousPerformance,
    FameCollapse,
    WeaponizedLegalCase,
    IllicitCollegiumExposure,
    PublicanusCorruption,
    DeliberateRumor,

    /// <summary>Phase 12 item 9's own real, reachable addition — a real Edict's own Reception (§5.1:
    /// "a genuine backlash chain reading Faction, affected pop groups/Curiales/Rival Houses, and
    /// severity") routed through this shared engine directly, per this type's own §1 framing as "not a
    /// new consequence system, but the shared engine... a handful of already-shipped Phase 12 moments
    /// have been quietly waiting for." See <see cref="Edicts.IssueManumissionEdictCommand"/>, <see
    /// cref="Edicts.GrantCitizenshipEdictCommand"/>, and <see cref="Edicts.IssueProscriptionCommand"/>.
    /// Purely additive: nothing in this file's own already-shipped, already-tested <see
    /// cref="RecordScandalCommand"/> pipeline switches on <see cref="ScandalSourceType"/> at all (only
    /// <see cref="ScandalSeverity"/> drives its Dignitas penalty), so appending this value changes no
    /// existing behavior.</summary>
    EdictBacklash,

    /// <summary>Phase 15 item 4's own real, reachable addition (<c>gens-notable-businesses-design.md</c>
    /// §4/§9): "a genuine business-specific Scandal — a new source this document adds to that system's
    /// own existing sourceType list" — a matter implicating a <see
    /// cref="NotableBusinesses.NotableBusiness"/>'s own conduct (adulterated goods, price gouging,
    /// cheating a supplier) rather than its owner's own personal conduct. See <see
    /// cref="NotableBusinesses.RecordBusinessScandalCommand"/>, which always suppresses the ordinary
    /// personal Dignitas penalty and Trait grant for this source (§4's own "distinct from the owner's
    /// own personal standing"), applying the real consequence to the business's own Reputation instead.
    /// Purely additive, matching <see cref="EdictBacklash"/>'s own identical reasoning: nothing in this
    /// file's already-shipped, already-tested pipeline switches on <see cref="ScandalSourceType"/> at
    /// all.</summary>
    BusinessMisconduct,

    /// <summary>Phase 15 item 5's own real, reachable addition (<c>gens-business-competition-design.md</c>
    /// §4/§10): "whether a cartel's own discovery should route through the Scandal system directly... this
    /// document assumes yes but doesn't formally amend that list itself" — this item resolves that open
    /// question by actually amending it. Unlike <see cref="BusinessMisconduct"/>, a discovered
    /// price-fixing conspiracy implicates the participating owner's own personal conduct directly (a
    /// household head conspiring against the market), so <see cref="BusinessCompetition.DiscoverCartelCommand"/>
    /// does <b>not</b> suppress the ordinary personal Dignitas penalty/Trait grant the way a Business
    /// Misconduct Scandal does. Purely additive, matching <see cref="BusinessMisconduct"/>'s own identical
    /// reasoning: nothing in this file's already-shipped, already-tested pipeline switches on <see
    /// cref="ScandalSourceType"/> at all.</summary>
    CartelDiscovery,
}

/// <summary>§6's severity ladder. <see cref="NotaCensoriaEligible"/> is kept for schema completeness
/// but reserved by this item for exactly one real, reachable path — <see
/// cref="DiscoverFabricationCommand"/>'s "retroactively the single worst-case scandal source this
/// project has built" (§4) — even though the formal Nota Censoria consequence itself (<see
/// cref="ScandalRecord.NotaCensoriaIssued"/>) never actually fires: §7 reserves that for "a sitting
/// Senator," and no Rome-track magistracy or Senator concept exists anywhere in this codebase (Phase 12
/// item 2's own doc comment omitted <c>consul</c>/<c>praetor</c>/etc. from <c>MagistracyOffice</c>
/// entirely) — the severity tier and the formal consequence it would otherwise unlock are two separate
/// facts, and this item only ever reaches the first.</summary>
public enum ScandalSeverity
{
    MinorEmbarrassment,
    PublicDisgrace,
    NotaCensoriaEligible,
}

/// <summary>§6's spread ladder. Every real trigger this item wires already carries <see
/// cref="Commands.Visibility.Public"/> at its own source event (<see
/// cref="Crime.CharacterImprisonedEvent"/>, <see cref="Legal.LegalCaseRuledEvent"/>, <see
/// cref="Collegia.CollegiumDissolvedEvent"/>) — none of them describe a matter genuinely "contained, not
/// yet public," so <see cref="HouseholdOnly"/> is never assigned. <see cref="Provincial"/> and <see
/// cref="RomeWide"/> are both gated by §6's own Prominence concept (Events §5) — confirmed by direct
/// search not to exist as a real field anywhere in this codebase (it appears only in doc-comment TODOs:
/// <c>EventWeightInputs.cs</c>, <c>MourningPeriod.cs</c>, <c>MonthlyReportProjector.cs</c>) — so <see
/// cref="RecordScandalCommand"/> always assigns <see cref="SettlementWide"/>, §6's own "ordinary default
/// once ambient spread runs its course," rather than inventing a Prominence stand-in the design doc
/// never asks for.</summary>
public enum ScandalScope
{
    HouseholdOnly,
    SettlementWide,
    Provincial,
    RomeWide,
}

/// <summary>§7/§10's two-audience reading of the same Scandal, read directly off <see
/// cref="Clientela.CharacterFactionAlignment"/> the way Phase 12 item 2 established that partition
/// (§3.1) — see <see cref="ScandalCatalog.FactionAlignedReadingPenalty"/>'s own doc comment for the
/// exact "we expected better of our own" reading this computes. Both readings equal the same base
/// severity-scaled figure whenever the scandalized household's own recorded head carries no <see
/// cref="Clientela.CharacterFactionAlignment"/> entry at all — true for nearly every Character outside
/// §3.1's own "political cast," matching that partition's identical sparse "no entry means unaligned"
/// default.</summary>
/// <param name="TraditionalistReading">How severely a Traditionalist-aligned audience reads this
/// Scandal.</param>
/// <param name="PopularistReading">How severely a Popularist-aligned audience reads this Scandal.</param>
public sealed record FactionDependentReception(int TraditionalistReading, int PopularistReading);

/// <summary>
/// One Scandal — a real, discrete, named record (§3), not a passive Dignitas modifier applied quietly
/// in the background. Kept forever once recorded, matching <see cref="Legal.LegalCase"/>'s and <see
/// cref="Crime.PunishableOffense"/>'s identical "kept for the campaign's lifetime" convention: <see
/// cref="ScandalRehabilitationSystem"/>'s own "a real, sustained stretch without further incident" gate
/// (§8) needs a scandalized household's full Scandal history, not just whether one is currently active.
///
/// <b>Household-level, matching <see cref="Legal.LegalCase"/>'s and <see
/// cref="Reputation.AdjustDignitasCommand"/>'s own identical convention</b> — §11's own data-model
/// sketch leaves <c>primaryCharacterOrHouseholdId</c> untyped, and this item resolves every real trigger
/// at the same household granularity Dignitas itself already moves at, since no Character-level
/// reputation primitive exists anywhere in this codebase to move instead.
///
/// <b>Deliberately narrower than §11's own data-model sketch in two ways, both explained directly
/// rather than silently dropped:</b> §11 also names a <c>damageControlActionsTaken</c> list and a
/// <c>dynastyChronicleEntryId</c> back-reference, neither modeled here. Damage Control (§8) is a
/// genuine, reasoned cut in its entirety — see this item's own roadmap write-up for why Suppression,
/// Spin, and Scapegoating each have no real mechanism to attach to yet. A Chronicle back-reference is
/// not merely unwired, it does not fit this codebase's own actual Chronicle architecture at all: <see
/// cref="Chronicle.ChronicleGenerationSystem"/> mints a fresh <see cref="RuntimeId{T}"/> for <see
/// cref="Chronicle.ChronicleEntry"/> strictly after the tick that produced the source event, and no
/// other domain record anywhere in this
/// codebase (not <see cref="Legal.LegalCase"/>, not <see cref="Crime.SentenceRecord"/>) is ever written
/// back to with the resulting entry ID — confirmed by direct search, not assumed. Modeling a field this
/// codebase's own real architecture has never once populated for any comparable record would invent
/// plumbing the design doc's own data-model sketch gestures at but no other Phase 12 item actually
/// builds.
/// </summary>
/// <param name="ScandalId">This record's own identity.</param>
/// <param name="PrimaryHouseholdId">The scandalized household.</param>
/// <param name="SourceType">§4 — see <see cref="ScandalSourceType"/>'s own doc comment for which values
/// this item actually reaches.</param>
/// <param name="Severity">§6 — see <see cref="ScandalSeverity"/>'s own doc comment.</param>
/// <param name="Scope">§6 — always <see cref="ScandalScope.SettlementWide"/> in this item; see <see
/// cref="ScandalScope"/>'s own doc comment for why.</param>
/// <param name="RecordedDate">When this Scandal was recorded — <see
/// cref="ScandalRehabilitationSystem"/>'s and <see cref="ScandalDecaySystem"/>'s own shared age-gate
/// anchor.</param>
/// <param name="OriginatedViaLibellusFamosus">§2/§5's anonymous-origination flag — a real,
/// source-agnostic bool any future caller can set directly through <see cref="RecordScandalCommand"/>,
/// matching <see cref="Crime.PunishableOffense.IsFabricated"/>'s identical "the flag is real, nothing
/// yet has a reason to set it true" precedent (the Libellus Famosus mechanism itself — §2, §5 — is not
/// built by this item).</param>
/// <param name="CurrentFameEffect">§7's Fame/Dignitas paradox — "can be negative or positive." Always
/// <c>null</c> in this item, matching <see cref="Epithets.Agnomen.FameEffect"/>'s identical "the field
/// exists, nothing can set it yet" precedent: Fame itself does not exist anywhere in this codebase
/// (confirmed directly, not assumed — Phase 12 item 1's own finding, reconfirmed here).</param>
/// <param name="ScandalMarkedTraitApplied">Whether the <see cref="ScandalCatalog.ScandalMarkedTraitId"/>
/// Reactive Trait actually landed on a real Character as a direct consequence of this Scandal — true
/// even when <see cref="RecordScandalCommand"/> itself skipped the grant because an earlier,
/// already-tested call site (<see cref="Legal.LegalCaseRuling"/>) had already applied it, so this field
/// still honestly reflects "yes, this household now carries the mark."</param>
/// <param name="NotaCensoriaIssued">§2/§7's formal, extreme consequence. Always <c>false</c> in this
/// item — see <see cref="ScandalSeverity.NotaCensoriaEligible"/>'s own doc comment for why the "sitting
/// Senator" precondition can never be checked.</param>
/// <param name="FactionReception">§7/§10 — see <see cref="FactionDependentReception"/>.</param>
/// <param name="IsActive">§9 — false once <see cref="ScandalDecaySystem"/> has faded this Scandal all
/// the way into pure Chronicle memory.</param>
public sealed record ScandalRecord(
    RuntimeId<ScandalRecord> ScandalId,
    RuntimeId<Household> PrimaryHouseholdId,
    ScandalSourceType SourceType,
    ScandalSeverity Severity,
    ScandalScope Scope,
    GameDate RecordedDate,
    bool OriginatedViaLibellusFamosus,
    int? CurrentFameEffect,
    bool ScandalMarkedTraitApplied,
    bool NotaCensoriaIssued,
    FactionDependentReception FactionReception,
    bool IsActive = true);

/// <summary>Read-side helpers over <see cref="WorldState.ScandalRecords"/>, matching <see
/// cref="Crime.PunishableOffenseResolver"/>'s identical "a small, hand-curated collection doesn't need a
/// maintained secondary index yet" linear-scan convention.</summary>
public static class ScandalResolver
{
    public static bool TryGet(WorldState state, RuntimeId<ScandalRecord> scandalId, out ScandalRecord? record) =>
        state.ScandalRecords.TryGet(scandalId, out record);

    /// <summary>The most recent <see cref="ScandalRecord.RecordedDate"/> for <paramref
    /// name="householdId"/>, regardless of <see cref="ScandalRecord.IsActive"/> or severity — §8's
    /// rehabilitation clock resets on "a further incident," not merely a further *active* one.</summary>
    public static GameDate? MostRecentScandalDate(WorldState state, RuntimeId<Household> householdId)
    {
        GameDate? latest = null;
        foreach (var entry in state.ScandalRecords.InAscendingOrder())
        {
            if (entry.Value.PrimaryHouseholdId != householdId)
                continue;
            if (latest is null || entry.Value.RecordedDate.TotalMonths > latest.Value.TotalMonths)
                latest = entry.Value.RecordedDate;
        }

        return latest;
    }
}
