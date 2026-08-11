using Gens.Simulation.Queries;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Queries;

public sealed class CampaignClockQueryTests
{
    [Test]
    public void ProjectionMatchesGameDateConversionAtEpoch()
    {
        var state = new WorldState(new GameDate(0));
        var query = new CampaignClockQuery();

        var projection = query.Execute(state, "player");

        Assert.That(projection.DisplayYear, Is.EqualTo(754));
        Assert.That(projection.Era, Is.EqualTo("BCE"));
        Assert.That(projection.MonthOfYear, Is.EqualTo(1));
    }

    [Test]
    public void ProjectionCrossesIntoCe()
    {
        var state = new WorldState(new GameDate(754 * 12));
        var query = new CampaignClockQuery();

        var projection = query.Execute(state, "player");

        Assert.That(projection.DisplayYear, Is.EqualTo(1));
        Assert.That(projection.Era, Is.EqualTo("CE"));
    }

    [Test]
    public void ProjectionIsIdenticalRegardlessOfObserver()
    {
        var state = new WorldState(new GameDate(100));
        var query = new CampaignClockQuery();

        Assert.That(query.Execute(state, "player"), Is.EqualTo(query.Execute(state, "rival")));
    }
}
