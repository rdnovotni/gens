using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>Applies a <see cref="PermanentInjury"/> to a living Character (<c>gens-familia-design.md</c>
/// §3.1). The explicit "hook" other future systems (Military & Combat, Labor & Slavery punishment,
/// Natural Disasters, difficult childbirth) will call once they exist — this command doesn't need its
/// own caller wired up elsewhere in this task.</summary>
public sealed record ApplyPermanentInjuryCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    PermanentInjuryTarget Target,
    int Magnitude,
    string Cause) : ICommand;

/// <summary>Emitted whenever an <see cref="ApplyPermanentInjuryCommand"/> is accepted.</summary>
public sealed record PermanentInjuryAppliedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    PermanentInjuryTarget Target,
    int Magnitude,
    string Cause,
    string? CausationId) : IDomainEvent
{
    public string Type => "characters.permanentInjuryApplied";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="ApplyPermanentInjuryCommand"/> (ADR 0006).
/// <see cref="Magnitude"/>/<see cref="Cause"/> shape validation (1-100, non-empty) is intentionally
/// re-checked here rather than left solely to <see cref="PermanentInjury"/>'s constructor: <c>validate</c>
/// must reject bad input before <c>mutate</c> ever runs, so a malformed command is cleanly rejected
/// instead of throwing partway through a mutation.</summary>
public static class ApplyPermanentInjuryCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("characters.injury.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("characters.injury.characterDeceased");
    public static readonly ValidationErrorCode InvalidMagnitude = new("characters.injury.invalidMagnitude");
    public static readonly ValidationErrorCode EmptyCause = new("characters.injury.emptyCause");

    public static readonly CommandPipeline<WorldState, ApplyPermanentInjuryCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, ApplyPermanentInjuryCommand command)
    {
        if (command.Magnitude is < 1 or > 100)
            return InvalidMagnitude;
        if (string.IsNullOrWhiteSpace(command.Cause))
            return EmptyCause;
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!character.IsAlive)
            return CharacterDeceased;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, ApplyPermanentInjuryCommand command)
    {
        state.Characters.TryGet(command.CharacterId, out var character);
        var injury = new PermanentInjury(command.Target, command.Magnitude, command.Cause, command.SubmittedDate);

        state.Characters.Remove(command.CharacterId);
        state.Characters.Add(
            command.CharacterId,
            character with { PermanentInjuries = character.PermanentInjuries.Append(injury).ToArray() });

        return new IDomainEvent[]
        {
            new PermanentInjuryAppliedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, command.Target,
                command.Magnitude, command.Cause, command.CommandId.ToTaggedString()),
        };
    }
}
