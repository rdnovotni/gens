using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Education;

/// <summary>Emitted once an <see cref="EducationalTrackEnrollment"/> reaches its Track's <see
/// cref="EducationTrackDefinition.CompletionMonths"/>.</summary>
public sealed record EducationalTrackCompletedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    DefinitionId<EducationTrack> TrackId,
    string? CausationId) : IDomainEvent
{
    public string Type => "education.trackCompleted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(CharacterId.ToTaggedString());
}

/// <summary>
/// The monthly Educational Track tick (Phase 17 item 2; §3): every Character with an active <see
/// cref="EducationalTrackEnrollment"/> gains their Track's <see
/// cref="EducationTrackDefinition.MonthlyAttributeGain"/> on its <see
/// cref="EducationTrackDefinition.PrimaryAttribute"/> (and <see
/// cref="EducationTrackDefinition.SecondaryAttribute"/>, if any), scaled by <see
/// cref="EducationTrackDefinition.DistinguishedTierMultiplier"/> while <see
/// cref="EducationalTrackEnrollment.DistinguishedTierActive"/> is set, applied through <see
/// cref="EducationAttributeAdjustor"/> — the first-ever writer of <see cref="Character.Attributes"/> post
/// creation (see that helper's own doc comment). Philosophy's Cultural Prestige accrual (§3) applies
/// alongside the attribute gain when the student belongs to a household. Once <see
/// cref="EducationTrackDefinition.CompletionMonths"/> of active enrollment have elapsed, the enrollment is
/// marked complete and <see cref="EducationalTrackCompletedEvent"/> fires — from that month on, a
/// completed Rhetoric Track keeps satisfying <see
/// cref="EducationGateResolver.CanContestMagistracyAboveLowestRung"/> for the rest of that Character's
/// life, per <see cref="EducationalTrackEnrollment"/>'s own doc comment.
/// </summary>
public sealed class EducationalTrackProgressSystem : IMonthlySystem<WorldState>
{
    public string Id => "education.educationalTrackProgress";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "educationalTrackEnrollments", "characters" };
    public IReadOnlyCollection<string> Writes { get; } =
        new[] { "educationalTrackEnrollments", "characters", "householdCulturalPrestiges", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: completing an enrollment replaces the entry being iterated.
        foreach (var entry in state.EducationalTrackEnrollments.InAscendingOrder().ToArray())
        {
            var enrollment = entry.Value;
            if (!EducationalTrackEnrollmentResolver.IsActive(enrollment))
                continue;
            if (!state.Characters.TryGet(enrollment.CharacterId, out var character) || !character!.IsAlive)
                continue;
            if (!KnownEducationTracks.Catalog.TryGet(enrollment.TrackId, out var track))
                continue;

            var multiplier = enrollment.DistinguishedTierActive ? track!.DistinguishedTierMultiplier : 1;
            var gain = track!.MonthlyAttributeGain * multiplier;

            var adjusted = EducationAttributeAdjustor.Apply(character, track.PrimaryAttribute, gain);
            if (track.SecondaryAttribute is { } secondary)
                adjusted = EducationAttributeAdjustor.Apply(adjusted, secondary, gain);
            state.Characters.Remove(enrollment.CharacterId);
            state.Characters.Add(enrollment.CharacterId, adjusted);

            if (track.GrantsCulturalPrestige && adjusted.Household is { } householdId)
                CulturalPrestigeResolver.Apply(state, householdId, track.MonthlyPrestigeGain);

            var monthsElapsed = context.Date.TotalMonths - enrollment.StartedDate.TotalMonths;
            if (monthsElapsed >= track.CompletionMonths)
            {
                state.EducationalTrackEnrollments.Remove(enrollment.CharacterId);
                state.EducationalTrackEnrollments.Add(enrollment.CharacterId, enrollment with { CompletedDate = context.Date });
                events.Add(new EducationalTrackCompletedEvent(
                    state.EventIds.Issue(), context.Date, enrollment.CharacterId, enrollment.TrackId, CausationId: null));
            }
        }

        return events;
    }
}
