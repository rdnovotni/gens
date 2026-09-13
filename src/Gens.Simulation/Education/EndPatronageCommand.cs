using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Education;

/// <summary>
/// Cancels one active Cultural Patronage commitment (Phase 17 item 2; §7) — the single cancellation path
/// for both <see cref="CulturalPatronageType.LiteraryPatron"/> and <see
/// cref="CulturalPatronageType.Symposium"/>, addressed by <see cref="RecordId"/> rather than by
/// (household, type) so a caller that already holds the record (e.g. from <see
/// cref="CulturalPatronageResolver.ActiveRecord"/>) does not need to re-derive it. Sets <see
/// cref="CulturalPatronageRecord.EndedDate"/> rather than removing the
/// record outright — the record is kept forever once created (see that record's own doc comment).
/// </summary>
public sealed record EndPatronageCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<CulturalPatronageRecord> RecordId) : ICommand;

/// <summary>Emitted whenever an <see cref="EndPatronageCommand"/> is accepted.</summary>
public sealed record CulturalPatronageEndedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<CulturalPatronageRecord> RecordId,
    CulturalPatronageType PatronageType,
    string? CausationId) : IDomainEvent
{
    public string Type => "education.culturalPatronageEnded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="EndPatronageCommand"/> (ADR 0006).</summary>
public static class EndPatronageCommands
{
    public static readonly ValidationErrorCode RecordNotFound = new("education.endPatronage.recordNotFound");
    public static readonly ValidationErrorCode AlreadyEnded = new("education.endPatronage.alreadyEnded");

    public static readonly CommandPipeline<WorldState, EndPatronageCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, EndPatronageCommand command)
    {
        if (!state.CulturalPatronageRecords.TryGet(command.RecordId, out var record))
            return RecordNotFound;
        if (!CulturalPatronageResolver.IsActive(record!))
            return AlreadyEnded;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, EndPatronageCommand command)
    {
        state.CulturalPatronageRecords.TryGet(command.RecordId, out var record);
        state.CulturalPatronageRecords.Remove(command.RecordId);
        state.CulturalPatronageRecords.Add(command.RecordId, record! with { EndedDate = command.SubmittedDate });

        return new IDomainEvent[]
        {
            new CulturalPatronageEndedEvent(
                state.EventIds.Issue(), command.SubmittedDate, record.HouseholdId, command.RecordId, record.Type,
                command.CommandId.ToTaggedString()),
        };
    }
}
