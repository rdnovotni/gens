using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.State;

public sealed class WorldStateTests
{
    [Test]
    public void CountersAreIndependentPerEntityKind()
    {
        var state = new WorldState(new GameDate(0));

        var firstCharacter = state.CharacterIds.Issue();
        var firstPlot = state.PlotIds.Issue();
        state.CharacterIds.Issue();

        Assert.That(firstCharacter.Value, Is.EqualTo(0));
        Assert.That(firstPlot.Value, Is.EqualTo(0));
        Assert.That(state.CharacterIds.Peek, Is.EqualTo(2));
    }

    [Test]
    public void CommandSequenceNumbersAreMonotonic()
    {
        var state = new WorldState(new GameDate(0));

        var first = state.IssueCommandSequenceNumber();
        var second = state.IssueCommandSequenceNumber();

        Assert.That(first, Is.EqualTo(0));
        Assert.That(second, Is.EqualTo(1));
        Assert.That(state.NextCommandSequenceNumber, Is.EqualTo(2));
    }

    [Test]
    public void AdvanceMonthMovesTheClockForwardByOne()
    {
        var state = new WorldState(new GameDate(5));

        state.AdvanceMonth();

        Assert.That(state.Date, Is.EqualTo(new GameDate(6)));
    }

    [Test]
    public void CharacterRegistryIteratesInAscendingIdOrder()
    {
        var state = new WorldState(new GameDate(0));
        var second = state.CharacterIds.Issue();
        var first = state.CharacterIds.Issue();
        var secondCharacter = CharacterTestFixtures.Minimal(second, praenomen: "Second");
        var firstCharacter = CharacterTestFixtures.Minimal(first, praenomen: "First");
        state.Characters.Add(second, secondCharacter);
        state.Characters.Add(first, firstCharacter);

        var order = state.Characters.InAscendingOrder().Select(entry => entry.Value).ToArray();

        Assert.That(order, Is.EqualTo(new[] { secondCharacter, firstCharacter }));
    }
}
