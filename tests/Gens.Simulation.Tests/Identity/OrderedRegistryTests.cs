using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Identity;

public sealed class OrderedRegistryTests
{
    [Test]
    public void IterationIsAscendingRegardlessOfInsertionOrder()
    {
        var counter = new RuntimeIdCounter<Character>();
        var first = counter.Issue();
        var second = counter.Issue();
        var third = counter.Issue();

        var registry = new OrderedRegistry<RuntimeId<Character>, string>();
        registry.Add(third, "third");
        registry.Add(first, "first");
        registry.Add(second, "second");

        var ordered = registry.InAscendingOrder().Select(entry => entry.Value).ToArray();
        Assert.That(ordered, Is.EqualTo(new[] { "first", "second", "third" }));
    }

    [Test]
    public void DuplicateIdIsRejected()
    {
        var counter = new RuntimeIdCounter<Character>();
        var id = counter.Issue();
        var registry = new OrderedRegistry<RuntimeId<Character>, string>();
        registry.Add(id, "one");

        Assert.That(() => registry.Add(id, "two"), Throws.ArgumentException);
    }

    [Test]
    public void TryGetReturnsFalseForMissingId()
    {
        var counter = new RuntimeIdCounter<Character>();
        var id = counter.Issue();
        var registry = new OrderedRegistry<RuntimeId<Character>, string>();

        Assert.That(registry.TryGet(id, out _), Is.False);
    }
}
