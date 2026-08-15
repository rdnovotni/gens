using Gens.Simulation.Characters;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class LaborFlightSystemTests
{
    private const string FlightStreamName = "test-flight";
    private const string OutcomeStreamName = "test-outcome";

    private static readonly RegimenSettings Harsh = new(
        DietTier.Meager, AccommodationTier.Bare, FreedomsTier.Confined, DisciplineTier.Harsh);

    [Test]
    public void AHighRiskEnslavedCharacterFleesOnASuccessfulOpportunityRoll()
    {
        var state = new WorldState(new GameDate(50));
        var householdId = state.HouseholdIds.Issue();
        var location = state.SettlementIds.Issue();
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, status: LegalStatus.Enslaved, household: householdId, location: location,
            condition: new Condition(80, 20, 0, 20, 50), regimen: Harsh,
            duty: new DutyAssignment(householdId, DutySlot.Cook, new GameDate(40))));
        var system = new LaborFlightSystem(FlightStreamName, OutcomeStreamName);
        var streams = StreamsWithFlightDraw(v => v < 100);

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(50), streams));

        Assert.That(events.OfType<CharacterFledEvent>().Single().CharacterId, Is.EqualTo(id));
        state.Characters.TryGet(id, out var fled);
        Assert.That(fled.Flight, Is.Not.Null);
        Assert.That(fled.Flight!.Value.FormerHousehold, Is.EqualTo(householdId));
        Assert.That(fled.Duty, Is.Null);
        Assert.That(fled.Household, Is.Null);
    }

    [Test]
    public void ALowRiskCharacterDoesNotFleeOnAFailedOpportunityRoll()
    {
        var state = new WorldState(new GameDate(50));
        var householdId = state.HouseholdIds.Issue();
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, status: LegalStatus.Enslaved, household: householdId, condition: new Condition(80, 20, 100, 20, 50),
            duty: new DutyAssignment(householdId, DutySlot.Cook, new GameDate(40))));
        var system = new LaborFlightSystem(FlightStreamName, OutcomeStreamName);
        var streams = StreamsWithFlightDraw(v => v > 999_000);

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(50), streams));

        Assert.That(events.OfType<CharacterFledEvent>(), Is.Empty);
        state.Characters.TryGet(id, out var unchanged);
        Assert.That(unchanged.Flight, Is.Null);
    }

    [Test]
    public void AFreedmanNeverFleesRegardlessOfTheOpportunityRoll()
    {
        var state = new WorldState(new GameDate(50));
        var householdId = state.HouseholdIds.Issue();
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, status: LegalStatus.Freedman, household: householdId, condition: new Condition(80, 20, 0, 20, 50),
            duty: new DutyAssignment(householdId, DutySlot.Cook, new GameDate(40))));
        var system = new LaborFlightSystem(FlightStreamName, OutcomeStreamName);
        var streams = StreamsWithFlightDraw(v => v < 100);

        system.Tick(state, new MonthlyTickContext(new GameDate(50), streams));

        state.Characters.TryGet(id, out var unchanged);
        Assert.That(unchanged.Flight, Is.Null);
    }

    [Test]
    public void APursuitCountdownDecrementsWithoutResolvingBeforeItReachesZero()
    {
        var state = new WorldState(new GameDate(50));
        var householdId = state.HouseholdIds.Issue();
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, status: LegalStatus.Enslaved,
            flight: new FledRecord(new GameDate(45), householdId, null),
            pursuit: new PursuitRecord(2, null)));
        var system = new LaborFlightSystem(FlightStreamName, OutcomeStreamName);
        var streams = TrivialStreams();

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(50), streams));

        Assert.That(events.OfType<PursuitResolvedEvent>(), Is.Empty);
        state.Characters.TryGet(id, out var updated);
        Assert.That(updated.Pursuit!.Value.MonthsRemaining, Is.EqualTo(1));
    }

    [Test]
    public void PursuitResolvesWithRecaptureOnALowOutcomeRoll()
    {
        var state = new WorldState(new GameDate(50));
        var householdId = state.HouseholdIds.Issue();
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, status: LegalStatus.Enslaved, condition: new Condition(80, 20, 50, 20, 50),
            flight: new FledRecord(new GameDate(45), householdId, null),
            pursuit: new PursuitRecord(1, null)));
        var system = new LaborFlightSystem(FlightStreamName, OutcomeStreamName);
        var streams = StreamsWithOutcomeDraw(v => v < 100);

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(50), streams));

        Assert.That(events.OfType<PursuitResolvedEvent>().Single().Outcome, Is.EqualTo(PursuitOutcome.Recaptured));
        state.Characters.TryGet(id, out var recaptured);
        Assert.That(recaptured.Household, Is.EqualTo(householdId));
        Assert.That(recaptured.Flight, Is.Null);
        Assert.That(recaptured.Pursuit, Is.Null);
        Assert.That(recaptured.Condition.Loyalty, Is.LessThan(50));
    }

    [Test]
    public void PursuitResolvesWithPermanentLossOnAMidOutcomeRoll()
    {
        var state = new WorldState(new GameDate(50));
        var householdId = state.HouseholdIds.Issue();
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, status: LegalStatus.Enslaved,
            flight: new FledRecord(new GameDate(45), householdId, null),
            pursuit: new PursuitRecord(1, null)));
        var system = new LaborFlightSystem(FlightStreamName, OutcomeStreamName);
        var streams = StreamsWithOutcomeDraw(v => v is >= 500_000 and < 600_000);

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(50), streams));

        Assert.That(events.OfType<PursuitResolvedEvent>().Single().Outcome, Is.EqualTo(PursuitOutcome.PermanentlyLost));
        state.Characters.TryGet(id, out var lost);
        Assert.That(lost.Pursuit, Is.Null);
        Assert.That(lost.Household, Is.Null);
        Assert.That(lost.IsAlive, Is.True);
    }

    [Test]
    public void PursuitResolvesWithCapturedWithHarmOnAHighOutcomeRoll()
    {
        var state = new WorldState(new GameDate(50));
        var householdId = state.HouseholdIds.Issue();
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, status: LegalStatus.Enslaved,
            flight: new FledRecord(new GameDate(45), householdId, null),
            pursuit: new PursuitRecord(1, null)));
        var system = new LaborFlightSystem(FlightStreamName, OutcomeStreamName);
        var streams = StreamsWithOutcomeDraw(v => v > 999_000);

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(50), streams));

        Assert.That(events.OfType<PursuitResolvedEvent>().Single().Outcome, Is.EqualTo(PursuitOutcome.CapturedWithHarm));
        state.Characters.TryGet(id, out var harmed);
        Assert.That(harmed.IsAlive, Is.False);
        Assert.That(harmed.DeathRecord!.Value.Cause, Is.EqualTo(DeathCause.Violence));
    }

    [Test]
    public void APendingTestamentoGrantResolvesWhenTheGrantorDies()
    {
        var state = new WorldState(new GameDate(50));
        var grantorId = state.CharacterIds.Issue();
        var id = state.CharacterIds.Issue();
        state.Characters.Add(grantorId, CharacterTestFixtures.Minimal(
            grantorId, nomen: "Julius", deathRecord: new DeathRecord(new GameDate(48), DeathCause.Disease, 60)));
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, status: LegalStatus.Enslaved,
            manumissionPlan: new ManumissionPlan(grantorId, ManumissionType.Testamento)));
        var system = new LaborFlightSystem(FlightStreamName, OutcomeStreamName);
        var streams = TrivialStreams();

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(50), streams));

        Assert.That(events.OfType<TestamentoManumissionResolvedEvent>().Single().CharacterId, Is.EqualTo(id));
        state.Characters.TryGet(id, out var freed);
        Assert.That(freed.LegalStatus, Is.EqualTo(LegalStatus.Freedman));
        Assert.That(freed.Nomen, Is.EqualTo("Julius"));
        Assert.That(freed.ManumissionPlan, Is.Null);
    }

    private static RandomStreamSet TrivialStreams()
    {
        var streams = new RandomStreamSet();
        streams.Add(FlightStreamName, 0, 1);
        streams.Add(OutcomeStreamName, 0, 1);
        return streams;
    }

    private static RandomStreamSet StreamsWithFlightDraw(Predicate<uint> matchesFirstDraw)
    {
        for (ulong seed = 0; seed < 100_000; seed++)
        {
            var probe = new RandomStreamSet();
            probe.Add(FlightStreamName, seed, 1);
            if (matchesFirstDraw(probe.NextUInt(FlightStreamName, FlightRiskCalculator.RollPrecision)))
            {
                var streams = new RandomStreamSet();
                streams.Add(FlightStreamName, seed, 1);
                streams.Add(OutcomeStreamName, 0, 1);
                return streams;
            }
        }

        throw new InvalidOperationException("No seed found matching the requested first draw.");
    }

    private static RandomStreamSet StreamsWithOutcomeDraw(Predicate<uint> matchesFirstDraw)
    {
        for (ulong seed = 0; seed < 100_000; seed++)
        {
            var probe = new RandomStreamSet();
            probe.Add(OutcomeStreamName, seed, 1);
            if (matchesFirstDraw(probe.NextUInt(OutcomeStreamName, FlightRiskCalculator.RollPrecision)))
            {
                var streams = new RandomStreamSet();
                streams.Add(FlightStreamName, 0, 1);
                streams.Add(OutcomeStreamName, seed, 1);
                return streams;
            }
        }

        throw new InvalidOperationException("No seed found matching the requested first draw.");
    }
}
