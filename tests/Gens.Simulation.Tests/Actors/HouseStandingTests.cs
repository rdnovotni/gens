using Gens.Simulation.Actors;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Actors;

/// <summary>Phase 10 item 5 ("house standing... dossiers, information staleness") storage-layer
/// coverage, mirroring <see cref="Policies.ChangeRitesBudgetCommandTests"/>'s resolver-pattern shape.</summary>
public sealed class HouseStandingTests
{
    [Test]
    public void BetweenNormalizesRegardlessOfArgumentOrder()
    {
        var state = new WorldState(new GameDate(0));
        var x = state.ActorIds.Issue();
        var y = state.ActorIds.Issue();

        Assert.That(HouseStandingKey.Between(x, y), Is.EqualTo(HouseStandingKey.Between(y, x)));
    }

    [Test]
    public void BetweenRejectsTheSameActorTwice()
    {
        var state = new WorldState(new GameDate(0));
        var x = state.ActorIds.Issue();

        Assert.Throws<ArgumentException>(() => HouseStandingKey.Between(x, x));
    }

    [Test]
    public void ResolverDefaultsToNeutralForAnUntrackedPair()
    {
        var state = new WorldState(new GameDate(0));
        var x = state.ActorIds.Issue();
        var y = state.ActorIds.Issue();

        Assert.That(HouseStandingResolver.GetEffectiveStanding(state, x, y), Is.EqualTo(HouseStandingLevel.Neutral));
    }

    [Test]
    public void ResolverReadsAStoredEntryRegardlessOfLookupOrder()
    {
        var state = new WorldState(new GameDate(0));
        var x = state.ActorIds.Issue();
        var y = state.ActorIds.Issue();
        state.HouseStandings.Add(HouseStandingKey.Between(x, y), new HouseStanding(HouseStandingLevel.Feuding));

        Assert.Multiple(() =>
        {
            Assert.That(HouseStandingResolver.GetEffectiveStanding(state, x, y), Is.EqualTo(HouseStandingLevel.Feuding));
            Assert.That(HouseStandingResolver.GetEffectiveStanding(state, y, x), Is.EqualTo(HouseStandingLevel.Feuding));
        });
    }
}
