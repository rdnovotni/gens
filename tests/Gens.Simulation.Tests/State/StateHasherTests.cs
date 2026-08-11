using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.State;

public sealed class StateHasherTests
{
    [Test]
    public void IdenticalOperationSequencesProduceIdenticalHashes()
    {
        var stateA = BuildState();
        var stateB = BuildState();

        Assert.That(StateHasher.Hash(stateA), Is.EqualTo(StateHasher.Hash(stateB)));
    }

    [Test]
    public void DifferentCounterValueChangesTheHash()
    {
        var baseline = BuildState();
        var baselineHash = StateHasher.Hash(baseline);

        var different = BuildState();
        different.PlotIds.Issue();

        Assert.That(StateHasher.Hash(different), Is.Not.EqualTo(baselineHash));
    }

    [Test]
    public void DifferentDateChangesTheHash()
    {
        var atMonthZero = new WorldState(new GameDate(0));
        var atMonthOne = new WorldState(new GameDate(1));

        Assert.That(StateHasher.Hash(atMonthZero), Is.Not.EqualTo(StateHasher.Hash(atMonthOne)));
    }

    [Test]
    public void KnowledgeEntryChangesTheHash()
    {
        var baseline = BuildState();
        var baselineHash = StateHasher.Hash(baseline);

        var withKnowledge = BuildState();
        withKnowledge.Knowledge.Set(
            new KnowledgeKey("player", "char_0000001", "location"),
            new KnowledgeEntry("Rome", KnowledgeConfidence.Certain, withKnowledge.Date, null));

        Assert.That(StateHasher.Hash(withKnowledge), Is.Not.EqualTo(baselineHash));
    }

    private static WorldState BuildState()
    {
        var state = new WorldState(new GameDate(10));
        var first = state.CharacterIds.Issue();
        var second = state.CharacterIds.Issue();
        state.Characters.Add(first, new object());
        state.Characters.Add(second, new object());
        return state;
    }
}
