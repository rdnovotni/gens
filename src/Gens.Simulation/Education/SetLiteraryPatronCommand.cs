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
/// Begins a household's standing Literary Patronage commitment (Phase 17 item 2; §7) — the founding
/// entry point into this domain's <see cref="CulturalPatronageType.LiteraryPatron"/> partition, matching
/// <see cref="Religion.SetPatronDeityCommand"/>'s identical "one-time founding pick, a different command
/// (<see cref="EndPatronageCommand"/>) cancels it" shape. A household may hold at most one active
/// Literary Patronage at a time; a second call is rejected rather than replacing the first, mirroring
/// <see cref="Religion.SetPatronDeityCommands.AlreadyChosen"/>'s identical rejection rather than a silent
/// overwrite.
/// </summary>
public sealed record SetLiteraryPatronCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> HostCharacterId) : ICommand;

/// <summary>Emitted whenever a <see cref="SetLiteraryPatronCommand"/> is accepted. <see
/// cref="Visibility"/> is <see cref="Commands.Visibility.Public"/>, matching <see
/// cref="Religion.PatronDeitySetEvent"/>'s identical reasoning: §7 frames sponsoring a poet or scholar as
/// a household's real, publicly legible cultural standing, not a private household fact.</summary>
public sealed record CulturalPatronageStartedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<CulturalPatronageRecord> RecordId,
    CulturalPatronageType PatronageType,
    string? CausationId) : IDomainEvent
{
    public string Type => "education.culturalPatronageStarted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="SetLiteraryPatronCommand"/> (ADR 0006).</summary>
public static class SetLiteraryPatronCommands
{
    public static readonly ValidationErrorCode AlreadyActive = new("education.setLiteraryPatron.alreadyActive");
    public static readonly ValidationErrorCode HostCharacterNotFound = new("education.setLiteraryPatron.hostCharacterNotFound");
    public static readonly ValidationErrorCode HostCharacterDeceased = new("education.setLiteraryPatron.hostCharacterDeceased");
    public static readonly ValidationErrorCode HostNotInHousehold = new("education.setLiteraryPatron.hostNotInHousehold");

    public static readonly CommandPipeline<WorldState, SetLiteraryPatronCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, SetLiteraryPatronCommand command)
    {
        if (CulturalPatronageResolver.ActiveRecord(state, command.HouseholdId, CulturalPatronageType.LiteraryPatron) is not null)
            return AlreadyActive;
        if (!state.Characters.TryGet(command.HostCharacterId, out var host))
            return HostCharacterNotFound;
        if (!host!.IsAlive)
            return HostCharacterDeceased;
        if (host.Household != command.HouseholdId)
            return HostNotInHousehold;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, SetLiteraryPatronCommand command)
    {
        var recordId = state.CulturalPatronageRecordIds.Issue();
        state.CulturalPatronageRecords.Add(
            recordId,
            new CulturalPatronageRecord(
                recordId, command.HouseholdId, CulturalPatronageType.LiteraryPatron, command.HostCharacterId, command.SubmittedDate));

        return new IDomainEvent[]
        {
            new CulturalPatronageStartedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, recordId,
                PatronageType: CulturalPatronageType.LiteraryPatron, command.CommandId.ToTaggedString()),
        };
    }
}
