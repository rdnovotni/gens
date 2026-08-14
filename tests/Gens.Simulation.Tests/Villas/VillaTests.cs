using Gens.Simulation.Buildings;
using Gens.Simulation.Villas;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Villas;

public sealed class VillaTests
{
    [Test]
    public void RusticaRejectsRoomsGatedToALaterStage()
    {
        var villa = new Villa();
        var peristylium = new VillaRoomInstance(
            new VillaRoomDefinition("peristylium", VillaStage.Urbana));

        Assert.That(() => villa.AddRoom(peristylium), Throws.InvalidOperationException);
    }

    [Test]
    public void RoomsAreStableOrderedAndReuseBuildingCondition()
    {
        var villa = new Villa(VillaStage.Urbana);
        villa.AddRoom(new VillaRoomInstance(
            new VillaRoomDefinition("xenodochium", VillaStage.Urbana),
            capacityTier: VillaRoomCapacityTier.Ample,
            condition: BuildingCondition.Sound));
        villa.AddRoom(new VillaRoomInstance(new VillaRoomDefinition("atrium", VillaStage.Rustica)));

        Assert.That(villa.Rooms.Select(room => room.Definition.Key), Is.EqualTo(new[] { "atrium", "xenodochium" }));
        Assert.That(villa.Rooms.Last().Condition, Is.EqualTo(BuildingCondition.Sound));
        Assert.That(villa.UsedRoomSlots, Is.EqualTo(2));
    }

    [Test]
    public void OutpostCannotAdvance()
    {
        var villa = new Villa(isOutpost: true);

        Assert.That(() => villa.AdvanceStage(), Throws.InvalidOperationException);
    }
}
