using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Land;

public sealed class RegionTests
{
    [Test]
    public void CreateRejectsAnEmptyName()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();

        Assert.Throws<ArgumentException>(() => Region.Create(regionId, ""));
        Assert.Throws<ArgumentException>(() => Region.Create(regionId, "   "));
    }

    [Test]
    public void CreateStoresIdAndName()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();

        var region = Region.Create(regionId, "Italian Heartland");

        Assert.That(region.Id, Is.EqualTo(regionId));
        Assert.That(region.Name, Is.EqualTo("Italian Heartland"));
    }

    [Test]
    public void RegionsRegistryIteratesInAscendingIdOrder()
    {
        var state = new WorldState(new GameDate(0));
        var idA = state.RegionIds.Issue();
        var idB = state.RegionIds.Issue();

        state.Regions.Add(idB, Region.Create(idB, "Gallic Frontier"));
        state.Regions.Add(idA, Region.Create(idA, "Italian Heartland"));

        var ids = state.Regions.InAscendingOrder().Select(e => e.Key).ToArray();
        Assert.That(ids, Is.EqualTo(new[] { idA, idB }));
    }
}
