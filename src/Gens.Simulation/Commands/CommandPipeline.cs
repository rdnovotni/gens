using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Commands;

/// <summary>
/// One command path (rule 2): player actions, AI decisions, steward automation, events, debug
/// tools, and migration repairs all submit the same envelope shape (ADR 0006). Every command
/// carries who submitted it, when, and — for automation-originated commands — what caused it.
/// </summary>
public interface ICommand
{
    RuntimeId<Command> CommandId { get; }

    /// <summary>The submitting actor's tagged ID, or the reserved <c>"system"</c> sentinel for
    /// pure-automation commands with no single responsible character.</summary>
    string ActorId { get; }

    GameDate SubmittedDate { get; }

    /// <summary>The command or event ID that produced this command, or <c>null</c> for a
    /// player-submitted command with no prior cause.</summary>
    string? CausationId { get; }
}

/// <summary>
/// A stable, versioned validation failure code (e.g. <c>"labor.assignDuty.slotOccupied"</c>), never
/// free text (ADR 0006) — UI and AI decision-making branch on this reliably instead of parsing prose.
/// </summary>
public readonly record struct ValidationErrorCode(string Code)
{
    public override string ToString() => Code;
}

/// <summary>
/// The result of executing one command. <see cref="SequenceNumber"/> is assigned only on acceptance
/// — the total order the Phase 2 replay-determinism exit gate depends on. A rejected command never
/// reaches <c>_mutate</c> and so never consumes a sequence number or an RNG draw (ADR 0006).
/// </summary>
public readonly record struct CommandResult(bool Accepted, IReadOnlyList<IDomainEvent> Events, ValidationErrorCode? Error, long? SequenceNumber)
{
    public static CommandResult Rejected(ValidationErrorCode error) => new(false, Array.Empty<IDomainEvent>(), error, null);

    public static CommandResult Success(long sequenceNumber, params IDomainEvent[] events) => new(true, events, null, sequenceNumber);
}

/// <summary>
/// The envelope every domain event carries (ADR 0007), in addition to its own payload. Events are
/// immutable once emitted and are the only channel later systems (the Monthly Report, the Dynasty
/// Chronicle, knowledge propagation) read from — no system reconstructs "what happened this month"
/// by re-reading another system's raw <c>WorldState</c> partition.
/// </summary>
public interface IDomainEvent
{
    RuntimeId<DomainEventEntity> EventId { get; }

    /// <summary>A stable type tag identifying the event's payload shape.</summary>
    string Type { get; }

    /// <summary>The payload schema version, so a payload shape can evolve without breaking older readers.</summary>
    int SchemaVersion { get; }

    /// <summary>The tick this event was produced in, not the tick it is read in.</summary>
    GameDate OccurredDate { get; }

    /// <summary>Every entity this event is about, in a fixed (ADR 0004) order — not just the one that
    /// emitted it.</summary>
    IReadOnlyList<string> SubjectIds { get; }

    /// <summary>Who can know this happened, at what confidence, via what channel. Mandatory, not
    /// optional metadata (ADR 0007, ADR 0008).</summary>
    Visibility Visibility { get; }

    /// <summary>The command or parent event ID that produced this event.</summary>
    string? CausationId { get; }
}

/// <summary>
/// Validates a command before allowing its handler to mutate state, then assigns a deterministic
/// sequence number at the moment of acceptance (ADR 0006). <paramref name="validate"/>
/// (constructor parameter) must be pure and never draw from a random stream — only <paramref
/// name="mutate"/>, which runs strictly after a successful validation, may consume RNG. This makes
/// atomicity free: a rejected command cannot have perturbed state or RNG, since it never reached
/// <paramref name="mutate"/> at all.
/// </summary>
public sealed class CommandPipeline<TState, TCommand> where TCommand : ICommand
{
    private readonly Func<TState, TCommand, ValidationErrorCode?> _validate;
    private readonly Func<TState, TCommand, IReadOnlyList<IDomainEvent>> _mutate;
    private readonly Func<TState, long> _issueSequenceNumber;

    public CommandPipeline(
        Func<TState, TCommand, ValidationErrorCode?> validate,
        Func<TState, TCommand, IReadOnlyList<IDomainEvent>> mutate,
        Func<TState, long> issueSequenceNumber) =>
        (_validate, _mutate, _issueSequenceNumber) = (validate, mutate, issueSequenceNumber);

    public CommandResult Execute(TState state, TCommand command)
    {
        var error = _validate(state, command);
        if (error is { } code)
            return CommandResult.Rejected(code);

        var sequenceNumber = _issueSequenceNumber(state);
        var events = _mutate(state, command);
        return CommandResult.Success(sequenceNumber, events as IDomainEvent[] ?? events.ToArray());
    }
}
