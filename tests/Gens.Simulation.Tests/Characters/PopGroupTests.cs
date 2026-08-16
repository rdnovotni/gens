using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Numerics;
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
    public void CreateDefaultsEveryNewFieldForAnOrdinaryGroup()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();

        var group = PopGroup.Create(settlementId, PopGroupType.Coloni, 10);

        Assert.That(group.LegalStatusDistribution, Is.EqualTo(new LegalStatusDistribution(0, 0, 10, 0)));
        Assert.That(group.Culture, Is.EqualTo(new DefinitionId<Culture>("roman")));
        Assert.That(group.WealthBand, Is.EqualTo(WealthBand.Subsistence));
        Assert.That(group.NeedsProfile, Is.EqualTo(DietTier.Meager));
        Assert.That(group.EmploymentRatio, Is.EqualTo(Fixed64.One));
        Assert.That(group.HousingSatisfaction, Is.EqualTo(Fixed64.One));
        Assert.That(group.Contentment, Is.EqualTo(Fixed64.One));
        Assert.That(group.HealthExposure, Is.EqualTo(Fixed64.Zero));
    }

    [Test]
    public void CreateDefaultsAnEmptyLegalStatusDistributionForNonHouseholdEnslaved()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();

        var group = PopGroup.Create(settlementId, PopGroupType.NonHouseholdEnslaved, 10);

        Assert.That(group.LegalStatusDistribution, Is.EqualTo(LegalStatusDistribution.Empty));
    }

    [Test]
    public void CreateRejectsANonEmptyLegalStatusDistributionForNonHouseholdEnslaved()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();

        Assert.Throws<ArgumentException>(() => PopGroup.Create(
            settlementId, PopGroupType.NonHouseholdEnslaved, 10,
            legalStatusDistribution: new LegalStatusDistribution(1, 0, 0, 0)));
    }

    [Test]
    public void CreateRejectsALegalStatusDistributionTotalExceedingSize()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();

        Assert.Throws<ArgumentOutOfRangeException>(() => PopGroup.Create(
            settlementId, PopGroupType.Coloni, 5,
            legalStatusDistribution: new LegalStatusDistribution(3, 3, 0, 0)));
    }

    [Test]
    public void CreateRejectsANegativeLegalStatusDistributionCount()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();

        Assert.Throws<ArgumentOutOfRangeException>(() => PopGroup.Create(
            settlementId, PopGroupType.Coloni, 5,
            legalStatusDistribution: new LegalStatusDistribution(-1, 0, 0, 0)));
    }

    [Test]
    public void CreateRejectsANegativeEmploymentRatio()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();

        Assert.Throws<ArgumentOutOfRangeException>(() => PopGroup.Create(
            settlementId, PopGroupType.Coloni, 5, employmentRatio: Fixed64.FromInt(-1)));
    }

    [Test]
    public void CreateRejectsANegativeHousingSatisfaction()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();

        Assert.Throws<ArgumentOutOfRangeException>(() => PopGroup.Create(
            settlementId, PopGroupType.Coloni, 5, housingSatisfaction: Fixed64.FromInt(-1)));
    }

    [Test]
    public void CreateRejectsANegativeContentment()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();

        Assert.Throws<ArgumentOutOfRangeException>(() => PopGroup.Create(
            settlementId, PopGroupType.Coloni, 5, contentment: Fixed64.FromInt(-1)));
    }

    [Test]
    public void CreateRejectsANegativeHealthExposure()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();

        Assert.Throws<ArgumentOutOfRangeException>(() => PopGroup.Create(
            settlementId, PopGroupType.Coloni, 5, healthExposure: Fixed64.FromInt(-1)));
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
