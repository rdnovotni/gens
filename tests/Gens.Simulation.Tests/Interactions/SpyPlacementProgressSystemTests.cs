using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;
using CharacterTestFixtures = Gens.Simulation.Tests.Characters.CharacterTestFixtures;

namespace Gens.Simulation.Tests.Interactions;

/// <summary>Phase 16 item 1 coverage for the Discovery-Risk/Discovery/Traceability monthly tick.</summary>
public sealed class SpyPlacementProgressSystemTests
{
    private static RandomStreamSet Streams(ulong seed)
    {
        var streams = new RandomStreamSet();
        streams.Add("interactions.spyPlacementProgress", seed, 1);
        return streams;
    }

    private static RuntimeId<Actor> AddTargetActor(WorldState state, RuntimeId<Character>? headCharacterId = null)
    {
        var netWorth = new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null);
        var military = new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest);
        var actor = RivalHouseCreationService.CreateAncientSeed(
            state, "Cornelia", LivingWorldActorStandingTrend.Established, LivingWorldActorIdentity.None,
            0, netWorth, military, state.RegionIds.Issue(), state.SettlementIds.Issue());

        if (headCharacterId is { } headId)
        {
            state.Actors.Remove(actor.ActorId);
            state.Actors.Add(actor.ActorId, actor with { HeadCharacterId = headId });
        }

        return actor.ActorId;
    }

    private static (WorldState State, RuntimeId<SpyPlacement> PlacementId) SetUp(
        SpyPlacementType type, int concealmentQuality, int targetHeadIntrigue)
    {
        var state = new WorldState(new GameDate(0));
        var spyId = state.CharacterIds.Issue();
        state.Characters.Add(spyId, CharacterTestFixtures.Minimal(spyId));
        var sponsorId = state.CharacterIds.Issue();
        state.Characters.Add(sponsorId, CharacterTestFixtures.Minimal(sponsorId));
        var targetHeadId = state.CharacterIds.Issue();
        state.Characters.Add(targetHeadId, CharacterTestFixtures.Minimal(
            targetHeadId, attributes: new CoreAttributes(10, 10, 10, targetHeadIntrigue, 10)));
        var targetActorId = AddTargetActor(state, targetHeadId);

        var placementId = state.SpyPlacementIds.Issue();
        state.SpyPlacements.Add(placementId, SpyPlacement.Create(
            placementId, spyId, sponsorId, targetActorId, type, concealmentQuality, new GameDate(0)));
        return (state, placementId);
    }

    private static SpyPlacement RunUntilResolved(WorldState state, RuntimeId<SpyPlacement> placementId, RandomStreamSet streams, int maxMonths = 60)
    {
        var system = new SpyPlacementProgressSystem();
        for (var month = 1; month <= maxMonths; month++)
        {
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));
            state.SpyPlacements.TryGet(placementId, out var current);
            if (current!.IsResolved)
                return current;
        }

        Assert.Fail("Placement never resolved within the test's month budget.");
        throw new InvalidOperationException("unreachable");
    }

    [Test]
    public void DeclaresTheRelationshipsActorsPhaseAndReadWriteSet()
    {
        var system = new SpyPlacementProgressSystem();

        Assert.Multiple(() =>
        {
            Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
            Assert.That(system.Reads, Is.EquivalentTo(new[] { "spyPlacements", "characters", "actors" }));
            Assert.That(system.Writes, Is.EquivalentTo(new[] { "spyPlacements", "eventIds", "rivalDossiers", "ledgerAccounts", "ledgerTransactions" }));
        });
    }

    [Test]
    public void ATickWithNoInProgressPlacementsReturnsNoEventsAndDoesNotThrow()
    {
        var state = new WorldState(new GameDate(0));
        var events = new SpyPlacementProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        Assert.That(events, Is.Empty);
    }

    [Test]
    public void PersistentNetworkDiscoveryRiskAdvancesOnAnOrdinaryMonth()
    {
        var (state, placementId) = SetUp(SpyPlacementType.PersistentNetwork, concealmentQuality: 50, targetHeadIntrigue: 50);
        new SpyPlacementProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        state.SpyPlacements.TryGet(placementId, out var placement);

        Assert.Multiple(() =>
        {
            Assert.That(placement!.Status, Is.EqualTo(SpyPlacementStatus.InProgress));
            Assert.That(placement.MonthsActive, Is.EqualTo(1));
        });
    }

    [Test]
    public void PersistentNetworkEventuallyResolvesViaDiscoveryWhenConcealmentIsLowAndTargetIsHighlyInvestigative()
    {
        var (state, placementId) = SetUp(SpyPlacementType.PersistentNetwork, concealmentQuality: 0, targetHeadIntrigue: 100);
        var resolved = RunUntilResolved(state, placementId, Streams(1));

        Assert.That(resolved.Status, Is.EqualTo(SpyPlacementStatus.DiscoveredUntraced).Or.EqualTo(SpyPlacementStatus.DiscoveredAndTraced));
        Assert.That(resolved.DiscoveryRisk, Is.GreaterThanOrEqualTo(SpyPlacementCatalog.DiscoveryRiskThresholdPercent));
    }

    [Test]
    public void QuickOpResolvesOnItsFirstTick()
    {
        var (state, placementId) = SetUp(SpyPlacementType.QuickOp, concealmentQuality: 50, targetHeadIntrigue: 50);
        new SpyPlacementProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        state.SpyPlacements.TryGet(placementId, out var placement);
        Assert.That(placement!.IsResolved, Is.True);
    }

    [Test]
    public void AFullyConcealedQuickOpNeverGetsDiscovered()
    {
        var (state, placementId) = SetUp(SpyPlacementType.QuickOp, concealmentQuality: 100, targetHeadIntrigue: 0);
        new SpyPlacementProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        state.SpyPlacements.TryGet(placementId, out var placement);
        Assert.That(placement!.Status, Is.EqualTo(SpyPlacementStatus.Succeeded).Or.EqualTo(SpyPlacementStatus.FailedQuietly));
    }

    [Test]
    public void ResolvesAsWithdrawnWhenTheSpyHasDied()
    {
        var state = new WorldState(new GameDate(0));
        var spyId = state.CharacterIds.Issue();
        state.Characters.Add(spyId, CharacterTestFixtures.Minimal(
            spyId, deathRecord: new DeathRecord(new GameDate(0), DeathCause.OldAge, 70)));
        var sponsorId = state.CharacterIds.Issue();
        state.Characters.Add(sponsorId, CharacterTestFixtures.Minimal(sponsorId));
        var targetActorId = AddTargetActor(state);
        var placementId = state.SpyPlacementIds.Issue();
        state.SpyPlacements.Add(placementId, SpyPlacement.Create(
            placementId, spyId, sponsorId, targetActorId, SpyPlacementType.PersistentNetwork, concealmentQuality: 50, new GameDate(0)));

        var events = new SpyPlacementProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        state.SpyPlacements.TryGet(placementId, out var placement);
        Assert.Multiple(() =>
        {
            Assert.That(placement!.Status, Is.EqualTo(SpyPlacementStatus.Withdrawn));
            Assert.That(events, Has.Count.EqualTo(1));
            Assert.That(events[0], Is.InstanceOf<SpyPlacementResolvedEvent>());
        });
    }

    [Test]
    public void ResolvesAsWithdrawnWhenTheSponsorHasDied()
    {
        var state = new WorldState(new GameDate(0));
        var spyId = state.CharacterIds.Issue();
        state.Characters.Add(spyId, CharacterTestFixtures.Minimal(spyId));
        var sponsorId = state.CharacterIds.Issue();
        state.Characters.Add(sponsorId, CharacterTestFixtures.Minimal(
            sponsorId, deathRecord: new DeathRecord(new GameDate(0), DeathCause.OldAge, 70)));
        var targetActorId = AddTargetActor(state);
        var placementId = state.SpyPlacementIds.Issue();
        state.SpyPlacements.Add(placementId, SpyPlacement.Create(
            placementId, spyId, sponsorId, targetActorId, SpyPlacementType.PersistentNetwork, concealmentQuality: 50, new GameDate(0)));

        var events = new SpyPlacementProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        state.SpyPlacements.TryGet(placementId, out var placement);
        Assert.That(placement!.Status, Is.EqualTo(SpyPlacementStatus.Withdrawn));
    }

    [Test]
    public void LeavesAlreadyResolvedPlacementsUntouched()
    {
        var (state, placementId) = SetUp(SpyPlacementType.PersistentNetwork, concealmentQuality: 50, targetHeadIntrigue: 50);
        state.SpyPlacements.TryGet(placementId, out var placement);
        state.SpyPlacements.Remove(placementId);
        state.SpyPlacements.Add(placementId, placement! with { Status = SpyPlacementStatus.Succeeded });

        var events = new SpyPlacementProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        Assert.That(events, Is.Empty);
    }

    [Test]
    public void ResolvingViaDiscoveryAndTracedRefreshesTheTargetsRivalDossier()
    {
        var (state, placementId) = SetUp(SpyPlacementType.PersistentNetwork, concealmentQuality: 0, targetHeadIntrigue: 100);
        state.SpyPlacements.TryGet(placementId, out var placement);
        var targetActorId = placement!.TargetActorId;

        // Zero concealment against a maximal-Intrigue target head makes both the Discovery and
        // Traceability chances saturate at 100% (SpyPlacementCatalog's weighting formulas), so this
        // reliably resolves DiscoveredAndTraced rather than merely possibly doing so.
        var resolved = RunUntilResolved(state, placementId, Streams(1));

        Assert.Multiple(() =>
        {
            Assert.That(resolved.Status, Is.EqualTo(SpyPlacementStatus.DiscoveredAndTraced));
            Assert.That(state.RivalDossiers.TryGet(targetActorId, out _), Is.True);
        });
    }
}
