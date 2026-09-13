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
/// Starts (or retargets) a Character's Culture drift toward <see cref="TargetCultureId"/> (Phase 17 item
/// 2; §2). Named explicitly rather than left implicit: §2 describes the drift mechanism itself (rate by
/// lifecycle stage, Foreign Tutor/Study Abroad acceleration) but names no single triggering command of
/// its own — this is this implementation's own minimal entry point establishing a drift target, mirroring
/// <see cref="Religion.SetPatronDeityCommand"/>'s identical "an explicit command establishes state a
/// design doc otherwise only describes the consequences of" convention. A caller most naturally submits
/// this once a Character is embedded in a foreign cultural context (e.g. a Study Abroad Journey's
/// destination culture, or simply a household head's own choice) — sourcing that decision is this
/// command's caller's job, not this command's.
/// </summary>
public sealed record SetCulturalDriftTargetCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    DefinitionId<Identity.Culture> TargetCultureId) : ICommand;

/// <summary>Emitted whenever a <see cref="SetCulturalDriftTargetCommand"/> is accepted.</summary>
public sealed record CulturalDriftTargetSetEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    DefinitionId<Identity.Culture> TargetCultureId,
    string? CausationId) : IDomainEvent
{
    public string Type => "education.culturalDriftTargetSet";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(CharacterId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="SetCulturalDriftTargetCommand"/> (ADR 0006).</summary>
public static class SetCulturalDriftTargetCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("education.setCulturalDriftTarget.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("education.setCulturalDriftTarget.characterDeceased");
    public static readonly ValidationErrorCode AlreadyThatCulture = new("education.setCulturalDriftTarget.alreadyThatCulture");

    public static readonly CommandPipeline<WorldState, SetCulturalDriftTargetCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, SetCulturalDriftTargetCommand command)
    {
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!character!.IsAlive)
            return CharacterDeceased;
        if (character.Culture == command.TargetCultureId)
            return AlreadyThatCulture;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, SetCulturalDriftTargetCommand command)
    {
        CulturalDriftResolver.SetTarget(state, command.CharacterId, command.TargetCultureId);

        return new IDomainEvent[]
        {
            new CulturalDriftTargetSetEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, command.TargetCultureId,
                command.CommandId.ToTaggedString()),
        };
    }
}
