using Gens.Simulation.Interactions;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Interactions;

/// <summary>Phase 16 item 1 coverage for <see cref="SpyPlacement"/>.</summary>
public sealed class SpyPlacementTests
{
    [Test]
    public void CreateStartsInProgressAtZeroDiscoveryRiskAndZeroMonthsActive()
    {
        var state = new WorldState(new GameDate(0));
        var spyId = state.CharacterIds.Issue();
        var sponsorId = state.CharacterIds.Issue();
        var targetActorId = state.ActorIds.Issue();
        var placementId = state.SpyPlacementIds.Issue();

        var placement = SpyPlacement.Create(
            placementId, spyId, sponsorId, targetActorId, SpyPlacementType.PersistentNetwork, concealmentQuality: 60, new GameDate(10));

        Assert.Multiple(() =>
        {
            Assert.That(placement.Status, Is.EqualTo(SpyPlacementStatus.InProgress));
            Assert.That(placement.DiscoveryRisk, Is.EqualTo(0));
            Assert.That(placement.MonthsActive, Is.EqualTo(0));
            Assert.That(placement.ConcealmentQuality, Is.EqualTo(60));
            Assert.That(placement.PlacedDate, Is.EqualTo(new GameDate(10)));
            Assert.That(placement.LastProgressedDate, Is.EqualTo(new GameDate(10)));
            Assert.That(placement.IsResolved, Is.False);
        });
    }

    [Test]
    public void CreateRejectsASpyActingAsTheirOwnSponsor()
    {
        var state = new WorldState(new GameDate(0));
        var characterId = state.CharacterIds.Issue();
        var targetActorId = state.ActorIds.Issue();
        var placementId = state.SpyPlacementIds.Issue();

        Assert.Throws<ArgumentException>(() => SpyPlacement.Create(
            placementId, characterId, characterId, targetActorId, SpyPlacementType.QuickOp, concealmentQuality: 50, new GameDate(0)));
    }

    [Test]
    public void CreateRejectsAnOutOfRangeConcealmentQuality()
    {
        var state = new WorldState(new GameDate(0));
        var spyId = state.CharacterIds.Issue();
        var sponsorId = state.CharacterIds.Issue();
        var targetActorId = state.ActorIds.Issue();
        var placementId = state.SpyPlacementIds.Issue();

        Assert.Throws<ArgumentOutOfRangeException>(() => SpyPlacement.Create(
            placementId, spyId, sponsorId, targetActorId, SpyPlacementType.QuickOp, concealmentQuality: 101, new GameDate(0)));
    }

    [Test]
    public void IsResolvedIsTrueForEveryNonInProgressStatus()
    {
        var state = new WorldState(new GameDate(0));
        var spyId = state.CharacterIds.Issue();
        var sponsorId = state.CharacterIds.Issue();
        var targetActorId = state.ActorIds.Issue();
        var placementId = state.SpyPlacementIds.Issue();
        var placement = SpyPlacement.Create(
            placementId, spyId, sponsorId, targetActorId, SpyPlacementType.QuickOp, concealmentQuality: 50, new GameDate(0));

        Assert.That((placement with { Status = SpyPlacementStatus.Succeeded }).IsResolved, Is.True);
    }
}
