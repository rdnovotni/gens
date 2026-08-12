using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Identity;

public sealed class RuntimeIdTests
{
    [Test]
    public void CounterIssuesMonotonicallyIncreasingIds()
    {
        var counter = new RuntimeIdCounter<Character>();
        var first = counter.Issue();
        var second = counter.Issue();

        Assert.That(first.Value, Is.EqualTo(0));
        Assert.That(second.Value, Is.EqualTo(1));
        Assert.That(first < second, Is.True);
    }

    [Test]
    public void DifferentEntityKindsHaveIndependentCounters()
    {
        var characters = new RuntimeIdCounter<Character>();
        var plots = new RuntimeIdCounter<Plot>();

        characters.Issue();
        characters.Issue();
        var plot = plots.Issue();

        Assert.That(plot.Value, Is.EqualTo(0));
    }

    [Test]
    public void TaggedStringRoundTrips()
    {
        var counter = new RuntimeIdCounter<Character>();
        counter.Issue();
        var id = counter.Issue();

        var tagged = id.ToTaggedString();
        Assert.That(tagged, Is.EqualTo("char_0000001"));
        Assert.That(RuntimeId<Character>.Parse(tagged), Is.EqualTo(id));
    }

    [Test]
    public void ParseRejectsMismatchedTag()
    {
        Assert.That(() => RuntimeId<Character>.Parse("plot_0000001"), Throws.TypeOf<FormatException>());
    }

    [Test]
    public void RestoredCounterContinuesFromGivenValue()
    {
        var restored = RuntimeIdCounter<Character>.Restore(5);
        Assert.That(restored.Issue().Value, Is.EqualTo(5));
    }
}
