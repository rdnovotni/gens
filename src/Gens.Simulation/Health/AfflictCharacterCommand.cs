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
/// "this command doesn't need its own caller wired up elsewhere in this task" precedent. Deliberately
/// carries only <see cref="ConditionId"/>, not <see cref="HealthConditionDefinition.Category"/>/<see
/// cref="HealthConditionDefinition.HasCure"/> — those are resolved from the <see
/// cref="HealthConditionCatalog"/> the pipeline is built against (<see
/// cref="AfflictCharacterCommands.BuildPipeline"/>), the same "caller-loaded content" shape <see
/// cref="Languages.AcquireLanguageCommands.BuildPipeline"/> already established, so a caller can never
/// mislabel a registered condition's own real category/curability.</summary>
public sealed record AfflictCharacterCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    DefinitionId<HealthConditionDefinition> ConditionId,
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

/// <summary>The validate/mutate pipeline for <see cref="AfflictCharacterCommand"/> (ADR 0006), built
/// against a <see cref="HealthConditionCatalog"/> the same way <see
/// cref="Languages.AcquireLanguageCommands.BuildPipeline"/> is built against a <c>LanguageCatalog</c>.
/// A living Character cannot be afflicted twice with the same condition at once (<see
/// cref="AlreadyActive"/>), and cannot be afflicted with a condition they are already <see
/// cref="HealthQueries.IsImmune"/> to (<see cref="CharacterImmune"/>) — the real mechanical payoff §5
/// describes for a Plague Survivor.</summary>
public static class AfflictCharacterCommands
{
    public static readonly ValidationErrorCode UnknownCondition = new("health.afflict.unknownCondition");
    public static readonly ValidationErrorCode CharacterNotFound = new("health.afflict.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("health.afflict.characterDeceased");
    public static readonly ValidationErrorCode InvalidSeverity = new("health.afflict.invalidSeverity");
    public static readonly ValidationErrorCode AlreadyActive = new("health.afflict.alreadyActive");
    public static readonly ValidationErrorCode CharacterImmune = new("health.afflict.characterImmune");

    public static CommandPipeline<WorldState, AfflictCharacterCommand> BuildPipeline(HealthConditionCatalog conditions)
    {
        if (conditions is null)
            throw new ArgumentNullException(nameof(conditions));

        return new CommandPipeline<WorldState, AfflictCharacterCommand>(
            validate: (state, command) => Validate(state, command, conditions),
            mutate: (state, command) => Mutate(state, command, conditions),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, AfflictCharacterCommand command, HealthConditionCatalog conditions)
    {
        if (command.Severity is < 1 or > 100)
            return InvalidSeverity;
        if (!conditions.TryGet(command.ConditionId, out _))
            return UnknownCondition;
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

    private static IDomainEvent[] Mutate(WorldState state, AfflictCharacterCommand command, HealthConditionCatalog conditions)
    {
        var definition = conditions.Get(command.ConditionId);
        var id = state.CharacterHealthConditionIds.Issue();
        var condition = CharacterHealthCondition.Create(
            id, command.CharacterId, command.ConditionId, definition.Category, definition.HasCure,
            command.Severity, command.SubmittedDate);
        state.CharacterHealthConditions.Add(id, condition);

        return new IDomainEvent[]
        {
            new CharacterAfflictedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, command.ConditionId,
                definition.Category, command.Severity, command.CommandId.ToTaggedString()),
        };
    }
}
