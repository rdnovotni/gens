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

/// <summary>Emitted whenever a Character's accrued Culture drift crosses <see
/// cref="EducationCulturalDriftCatalog.DriftThresholdMonths"/> and their <see cref="Character.Culture"/>
/// is reassigned (Phase 17 item 2; §2) — the only place that field changes post-creation.</summary>
public sealed record CultureDriftedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    DefinitionId<Identity.Culture> FromCultureId,
    DefinitionId<Identity.Culture> ToCultureId,
    string? CausationId) : IDomainEvent
{
    public string Type => "education.cultureDrifted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// The monthly Culture drift tick (Phase 17 item 2; §2): every Character with an active <see
/// cref="CulturalDriftState"/> accrues <see cref="EducationCulturalDriftCatalog.FastDriftMonthsPerMonth"/>
/// (Childhood/Adolescence) or <see cref="EducationCulturalDriftCatalog.SlowDriftMonthsPerMonth"/>
/// (Adult/Elderly), doubled again by <see
/// cref="EducationCulturalDriftCatalog.ForeignTutorAccelerationMultiplier"/> while that Character's own
/// household holds an active <see cref="EducationRole.ForeignTutor"/> assignment whose tutor's own <see
/// cref="Character.Culture"/> matches the drift's target (§3.3's "accelerated while an active Foreign
/// Tutor assignment points at a different culture"). Study Abroad's own sharper acceleration (§4) is not
/// applied here — that hook belongs to this ticket's later Institutions of Renown slice, which reads and
/// writes this same <see cref="CulturalDriftState"/> partition directly from its own system rather than
/// this one needing to know about a Travel concept it has no dependency on.
/// </summary>
public sealed class CulturalDriftSystem : IMonthlySystem<WorldState>
{
    public string Id => "education.culturalDrift";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "culturalDriftStates", "characters", "educationRoleAssignments" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "culturalDriftStates", "characters", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: crossing the threshold removes the entry being iterated.
        foreach (var entry in state.CulturalDriftStates.InAscendingOrder().ToArray())
        {
            var drift = entry.Value;
            if (!state.Characters.TryGet(drift.CharacterId, out var character) || !character!.IsAlive)
                continue;

            var stage = character.GetLifecycleStage(state.Date);
            var rate = stage is LifecycleStage.Infant or LifecycleStage.Child or LifecycleStage.Adolescent
                ? EducationCulturalDriftCatalog.FastDriftMonthsPerMonth
                : EducationCulturalDriftCatalog.SlowDriftMonthsPerMonth;

            if (character.Household is { } householdId &&
                state.EducationRoleAssignments.TryGet(householdId, out var assignment) &&
                assignment!.Role == EducationRole.ForeignTutor &&
                state.Characters.TryGet(assignment.TutorId, out var tutor) &&
                tutor!.Culture == drift.TargetCultureId)
            {
                rate *= EducationCulturalDriftCatalog.ForeignTutorAccelerationMultiplier;
            }

            var progress = drift.ProgressMonths + rate;
            if (progress < EducationCulturalDriftCatalog.DriftThresholdMonths)
            {
                state.CulturalDriftStates.Remove(drift.CharacterId);
                state.CulturalDriftStates.Add(drift.CharacterId, drift with { ProgressMonths = progress });
                continue;
            }

            var fromCulture = character.Culture;
            state.Characters.Remove(drift.CharacterId);
            state.Characters.Add(drift.CharacterId, character with { Culture = drift.TargetCultureId });
            state.CulturalDriftStates.Remove(drift.CharacterId);

            events.Add(new CultureDriftedEvent(
                state.EventIds.Issue(), context.Date, drift.CharacterId, fromCulture, drift.TargetCultureId, CausationId: null));
        }

        return events;
    }
}
