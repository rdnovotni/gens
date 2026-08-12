using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.ExitGate;

/// <summary>
/// Proves the Phase 2 exit gate, verbatim from the roadmap: "the same seed plus the same ordered
/// commands produces identical event logs and state hashes across repeated headless runs. A
/// rejected or failed command leaves state and RNG unchanged."
/// </summary>
public sealed class DeterministicReplayTests
{
    private const ulong RootSeed = 987654321UL;

    [Test]
    public void SameSeedSameCommandsProduceIdenticalEventLogsAndHashes()
    {
        var runA = RunScenario();
        var runB = RunScenario();

        Assert.That(runA.Events.Select(Describe), Is.EqualTo(runB.Events.Select(Describe)));
        Assert.That(runA.HashesPerTick, Is.EqualTo(runB.HashesPerTick));
    }

    [Test]
    public void RejectedCommandLeavesStateHashAndRngUnchanged()
    {
        var (state, streams, pipeline) = SetupHarness();
        // Issuing the CommandId to build the command is bookkeeping outside the pipeline's control
        // (ADR 0006 only makes the *sequence number* acceptance-only) — capture "before" only once
        // the command itself exists, so the comparison isolates what Execute does, not what
        // constructing its argument did.
        var command = new CreateCharacterCommand(state.CommandIds.Issue(), "system", state.Date, null, ShouldFail: true);
        var beforeHash = StateHasher.Hash(state);
        var beforeStreamState = streams.CaptureStates();

        var result = pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(StateHasher.Hash(state), Is.EqualTo(beforeHash));
        Assert.That(streams.CaptureStates(), Is.EqualTo(beforeStreamState));
    }

    [Test]
    public void RejectedCommandDoesNotConsumeASequenceNumber()
    {
        var (state, _, pipeline) = SetupHarness();

        var accepted1 = pipeline.Execute(state, new CreateCharacterCommand(state.CommandIds.Issue(), "system", state.Date, null, false));
        var rejected = pipeline.Execute(state, new CreateCharacterCommand(state.CommandIds.Issue(), "system", state.Date, null, true));
        var accepted2 = pipeline.Execute(state, new CreateCharacterCommand(state.CommandIds.Issue(), "system", state.Date, null, false));

        Assert.That(accepted1.SequenceNumber, Is.EqualTo(0));
        Assert.That(rejected.SequenceNumber, Is.Null);
        Assert.That(accepted2.SequenceNumber, Is.EqualTo(1));
    }

