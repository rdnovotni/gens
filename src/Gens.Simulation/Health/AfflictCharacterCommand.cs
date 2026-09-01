using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Health;

/// <summary>Opens a new standing <see cref="CharacterHealthCondition"/> case on a living Character
/// (Phase 14 item 1). The explicit "hook" future systems — Phase 14 item 2's endemic-exposure rolls and
/// contact-graph contagion, item 3/5's disaster-aftermath flares, Natural Disasters' Flood/Famine
/// triggers — will call once they exist, matching <see cref="ApplyPermanentInjuryCommand"/>'s identical
/// "this command doesn't need its own caller wired up elsewhere in this task" precedent.</summary>
public sealed record AfflictCharacterCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    DefinitionId<HealthConditionDefinition> ConditionId,
    HealthConditionCategory Category,
    bool HasCure,
    int Severity) : ICommand;

/// <summary>Emitted whenever an <see cref="AfflictCharacterCommand"/> is accepted.</summary>
public sealed record CharacterAfflictedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    DefinitionId<HealthConditionDefinition> ConditionId,
    HealthConditionCategory Category,
    int Severity,
    string? CausationId) : IDomainEvent
{
    public string Type => "health.characterAfflicted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="AfflictCharacterCommand"/> (ADR 0006). A
/// living Character cannot be afflicted twice with the same condition at once (<see
/// cref="AlreadyActive"/>), and cannot be afflicted with a condition they are already <see
/// cref="HealthQueries.IsImmune"/> to (<see cref="CharacterImmune"/>) — the real mechanical payoff §5
/// describes for a Plague Survivor.</summary>
public static class AfflictCharacterCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("health.afflict.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("health.afflict.characterDeceased");
    public static readonly ValidationErrorCode InvalidSeverity = new("health.afflict.invalidSeverity");
    public static readonly ValidationErrorCode AlreadyActive = new("health.afflict.alreadyActive");
    public static readonly ValidationErrorCode CharacterImmune = new("health.afflict.characterImmune");

    public static readonly CommandPipeline<WorldState, AfflictCharacterCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AfflictCharacterCommand command)
    {
        if (command.Severity is < 1 or > 100)
            return InvalidSeverity;
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!character.IsAlive)
            return CharacterDeceased;
        if (HealthQueries.HasActiveCondition(state, command.CharacterId, command.ConditionId))
            return AlreadyActive;
        if (HealthQueries.IsImmune(state, command.CharacterId, command.ConditionId))
            return CharacterImmune;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AfflictCharacterCommand command)
    {
        var id = state.CharacterHealthConditionIds.Issue();
        var condition = CharacterHealthCondition.Create(
            id, command.CharacterId, command.ConditionId, command.Category, command.HasCure,
            command.Severity, command.SubmittedDate);
        state.CharacterHealthConditions.Add(id, condition);

        return new IDomainEvent[]
        {
            new CharacterAfflictedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, command.ConditionId,
                command.Category, command.Severity, command.CommandId.ToTaggedString()),
        };
    }
}
