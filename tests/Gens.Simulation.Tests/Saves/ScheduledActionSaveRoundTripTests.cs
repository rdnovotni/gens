using Gens.Simulation.Random;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

public sealed class ScheduledActionSaveRoundTripTests
{
    [Test]
    public void ScheduledActionsRoundTripThroughASaveWithTheSameHash()
    {
        var state = new WorldState(new GameDate(3));
        var dueId = state.ScheduledActionIds.Issue();
        var futureId = state.ScheduledActionIds.Issue();
        state.ScheduledActions.Add(
            new ScheduledActionKey(new GameDate(3), dueId),
            new ScheduledActionEntry(dueId, new GameDate(3), "system", "test.action", "{\"n\":1}", "event_0000001"));
        state.ScheduledActions.Add(
            new ScheduledActionKey(new GameDate(9), futureId),
            new ScheduledActionEntry(futureId, new GameDate(9), "system", "test.other", "{}", null));

        var streams = new RandomStreamSet();
        streams.AddDerived("campaign", 1234UL);

        var beforeHash = StateHasher.Hash(state);
        var path = Path.Combine(Path.GetTempPath(), $"gens-save-test-{Guid.NewGuid():N}.gens");
        try
        {
            SaveWriter.Write(path, state, streams, "0.0.0-test", "content-hash-placeholder");
            var loaded = SaveReader.Read(path);

            Assert.That(StateHasher.Hash(loaded.State), Is.EqualTo(beforeHash));
            Assert.That(loaded.State.ScheduledActions.Count, Is.EqualTo(2));
            Assert.That(loaded.State.ScheduledActionIds.Peek, Is.EqualTo(state.ScheduledActionIds.Peek));

            var restoredDue = loaded.State.ScheduledActions.InAscendingOrder().First().Value;
            Assert.That(restoredDue.ActionId, Is.EqualTo(dueId));
            Assert.That(restoredDue.DueDate, Is.EqualTo(new GameDate(3)));
            Assert.That(restoredDue.ActorId, Is.EqualTo("system"));
            Assert.That(restoredDue.ActionType, Is.EqualTo("test.action"));
            Assert.That(restoredDue.PayloadJson, Is.EqualTo("{\"n\":1}"));
            Assert.That(restoredDue.CausationId, Is.EqualTo("event_0000001"));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
