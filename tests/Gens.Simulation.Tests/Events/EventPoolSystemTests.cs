using Gens.Simulation.Commands;
using Gens.Simulation.Events;
using Gens.Simulation.Identity;
using Gens.Simulation.Policies;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Tests.Characters;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Events;

public sealed class EventPoolSystemTests
{
    private const string StreamName = "test-events-pool";

    [Test]
    public void ScriptedHouseholdOmenFiresOnceEligibleAndAutoResolvesItsFirstStage()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        state.HouseholdPolicies.Add(householdId, new HouseholdPolicyState(householdId, RitesBudgetTier.Standard, new GameDate(0)));

        var catalog = SampleEventDefinitions.BuildCatalog();
        var system = new EventPoolSystem(catalog, StreamName);
        var streams = new RandomStreamSet();
        streams.Add(StreamName, 0, 1);

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(0), streams));

        Assert.That(events.OfType<EventFiredEvent>().Any(e => e.DefinitionId.Equals(SampleEventDefinitions.HouseholdOmen)), Is.True);
        var resolved = events.OfType<EventOptionResolvedEvent>().Single(e => e.DefinitionId.Equals(SampleEventDefinitions.HouseholdOmen));
        Assert.That(resolved.OptionId, Is.EqualTo(SampleEventDefinitions.ConsultTheHaruspex));

        var instance = state.EventInstances.InAscendingOrder().Select(e => e.Value)
            .Single(i => i.DefinitionId.Equals(SampleEventDefinitions.HouseholdOmen));
        Assert.That(instance.Status, Is.EqualTo(EventInstanceStatus.AwaitingNextStage));
        Assert.That(instance.CurrentStageIndex, Is.EqualTo(0));
    }

    [Test]
    public void ScriptedHouseholdOmenDoesNotRefireOnceItHasAlreadyFired()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        state.HouseholdPolicies.Add(householdId, new HouseholdPolicyState(householdId, RitesBudgetTier.Standard, new GameDate(0)));

        var catalog = SampleEventDefinitions.BuildCatalog();
        var system = new EventPoolSystem(catalog, StreamName);
        var streams = new RandomStreamSet();
        streams.Add(StreamName, 0, 1);

        system.Tick(state, new MonthlyTickContext(new GameDate(0), streams));
        var instancesAfterFirstTick = state.EventInstances.Count;

        var secondTickEvents = system.Tick(state, new MonthlyTickContext(new GameDate(1), streams));

        Assert.That(secondTickEvents.OfType<EventFiredEvent>().Any(e => e.DefinitionId.Equals(SampleEventDefinitions.HouseholdOmen)), Is.False);
        Assert.That(state.EventInstances.Count, Is.EqualTo(instancesAfterFirstTick));
    }

    [Test]
    public void TheSecondStageOpensAfterItsDelayAndAutoResolvesToATerminalState()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        state.HouseholdPolicies.Add(householdId, new HouseholdPolicyState(householdId, RitesBudgetTier.Standard, new GameDate(0)));

        var catalog = SampleEventDefinitions.BuildCatalog();
        var system = new EventPoolSystem(catalog, StreamName);
        var streams = new RandomStreamSet();
        streams.Add(StreamName, 0, 1);

        system.Tick(state, new MonthlyTickContext(new GameDate(0), streams));
        system.Tick(state, new MonthlyTickContext(new GameDate(1), streams));
        var monthTwoEvents = system.Tick(state, new MonthlyTickContext(new GameDate(2), streams));

        Assert.That(monthTwoEvents.OfType<EventStageAdvancedEvent>().Any(e => e.DefinitionId.Equals(SampleEventDefinitions.HouseholdOmen)), Is.True);
        var instance = state.EventInstances.InAscendingOrder().Select(e => e.Value)
            .Single(i => i.DefinitionId.Equals(SampleEventDefinitions.HouseholdOmen));
        Assert.That(instance.Status, Is.EqualTo(EventInstanceStatus.Resolved));
        Assert.That(instance.CurrentStageIndex, Is.EqualTo(1));
        Assert.That(instance.ResolvedOptionId, Is.EqualTo(SampleEventDefinitions.AcknowledgeTheOmen));
    }

    [Test]
    public void AnUnresolvedInstanceExpiresAfterItsWindowWhenAutoResolveIsDisabled()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        state.HouseholdPolicies.Add(householdId, new HouseholdPolicyState(householdId, RitesBudgetTier.Standard, new GameDate(0)));

        var catalog = SampleEventDefinitions.BuildCatalog();
        var system = new EventPoolSystem(catalog, StreamName, shouldAutoResolve: static (_, _, _) => false);
        var streams = new RandomStreamSet();
        streams.Add(StreamName, 0, 1);

        system.Tick(state, new MonthlyTickContext(new GameDate(0), streams));
        // Stage 0's own expiryMonths is 3 (see SampleEventDefinitions); month 4 is the first tick strictly after that window.
        for (var month = 1; month <= 4; month++)
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));

        var instance = state.EventInstances.InAscendingOrder().Select(e => e.Value)
            .Single(i => i.DefinitionId.Equals(SampleEventDefinitions.HouseholdOmen));
        Assert.That(instance.Status, Is.EqualTo(EventInstanceStatus.Expired));
    }

    [Test]
    public void APersonalWeightedRollFiresWhenTheDrawLandsInsideTheFireRangeAndPicksTheHighestScoringOption()
    {
        var definition = MakeWeightedPersonalDefinition(weight: 30);
        var totalWeight = (uint)(EventPoolSystem.NoFireWeight + 30);
        var streams = StreamsWithFirstDraw(StreamName, totalWeight, draw => draw >= EventPoolSystem.NoFireWeight);

        var state = new WorldState(new GameDate(0));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));

        var catalog = new EventCatalog(new[] { definition });
        var system = new EventPoolSystem(catalog, StreamName);

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(0), streams));

        Assert.That(events.OfType<EventFiredEvent>().Any(), Is.True);
        var resolved = events.OfType<EventOptionResolvedEvent>().Single();
        Assert.That(resolved.OptionId.Value, Is.EqualTo("high-score-option"));
    }

    [Test]
    public void APersonalWeightedRollDoesNotFireWhenTheDrawLandsInsideTheNoFireRange()
    {
        var definition = MakeWeightedPersonalDefinition(weight: 30);
        var totalWeight = (uint)(EventPoolSystem.NoFireWeight + 30);
        var streams = StreamsWithFirstDraw(StreamName, totalWeight, draw => draw < EventPoolSystem.NoFireWeight);

        var state = new WorldState(new GameDate(0));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));

        var catalog = new EventCatalog(new[] { definition });
        var system = new EventPoolSystem(catalog, StreamName);

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(0), streams));

        Assert.That(events.OfType<EventFiredEvent>().Any(), Is.False);
        Assert.That(state.EventInstances.Count, Is.EqualTo(0));
    }

    private static EventDefinition MakeWeightedPersonalDefinition(int weight)
    {
        var highScore = new EventOptionDefinition(
            new DefinitionId<EventOptionDefinition>("high-score-option"), "n", "d",
            eligibility: static (_, _) => null,
            scoreForAi: static (_, _) => 10.0,
            resolve: static (_, _, _, _) => Array.Empty<IDomainEvent>());
        var lowScore = new EventOptionDefinition(
            new DefinitionId<EventOptionDefinition>("low-score-option"), "n", "d",
            eligibility: static (_, _) => null,
            scoreForAi: static (_, _) => 1.0,
            resolve: static (_, _, _, _) => Array.Empty<IDomainEvent>());
        var stage = new EventStageDefinition(0, "n", "d", new[] { highScore, lowScore }, expiryMonths: 2);
        return new EventDefinition(
            new DefinitionId<EventDefinition>("weighted-personal-test"),
            EventScope.Personal, "n", "d", EventTriggerKind.Random,
            eligibility: static (_, _) => true,
            stages: new[] { stage },
            weight: (_, _) => weight);
    }

    /// <summary>Deterministically finds a seed whose first draw against <paramref
    /// name="exclusiveUpperBound"/> satisfies <paramref name="matchesFirstDraw"/>, mirroring
    /// <c>CharacterLifecycleSystemTests.StreamsWithDraws</c>'s identical probing shape.</summary>
    private static RandomStreamSet StreamsWithFirstDraw(string streamName, uint exclusiveUpperBound, Predicate<uint> matchesFirstDraw)
    {
        for (ulong seed = 0; seed < 100_000; seed++)
        {
            var probe = new RandomStreamSet();
            probe.Add(streamName, seed, 1);
            if (matchesFirstDraw(probe.NextUInt(streamName, exclusiveUpperBound)))
            {
                var streams = new RandomStreamSet();
                streams.Add(streamName, seed, 1);
                return streams;
            }
        }

        throw new InvalidOperationException("No seed found matching the requested first draw.");
    }
}
