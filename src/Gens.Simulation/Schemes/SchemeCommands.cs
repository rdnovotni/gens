using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Schemes;

/// <summary>Stage 1 (Initiation, §10) — commits a Character to a Scheme against a target. Actor-
/// agnostic like every command in this codebase: a player-submitted command and an NPC's automated
/// choice (§8.3's "Ambition + Boldness + Vengefulness axes let ANY Character initiate an interaction
/// unprompted") both go through this exact command and <see cref="SchemeCommands.InitiatePipeline"/>.</summary>
public sealed record InitiateSchemeCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    SchemeType Type,
    RuntimeId<Character> InitiatorCharacterId,
    RuntimeId<Character> TargetCharacterId,
    RuntimeId<Character>? AssistingAgentCharacterId) : ICommand;

/// <summary>Stage 4 (Counter-play, §10) — the target investigates/confronts/counter-schemes once
/// suspicion has crossed the discovery threshold. Modeled as a single command that always foils the
/// scheme outright (<see cref="SchemeOutcome.DiscoveredAndFoiled"/>): a genuine counter-scheme spawning
/// its own <see cref="SchemeInstance"/> back at the original initiator is real future depth this
/// package does not build (§10's own "real back-and-forth" is honored by giving the target an active
/// choice with a deadline, not by every possible counter-play shape).</summary>
public sealed record CounterPlaySchemeCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<SchemeInstance> SchemeId) : ICommand;

public sealed record SchemeInitiatedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<SchemeInstance> SchemeId,
    SchemeType SchemeType,
    RuntimeId<Character> InitiatorCharacterId,
    RuntimeId<Character> TargetCharacterId,
    string? CausationId) : IDomainEvent
{
    public string Type => "schemes.initiated";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { InitiatorCharacterId.ToTaggedString(), TargetCharacterId.ToTaggedString() };

    // Private to both parties until discovery — matching §10 stage 3's premise that a scheme's own
    // existence is exactly what Discovery Risk is rolling to reveal.
    public Visibility Visibility => Visibility.Private(InitiatorCharacterId.ToTaggedString(), TargetCharacterId.ToTaggedString());
}

public sealed record SchemeResolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<SchemeInstance> SchemeId,
    RuntimeId<Character> InitiatorCharacterId,
    RuntimeId<Character> TargetCharacterId,
    SchemeOutcome Outcome,
    string? CausationId) : IDomainEvent
{
    public string Type => "schemes.resolved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { InitiatorCharacterId.ToTaggedString(), TargetCharacterId.ToTaggedString() };

    // A Discovered outcome is public — the whole point of discovery; a quiet outcome (Succeeded or
    // FailedQuietly) stays private to the two parties, matching SchemeInitiatedEvent's own visibility.
    public Visibility Visibility => Outcome is SchemeOutcome.DiscoveredAndFoiled or SchemeOutcome.DiscoveredAndEscalated
        ? Visibility.Public
        : Visibility.Private(InitiatorCharacterId.ToTaggedString(), TargetCharacterId.ToTaggedString());
}

/// <summary>Validate/mutate pipelines for the scheme commands (ADR 0006).</summary>
public static class SchemeCommands
{
    public static readonly ValidationErrorCode SameCharacter = new("schemes.initiate.sameCharacter");
    public static readonly ValidationErrorCode UnknownCharacter = new("schemes.unknownCharacter");
    public static readonly ValidationErrorCode SchemeNotFound = new("schemes.schemeNotFound");
    public static readonly ValidationErrorCode NotAwaitingCounterPlay = new("schemes.counterPlay.notAwaitingCounterPlay");

    public static readonly CommandPipeline<WorldState, InitiateSchemeCommand> InitiatePipeline = new(
        validate: ValidateInitiate, mutate: MutateInitiate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    public static readonly CommandPipeline<WorldState, CounterPlaySchemeCommand> CounterPlayPipeline = new(
        validate: ValidateCounterPlay, mutate: MutateCounterPlay, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? ValidateInitiate(WorldState state, InitiateSchemeCommand command)
    {
        if (command.InitiatorCharacterId == command.TargetCharacterId)
            return SameCharacter;
        if (!state.Characters.TryGet(command.InitiatorCharacterId, out _) || !state.Characters.TryGet(command.TargetCharacterId, out _))
            return UnknownCharacter;

        return null;
    }

    private static IDomainEvent[] MutateInitiate(WorldState state, InitiateSchemeCommand command)
    {
        var schemeId = state.SchemeIds.Issue();
        var scheme = new SchemeInstance(
            schemeId, command.Type, command.InitiatorCharacterId, command.TargetCharacterId, command.AssistingAgentCharacterId,
            command.SubmittedDate, Progress: 0, DiscoveryRisk: 0, SchemeStage.Progressing);
        state.Schemes.Add(schemeId, scheme);

        return new IDomainEvent[]
        {
            new SchemeInitiatedEvent(
                state.EventIds.Issue(), command.SubmittedDate, schemeId, command.Type, command.InitiatorCharacterId,
                command.TargetCharacterId, command.CommandId.ToTaggedString()),
        };
    }

    private static ValidationErrorCode? ValidateCounterPlay(WorldState state, CounterPlaySchemeCommand command)
    {
        if (!state.Schemes.TryGet(command.SchemeId, out var scheme))
            return SchemeNotFound;
        if (scheme!.Stage != SchemeStage.AwaitingCounterPlay)
            return NotAwaitingCounterPlay;

        return null;
    }

    private static IDomainEvent[] MutateCounterPlay(WorldState state, CounterPlaySchemeCommand command)
    {
        state.Schemes.TryGet(command.SchemeId, out var scheme);
        var resolved = scheme! with { Stage = SchemeStage.Resolved, Outcome = SchemeOutcome.DiscoveredAndFoiled, ResolvedDate = command.SubmittedDate };
        state.Schemes.Remove(command.SchemeId);
        state.Schemes.Add(command.SchemeId, resolved);

        return new IDomainEvent[]
        {
            new SchemeResolvedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.SchemeId, scheme.InitiatorCharacterId, scheme.TargetCharacterId,
                SchemeOutcome.DiscoveredAndFoiled, command.CommandId.ToTaggedString()),
        };
    }
}
