using Gens.Simulation.Commands;
using Gens.Simulation.Random;

namespace Gens.Simulation.Time;

public readonly record struct MonthlyTickContext(GameDate Date, RandomStreamSet RandomStreams);

/// <summary>A deterministic unit of work executed during every monthly tick.</summary>
public interface IMonthlySystem<in TState>
{
    string Id { get; }
    IReadOnlyList<IDomainEvent> Tick(TState state, MonthlyTickContext context);
}

/// <summary>Runs monthly systems in their explicit registration order.</summary>
public sealed class MonthlySimulation<TState>
{
    private readonly IReadOnlyList<IMonthlySystem<TState>> _systems;

    public MonthlySimulation(IEnumerable<IMonthlySystem<TState>> systems)
    {
        if (systems is null)
            throw new ArgumentNullException(nameof(systems));
        var materialized = systems.ToArray();
        if (materialized.Any(static system => system is null))
            throw new ArgumentException("Monthly systems cannot contain null entries.", nameof(systems));

        var duplicate = materialized
            .GroupBy(static system => system.Id, StringComparer.Ordinal)
            .FirstOrDefault(static group => string.IsNullOrWhiteSpace(group.Key) || group.Count() > 1);
        if (duplicate is not null)
            throw new ArgumentException("Monthly system IDs must be non-empty and unique.", nameof(systems));

        _systems = materialized;
    }

    public IReadOnlyList<IDomainEvent> Tick(TState state, GameDate date, RandomStreamSet randomStreams)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));

        var context = new MonthlyTickContext(date, randomStreams);
        var events = new List<IDomainEvent>();
        foreach (var system in _systems)
        {
            var systemEvents = system.Tick(state, context)
                ?? throw new InvalidOperationException($"Monthly system '{system.Id}' returned null events.");
            events.AddRange(systemEvents);
        }

        return events;
    }
}
