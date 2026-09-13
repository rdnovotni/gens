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

/// <summary>
/// Enrolls a Character in one of <see cref="KnownEducationTracks"/>'s three Educational Tracks (Phase 17
/// item 2; §3). Gated on <see cref="LifecycleStage.Adolescent"/> (§3's own stage framing) and on not
/// already holding an active enrollment (see <see cref="EducationalTrackEnrollment"/>'s own doc comment
/// for why a second, concurrent Track is out of scope). Deliberately never checks <see cref="Sex"/> —
/// §10's own "daughters may take any Track" decision this ticket confirmed with the user: downstream
/// framing differs for Rhetoric (marriage-negotiation leverage/Clientela/Symposium hosting rather than
/// magistracy access), not eligibility to start. Also gated (correctness fix) on the student's own
/// household actually owning/occupying an <see cref="EducationBuildingResolver.HasOperationalBuilding"/>
/// building matching the Track's own <see cref="EducationTrackDefinition.DeliveringBuildingIds"/> — a
/// Track has nowhere to be taught without one.
///
/// Deliberate scope decision: this check runs once, at enrollment. <see
/// cref="EducationalTrackProgressSystem"/> does not re-check building availability every month and pause
/// progress if the delivering building is later destroyed or loses its staffing — unlike <see
/// cref="Companions.PositionVacancySystem"/>'s own monthly re-validation of its own preconditions, doing
/// the same here would need <see cref="EducationalTrackEnrollment"/>'s completion timer to switch from a
/// simple "months since <see cref="EducationalTrackEnrollment.StartedDate"/>" calculation to an
/// incrementally-accumulated "months actually progressed" counter (so a paused month doesn't silently
/// still count toward <see cref="EducationTrackDefinition.CompletionMonths"/>) — a materially larger,
/// hash-shape-changing surgery than this fix's own scope. Left as an explicit follow-up rather than
/// implemented here.
/// </summary>
public sealed record StartEducationalTrackCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    DefinitionId<EducationTrack> TrackId) : ICommand;

/// <summary>Emitted whenever a <see cref="StartEducationalTrackCommand"/> is accepted.</summary>
public sealed record EducationalTrackStartedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    DefinitionId<EducationTrack> TrackId,
    string? CausationId) : IDomainEvent
{
    public string Type => "education.trackStarted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(CharacterId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="StartEducationalTrackCommand"/> (ADR 0006).</summary>
public static class StartEducationalTrackCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("education.startTrack.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("education.startTrack.characterDeceased");
    public static readonly ValidationErrorCode NotAdolescent = new("education.startTrack.notAdolescent");
    public static readonly ValidationErrorCode AlreadyEnrolled = new("education.startTrack.alreadyEnrolled");
    public static readonly ValidationErrorCode UnknownTrack = new("education.startTrack.unknownTrack");
    public static readonly ValidationErrorCode NoOperationalBuilding = new("education.startTrack.noOperationalBuilding");

    public static readonly CommandPipeline<WorldState, StartEducationalTrackCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, StartEducationalTrackCommand command)
    {
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!character!.IsAlive)
            return CharacterDeceased;
        if (character.GetLifecycleStage(state.Date) != LifecycleStage.Adolescent)
            return NotAdolescent;
        if (!KnownEducationTracks.Catalog.TryGet(command.TrackId, out var track))
            return UnknownTrack;
        if (character.Household is not { } householdId ||
            !EducationBuildingResolver.HasOperationalBuilding(state, householdId, track!.DeliveringBuildingIds))
        {
            return NoOperationalBuilding;
        }

        if (EducationalTrackEnrollmentResolver.TryGet(state, command.CharacterId, out var existing) &&
            EducationalTrackEnrollmentResolver.IsActive(existing))
        {
            return AlreadyEnrolled;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, StartEducationalTrackCommand command)
    {
        if (state.EducationalTrackEnrollments.TryGet(command.CharacterId, out _))
            state.EducationalTrackEnrollments.Remove(command.CharacterId);
        state.EducationalTrackEnrollments.Add(
            command.CharacterId, new EducationalTrackEnrollment(command.CharacterId, command.TrackId, command.SubmittedDate));

        return new IDomainEvent[]
        {
            new EducationalTrackStartedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, command.TrackId, command.CommandId.ToTaggedString()),
        };
    }
}
