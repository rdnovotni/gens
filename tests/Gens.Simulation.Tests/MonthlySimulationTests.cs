using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests;

public sealed class MonthlySimulationTests
{
    // No declared prerequisites and the same phase, so the final tiebreak is ordinal system ID
    // (ADR 0004/0005) — "economy" sorts before "population" regardless of registration order.
    private static readonly string[] ExpectedState = { "economy:10", "population:10" };
    private static readonly string[] ExpectedSystemIds = { "economy", "population" };

    [Test]
    public void SamePhaseNoPrerequisiteSystemsSortByOrdinalId()
    {
        var state = new List<string>();
        var simulation = new MonthlySimulation<List<string>>(new[]
        {
            new RecordingSystem("population"),
            new RecordingSystem("economy"),
        });

        var events = simulation.Tick(state, new GameDate(10), new RandomStreamSet());

        Assert.That(state, Is.EqualTo(ExpectedState));
        Assert.That(events.Cast<SystemRan>().Select(static item => item.Id),
            Is.EqualTo(ExpectedSystemIds));
    }

    [Test]
    public void DuplicateSystemIdsAreRejected()
    {
        var systems = new[] { new RecordingSystem("economy"), new RecordingSystem("economy") };

        Assert.That(() => new MonthlySimulation<List<string>>(systems), Throws.ArgumentException);
    }

    [Test]
    public void PhaseOrderDominatesPrerequisiteAndIdOrder()
    {
        var simulation = new MonthlySimulation<List<string>>(new[]
        {
            new RecordingSystem("zzz-reports", TickPhase.Reports),
            new RecordingSystem("aaa-production", TickPhase.Production),
        });

        var ids = simulation.OrderedSystems.Select(system => system.Id);

        Assert.That(ids, Is.EqualTo(new[] { "aaa-production", "zzz-reports" }));
    }

    [Test]
    public void PrerequisiteOrdersSystemsWithinAPhase()
    {
        var simulation = new MonthlySimulation<List<string>>(new[]
        {
            new RecordingSystem("second", TickPhase.Lifecycle, prerequisites: new[] { "first" }),
            new RecordingSystem("first", TickPhase.Lifecycle),
        });

        var ids = simulation.OrderedSystems.Select(system => system.Id);

        Assert.That(ids, Is.EqualTo(new[] { "first", "second" }));
    }

    [Test]
    public void UnknownPrerequisiteThrows()
    {
        var systems = new[] { new RecordingSystem("only", prerequisites: new[] { "missing" }) };

        Assert.That(() => new MonthlySimulation<List<string>>(systems), Throws.InvalidOperationException);
    }

    [Test]
    public void CyclicPrerequisiteThrows()
    {
        var systems = new[]
        {
            new RecordingSystem("a", prerequisites: new[] { "b" }),
            new RecordingSystem("b", prerequisites: new[] { "a" }),
        };

        Assert.That(() => new MonthlySimulation<List<string>>(systems), Throws.InvalidOperationException);
    }

    [Test]
    public void PrerequisiteInALaterPhaseThrows()
    {
        var systems = new[]
        {
            new RecordingSystem("early", TickPhase.Lifecycle, prerequisites: new[] { "late" }),
            new RecordingSystem("late", TickPhase.Reports),
        };

        Assert.That(() => new MonthlySimulation<List<string>>(systems), Throws.InvalidOperationException);
    }

    private sealed class RecordingSystem : IMonthlySystem<List<string>>
    {
        private static readonly RuntimeIdCounter<DomainEventEntity> EventIds = new();

        public RecordingSystem(string id, TickPhase phase = TickPhase.Lifecycle, IReadOnlyCollection<string>? prerequisites = null)
        {
            Id = id;
            Phase = phase;
            Prerequisites = prerequisites ?? Array.Empty<string>();
        }

        public string Id { get; }
        public TickPhase Phase { get; }
        public IReadOnlyCollection<string> Reads => Array.Empty<string>();
        public IReadOnlyCollection<string> Writes => Array.Empty<string>();
        public IReadOnlyCollection<string> Prerequisites { get; }

        public IReadOnlyList<IDomainEvent> Tick(List<string> state, MonthlyTickContext context)
        {
            state.Add($"{Id}:{context.Date.TotalMonths}");
            return new IDomainEvent[] { new SystemRan(EventIds.Issue(), context.Date, Id) };
        }
    }

    private sealed record SystemRan(RuntimeId<DomainEventEntity> EventId, GameDate OccurredDate, string Id) : IDomainEvent
    {
        public string Type => "test.systemRan";
        public int SchemaVersion => 1;
        public IReadOnlyList<string> SubjectIds => Array.Empty<string>();
        public Visibility Visibility => Visibility.Public;
        public string? CausationId => null;
    }
}
