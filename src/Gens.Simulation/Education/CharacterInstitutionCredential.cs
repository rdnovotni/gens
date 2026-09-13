using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Education;

/// <summary>
/// A permanent per-Character credential earned by completing a Study Abroad Journey at one of the five
/// Institutions of Renown (Phase 17 item 2; §12's own <c>CharacterInstitutionCredential</c> model), keyed
/// by the Character it describes — one credential per Character, matching <see
/// cref="Languages.LiteracyRecord"/>'s identical "the owning entity is already a unique key" shape. A
/// Character who studies abroad a second time (at a different institution) simply replaces this entry —
/// nothing in this ticket's scope stacks multiple simultaneous credentials, matching <see
/// cref="EducationalTrackEnrollment"/>'s own "starting a second Track replaces the first" precedent.
///
/// Read by: Track progression speed (a forward hook only — no consumer wired in this ticket), Symposium/
/// Patronage bonus (a forward hook only, per <see cref="CulturalPatronageCycleSystem"/>'s own doc
/// comment), marriage/magistracy gates (<see
/// cref="EducationGateResolver.CanContestMagistracyAboveLowestRung"/>, Rhodes specifically), and a
/// medicine-specialty flag left as a forward hook only — no Disease &amp; Public Health consumer exists
/// yet, matching <see cref="Religion.PatronDeity"/>'s own "shared-axis-first, bespoke-hooks-later"
/// precedent.
/// </summary>
public sealed record CharacterInstitutionCredential(
    RuntimeId<Character> CharacterId,
    DefinitionId<InstitutionOfRenown> InstitutionId,
    GameDate GrantedDate);

/// <summary>Read-side helpers over <see cref="WorldState.CharacterInstitutionCredentials"/>.</summary>
public static class CharacterInstitutionCredentialResolver
{
    public static bool TryGet(WorldState state, RuntimeId<Character> characterId, out CharacterInstitutionCredential credential) =>
        state.CharacterInstitutionCredentials.TryGet(characterId, out credential);

    public static bool HasCredentialFrom(WorldState state, RuntimeId<Character> characterId, DefinitionId<InstitutionOfRenown> institutionId) =>
        TryGet(state, characterId, out var credential) && credential.InstitutionId == institutionId;

    public static void Grant(WorldState state, RuntimeId<Character> characterId, DefinitionId<InstitutionOfRenown> institutionId, GameDate date)
    {
        if (state.CharacterInstitutionCredentials.TryGet(characterId, out _))
            state.CharacterInstitutionCredentials.Remove(characterId);
        state.CharacterInstitutionCredentials.Add(characterId, new CharacterInstitutionCredential(characterId, institutionId, date));
    }
}
