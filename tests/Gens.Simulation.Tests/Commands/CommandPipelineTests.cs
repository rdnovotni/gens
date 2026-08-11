using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Commands;

public sealed class CommandPipelineTests
{
    private static readonly ValidationErrorCode SlotOccupied = new("test.slotOccupied");

    [Test]
    public void AcceptedCommandGetsIncreasingSequenceNumbers()
    {
        var state = new WorldState(new GameDate(0));
        var pipeline = AlwaysAcceptPipeline();

        var first = pipeline.Execute(state, MakeCommand(state));
        var second = pipeline.Execute(state, MakeCommand(state));

        Assert.That(first.SequenceNumber, Is.EqualTo(0));
        Assert.That(second.SequenceNumber, Is.EqualTo(1));
    }

    [Test]
    public void RejectedCommandHasNoSequenceNumberAndNoEvents()
    {
        var state = new WorldState(new GameDate(0));
        var pipeline = new CommandPipeline<WorldState, TestCommand>(
            validate: (_, _) => SlotOccupied,
            mutate: (_, _) => throw new InvalidOperationException("mutate must not run for a rejected command"),
            issueSequenceNumber: worldState => worldState.IssueCommandSequenceNumber());

        var result = pipeline.Execute(state, MakeCommand(state));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.SequenceNumber, Is.Null);
        Assert.That(result.Events, Is.Empty);
        Assert.That(result.Error, Is.EqualTo(SlotOccupied));
    }

    [Test]
    public void RejectedCommandsDoNotConsumeASequenceNumber()
    {
        var state = new WorldState(new GameDate(0));
        var accept = AlwaysAcceptPipeline();
        var reject = new CommandPipeline<WorldState, TestCommand>(
            validate: (_, _) => SlotOccupied,
            mutate: (_, _) => Array.Empty<IDomainEvent>(),
            issueSequenceNumber: worldState => worldState.IssueCommandSequenceNumber());

        var acceptedFirst = accept.Execute(state, MakeCommand(state));
        var rejected = reject.Execute(state, MakeCommand(state));
        var acceptedSecond = accept.Execute(state, MakeCommand(state));

        Assert.That(acceptedFirst.SequenceNumber, Is.EqualTo(0));
        Assert.That(rejected.SequenceNumber, Is.Null);
        Assert.That(acceptedSecond.SequenceNumber, Is.EqualTo(1));
    }

    private static CommandPipeline<WorldState, TestCommand> AlwaysAcceptPipeline() => new(
        validate: (_, _) => null,
        mutate: (_, _) => Array.Empty<IDomainEvent>(),
        issueSequenceNumber: state => state.IssueCommandSequenceNumber());

    private static TestCommand MakeCommand(WorldState state) =>
        new(state.CommandIds.Issue(), "system", state.Date, null);

    private sealed record TestCommand(RuntimeId<Command> CommandId, string ActorId, GameDate SubmittedDate, string? CausationId) : ICommand;
}
