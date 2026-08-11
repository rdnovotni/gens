using Gens.Simulation.Commands;
using Gens.Simulation.Random;

namespace Gens.Simulation.Time;

public readonly record struct MonthlyTickContext(GameDate Date, RandomStreamSet RandomStreams);

/// <summary>The ten fixed monthly tick phases, run in this order every tick (ADR 0005).</summary>
public enum TickPhase
{
    ScheduledCommands,
    Lifecycle,
    Production,
    EmploymentNeeds,
    MarketsLedger,
    RelationshipsActors,
    Hazards,
    Events,
    Reports,
    InvariantChecks,
}

/// <summary>A deterministic unit of work executed during every monthly tick. Declares its phase,
/// its read/write partition tags, and the same-phase prerequisites that must run before it
/// (ADR 0005) — the scheduler topologically sorts once from these declarations rather than relying
/// on registration order.</summary>
public interface IMonthlySystem<in TState>
{
    string Id { get; }
    TickPhase Phase { get; }
    IReadOnlyCollection<string> Reads { get; }
    IReadOnlyCollection<string> Writes { get; }
    IReadOnlyCollection<string> Prerequisites { get; }
    IReadOnlyList<IDomainEvent> Tick(TState state, MonthlyTickContext context);
}

/// <summary>Runs monthly systems in a deterministic order: phase (declaration order of
/// <see cref="TickPhase"/>) first, then same-phase prerequisite order, then ordinal system ID as the
/// final tiebreak (ADR 0004, ADR 0005). The order is computed once at construction; a missing or
/// cyclic prerequisite, or a prerequisite naming a system in a later phase, fails immediately rather
/// than falling back to registration order.</summary>
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

        var byId = materialized.ToDictionary(static system => system.Id, StringComparer.Ordinal);

        foreach (var system in materialized)
        {
            foreach (var prerequisiteId in system.Prerequisites)
            {
                if (!byId.TryGetValue(prerequisiteId, out var prerequisite))
                    throw new InvalidOperationException(
                        $"Monthly system '{system.Id}' declares an unknown prerequisite '{prerequisiteId}'.");
                if (prerequisite.Phase > system.Phase)
                    throw new InvalidOperationException(
                        $"Monthly system '{system.Id}' in phase '{system.Phase}' cannot depend on " +
                        $"'{prerequisiteId}' in later phase '{prerequisite.Phase}'.");
            }
        }

        _systems = materialized
            .GroupBy(static system => system.Phase)
            .OrderBy(static group => group.Key)
            .SelectMany(group => TopologicalSortWithinPhase(group.ToArray(), byId))
            .ToArray();
    }

    /// <summary>The final, deterministically sorted execution order.</summary>
    public IReadOnlyList<IMonthlySystem<TState>> OrderedSystems => _systems;

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

    private static List<IMonthlySystem<TState>> TopologicalSortWithinPhase(
        IMonthlySystem<TState>[] phaseSystems,
        Dictionary<string, IMonthlySystem<TState>> byId)
    {
        var remaining = new SortedDictionary<string, IMonthlySystem<TState>>(StringComparer.Ordinal);
        foreach (var system in phaseSystems)
            remaining[system.Id] = system;

        var resolved = new HashSet<string>(StringComparer.Ordinal);
        var ordered = new List<IMonthlySystem<TState>>(phaseSystems.Length);

        while (remaining.Count > 0)
        {
            string? next = null;
            foreach (var candidateId in remaining.Keys)
            {
                var candidate = remaining[candidateId];
                var samePhasePrerequisitesMet = candidate.Prerequisites
                    .Where(id => byId.TryGetValue(id, out var prerequisite) && prerequisite.Phase == candidate.Phase)
                    .All(resolved.Contains);
                if (samePhasePrerequisitesMet)
                {
                    next = candidateId;
                    break;
                }
            }

            if (next is null)
                throw new InvalidOperationException(
                    "A cyclic prerequisite dependency was detected among monthly systems: " +
                    string.Join(", ", remaining.Keys) + ".");

            ordered.Add(remaining[next]);
            resolved.Add(next);
            remaining.Remove(next);
        }

        return ordered;
    }
}
