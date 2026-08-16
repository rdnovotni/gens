using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Numerics;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class SocialMobilitySystemTests
{
    [Test]
    public void FavorableOperariiEmploymentPullsColoniTowardIt()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.Coloni),
            PopGroup.Create(settlementId, PopGroupType.Coloni, 1_000, employmentRatio: Fixed64.One));
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.Operarii),
            PopGroup.Create(settlementId, PopGroupType.Operarii, 500, employmentRatio: Fixed64.FromInt(2)));

        var events = new SocialMobilitySystem().Tick(state, Context(state));

        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Coloni), out var coloni);
        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Operarii), out var operarii);
        Assert.That(coloni.Size, Is.EqualTo(980));
        Assert.That(operarii.Size, Is.EqualTo(520));
        var line = ((SocialMobilityAppliedEvent)events.Single());
        Assert.That(line.Source, Is.EqualTo(PopGroupType.Coloni));
        Assert.That(line.Destination, Is.EqualTo(PopGroupType.Operarii));
        Assert.That(line.Count, Is.EqualTo(20));
    }

    [Test]
    public void EliteDiscretionaryNegotiatoresRiseIntoCurialesWhenThereIsRoom()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.Negotiatores),
            PopGroup.Create(settlementId, PopGroupType.Negotiatores, 1_000, wealthBand: WealthBand.EliteDiscretionary));
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.Curiales),
            PopGroup.Create(settlementId, PopGroupType.Curiales, 100, employmentRatio: Fixed64.FromInt(2)));

        new SocialMobilitySystem().Tick(state, Context(state));

        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Negotiatores), out var negotiatores);
        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Curiales), out var curiales);
        Assert.That(negotiatores.Size, Is.EqualTo(990));
        Assert.That(curiales.Size, Is.EqualTo(110));
    }

    [Test]
    public void VeteransDriftBackToColoniAbsentARecruitmentDrive()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.Veterans),
            PopGroup.Create(settlementId, PopGroupType.Veterans, 1_000));
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.Coloni),
            PopGroup.Create(settlementId, PopGroupType.Coloni, 500));

        new SocialMobilitySystem().Tick(state, Context(state));

        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Veterans), out var veterans);
        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Coloni), out var coloni);
        Assert.That(veterans.Size, Is.EqualTo(985));
        Assert.That(coloni.Size, Is.EqualTo(515));
    }

    [Test]
    public void ManumissionEntersDestinationGroupsAsFreedmen()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.NonHouseholdEnslaved),
            PopGroup.Create(settlementId, PopGroupType.NonHouseholdEnslaved, 1_000));
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.Operarii),
            PopGroup.Create(settlementId, PopGroupType.Operarii, 200));
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.Opifices),
            PopGroup.Create(settlementId, PopGroupType.Opifices, 200));

        new SocialMobilitySystem().Tick(state, Context(state));

        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.NonHouseholdEnslaved), out var enslaved);
        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Operarii), out var operarii);
        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Opifices), out var opifices);
        // floor(1000 * 0.005) = 5 total; split 3/2 favoring Operarii on the odd remainder.
        Assert.That(enslaved.Size, Is.EqualTo(995));
        Assert.That(operarii.Size, Is.EqualTo(203));
        Assert.That(opifices.Size, Is.EqualTo(202));
        Assert.That(operarii.LegalStatusDistribution.Freedman, Is.EqualTo(3));
        Assert.That(opifices.LegalStatusDistribution.Freedman, Is.EqualTo(2));
        Assert.That(enslaved.LegalStatusDistribution, Is.EqualTo(LegalStatusDistribution.Empty));
    }

    [Test]
    public void TotalSettlementPopulationIsConservedAcrossEveryPathway()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Coloni), PopGroup.Create(settlementId, PopGroupType.Coloni, 1_000, employmentRatio: Fixed64.One));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Operarii), PopGroup.Create(settlementId, PopGroupType.Operarii, 500, employmentRatio: Fixed64.FromInt(2)));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Opifices), PopGroup.Create(settlementId, PopGroupType.Opifices, 300));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Negotiatores), PopGroup.Create(settlementId, PopGroupType.Negotiatores, 200, wealthBand: WealthBand.EliteDiscretionary));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Curiales), PopGroup.Create(settlementId, PopGroupType.Curiales, 100, employmentRatio: Fixed64.FromInt(2)));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Veterans), PopGroup.Create(settlementId, PopGroupType.Veterans, 400));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.NonHouseholdEnslaved), PopGroup.Create(settlementId, PopGroupType.NonHouseholdEnslaved, 600));

        var before = state.PopGroups.InAscendingOrder().Sum(entry => entry.Value.Size);

        new SocialMobilitySystem().Tick(state, Context(state));

        var after = state.PopGroups.InAscendingOrder().Sum(entry => entry.Value.Size);
        Assert.That(after, Is.EqualTo(before));
    }

    [Test]
    public void MissingLandingGroupIsLeftUncreated()
    {
        var state = NewWorldState();
        var settlementId = SetupSettlement(state);
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Veterans), PopGroup.Create(settlementId, PopGroupType.Veterans, 1_000));
        // No Coloni PopGroup registered at all.

        Assert.That(() => new SocialMobilitySystem().Tick(state, Context(state)), Throws.Nothing);
        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Veterans), out var veterans);
        Assert.That(veterans.Size, Is.EqualTo(1_000));
        Assert.That(state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Coloni), out _), Is.False);
    }

    private static WorldState NewWorldState() => new(new GameDate(0));

    private static MonthlyTickContext Context(WorldState state) => new(state.Date, new RandomStreamSet());

    private static RuntimeId<Settlement> SetupSettlement(WorldState state)
    {
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        return settlementId;
    }
}
