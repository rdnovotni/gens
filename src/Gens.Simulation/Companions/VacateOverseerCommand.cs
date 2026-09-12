using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Companions;

/// <summary>Deliberately ends an active <see cref="OverseerAssignment"/> (Phase 17 item 1) — the
/// player-initiated counterpart to <see cref="PositionVacancySystem"/>'s own automatic vacancy-on-death/
/// left-household branch. Replace-with-<see cref="OverseerAssignment.EndDate"/>, never delete, matching
/// <see cref="Magistracies.MagistracyRecord"/>'s identical convention.</summary>
public sealed record VacateOverseerCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<OverseerAssignment> RecordId) : ICommand;

/// <summary>Emitted whenever a <see cref="VacateOverseerCommand"/> is accepted.</summary>
public sealed record OverseerVacatedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<OverseerAssignment> RecordId,
    RuntimeId<Character> HolderId,
    OverseerRole Role,
    string? CausationId) : IDomainEvent
{
    public string Type => "companions.overseerVacated";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HolderId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="VacateOverseerCommand"/> (ADR 0006).</summary>
public static class VacateOverseerCommands
{
    public static readonly ValidationErrorCode RecordNotFound = new("companions.vacateOverseer.recordNotFound");
    public static readonly ValidationErrorCode RecordAlreadyEnded = new("companions.vacateOverseer.recordAlreadyEnded");

    public static readonly CommandPipeline<WorldState, VacateOverseerCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, VacateOverseerCommand command)
    {
        if (!state.OverseerAssignments.TryGet(command.RecordId, out var record))
            return RecordNotFound;
        if (!OverseerResolver.IsActive(record!))
            return RecordAlreadyEnded;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, VacateOverseerCommand command)
    {
        state.OverseerAssignments.TryGet(command.RecordId, out var existing);
        state.OverseerAssignments.Remove(command.RecordId);
        state.OverseerAssignments.Add(command.RecordId, existing! with { EndDate = command.SubmittedDate });

        return new IDomainEvent[]
        {
            new OverseerVacatedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.RecordId, existing.HolderId, existing.Role,
                command.CommandId.ToTaggedString()),
        };
    }
}
