using Gens.Simulation.Commands;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Time;

public sealed class WriteSetVerifyingSimulationTests
{
    [Test]
    public void DeclaredWriteIsAllowed()
    {
        var simulation = new WriteSetVerifyingSimulation(new[] { new CharacterCreatingSystem(declareWrite: true) });
        var state = new WorldState(new GameDate(0));

        Assert.That(() => simulation.Tick(state, new GameDate(0), new RandomStreamSet()), Throws.Nothing);
    }

    [Test]
    public void UndeclaredWriteThrowsInDebugBuilds()
    {
        var simulation = new WriteSetVerifyingSimulation(new[] { new CharacterCreatingSystem(declareWrite: false) });
        var state = new WorldState(new GameDate(0));

#if DEBUG
        Assert.That(() => simulation.Tick(state, new GameDate(0), new RandomStreamSet()),
            Throws.InvalidOperationException);
#else
        Assert.That(() => simulation.Tick(state, new GameDate(0), new RandomStreamSet()), Throws.Nothing);
#endif
    }

    private sealed class CharacterCreatingSystem(bool declareWrite) : IMonthlySystem<WorldState>
    {
        public string Id => "character-creator";
        public TickPhase Phase => TickPhase.Lifecycle;
        public IReadOnlyCollection<string> Reads => Array.Empty<string>();
        public IReadOnlyCollection<string> Writes => declareWrite ? new[] { "characterIds", "characters" } : Array.Empty<string>();
        public IReadOnlyCollection<string> Prerequisites => Array.Empty<string>();

        public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
        {
            var id = state.CharacterIds.Issue();
            state.Characters.Add(id, CharacterTestFixtures.Minimal(id));
            return Array.Empty<IDomainEvent>();
        }
    }
}
