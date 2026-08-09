namespace Gens.Simulation.Commands;

public interface ICommand;
public interface IDomainEvent;

public readonly record struct CommandResult(bool Accepted, IReadOnlyList<IDomainEvent> Events, string? Error)
{
    public static CommandResult Rejected(string error) => new(false, Array.Empty<IDomainEvent>(), error);
    public static CommandResult Success(params IDomainEvent[] events) => new(true, events, null);
}

/// <summary>Validates a command before allowing its handler to mutate state.</summary>
public sealed class CommandPipeline<TState, TCommand> where TCommand : ICommand
{
    private readonly Func<TState, TCommand, string?> _validate;
    private readonly Func<TState, TCommand, IReadOnlyList<IDomainEvent>> _mutate;

    public CommandPipeline(
        Func<TState, TCommand, string?> validate,
        Func<TState, TCommand, IReadOnlyList<IDomainEvent>> mutate) =>
        (_validate, _mutate) = (validate, mutate);

    public CommandResult Execute(TState state, TCommand command)
    {
        var error = _validate(state, command);
        return error is null
            ? new CommandResult(true, _mutate(state, command), null)
            : CommandResult.Rejected(error);
    }
}

