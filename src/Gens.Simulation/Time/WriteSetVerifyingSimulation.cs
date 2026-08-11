using Gens.Simulation.Commands;
using Gens.Simulation.Random;
using Gens.Simulation.State;

namespace Gens.Simulation.Time;

/// <summary>
/// Debug-only wrapper over <see cref="MonthlySimulation{TState}"/> specialized to <see
/// cref="WorldState"/> that verifies each system's actual mutations stayed inside its declared
/// <see cref="IMonthlySystem{TState}.Writes"/> set (ADR 0005's "debug builds verify the declared
/// access set"). Kept separate from the generic engine so <see cref="MonthlySimulation{TState}"/>
/// stays usable with lightweight test doubles that are not <see cref="WorldState"/>.
/// </summary>
public sealed class WriteSetVerifyingSimulation
{
    private readonly MonthlySimulation<WorldState> _inner;

    public WriteSetVerifyingSimulation(IEnumerable<IMonthlySystem<WorldState>> systems) =>
        _inner = new MonthlySimulation<WorldState>(systems);

    /// <summary>The final, deterministically sorted execution order (ADR 0004, ADR 0005).</summary>
    public IReadOnlyList<IMonthlySystem<WorldState>> OrderedSystems => _inner.OrderedSystems;

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, GameDate date, RandomStreamSet randomStreams)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));

        var context = new MonthlyTickContext(date, randomStreams);
        var events = new List<IDomainEvent>();
        foreach (var system in _inner.OrderedSystems)
        {
#if DEBUG
            var before = state.CapturePartitionVersions();
#endif
            var systemEvents = system.Tick(state, context)
                ?? throw new InvalidOperationException($"Monthly system '{system.Id}' returned null events.");
            events.AddRange(systemEvents);
#if DEBUG
            var after = state.CapturePartitionVersions();
            var undeclared = after
                .Where(pair => !before.TryGetValue(pair.Key, out var previous) || previous != pair.Value)
                .Select(pair => pair.Key)
                .Where(tag => !system.Writes.Contains(tag, StringComparer.Ordinal))
                .ToArray();
            if (undeclared.Length > 0)
                throw new InvalidOperationException(
                    $"Monthly system '{system.Id}' mutated partition(s) [{string.Join(", ", undeclared)}] " +
                    "not present in its declared Writes set.");
#endif
        }

        return events;
    }
}
