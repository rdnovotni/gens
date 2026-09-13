using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Education;

/// <summary>
/// One Character's Educational Track enrollment (Phase 17 item 2; §3), keyed by the Character it
/// describes — one per Character, matching <see cref="Languages.LiteracyRecord"/>'s identical "the
/// owning entity is already a unique key" shape. <see cref="CompletedDate"/> null means still in
/// progress; a completed enrollment is replaced, not removed, so a completed Rhetoric Track keeps
/// satisfying <see cref="EducationGateResolver.CanContestMagistracyAboveLowestRung"/> for the rest of
/// that Character's life. Starting a second Track after completing (or abandoning) a first replaces this
/// entry outright — nothing in this ticket's scope models concurrent enrollment in two Tracks at once.
/// </summary>
public sealed record EducationalTrackEnrollment(
    RuntimeId<Character> CharacterId,
    DefinitionId<EducationTrack> TrackId,
    GameDate StartedDate,
    bool DistinguishedTierActive = false,
    GameDate? CompletedDate = null);

/// <summary>Read-side helpers over <see cref="WorldState.EducationalTrackEnrollments"/>.</summary>
public static class EducationalTrackEnrollmentResolver
{
    public static bool TryGet(WorldState state, RuntimeId<Character> characterId, out EducationalTrackEnrollment enrollment) =>
        state.EducationalTrackEnrollments.TryGet(characterId, out enrollment);

    public static bool IsActive(EducationalTrackEnrollment enrollment) => enrollment.CompletedDate is null;

    /// <summary>True once a Character has ever completed <paramref name="trackId"/> — used by <see
    /// cref="EducationGateResolver.CanContestMagistracyAboveLowestRung"/> for Rhetoric specifically.
    /// Reads <see cref="CompletedEducationalTracksResolver"/>'s own permanent history rather than this
    /// replaceable active-enrollment slot (correctness fix: starting a second Track after completing the
    /// first used to overwrite/delete the only record of that completion, silently revoking a supposedly
    /// permanent effect — see <see cref="CompletedEducationalTracks"/>'s own doc comment).</summary>
    public static bool HasCompleted(WorldState state, RuntimeId<Character> characterId, DefinitionId<EducationTrack> trackId) =>
        CompletedEducationalTracksResolver.HasCompleted(state, characterId, trackId);
}

/// <summary>Cumulative history of every Educational Track a Character has ever completed (Phase 17 item
/// 2 correctness fix; §3's own "a completed Rhetoric Track's magistracy-qualification effect is meant to
/// be permanent" reading), keyed by the Character it describes — unlike <see
/// cref="EducationalTrackEnrollment"/>'s single replaceable active-enrollment slot, this list is
/// append-only: starting (and later completing) a different Track after finishing an earlier one adds to
/// this list rather than displacing it, so <see cref="EducationalTrackEnrollmentResolver.HasCompleted"/>
/// keeps reading true for a Track finished long ago even once the active enrollment slot has moved on to
/// something else. Sparse: a Character who has never completed a Track has no entry.</summary>
public sealed record CompletedEducationalTracks(
    RuntimeId<Character> CharacterId,
    IReadOnlyList<DefinitionId<EducationTrack>> TrackIds);

/// <summary>Read/write helpers over <see cref="WorldState.CompletedEducationalTracks"/>.</summary>
public static class CompletedEducationalTracksResolver
{
    public static bool HasCompleted(WorldState state, RuntimeId<Character> characterId, DefinitionId<EducationTrack> trackId) =>
        state.CompletedEducationalTracks.TryGet(characterId, out var record) && record.TrackIds.Contains(trackId);

    /// <summary>Appends <paramref name="trackId"/> to <paramref name="characterId"/>'s permanent
    /// completed-track history. A no-op if it is already recorded, so re-recording the same completion
    /// never duplicates the entry.</summary>
    public static void Record(WorldState state, RuntimeId<Character> characterId, DefinitionId<EducationTrack> trackId)
    {
        if (state.CompletedEducationalTracks.TryGet(characterId, out var existing))
        {
            if (existing.TrackIds.Contains(trackId))
                return;

            state.CompletedEducationalTracks.Remove(characterId);
            state.CompletedEducationalTracks.Add(
                characterId, existing with { TrackIds = existing.TrackIds.Append(trackId).ToArray() });
            return;
        }

        state.CompletedEducationalTracks.Add(characterId, new CompletedEducationalTracks(characterId, new[] { trackId }));
    }
}
