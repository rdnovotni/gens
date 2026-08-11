using Gens.Simulation.Commands;
using Gens.Simulation.Random;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests;

public sealed class MonthlySimulationTests
{
    private static readonly string[] ExpectedState = { "population:10", "economy:10" };
    private static readonly string[] ExpectedSystemIds = { "population", "economy" };

    [Test]
    public void SystemsAndEventsUseRegistrationOrder()
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

    private sealed class RecordingSystem(string id) : IMonthlySystem<List<string>>
    {
        public string Id => id;

        public IReadOnlyList<IDomainEvent> Tick(List<string> state, MonthlyTickContext context)
        {
            state.Add($"{Id}:{context.Date.TotalMonths}");
            return new IDomainEvent[] { new SystemRan(Id) };
        }
    }

    private sealed record SystemRan(string Id) : IDomainEvent;
}
