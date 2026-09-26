using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>Records a new marriage between two living, currently-unmarried Characters
/// (<c>gens-familia-design.md</c> §5). Opposite-sex only: <c>gens-romance-sexuality-lineage-design.md</c>
/// §5 requires that a same-sex romantic relationship, however genuinely tracked elsewhere in the Romance
/// module, "never plug into Familia's marriage... mechanics" — this is the one, correct enforcement
/// point for that rule, since every caller (including <see cref="Romance.ProposeMarriageCommand"/>)
/// routes through this same pipeline and so inherits the same protection rather than each needing to
/// reimplement it.</summary>
public sealed record RecordMarriageCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    RuntimeId<Character> SpouseId) : ICommand;

/// <summary>Emitted whenever a <see cref="RecordMarriageCommand"/> is accepted.</summary>
public sealed record CharactersMarriedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    RuntimeId<Character> SpouseId,
    string? CausationId) : IDomainEvent
{
    public string Type => "characters.married";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString(), SpouseId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="RecordMarriageCommand"/> (ADR 0006).</summary>
public static class RecordMarriageCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("characters.marriage.characterNotFound");
    public static readonly ValidationErrorCode SpouseNotFound = new("characters.marriage.spouseNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("characters.marriage.characterDeceased");
    public static readonly ValidationErrorCode SpouseDeceased = new("characters.marriage.spouseDeceased");
    public static readonly ValidationErrorCode AlreadyMarried = new("characters.marriage.alreadyMarried");
    public static readonly ValidationErrorCode SelfMarriage = new("characters.marriage.selfMarriage");

    /// <summary><c>gens-romance-sexuality-lineage-design.md</c> §5's exclusion — see this class's own
    /// doc comment.</summary>
    public static readonly ValidationErrorCode SameSexNotSupported = new("characters.marriage.sameSexNotSupported");

    public static readonly CommandPipeline<WorldState, RecordMarriageCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RecordMarriageCommand command)
    {
        if (command.CharacterId == command.SpouseId)
            return SelfMarriage;
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!state.Characters.TryGet(command.SpouseId, out var spouse))
            return SpouseNotFound;
        if (!character.IsAlive)
            return CharacterDeceased;
        if (!spouse.IsAlive)
            return SpouseDeceased;
        if (character.CurrentSpouseId is not null || spouse.CurrentSpouseId is not null)
            return AlreadyMarried;
        if (character.Sex == spouse.Sex)
            return SameSexNotSupported;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, RecordMarriageCommand command)
    {
        state.Characters.TryGet(command.CharacterId, out var character);
        state.Characters.TryGet(command.SpouseId, out var spouse);

        var characterRecord = new MarriageRecord(command.SpouseId, command.SubmittedDate, null, null);
        var spouseRecord = new MarriageRecord(command.CharacterId, command.SubmittedDate, null, null);

        state.Characters.Remove(command.CharacterId);
        state.Characters.Add(
            command.CharacterId,
            character with { MaritalHistory = character.MaritalHistory.Append(characterRecord).ToArray() });

        state.Characters.Remove(command.SpouseId);
        state.Characters.Add(
            command.SpouseId,
            spouse with { MaritalHistory = spouse.MaritalHistory.Append(spouseRecord).ToArray() });

        return new IDomainEvent[]
        {
            new CharactersMarriedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, command.SpouseId,
                command.CommandId.ToTaggedString()),
        };
    }
}
