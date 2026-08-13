using Gens.Simulation.Characters;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class PopGroupTests
{
    [Test]
    public void CreateRejectsANegativeSize()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();

        Assert.Throws<ArgumentOutOfRangeException>(() => PopGroup.Create(settlementId, PopGroupType.Coloni, -1));
    }

    [Test]
    public void PopGroupKeyOrdersBySettlementThenGroupType()
    {
        var state = new WorldState(new GameDate(0));
        var settlementA = state.SettlementIds.Issue();
        var settlementB = state.SettlementIds.Issue();

        var keyA = new PopGroupKey(settlementA, PopGroupType.Veterans);
        var keyB = new PopGroupKey(settlementB, PopGroupType.Coloni);
        var sameSettlementLower = new PopGroupKey(settlementA, PopGroupType.Coloni);

        Assert.That(keyA.CompareTo(keyB), Is.LessThan(0));
        Assert.That(sameSettlementLower.CompareTo(keyA), Is.LessThan(0));
    }

    [Test]
    public void PopGroupsRegistryIteratesInAscendingKeyOrderRegardlessOfInsertionOrder()
    {
        var state = new WorldState(new GameDate(0));
        var settlement = state.SettlementIds.Issue();

        state.PopGroups.Add(new PopGroupKey(settlement, PopGroupType.Veterans), PopGroup.Create(settlement, PopGroupType.Veterans, 3));
        state.PopGroups.Add(new PopGroupKey(settlement, PopGroupType.Coloni), PopGroup.Create(settlement, PopGroupType.Coloni, 10));

        var ordered = state.PopGroups.InAscendingOrder().Select(entry => entry.Key.GroupType).ToArray();

        Assert.That(ordered, Is.EqualTo(new[] { PopGroupType.Coloni, PopGroupType.Veterans }));
    }
}
