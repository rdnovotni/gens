using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Interactions;

/// <summary>Starts a new <see cref="Scheme"/> (<c>gens-characters-design.md</c> §10.1's Initiation
/// step). Actor-agnostic like every other command in this codebase (rule 2) — a player-submitted
/// command and an NPC's automated choice (Phase 10 item 6: "for both player and NPC actions") both
/// submit this exact command through the same <see cref="CommandPipeline{TState,TCommand}"/>.</summary>
public sealed record InitiateSchemeCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> InitiatorCharacterId,
    RuntimeId<Character> TargetCharacterId,
    SchemeType Type) : ICommand;

/// <summary>Emitted whenever an <see cref="InitiateSchemeCommand"/> is accepted. Private to the two
/// participants — a Scheme is, by its own nature, not something its initiator broadcasts (mirroring
/// <see cref="Characters.RelationshipInteractionRecordedEvent"/>'s identical visibility reasoning).
/// Deliberately does not name <see cref="Scheme.Type"/> in <see cref="Visibility"/> or otherwise widen
/// who can see it — discovery risk (§10.3), not this event, is what eventually surfaces a Scheme to
/// anyone beyond its two participants.</summary>
public sealed record SchemeInitiatedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Scheme> SchemeId,
    RuntimeId<Character> InitiatorCharacterId,
    RuntimeId<Character> TargetCharacterId,
    SchemeType Type,
    string? CausationId) : IDomainEvent
{
    public string Type => "interactions.schemeInitiated";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { InitiatorCharacterId.ToTaggedString(), TargetCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(InitiatorCharacterId.ToTaggedString(), TargetCharacterId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="InitiateSchemeCommand"/> (ADR 0006).</summary>
public static class InitiateSchemeCommands
{
    public static readonly ValidationErrorCode SelfTargeted = new("interactions.initiateScheme.selfTargeted");
    public static readonly ValidationErrorCode InitiatorNotFound = new("interactions.initiateScheme.initiatorNotFound");
    public static readonly ValidationErrorCode TargetNotFound = new("interactions.initiateScheme.targetNotFound");
    public static readonly ValidationErrorCode InitiatorDeceased = new("interactions.initiateScheme.initiatorDeceased");
    public static readonly ValidationErrorCode TargetDeceased = new("interactions.initiateScheme.targetDeceased");

    /// <summary>Blocks a second concurrent Scheme of the same pair/type while one is already <see
    /// cref="SchemeStatus.InProgress"/> — nothing in §10 forbids an initiator running unrelated Schemes
    /// against different targets or of a different type at once, only this exact duplicate.</summary>
    public static readonly ValidationErrorCode AlreadyInProgress = new("interactions.initiateScheme.alreadyInProgress");

    public static readonly CommandPipeline<WorldState, InitiateSchemeCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, InitiateSchemeCommand command)
    {
        if (command.InitiatorCharacterId == command.TargetCharacterId)
            return SelfTargeted;
        if (!state.Characters.TryGet(command.InitiatorCharacterId, out var initiator))
            return InitiatorNotFound;
        if (!state.Characters.TryGet(command.TargetCharacterId, out var target))
            return TargetNotFound;
        if (!initiator.IsAlive)
            return InitiatorDeceased;
        if (!target.IsAlive)
            return TargetDeceased;

        var alreadyInProgress = state.Schemes.InAscendingOrder().Any(entry =>
            entry.Value.Status == SchemeStatus.InProgress &&
            entry.Value.InitiatorCharacterId == command.InitiatorCharacterId &&
            entry.Value.TargetCharacterId == command.TargetCharacterId &&
            entry.Value.Type == command.Type);
        if (alreadyInProgress)
            return AlreadyInProgress;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, InitiateSchemeCommand command)
    {
        var schemeId = state.SchemeIds.Issue();
        var scheme = Scheme.Create(schemeId, command.InitiatorCharacterId, command.TargetCharacterId, command.Type, command.SubmittedDate);
        state.Schemes.Add(schemeId, scheme);

        return new IDomainEvent[]
        {
            new SchemeInitiatedEvent(
                state.EventIds.Issue(), command.SubmittedDate, schemeId, command.InitiatorCharacterId,
                command.TargetCharacterId, command.Type, command.CommandId.ToTaggedString()),
        };
    }
}
