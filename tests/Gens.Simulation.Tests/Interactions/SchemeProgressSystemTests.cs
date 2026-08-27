using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.Land;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;
using CharacterTestFixtures = Gens.Simulation.Tests.Characters.CharacterTestFixtures;

namespace Gens.Simulation.Tests.Interactions;

/// <summary>Phase 10 item 6 coverage for the Progress/Discovery/Counter-play/Resolution monthly tick.</summary>
public sealed class SchemeProgressSystemTests
{
    private static RandomStreamSet Streams(ulong seed)
    {
        var streams = new RandomStreamSet();
        streams.Add("interactions.schemeProgress", seed, 1);
        return streams;
    }

    private static Character WithIntrigue(RuntimeId<Character> id, int intrigue, DeathRecord? deathRecord = null) =>
        Character.Create(
            id: id,
            praenomen: "Marcus",
            nomen: "Aurelius",
            cognomen: null,
            sex: Sex.Male,
            birthDate: new GameDate(0),
            visualProfile: CharacterTestFixtures.MinimalVisualProfile,
            status: LegalStatus.RomanCitizen,
            socialClass: SocialClass.Plebeian,
            culture: new DefinitionId<Culture>("roman"),
            location: default(RuntimeId<Settlement>),
            household: null,
            attributes: new CoreAttributes(10, 10, 10, intrigue, 10),
            skills: new LaborSkills(10, 10, 10, 10, 10),
            condition: new Condition(80, 0, 50, 20, 50),
            source: CharacterSource.Familia,
            instantiatedAtMonth: 0,
            deathRecord: deathRecord);

    private static (WorldState State, RuntimeId<Scheme> SchemeId) SetUp(int initiatorIntrigue, int targetIntrigue)
    {
        var state = new WorldState(new GameDate(0));
        var initiatorId = state.CharacterIds.Issue();
        state.Characters.Add(initiatorId, WithIntrigue(initiatorId, initiatorIntrigue));
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(targetId, WithIntrigue(targetId, targetIntrigue));

        var schemeId = state.SchemeIds.Issue();
        state.Schemes.Add(schemeId, Scheme.Create(schemeId, initiatorId, targetId, SchemeType.Coercive, new GameDate(0)));
        return (state, schemeId);
    }

