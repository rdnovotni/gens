using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.State;

public sealed class KnowledgeStateTests
{
    [Test]
    public void SetAndGetRoundTrip()
    {
        var knowledge = new KnowledgeState();
        var key = new KnowledgeKey("player", "char_0000001", "location");
        var entry = new KnowledgeEntry("Rome", KnowledgeConfidence.Certain, new GameDate(3), "event_0000001");

        knowledge.Set(key, entry);

        Assert.That(knowledge.TryGet(key, out var found), Is.True);
        Assert.That(found, Is.EqualTo(entry));
    }

    [Test]
    public void ForObserverIsFilteredAndOrdered()
    {
        var knowledge = new KnowledgeState();
        knowledge.Set(new KnowledgeKey("player", "char_0000002", "location"),
            new KnowledgeEntry("Ostia", KnowledgeConfidence.Believed, new GameDate(1), null));
        knowledge.Set(new KnowledgeKey("player", "char_0000001", "location"),
            new KnowledgeEntry("Rome", KnowledgeConfidence.Certain, new GameDate(1), null));
        knowledge.Set(new KnowledgeKey("rival", "char_0000001", "location"),
            new KnowledgeEntry("Unknown", KnowledgeConfidence.Rumored, new GameDate(1), null));

        var subjects = knowledge.ForObserver("player").Select(pair => pair.Key.SubjectId).ToArray();

        Assert.That(subjects, Is.EqualTo(new[] { "char_0000001", "char_0000002" }));
    }

    [Test]
    public void TryGetReturnsFalseForMissingKey()
    {
        var knowledge = new KnowledgeState();
        Assert.That(knowledge.TryGet(new KnowledgeKey("player", "char_0000001", "location"), out _), Is.False);
    }
}
