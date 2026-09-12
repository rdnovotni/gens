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
/// Begins a household's standing Symposium commitment (Phase 17 item 2; §7) — the <see
/// cref="CulturalPatronageType.Symposium"/> counterpart to <see cref="SetLiteraryPatronCommand"/>, kept
/// as its own command rather than a shared one parameterized on <see cref="CulturalPatronageType"/>: the
/// two commitments are narratively distinct acts (funding a scholar's upkeep versus hosting a recurring
/// gathering) with their own independent cost/accrual figures (<see cref="CulturalPrestigeCatalog"/>) and
/// their own household-level "at most one active at a time" gate, matching <see
/// cref="Religion.SetPatronDeityCommand"/>'s and <see cref="Religion.ReconsecrateCommand"/>'s identical
/// "narratively distinct events get their own command" precedent applied to two co-existing commitments
/// instead of two sequential ones. Reuses <see cref="CulturalPatronageStartedEvent"/> rather than a
/// second, near-identical event type — the event's own <see cref="CulturalPatronageStartedEvent.PatronageType"/>
/// field already disambiguates which commitment started.
/// </summary>
public sealed record HostRecurringSymposiumCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> HostCharacterId) : ICommand;

/// <summary>The validate/mutate pipeline for <see cref="HostRecurringSymposiumCommand"/> (ADR 0006).</summary>
public static class HostRecurringSymposiumCommands
{
    public static readonly ValidationErrorCode AlreadyActive = new("education.hostRecurringSymposium.alreadyActive");
    public static readonly ValidationErrorCode HostCharacterNotFound = new("education.hostRecurringSymposium.hostCharacterNotFound");
    public static readonly ValidationErrorCode HostCharacterDeceased = new("education.hostRecurringSymposium.hostCharacterDeceased");
    public static readonly ValidationErrorCode HostNotInHousehold = new("education.hostRecurringSymposium.hostNotInHousehold");

    public static readonly CommandPipeline<WorldState, HostRecurringSymposiumCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, HostRecurringSymposiumCommand command)
    {
        if (CulturalPatronageResolver.ActiveRecord(state, command.HouseholdId, CulturalPatronageType.Symposium) is not null)
            return AlreadyActive;
        if (!state.Characters.TryGet(command.HostCharacterId, out var host))
            return HostCharacterNotFound;
        if (!host!.IsAlive)
            return HostCharacterDeceased;
        if (host.Household != command.HouseholdId)
            return HostNotInHousehold;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, HostRecurringSymposiumCommand command)
    {
        var recordId = state.CulturalPatronageRecordIds.Issue();
        state.CulturalPatronageRecords.Add(
            recordId,
            new CulturalPatronageRecord(
                recordId, command.HouseholdId, CulturalPatronageType.Symposium, command.HostCharacterId, command.SubmittedDate));

        return new IDomainEvent[]
        {
            new CulturalPatronageStartedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, recordId,
                PatronageType: CulturalPatronageType.Symposium, command.CommandId.ToTaggedString()),
        };
    }
}
