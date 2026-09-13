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
    /// cref="EducationGateResolver.CanContestMagistracyAboveLowestRung"/> for Rhetoric specifically.</summary>
    public static bool HasCompleted(WorldState state, RuntimeId<Character> characterId, DefinitionId<EducationTrack> trackId) =>
        TryGet(state, characterId, out var enrollment) && enrollment.TrackId == trackId && enrollment.CompletedDate is not null;
}