    private static Scheme RunUntilResolved(WorldState state, RuntimeId<Scheme> schemeId, RandomStreamSet streams, int maxMonths = 60)
    {
        var system = new SchemeProgressSystem();
        for (var month = 1; month <= maxMonths; month++)
        {
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));
            state.Schemes.TryGet(schemeId, out var current);
            if (current!.IsResolved)
                return current;
        }

        Assert.Fail("Scheme never resolved within the test's month budget.");
        throw new InvalidOperationException("unreachable");
    }

    [Test]
    public void DeclaresTheRelationshipsActorsPhaseAndReadWriteSet()
    {
        var system = new SchemeProgressSystem();

        Assert.Multiple(() =>
        {
            Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
            Assert.That(system.Reads, Is.EquivalentTo(new[] { "schemes", "characters", "actors" }));
            Assert.That(system.Writes, Is.EquivalentTo(new[] { "schemes", "eventIds", "rivalDossiers" }));
        });
    }

    [Test]
    public void ResolvingASchemeRefreshesTheTargetsRivalDossierWhenTheyHeadAnActor()
    {
        var (state, schemeId) = SetUp(initiatorIntrigue: 90, targetIntrigue: 5);
        state.Schemes.TryGet(schemeId, out var scheme);

        var netWorth = new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null);
        var military = new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest);
        var rivalActor = RivalHouseCreationService.CreateAncientSeed(
            state, "Cornelia", LivingWorldActorStandingTrend.Established, LivingWorldActorIdentity.None,
            0, netWorth, military, state.RegionIds.Issue(), state.SettlementIds.Issue());
        state.Actors.Remove(rivalActor.ActorId);
        state.Actors.Add(rivalActor.ActorId, rivalActor with { HeadCharacterId = scheme!.TargetCharacterId });

        RunUntilResolved(state, schemeId, Streams(1));

        Assert.That(state.RivalDossiers.TryGet(rivalActor.ActorId, out var dossier), Is.True);
        Assert.That(dossier!.Summary, Does.Contain("Scheme"));
    }

    [Test]
    public void ATickWithNoInProgressSchemesReturnsNoEventsAndDoesNotThrow()
    {
        var state = new WorldState(new GameDate(0));
        var events = new SchemeProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        Assert.That(events, Is.Empty);
    }

    [Test]
    public void ProgressAndDiscoveryRiskBothAdvanceOnAnOrdinaryMonth()
    {
        var (state, schemeId) = SetUp(initiatorIntrigue: 50, targetIntrigue: 50);
        new SchemeProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        state.Schemes.TryGet(schemeId, out var scheme);

        Assert.Multiple(() =>
        {
            Assert.That(scheme!.Progress, Is.GreaterThan(0));
            Assert.That(scheme.DiscoveryRisk, Is.GreaterThan(0));
            Assert.That(scheme.Status, Is.EqualTo(SchemeStatus.InProgress));
        });
    }

    [Test]
    public void ResolvesCleanlyWhenProgressCompletesWellBeforeDiscoveryRisk()
    {
        // High initiator Intrigue drives Progress fast; zero target Intrigue keeps DiscoveryRisk
        // growing only at its flat per-month base, staying far under the discovery threshold.
        var (state, schemeId) = SetUp(initiatorIntrigue: 100, targetIntrigue: 0);
        var resolved = RunUntilResolved(state, schemeId, Streams(1));

        Assert.That(resolved.Status, Is.EqualTo(SchemeStatus.Succeeded).Or.EqualTo(SchemeStatus.FailedQuietly));
    }

    [Test]
    public void ResolvesViaDiscoveryWhenRiskCrossesThresholdBeforeProgressCompletes()
    {
        // Zero initiator Intrigue keeps Progress crawling at its flat per-month base; maximal target
        // Intrigue drives DiscoveryRisk past the threshold long before Progress could ever complete.
        var (state, schemeId) = SetUp(initiatorIntrigue: 0, targetIntrigue: 100);
        var resolved = RunUntilResolved(state, schemeId, Streams(1));

        Assert.That(resolved.Status, Is.EqualTo(SchemeStatus.DiscoveredAndFoiled).Or.EqualTo(SchemeStatus.DiscoveredAndEscalated));
        Assert.That(resolved.Progress, Is.LessThan(Scheme.MaxValue));
    }

    [Test]
    public void ResolvesAsFailedQuietlyWhenTheInitiatorHasDied()
    {
        var state = new WorldState(new GameDate(0));
        var initiatorId = state.CharacterIds.Issue();
        state.Characters.Add(initiatorId, WithIntrigue(initiatorId, 50, new DeathRecord(new GameDate(0), DeathCause.OldAge, 70)));
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(targetId, WithIntrigue(targetId, 50));
        var schemeId = state.SchemeIds.Issue();
        state.Schemes.Add(schemeId, Scheme.Create(schemeId, initiatorId, targetId, SchemeType.Coercive, new GameDate(0)));

        var events = new SchemeProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        state.Schemes.TryGet(schemeId, out var scheme);
        Assert.Multiple(() =>
        {
            Assert.That(scheme!.Status, Is.EqualTo(SchemeStatus.FailedQuietly));
            Assert.That(events, Has.Count.EqualTo(1));
            Assert.That(events[0], Is.InstanceOf<SchemeResolvedEvent>());
        });
    }

    [Test]
    public void ResolvesAsFailedQuietlyWhenTheTargetHasDied()
    {
        var state = new WorldState(new GameDate(0));
        var initiatorId = state.CharacterIds.Issue();
        state.Characters.Add(initiatorId, WithIntrigue(initiatorId, 50));
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(targetId, WithIntrigue(targetId, 50, new DeathRecord(new GameDate(0), DeathCause.OldAge, 70)));
        var schemeId = state.SchemeIds.Issue();
        state.Schemes.Add(schemeId, Scheme.Create(schemeId, initiatorId, targetId, SchemeType.Coercive, new GameDate(0)));

        var events = new SchemeProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        state.Schemes.TryGet(schemeId, out var scheme);
        Assert.Multiple(() =>
        {
            Assert.That(scheme!.Status, Is.EqualTo(SchemeStatus.FailedQuietly));
            Assert.That(events, Has.Count.EqualTo(1));
            Assert.That(events[0], Is.InstanceOf<SchemeResolvedEvent>());
        });
    }

    [Test]
    public void LeavesAlreadyResolvedSchemesUntouched()
    {
        var (state, schemeId) = SetUp(initiatorIntrigue: 50, targetIntrigue: 50);
        state.Schemes.TryGet(schemeId, out var scheme);
        state.Schemes.Remove(schemeId);
        state.Schemes.Add(schemeId, scheme! with { Status = SchemeStatus.Succeeded });

        var events = new SchemeProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        Assert.That(events, Is.Empty);
    }
}
