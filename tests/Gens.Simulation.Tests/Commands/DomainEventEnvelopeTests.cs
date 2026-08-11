using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Commands;

public sealed class DomainEventEnvelopeTests
{
    [Test]
    public void SubjectIdsPreserveDeclaredOrder()
    {
        var counter = new RuntimeIdCounter<DomainEventEntity>();
        var subjectIds = new[] { "char_0000003", "char_0000001", "char_0000002" };
        var domainEvent = new TestEvent(counter.Issue(), new GameDate(1), subjectIds, Visibility.Public, null);

        Assert.That(domainEvent.SubjectIds, Is.EqualTo(subjectIds));
    }

    [Test]
    public void PublicVisibilityHasNoNamedObservers()
    {
        Assert.That(Visibility.Public.ObserverIds, Is.Empty);
        Assert.That(Visibility.Public.Channel, Is.EqualTo("public"));
    }

    [Test]
    public void PrivateVisibilityNamesObservers()
    {
        var visibility = Visibility.Private("char_0000001", "char_0000002");

        Assert.That(visibility.ObserverIds, Is.EqualTo(new[] { "char_0000001", "char_0000002" }));
        Assert.That(visibility.Channel, Is.EqualTo("witnessed"));
    }

    [Test]
    public void CausationIdChainsBackToCommandId()
    {
        var commandCounter = new RuntimeIdCounter<Command>();
        var commandId = commandCounter.Issue();
        var eventCounter = new RuntimeIdCounter<DomainEventEntity>();
        var domainEvent = new TestEvent(eventCounter.Issue(), new GameDate(1), Array.Empty<string>(), Visibility.Public, commandId.ToTaggedString());

        Assert.That(domainEvent.CausationId, Is.EqualTo(commandId.ToTaggedString()));
    }

    private sealed record TestEvent(
        RuntimeId<DomainEventEntity> EventId,
        GameDate OccurredDate,
        IReadOnlyList<string> SubjectIds,
        Visibility Visibility,
        string? CausationId) : IDomainEvent
    {
        public string Type => "test.event";
        public int SchemaVersion => 1;
    }
}
