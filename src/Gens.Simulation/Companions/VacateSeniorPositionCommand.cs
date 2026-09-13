using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Time;

namespace Gens.Simulation.Companions;

/// <summary>Deliberately ends an active <see cref="SeniorPositionAssignment"/> (Phase 17 item 1). When
/// <see cref="SeniorPositionAssignment.Title"/> is <see cref="SeniorPositionTitle.Procurator"/>, also
/// ends the linked <see cref="StewardshipAssignment"/> <see cref="AppointSecondSettlementProcuratorCommand"/> created
/// alongside it, in the same mutate step — mirroring how <see cref="StewardshipCommands.MutateEnd"/>
/// ends an ordinary Travel/Regency assignment, minus that path's own <see cref="ReturnReport"/>
/// generation, which is specific to a steward's own deliberate return, not a Court Position vacancy.</summary>
public sealed record VacateSeniorPositionCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<SeniorPositionAssignment> RecordId) : ICommand;

/// <summary>Emitted whenever a <see cref="VacateSeniorPositionCommand"/> is accepted.</summary>
public sealed record SeniorPositionVacatedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<SeniorPositionAssignment> RecordId,
    RuntimeId<Character> HolderId,
    SeniorPositionTitle Title,
    string? CausationId) : IDomainEvent
{
    public string Type => "companions.seniorPositionVacated";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HolderId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="VacateSeniorPositionCommand"/> (ADR 0006).</summary>
public static class VacateSeniorPositionCommands
{
    public static readonly ValidationErrorCode RecordNotFound = new("companions.vacateSeniorPosition.recordNotFound");
    public static readonly ValidationErrorCode RecordAlreadyEnded = new("companions.vacateSeniorPosition.recordAlreadyEnded");

    public static readonly CommandPipeline<WorldState, VacateSeniorPositionCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, VacateSeniorPositionCommand command)
    {
        if (!state.SeniorPositionAssignments.TryGet(command.RecordId, out var record))
            return RecordNotFound;
        if (!SeniorPositionResolver.IsActive(record!))
            return RecordAlreadyEnded;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, VacateSeniorPositionCommand command)
    {
        state.SeniorPositionAssignments.TryGet(command.RecordId, out var existing);
        state.SeniorPositionAssignments.Remove(command.RecordId);
        state.SeniorPositionAssignments.Add(command.RecordId, existing! with { EndDate = command.SubmittedDate });

        var events = new List<IDomainEvent>
        {
            new SeniorPositionVacatedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.RecordId, existing.HolderId, existing.Title,
                command.CommandId.ToTaggedString()),
        };

        if (existing.Title == SeniorPositionTitle.Procurator)
            events.AddRange(ProcuratorLink.EndLinkedStewardship(state, existing, command.SubmittedDate, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}
