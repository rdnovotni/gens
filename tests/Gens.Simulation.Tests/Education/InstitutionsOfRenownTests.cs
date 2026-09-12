using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Education;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Tests.Travel;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Education;

/// <summary>Phase 17 item 2 coverage, slice 4: Institutions of Renown (§4, §12).</summary>
public sealed class InstitutionsOfRenownTests
{
    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Character> CharacterId) HouseholdWithTraveler()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, nomen: "Cornelius", household: householdId));
        return (state, householdId, characterId);
    }

    private static void Fund(WorldState state, RuntimeId<Household> householdId, Money amount)
    {
        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), amount),
                new LedgerPosting(new LedgerAccountKey(LedgerAccountKind.System, "test:seed"), -amount),
            });
    }

    private static CommandPipeline<WorldState, BeginStudyAbroadCommand> Pipeline() =>
        BeginStudyAbroadCommands.BuildPipeline(TravelTestFixtures.BuildRegionCatalog(), TravelTestFixtures.BuildDistanceTierCatalog());

    // ---- TravelRoute integration ---------------------------------------------------------------

    [Test]
    public void TravelRouteResolvesAnInstitutionDestinationOffItsOwnContentDefinitionNotTheRegionCatalog()
    {
        var origin = TravelLocation.Home(default);
        var destination = TravelLocation.InstitutionOfRenown(KnownInstitutionsOfRenown.Massilia);

        var route = TravelRoute.Resolve(
            origin, destination, homeRegionId: default,
            TravelTestFixtures.BuildRegionCatalog(includeCapital: false), TravelTestFixtures.BuildDistanceTierCatalog());

        var institution = KnownInstitutionsOfRenown.Catalog.Get(KnownInstitutionsOfRenown.Massilia);
        Assert.Multiple(() =>
        {
            Assert.That(route.DistanceTier, Is.EqualTo(institution.DistanceTier));
            Assert.That(route.RiskExposure, Is.EqualTo(institution.BaseRiskLevel));
            Assert.That(route.DistanceTier, Is.EqualTo(DistanceTier.Moderate));
            Assert.That(route.RiskExposure, Is.EqualTo(RouteRiskLevel.Guarded));
        });
    }

    // ---- BeginStudyAbroadCommand -----------------------------------------------------------------

    [Test]
    public void BeginStudyAbroadCommandCreatesTheTripAndTheJourneySideRecord()
    {
        var (state, householdId, characterId) = HouseholdWithTraveler();

        var result = Pipeline().Execute(
            state,
            new BeginStudyAbroadCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, characterId, householdId, KnownInstitutionsOfRenown.Athens));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(state.TravelTrips.Count, Is.EqualTo(1));
            var tripId = state.TravelTrips.InAscendingOrder().First().Key;
            Assert.That(StudyAbroadJourneyResolver.TryGet(state, tripId, out var journey), Is.True);
            Assert.That(journey.InstitutionId, Is.EqualTo(KnownInstitutionsOfRenown.Athens));
            Assert.That(journey.HouseholdId, Is.EqualTo(householdId));
        });
    }

    [Test]
    public void BeginStudyAbroadCommandRejectsADeceasedTravelerOrAnUnknownInstitution()
    {
        var (state, householdId, characterId) = HouseholdWithTraveler();
        state.Characters.TryGet(characterId, out var character);
        state.Characters.Remove(characterId);
        state.Characters.Add(characterId, character! with { DeathRecord = new DeathRecord(new GameDate(0), DeathCause.OldAge, 40) });

        Assert.That(
            Pipeline().Execute(
                state,
                new BeginStudyAbroadCommand(
                    state.CommandIds.Issue(), "player", new GameDate(0), null, characterId, householdId, KnownInstitutionsOfRenown.Athens))
                .Error,
            Is.EqualTo(BeginStudyAbroadCommands.TravelerDeceased));

        var (state2, householdId2, characterId2) = HouseholdWithTraveler();
        Assert.That(
            Pipeline().Execute(
                state2,
                new BeginStudyAbroadCommand(
                    state2.CommandIds.Issue(), "player", new GameDate(0), null, characterId2, householdId2,
                    new DefinitionId<InstitutionOfRenown>("bogus")))
                .Error,
            Is.EqualTo(BeginStudyAbroadCommands.UnknownInstitution));
    }

    // ---- StudyAbroadProgressSystem -----------------------------------------------------------

    private static RuntimeId<TravelTrip> BeginAndArrive(
        WorldState state, RuntimeId<Household> householdId, RuntimeId<Character> characterId, DefinitionId<InstitutionOfRenown> institutionId)
    {
        Pipeline().Execute(
            state, new BeginStudyAbroadCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, characterId, householdId, institutionId));
        var tripId = state.TravelTrips.InAscendingOrder().First().Key;
        state.TravelTrips.TryGet(tripId, out var trip);
        state.TravelTrips.Remove(tripId);
        state.TravelTrips.Add(tripId, trip! with { Status = TravelTripStatus.Arrived, MonthsElapsed = 0 });
        return tripId;
    }

    [Test]
    public void StudyAbroadProgressSystemDrawsTheMonthlyCostWhileArrived()
    {
        var (state, householdId, characterId) = HouseholdWithTraveler();
        Fund(state, householdId, Money.FromDenarii(1000));
        BeginAndArrive(state, householdId, characterId, KnownInstitutionsOfRenown.Massilia);
        var institution = KnownInstitutionsOfRenown.Catalog.Get(KnownInstitutionsOfRenown.Massilia);

        new StudyAbroadProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        var account = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var acc) ? acc!.Balance : Money.Zero;
        Assert.That(account, Is.EqualTo(Money.FromDenarii(1000) - institution.CostPerMonth));
    }

    [Test]
    public void StudyAbroadProgressSystemGrantsCredentialAcceleratesDriftAndTransitionsToReturning()
    {
        var (state, householdId, characterId) = HouseholdWithTraveler();
        Fund(state, householdId, Money.FromDenarii(2000));
        var institution = KnownInstitutionsOfRenown.Catalog.Get(KnownInstitutionsOfRenown.Massilia);
        var tripId = BeginAndArrive(state, householdId, characterId, KnownInstitutionsOfRenown.Massilia);

        var system = new StudyAbroadProgressSystem();
        for (var i = 1; i <= institution.JourneyDurationMonths; i++)
            system.Tick(state, new MonthlyTickContext(new GameDate(i), new RandomStreamSet()));

        state.TravelTrips.TryGet(tripId, out var trip);
        Assert.Multiple(() =>
        {
            Assert.That(trip!.Status, Is.EqualTo(TravelTripStatus.Returning));
            Assert.That(trip.EncounterCompleted, Is.True);
            Assert.That(StudyAbroadJourneyResolver.TryGet(state, tripId, out _), Is.False);
            Assert.That(CharacterInstitutionCredentialResolver.HasCredentialFrom(state, characterId, KnownInstitutionsOfRenown.Massilia), Is.True);
            Assert.That(CulturalDriftResolver.TryGet(state, characterId, out var drift), Is.True);
            Assert.That(drift.TargetCultureId, Is.EqualTo(institution.PrimeCulturalAssociation));
            Assert.That(
                drift.ProgressMonths,
                Is.EqualTo(EducationCulturalDriftCatalog.StudyAbroadAccelerationMultiplier * EducationCulturalDriftCatalog.SlowDriftMonthsPerMonth));
        });
    }

    [Test]
    public void RhodesCredentialSatisfiesTheMagistracyContestGate()
    {
        var (state, householdId, characterId) = HouseholdWithTraveler();
        Fund(state, householdId, Money.FromDenarii(2000));
        var institution = KnownInstitutionsOfRenown.Catalog.Get(KnownInstitutionsOfRenown.Rhodes);
        BeginAndArrive(state, householdId, characterId, KnownInstitutionsOfRenown.Rhodes);

        var system = new StudyAbroadProgressSystem();
        for (var i = 1; i <= institution.JourneyDurationMonths; i++)
            system.Tick(state, new MonthlyTickContext(new GameDate(i), new RandomStreamSet()));

        Assert.That(EducationGateResolver.CanContestMagistracyAboveLowestRung(state, characterId), Is.True);
    }

    // ---- RenownAttractsRenownSystem ---------------------------------------------------------------

    [Test]
    public void RenownAttractsRenownSystemCrossesTheThresholdOnceBothConditionsHold()
    {
        var (state, householdId, characterId) = HouseholdWithTraveler();
        CulturalPrestigeResolver.Apply(state, householdId, RenownAttractsRenownCatalog.RecognitionPrestigeThreshold);

        var beforePatronage = new RenownAttractsRenownSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));
        Assert.That(beforePatronage, Is.Empty);

        SetLiteraryPatronCommands.Pipeline.Execute(
            state, new SetLiteraryPatronCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, characterId));

        var events = new RenownAttractsRenownSystem().Tick(state, new MonthlyTickContext(new GameDate(2), new RandomStreamSet()));

        Assert.Multiple(() =>
        {
            Assert.That(events.OfType<RenownAttractsRenownThresholdCrossedEvent>().Count(), Is.EqualTo(1));
            Assert.That(state.RenownAttractsRenownStates.TryGet(householdId, out var renown), Is.True);
            Assert.That(renown!.IncomingForeignStudentOpportunityActive, Is.True);
        });

        // Never re-fires once already active.
        var again = new RenownAttractsRenownSystem().Tick(state, new MonthlyTickContext(new GameDate(3), new RandomStreamSet()));
        Assert.That(again, Is.Empty);
    }

    // ---- Save round trip & determinism --------------------------------------------------------

    [Test]
    public void InstitutionsStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, householdId, characterId) = HouseholdWithTraveler();
        Fund(state, householdId, Money.FromDenarii(2000));
        var institution = KnownInstitutionsOfRenown.Catalog.Get(KnownInstitutionsOfRenown.Massilia);
        var tripId = BeginAndArrive(state, householdId, characterId, KnownInstitutionsOfRenown.Massilia);

        var system = new StudyAbroadProgressSystem();
        for (var i = 1; i <= institution.JourneyDurationMonths; i++)
            system.Tick(state, new MonthlyTickContext(new GameDate(i), new RandomStreamSet()));

        CulturalPrestigeResolver.Apply(state, householdId, RenownAttractsRenownCatalog.RecognitionPrestigeThreshold);
        SetLiteraryPatronCommands.Pipeline.Execute(
            state, new SetLiteraryPatronCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, characterId));
        new RenownAttractsRenownSystem().Tick(state, new MonthlyTickContext(new GameDate(30), new RandomStreamSet()));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.CharacterInstitutionCredentials.Count, Is.EqualTo(1));
            Assert.That(restored.CulturalDriftStates.Count, Is.EqualTo(1));
            Assert.That(restored.RenownAttractsRenownStates.Count, Is.EqualTo(1));
            Assert.That(restored.StudyAbroadJourneys.Count, Is.EqualTo(0));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }
}
