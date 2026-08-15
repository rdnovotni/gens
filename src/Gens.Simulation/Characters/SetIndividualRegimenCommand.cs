using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>Sets or clears a Character's individual Regimen override (<c>gens-labor-slavery-design.md</c>
/// §5) — a <c>null</c> <see cref="Regimen"/> falls back to the household's group default (<see
/// cref="RegimenResolver"/>).</summary>
public sealed record SetIndividualRegimenCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    RegimenSettings? Regimen) : ICommand;

/// <summary>Emitted whenever a <see cref="SetIndividualRegimenCommand"/> is accepted.</summary>
public sealed record IndividualRegimenSetEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    RegimenSettings? Regimen,
    string? CausationId) : IDomainEvent
{
    public string Type => "characters.individualRegimenSet";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="SetIndividualRegimenCommand"/> (ADR 0006).</summary>
public static class SetIndividualRegimenCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("characters.regimen.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("characters.regimen.characterDeceased");
    public static readonly ValidationErrorCode NotEnslaved = new("characters.regimen.notEnslaved");

    public static readonly CommandPipeline<WorldState, SetIndividualRegimenCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, SetIndividualRegimenCommand command)
    {
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!character.IsAlive)
            return CharacterDeceased;
        if (character.LegalStatus != LegalStatus.Enslaved)
            return NotEnslaved;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, SetIndividualRegimenCommand command)
    {
        state.Characters.TryGet(command.CharacterId, out var character);
        state.Characters.Remove(command.CharacterId);
        state.Characters.Add(command.CharacterId, character with { Regimen = command.Regimen });

        return new IDomainEvent[]
        {
            new IndividualRegimenSetEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, command.Regimen,
                command.CommandId.ToTaggedString()),
        };
    }
}
