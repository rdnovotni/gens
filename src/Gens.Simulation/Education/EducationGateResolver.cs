using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Languages;
using Gens.Simulation.State;

namespace Gens.Simulation.Education;

/// <summary>
/// Phase 17 item 2 slice 2 — the Literacy hard-gate wiring (<c>gens-education-culture-design.md</c> §3,
/// §10): pure read-side helpers over state this ticket's earlier work already owns (<see
/// cref="Languages.LiteracyQueries"/>, <see cref="CulturalPrestigeResolver"/>), with no new <see
/// cref="WorldState"/> partition of its own — every method here is a query, never a mutation.
///
/// <see cref="CanHoldLearningTierRole"/> and <see cref="CanUseCorrespondence"/> both default to
/// permissive when a Character carries no explicit <see cref="LiteracyRecord"/>: §3's own "never
/// separately tracked" framing for the overwhelming majority of the population means an absent record is
/// silence, not a known negative — this domain does not compute <see
/// cref="LiteracyDerivation.LegalStatusAndWealth"/>'s ambient default on demand (that record's own doc
/// comment explicitly reserves that to a future caller), so treating "no record" as "known illiterate"
/// would retroactively fail every already-shipped <see cref="Magistracies.HoldContestedElectionCommand"/>
/// and <see cref="Correspondence.SendLetterCommand"/> test and campaign predating this ticket. Only a
/// Character an explicit <see cref="SetLiteracyCommand"/> call has actually marked <c>IsLiterate: false</c>
/// is a real, hard rejection here — matching this codebase's "sparse partition, no entry means the
/// default" convention applied to a boolean gate instead of a running score.
///
/// <see cref="CanContestMagistracyAboveLowestRung"/> was deliberately deferred past slice 2 — §3's own
/// description of that gate reads a completed Rhetoric Educational Track or a Rhodes <c>
/// CharacterInstitutionCredential</c>, and only the former exists as of this slice (the latter lands with
/// this ticket's own Institutions of Renown slice — see that slice's amendment to this method). It is
/// also deliberately never wired as a hard block on <see cref="Magistracies.HoldContestedElectionCommand"/>:
/// that command's four local offices (§5.1-§5.4) were fully shipped by Phase 12 item 2, long before this
/// ticket's Rhetoric requirement existed, and retroactively rejecting every already-accepted contested
/// election that never authored a Rhetoric track would be an undocumented regression this ticket's own
/// scope does not call for. It remains a read-side fact this domain's own Cultural Prestige/
/// marriage-market framing and future UI can surface, not a magistracy-blocking validation.
/// </summary>
public static class EducationGateResolver
{
    /// <summary>§3's magistracy-contest standing gate: true once a Character has either completed the
    /// Rhetoric Track (<see cref="EducationalTrackEnrollmentResolver.HasCompleted"/>) or holds a Rhodes
    /// Institution credential (<see cref="CharacterInstitutionCredentialResolver.HasCredentialFrom"/>) —
    /// §12's own cross-reference naming Rhodes specifically among the five Institutions. Never wired as a
    /// hard block on any existing command; see this class's own doc comment for why.</summary>
    public static bool CanContestMagistracyAboveLowestRung(WorldState state, RuntimeId<Character> characterId) =>
        EducationalTrackEnrollmentResolver.HasCompleted(state, characterId, KnownEducationTracks.Rhetoric) ||
        CharacterInstitutionCredentialResolver.HasCredentialFrom(state, characterId, KnownInstitutionsOfRenown.Rhodes);

    /// <summary>§3's Learning-tier role gate — wired into <see
    /// cref="Magistracies.HoldContestedElectionCommand"/>'s challenger check (Phase 17 item 2 slice 2).</summary>
    public static bool CanHoldLearningTierRole(WorldState state, RuntimeId<Character> characterId) =>
        IsLiterateOrUntracked(state, characterId);

    /// <summary>§3's Correspondence gate — wired into <see
    /// cref="Correspondence.SendLetterCommand"/>'s drafter check (Phase 17 item 2 slice 2).</summary>
    public static bool CanUseCorrespondence(WorldState state, RuntimeId<Character> characterId) =>
        IsLiterateOrUntracked(state, characterId);

    /// <summary>§7's marriage-market Prestige gate — a thin pass-through to <see
    /// cref="CulturalPrestigeResolver.ClearsMarriageMarketThreshold"/>, gathered here alongside this
    /// domain's other gate checks rather than requiring every caller to know which Education sub-file
    /// owns which gate.</summary>
    public static bool ClearsCulturalPrestigeMarriageThreshold(WorldState state, RuntimeId<Household> householdId) =>
        CulturalPrestigeResolver.ClearsMarriageMarketThreshold(state, householdId);

    private static bool IsLiterateOrUntracked(WorldState state, RuntimeId<Character> characterId) =>
        !LiteracyQueries.TryGet(state, characterId, out var record) || record!.IsLiterate;
}
