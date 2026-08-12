using Gens.Simulation.Campaign;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Campaign;

public sealed class ScheduledActionSystemTests
{
    [Test]
    public void ActionsDueThisMonthAreDispatchedAndRemovedFromTheQueue()
    {
        var state = new WorldState(new GameDate(5));
        var dueId = state.ScheduledActionIds.Issue();
        state.ScheduledActions.Add(
            new ScheduledActionKey(new GameDate(5), dueId),
            new ScheduledActionEntry(dueId, new GameDate(5), "system", "test.action", "{}", null));

        var system = new ScheduledActionSystem();
        var context = new MonthlyTickContext(new GameDate(5), new RandomStreamSet());

        var events = system.Tick(state, context);

        Assert.That(state.ScheduledActions.Count, Is.EqualTo(0));
        Assert.That(events, Has.Count.EqualTo(1));
        var executed = (GenericCommandExecutedEvent)events[0];
        Assert.That(executed.CommandType, Is.EqualTo("test.action"));
        Assert.That(executed.ActorId, Is.EqualTo("system"));
    }

    [Test]
    public void ActionsDueInAFutureMonthStayQueued()
    {
        var state = new WorldState(new GameDate(5));
        var futureId = state.ScheduledActionIds.Issue();
        state.ScheduledActions.Add(
            new ScheduledActionKey(new GameDate(9), futureId),
            new ScheduledActionEntry(futureId, new GameDate(9), "system", "test.action", "{}", null));

        var system = new ScheduledActionSystem();
        var context = new MonthlyTickContext(new GameDate(5), new RandomStreamSet());

        var events = system.Tick(state, context);

        Assert.That(state.ScheduledActions.Count, Is.EqualTo(1));
        Assert.That(events, Is.Empty);
    }

    [Test]
    public void MultipleActionsDueTheSameMonthDispatchInDueDateThenActionIdOrder()
    {
        var state = new WorldState(new GameDate(5));
        var idZero = state.ScheduledActionIds.Issue();
        var idOne = state.ScheduledActionIds.Issue();
        // Add out of ID order to prove the queue orders by key, not insertion order.
        state.ScheduledActions.Add(new ScheduledActionKey(new GameDate(5), idOne), new ScheduledActionEntry(idOne, new GameDate(5), "system", "second-by-id", "{}", null));
        state.ScheduledActions.Add(new ScheduledActionKey(new GameDate(5), idZero), new ScheduledActionEntry(idZero, new GameDate(5), "system", "first-by-id", "{}", null));

        var system = new ScheduledActionSystem();
        var context = new MonthlyTickContext(new GameDate(5), new RandomStreamSet());

        var events = system.Tick(state, context).Cast<GenericCommandExecutedEvent>().ToArray();

        Assert.That(events.Select(e => e.CommandType), Is.EqualTo(new[] { "first-by-id", "second-by-id" }));
    }

    [Test]
    public void NoDueActionsProducesNoEventsAndDoesNotMutateCounters()
    {
        var state = new WorldState(new GameDate(5));
        var system = new ScheduledActionSystem();
        var context = new MonthlyTickContext(new GameDate(5), new RandomStreamSet());

        var beforeCommandIds = state.CommandIds.Peek;
        var beforeEventIds = state.EventIds.Peek;

        var events = system.Tick(state, context);

        Assert.That(events, Is.Empty);
        Assert.That(state.CommandIds.Peek, Is.EqualTo(beforeCommandIds));
        Assert.That(state.EventIds.Peek, Is.EqualTo(beforeEventIds));
    }
}
