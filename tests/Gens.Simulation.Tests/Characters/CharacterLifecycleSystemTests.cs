using Gens.Simulation.Characters;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class CharacterLifecycleSystemTests
{
    private const string MortalityStreamName = "test-mortality";

    [Test]
    public void StageChangeEventFiresExactlyAtTheBoundaryMonth()
    {
        var state = new WorldState(new GameDate(156));
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, birthDate: new GameDate(0), condition: new Condition(100, 0, 50, 20, 50)));
        var system = new CharacterLifecycleSystem(MortalityStreamName);
        var streams = StreamsWithDraws(MortalityStreamName, v => v > 999_000);

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(156), streams));

        var stageChanged = events.OfType<CharacterLifecycleStageChangedEvent>().Single();
        Assert.That(stageChanged.PreviousStage, Is.EqualTo(LifecycleStage.Child));
        Assert.That(stageChanged.CurrentStage, Is.EqualTo(LifecycleStage.Adolescent));
    }

    [Test]
    public void NoStageChangeEventFiresMidStage()
    {
        var state = new WorldState(new GameDate(100));
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, birthDate: new GameDate(0), condition: new Condition(100, 0, 50, 20, 50)));
        var system = new CharacterLifecycleSystem(MortalityStreamName);
        var streams = StreamsWithDraws(MortalityStreamName, v => v > 999_000);

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(100), streams));

        Assert.That(events.OfType<CharacterLifecycleStageChangedEvent>(), Is.Empty);
    }

    [Test]
    public void FatigueRecoversTenPointsPerMonth()
    {
        var state = new WorldState(new GameDate(50));
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, birthDate: new GameDate(0), condition: new Condition(80, 50, 50, 20, 50)));
        var system = new CharacterLifecycleSystem(MortalityStreamName);
        var streams = StreamsWithDraws(MortalityStreamName, v => v > 999_000);

        system.Tick(state, new MonthlyTickContext(new GameDate(50), streams));

        state.Characters.TryGet(id, out var updated);
        Assert.That(updated.Condition.Fatigue, Is.EqualTo(40));
    }

    [Test]
    public void FatigueRecoveryNeverGoesBelowZero()
    {
        var state = new WorldState(new GameDate(50));
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, birthDate: new GameDate(0), condition: new Condition(80, 5, 50, 20, 50)));
        var system = new CharacterLifecycleSystem(MortalityStreamName);
        var streams = StreamsWithDraws(MortalityStreamName, v => v > 999_000);

        system.Tick(state, new MonthlyTickContext(new GameDate(50), streams));

        state.Characters.TryGet(id, out var updated);
        Assert.That(updated.Condition.Fatigue, Is.EqualTo(0));
    }

    [Test]
    public void ElderlyHealthDeclinesOnlyOnTheBirthdayMonth()
    {
        var birthdayMonth = 60 * 12; // Elderly begins at 60; birthDate=0 so month 720 is a birthday month.
        var state = new WorldState(new GameDate(birthdayMonth));
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, birthDate: new GameDate(0), condition: new Condition(50, 0, 50, 20, 50)));
        var system = new CharacterLifecycleSystem(MortalityStreamName);
        var streams = StreamsWithDraws(MortalityStreamName, v => v > 999_000);

        system.Tick(state, new MonthlyTickContext(new GameDate(birthdayMonth), streams));

        state.Characters.TryGet(id, out var updated);
        Assert.That(updated.Condition.Health, Is.EqualTo(48));
    }

    [Test]
    public void ElderlyHealthDoesNotDeclineOffTheBirthdayMonth()
    {
        var nonBirthdayMonth = 60 * 12 + 3;
        var state = new WorldState(new GameDate(nonBirthdayMonth));
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, birthDate: new GameDate(0), condition: new Condition(50, 0, 50, 20, 50)));
        var system = new CharacterLifecycleSystem(MortalityStreamName);
        var streams = StreamsWithDraws(MortalityStreamName, v => v > 999_000);

        system.Tick(state, new MonthlyTickContext(new GameDate(nonBirthdayMonth), streams));

        state.Characters.TryGet(id, out var updated);
        Assert.That(updated.Condition.Health, Is.EqualTo(50));
    }

    [Test]
    public void AGuaranteedDeathRollProducesADeathRecordAndEvent()
    {
        var state = new WorldState(new GameDate(1));
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, birthDate: new GameDate(0), condition: new Condition(10, 0, 50, 20, 50)));
        var system = new CharacterLifecycleSystem(MortalityStreamName);
        var streams = StreamsWithDraws(MortalityStreamName, v => v < 100);

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(1), streams));

        state.Characters.TryGet(id, out var updated);
        Assert.That(updated.IsAlive, Is.False);
        Assert.That(updated.DeathRecord!.Value.Cause, Is.EqualTo(DeathCause.Disease));
        Assert.That(events.OfType<CharacterDiedEvent>().Single().CharacterId, Is.EqualTo(id));
    }

    [Test]
    public void DeathClosesBothSidesOfAnOpenMarriage()
    {
        var state = new WorldState(new GameDate(1));
        var deceasedId = state.CharacterIds.Issue();
        var spouseId = state.CharacterIds.Issue();
        var marriageStart = new GameDate(0);
        state.Characters.Add(deceasedId, CharacterTestFixtures.Minimal(
            deceasedId, birthDate: new GameDate(0), condition: new Condition(5, 0, 50, 20, 50),
            maritalHistory: new[] { new MarriageRecord(spouseId, marriageStart, null, null) }));
        state.Characters.Add(spouseId, CharacterTestFixtures.Minimal(
            spouseId, birthDate: new GameDate(0), condition: new Condition(100, 0, 50, 20, 50),
            maritalHistory: new[] { new MarriageRecord(deceasedId, marriageStart, null, null) }));

        var system = new CharacterLifecycleSystem(MortalityStreamName);
        // The deceased is processed first (lower ID): force their draw to guarantee death, and the
        // spouse's subsequent draw to guarantee survival.
        var streams = StreamsWithTwoDraws(MortalityStreamName, first => first < 100, second => second > 50_000);

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(1), streams));

        state.Characters.TryGet(deceasedId, out var deceased);
        state.Characters.TryGet(spouseId, out var spouse);

        Assert.That(deceased.IsAlive, Is.False);
        Assert.That(spouse.IsAlive, Is.True);

        var deceasedMarriage = deceased.MaritalHistory.Single();
        Assert.That(deceasedMarriage.EndDate, Is.EqualTo(new GameDate(1)));
        Assert.That(deceasedMarriage.EndReason, Is.EqualTo(MarriageEndReason.Death));

        var spouseMarriage = spouse.MaritalHistory.Single();
        Assert.That(spouseMarriage.EndDate, Is.EqualTo(new GameDate(1)));
        Assert.That(spouseMarriage.EndReason, Is.EqualTo(MarriageEndReason.Death));

        var diedEvent = events.OfType<CharacterDiedEvent>().Single();
        Assert.That(diedEvent.SpouseId, Is.EqualTo(spouseId));

        var marriageEndedEvent = events.OfType<MarriageEndedEvent>().Single();
        Assert.That(marriageEndedEvent.CharacterId, Is.EqualTo(deceasedId));
        Assert.That(marriageEndedEvent.SpouseId, Is.EqualTo(spouseId));
        Assert.That(marriageEndedEvent.Reason, Is.EqualTo(MarriageEndReason.Death));
    }

    [Test]
    public void ACharacterBornThisSameTickIsSkipped()
    {
        var state = new WorldState(new GameDate(5));
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(id, birthDate: new GameDate(5)));
        var system = new CharacterLifecycleSystem(MortalityStreamName);
        // No draw is ever made for a Character skipped as "born this same tick" (see the assertion
        // below), so which seed is used here is irrelevant.
        var streams = StreamsWithDraws(MortalityStreamName, static _ => true);

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(5), streams));

        Assert.That(events, Is.Empty);
        state.Characters.TryGet(id, out var unchanged);
        Assert.That(unchanged.IsAlive, Is.True);
    }

    /// <summary>Deterministically finds a seed whose first mortality draw satisfies <paramref
    /// name="matchesFirstDraw"/>, then returns a fresh stream registered with that seed — used to
    /// force a guaranteed death or a guaranteed survival without depending on an opaque, hand-picked
    /// magic seed.</summary>
    private static RandomStreamSet StreamsWithDraws(string streamName, Predicate<uint> matchesFirstDraw)
    {
        for (ulong seed = 0; seed < 100_000; seed++)
        {
            var probe = new RandomStreamSet();
            probe.Add(streamName, seed, 1);
            if (matchesFirstDraw(probe.NextUInt(streamName, 1_000_000)))
            {
                var streams = new RandomStreamSet();
                streams.Add(streamName, seed, 1);
                return streams;
            }
        }

        throw new InvalidOperationException("No seed found matching the requested first draw.");
    }

    private static RandomStreamSet StreamsWithTwoDraws(
        string streamName, Predicate<uint> matchesFirstDraw, Predicate<uint> matchesSecondDraw)
    {
        for (ulong seed = 0; seed < 100_000; seed++)
        {
            var probe = new RandomStreamSet();
            probe.Add(streamName, seed, 1);
            var first = probe.NextUInt(streamName, 1_000_000);
            var second = probe.NextUInt(streamName, 1_000_000);
            if (matchesFirstDraw(first) && matchesSecondDraw(second))
            {
                var streams = new RandomStreamSet();
                streams.Add(streamName, seed, 1);
                return streams;
            }
        }

        throw new InvalidOperationException("No seed found matching the requested draw pair.");
    }
}