    [Test]
    public void FailedMutationPropagatesUncaughtRatherThanPartiallyApplying()
    {
        var state = new WorldState(new GameDate(0));
        var pipeline = new CommandPipeline<WorldState, CreateCharacterCommand>(
            validate: (_, _) => null,
            mutate: (_, _) => throw new InvalidOperationException("simulated internal failure mid-mutation"),
            issueSequenceNumber: worldState => worldState.IssueCommandSequenceNumber());

        Assert.That(
            () => pipeline.Execute(state, new CreateCharacterCommand(state.CommandIds.Issue(), "system", state.Date, null, false)),
            Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void AddingUnrelatedRandomStreamDoesNotPerturbExistingDrawsAcrossAFullTick()
    {
        var baseline = RunTicksOnly(includeExtraStream: false);
        var withExtraStream = RunTicksOnly(includeExtraStream: true);

        Assert.That(withExtraStream.Draws, Is.EqualTo(baseline.Draws));
    }

    [Test]
    public void TopologicalSortIsStableAcrossShuffledInputOrder()
    {
        var systemsA = new IMonthlySystem<WorldState>[]
        {
            new GrowthSystem("b", new[] { "a" }),
            new GrowthSystem("a", Array.Empty<string>()),
            new GrowthSystem("c", Array.Empty<string>()),
        };
        var systemsB = new IMonthlySystem<WorldState>[]
        {
            new GrowthSystem("c", Array.Empty<string>()),
            new GrowthSystem("a", Array.Empty<string>()),
            new GrowthSystem("b", new[] { "a" }),
        };

        var simA = new MonthlySimulation<WorldState>(systemsA);
        var simB = new MonthlySimulation<WorldState>(systemsB);

        Assert.That(simA.OrderedSystems.Select(s => s.Id), Is.EqualTo(simB.OrderedSystems.Select(s => s.Id)));
    }

    private static (WorldState State, RandomStreamSet Streams, CommandPipeline<WorldState, CreateCharacterCommand> Pipeline) SetupHarness()
    {
        var state = new WorldState(new GameDate(0));
        var streams = new RandomStreamSet();
        streams.AddDerived("commands", RootSeed);
        var pipeline = BuildPipeline(streams);
        return (state, streams, pipeline);
    }

    private static CommandPipeline<WorldState, CreateCharacterCommand> BuildPipeline(RandomStreamSet streams) => new(
        validate: (_, command) => command.ShouldFail ? new ValidationErrorCode("test.rejected") : null,
        mutate: (worldState, _) =>
        {
            streams.NextUInt("commands");
            var id = worldState.CharacterIds.Issue();
            worldState.Characters.Add(id, CharacterTestFixtures.Minimal(id));
            return new IDomainEvent[]
            {
                new CharacterCreatedEvent(worldState.EventIds.Issue(), worldState.Date, id.ToTaggedString()),
            };
        },
        issueSequenceNumber: worldState => worldState.IssueCommandSequenceNumber());

    private static ScenarioResult RunScenario()
    {
        var state = new WorldState(new GameDate(0));
        var streams = new RandomStreamSet();
        streams.AddDerived("growth", RootSeed);
        streams.AddDerived("commands", RootSeed);

        var simulation = new MonthlySimulation<WorldState>(new IMonthlySystem<WorldState>[]
        {
            new GrowthSystem("growth", Array.Empty<string>()),
        });
        var pipeline = BuildPipeline(streams);

        var allEvents = new List<IDomainEvent>();
        var hashesPerTick = new List<ulong>();

        for (var month = 0; month < 3; month++)
        {
            var shouldFail = month == 1;
            var result = pipeline.Execute(
                state,
                new CreateCharacterCommand(state.CommandIds.Issue(), "system", state.Date, null, shouldFail));
            allEvents.AddRange(result.Events);

            var tickEvents = simulation.Tick(state, state.Date, streams);
            allEvents.AddRange(tickEvents);

            state.AdvanceMonth();
            hashesPerTick.Add(StateHasher.Hash(state));
        }

        return new ScenarioResult(allEvents, hashesPerTick);
    }

    private static DrawResult RunTicksOnly(bool includeExtraStream)
    {
        var state = new WorldState(new GameDate(0));
        var streams = new RandomStreamSet();
        streams.AddDerived("growth", RootSeed);
        if (includeExtraStream)
            streams.AddDerived("events", RootSeed);

        var simulation = new MonthlySimulation<WorldState>(new IMonthlySystem<WorldState>[]
        {
            new GrowthSystem("growth", Array.Empty<string>()),
        });

        var draws = new List<uint>();
        for (var month = 0; month < 3; month++)
        {
            simulation.Tick(state, state.Date, streams);
            draws.Add(streams.NextUInt("growth"));
            state.AdvanceMonth();
        }

        return new DrawResult(draws);
    }

    private static string Describe(IDomainEvent domainEvent) =>
        $"{domainEvent.EventId}:{domainEvent.Type}:{domainEvent.SchemaVersion}:" +
        $"{domainEvent.OccurredDate.TotalMonths}:{string.Join(",", domainEvent.SubjectIds)}";

    private sealed record ScenarioResult(IReadOnlyList<IDomainEvent> Events, IReadOnlyList<ulong> HashesPerTick);

    private sealed record DrawResult(IReadOnlyList<uint> Draws);

    private sealed record CreateCharacterCommand(
        RuntimeId<Command> CommandId,
        string ActorId,
        GameDate SubmittedDate,
        string? CausationId,
        bool ShouldFail) : ICommand;

    private sealed record CharacterCreatedEvent(RuntimeId<DomainEventEntity> EventId, GameDate OccurredDate, string SubjectId) : IDomainEvent
    {
        public string Type => "test.characterCreated";
        public int SchemaVersion => 1;
        public IReadOnlyList<string> SubjectIds => new[] { SubjectId };
        public Visibility Visibility => Visibility.Public;
        public string? CausationId => null;
    }

    private sealed class GrowthSystem : IMonthlySystem<WorldState>
    {
        public GrowthSystem(string id, IReadOnlyCollection<string> prerequisites)
        {
            Id = id;
            Prerequisites = prerequisites;
        }

        public string Id { get; }
        public TickPhase Phase => TickPhase.Lifecycle;
        public IReadOnlyCollection<string> Reads => Array.Empty<string>();
        public IReadOnlyCollection<string> Writes => new[] { "characterIds", "characters" };
        public IReadOnlyCollection<string> Prerequisites { get; }

        public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
        {
            var draw = context.RandomStreams.NextUInt(Id);
            if (draw % 2 != 0)
                return Array.Empty<IDomainEvent>();

            var id = state.CharacterIds.Issue();
            state.Characters.Add(id, CharacterTestFixtures.Minimal(id));
            return new IDomainEvent[]
            {
                new CharacterCreatedEvent(state.EventIds.Issue(), context.Date, id.ToTaggedString()),
            };
        }
    }
}
