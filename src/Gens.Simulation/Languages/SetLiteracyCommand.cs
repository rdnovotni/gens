using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Languages;

/// <summary>Sets (or overwrites) a named Character's <see cref="LiteracyRecord"/> (§3, §10). §3's own
/// household-scribe inversion — a household's own Tabularius or Tutor is often the actually-literate
/// one, not the head — is a real texture this command supports directly: nothing here restricts who a
/// caller may mark literate, so a household's freedman scribe gets exactly the same <see
/// cref="LiteracyRecord"/> shape as its own citizen head.</summary>
public sealed record SetLiteracyCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    bool IsLiterate,
    LiteracyDerivation DerivedFrom) : ICommand;

/// <summary>Emitted whenever a <see cref="SetLiteracyCommand"/> is accepted.</summary>
public sealed record LiteracyRecordSetEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    bool IsLiterate,
    string? CausationId) : IDomainEvent
{
    public string Type => "languages.literacySet";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(CharacterId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="SetLiteracyCommand"/> (ADR 0006).</summary>
public static class SetLiteracyCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("languages.setLiteracy.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("languages.setLiteracy.characterDeceased");

    public static readonly CommandPipeline<WorldState, SetLiteracyCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, SetLiteracyCommand command)
    {
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!character.IsAlive)
            return CharacterDeceased;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, SetLiteracyCommand command)
    {
        if (state.LiteracyRecords.TryGet(command.CharacterId, out _))
            state.LiteracyRecords.Remove(command.CharacterId);
        state.LiteracyRecords.Add(command.CharacterId, new LiteracyRecord(command.CharacterId, command.IsLiterate, command.DerivedFrom));

        return new IDomainEvent[]
        {
            new LiteracyRecordSetEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, command.IsLiterate,
                command.CommandId.ToTaggedString()),
        };
    }
}
