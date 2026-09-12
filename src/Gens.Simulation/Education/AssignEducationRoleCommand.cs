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

/// <summary>§3.3's two household tutor options this ticket builds a narrow placeholder for — <b>superseded
/// once Phase 17 item 1 (Companions &amp; Court Positions) lands</b>: a thin, Education-only stand-in, not
/// a general assignment framework, per this ticket's own confirmed scope decision.</summary>
public enum EducationRole
{
    PrivateTutor,
    ForeignTutor,
}

/// <summary>One household's standing tutor assignment (Phase 17 item 2; §3.3), keyed by the appointing
/// household — at most one active assignment per household, matching <see
/// cref="Languages.InterpresAppointment"/>'s identical shape. See <see cref="EducationRole"/>'s own doc
/// comment for this record's placeholder status.</summary>
public sealed record EducationRoleAssignment(RuntimeId<Household> HouseholdId, RuntimeId<Character> TutorId, EducationRole Role);

/// <summary>Assigns (or replaces) a household's standing Paedagogus (<see cref="EducationRole.PrivateTutor"/>)
/// or Foreign Tutor (<see cref="EducationRole.ForeignTutor"/>). A Foreign Tutor deliberately need not
/// belong to the household — §3.3/§2's own "a foreign tutor accelerates Culture drift toward their own
/// culture" only makes sense for someone from outside it — so, unlike <see
/// cref="Languages.AppointInterpresCommand"/>'s household-membership requirement, this command checks
/// only that the tutor is a real, living Character.</summary>
public sealed record AssignEducationRoleCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> TutorId,
    EducationRole Role) : ICommand;

/// <summary>Emitted whenever an <see cref="AssignEducationRoleCommand"/> is accepted.</summary>
public sealed record EducationRoleAssignedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> TutorId,
    EducationRole Role,
    string? CausationId) : IDomainEvent
{
    public string Type => "education.roleAssigned";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(HouseholdId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="AssignEducationRoleCommand"/> (ADR 0006).</summary>
public static class AssignEducationRoleCommands
{
    public static readonly ValidationErrorCode TutorNotFound = new("education.assignRole.tutorNotFound");
    public static readonly ValidationErrorCode TutorDeceased = new("education.assignRole.tutorDeceased");

    public static readonly CommandPipeline<WorldState, AssignEducationRoleCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AssignEducationRoleCommand command)
    {
        if (!state.Characters.TryGet(command.TutorId, out var tutor))
            return TutorNotFound;
        if (!tutor!.IsAlive)
            return TutorDeceased;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AssignEducationRoleCommand command)
    {
        if (state.EducationRoleAssignments.TryGet(command.HouseholdId, out _))
            state.EducationRoleAssignments.Remove(command.HouseholdId);
        state.EducationRoleAssignments.Add(
            command.HouseholdId, new EducationRoleAssignment(command.HouseholdId, command.TutorId, command.Role));

        return new IDomainEvent[]
        {
            new EducationRoleAssignedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.TutorId, command.Role,
                command.CommandId.ToTaggedString()),
        };
    }
}
