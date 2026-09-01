using Gens.Simulation.Characters;
using Gens.Simulation.Health;
using Gens.Simulation.Land;
using Gens.Simulation.Numerics;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Tests.Characters;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Health;

public sealed class EpidemicContagionSystemTests
{
    private const string StreamName = "test-epidemic-contagion";

    [Test]
    public void IgnitionOpensANewOutbreakAndSeedsOneResident()
    {
        var state = new WorldState(new GameDate(10));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, location: settlementId));

        var ignitionThreshold = Threshold(EpidemicSpreadCalculator.MonthlyIgnitionProbability(1.0));
        var seed = FindSeedForSequentialDraws(v => v < ignitionThreshold);
        var streams = new RandomStreamSet();
        streams.Add(StreamName, seed, 1);

        var system = new EpidemicContagionSystem(StreamName);
        var events = system.Tick(state, new MonthlyTickContext(new GameDate(10), streams));

        Assert.That(events.OfType<EpidemicOutbreakIgnitedEvent>().Any(e => e.ConditionId == DiseaseCatalog.Pestilence), Is.True);
        var afflicted = events.OfType<CharacterAfflictedEvent>().Single(e => e.ConditionId == DiseaseCatalog.Pestilence);
        Assert.That(afflicted.CharacterId, Is.EqualTo(characterId));
        Assert.That(afflicted.Category, Is.EqualTo(HealthConditionCategory.Acute));

        var outbreak = state.EpidemicOutbreaks.InAscendingOrder().Single(e => e.Value.ConditionId == DiseaseCatalog.Pestilence).Value;
        Assert.That(outbreak.Status, Is.EqualTo(EpidemicOutbreakStatus.Active));
    }

    [Test]
    public void PersonToPersonSpreadCanInfectAHouseholdCoMember()
    {
        var state = new WorldState(new GameDate(10));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var householdId = state.HouseholdIds.Issue();

        var infectedId = state.CharacterIds.Issue();
        state.Characters.Add(infectedId, CharacterTestFixtures.Minimal(infectedId, location: settlementId, household: householdId));
        var caseId = state.CharacterHealthConditionIds.Issue();
        state.CharacterHealthConditions.Add(caseId, CharacterHealthCondition.Create(
            caseId, infectedId, DiseaseCatalog.Pestilence, HealthConditionCategory.Acute, hasCure: false, severity: 40, new GameDate(9)));

        var susceptibleId = state.CharacterIds.Issue();
        state.Characters.Add(susceptibleId, CharacterTestFixtures.Minimal(
            susceptibleId, nomen: "Secundus", location: settlementId, household: householdId));

        var outbreakId = state.EpidemicOutbreakIds.Issue();
        state.EpidemicOutbreaks.Add(outbreakId, EpidemicOutbreak.Create(outbreakId, settlementId, DiseaseCatalog.Pestilence, new GameDate(9)));

        var spreadThreshold = Threshold(EpidemicSpreadCalculator.HouseholdContactSpreadProbability(1, 1.0, 1.0, 1.0));
        var seed = FindSeedForSequentialDraws(v => v < spreadThreshold);
        var streams = new RandomStreamSet();
        streams.Add(StreamName, seed, 1);

        var system = new EpidemicContagionSystem(StreamName);
        var events = system.Tick(state, new MonthlyTickContext(new GameDate(10), streams));

        var afflicted = events.OfType<CharacterAfflictedEvent>().Single(e => e.ConditionId == DiseaseCatalog.Pestilence);
        Assert.That(afflicted.CharacterId, Is.EqualTo(susceptibleId));
    }

    [Test]
    public void WaterborneSpreadDoesNotRequireHouseholdCoMembership()
    {
        var state = new WorldState(new GameDate(10));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));

        var infectedId = state.CharacterIds.Issue();
        state.Characters.Add(infectedId, CharacterTestFixtures.Minimal(infectedId, location: settlementId));
        var caseId = state.CharacterHealthConditionIds.Issue();
        state.CharacterHealthConditions.Add(caseId, CharacterHealthCondition.Create(
            caseId, infectedId, DiseaseCatalog.EntericFever, HealthConditionCategory.Acute, hasCure: true, severity: 40, new GameDate(9)));

        var susceptibleId = state.CharacterIds.Issue();
        state.Characters.Add(susceptibleId, CharacterTestFixtures.Minimal(susceptibleId, nomen: "Secundus", location: settlementId));

        var outbreakId = state.EpidemicOutbreakIds.Issue();
        state.EpidemicOutbreaks.Add(outbreakId, EpidemicOutbreak.Create(outbreakId, settlementId, DiseaseCatalog.EntericFever, new GameDate(9)));

        var waterborneThreshold = Threshold(EpidemicSpreadCalculator.WaterborneSpreadProbability(1, 1.0, 1.0));
        // Pestilence and Pox and Camp Fever have no active outbreak here, so each draws its own
        // ignition roll first (in EpidemicProfiles order) before Enteric Fever's waterborne roll.
        var seed = FindSeedForSequentialDraws(_ => true, _ => true, _ => true, v => v < waterborneThreshold);
        var streams = new RandomStreamSet();
        streams.Add(StreamName, seed, 1);

        var system = new EpidemicContagionSystem(StreamName);
        var events = system.Tick(state, new MonthlyTickContext(new GameDate(10), streams));

        var afflicted = events.OfType<CharacterAfflictedEvent>().Where(e => e.ConditionId == DiseaseCatalog.EntericFever).ToArray();
        Assert.That(afflicted.Any(e => e.CharacterId == susceptibleId), Is.True);
    }

    [Test]
    public void AnOutbreakEndsOnceItsLastActiveCaseResolves()
    {
        var state = new WorldState(new GameDate(10));
        var settlementId = state.SettlementIds.Issue();
        var outbreakId = state.EpidemicOutbreakIds.Issue();
        state.EpidemicOutbreaks.Add(outbreakId, EpidemicOutbreak.Create(outbreakId, settlementId, DiseaseCatalog.Pestilence, new GameDate(5)));

        var system = new EpidemicContagionSystem(StreamName);
        var events = system.Tick(state, new MonthlyTickContext(new GameDate(10), new RandomStreamSet()));

        Assert.That(events.OfType<EpidemicOutbreakEndedEvent>().Single().OutbreakId, Is.EqualTo(outbreakId));
        state.EpidemicOutbreaks.TryGet(outbreakId, out var updated);
        Assert.That(updated.Status, Is.EqualTo(EpidemicOutbreakStatus.Ended));
        Assert.That(updated.ResolvedDate, Is.EqualTo(new GameDate(10)));
    }

    [Test]
    public void SettlementQuarantineReducesTheSpreadMultiplier()
    {
        var withoutQuarantine = QuarantineEffectCalculator.SettlementSpreadMultiplier(false, false);
        var withQuarantine = QuarantineEffectCalculator.SettlementSpreadMultiplier(true, false);
        Assert.That(withQuarantine, Is.LessThan(withoutQuarantine));
    }

    [Test]
    public void AnActiveSettlementWideQuarantineAppliesAFeltMonthlyContentmentShock()
    {
        var state = new WorldState(AntoninePlagueEra.Start.NextMonth());
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var popKey = new PopGroupKey(settlementId, PopGroupType.Operarii);
        state.PopGroups.Add(popKey, PopGroup.Create(settlementId, PopGroupType.Operarii, 10, contentment: Fixed64.FromInt(1)));

        // A still-active case, so CloseOutbreaksWithNoRemainingCases does not immediately end the
        // outbreak before this test's own Contentment-cost step runs.
        var infectedId = state.CharacterIds.Issue();
        state.Characters.Add(infectedId, CharacterTestFixtures.Minimal(infectedId, location: settlementId));
        var caseId = state.CharacterHealthConditionIds.Issue();
        state.CharacterHealthConditions.Add(caseId, CharacterHealthCondition.Create(
            caseId, infectedId, DiseaseCatalog.Pestilence, HealthConditionCategory.Acute, hasCure: false, severity: 40, state.Date));

        var outbreakId = state.EpidemicOutbreakIds.Issue();
        state.EpidemicOutbreaks.Add(
            outbreakId,
            EpidemicOutbreak.Create(outbreakId, settlementId, DiseaseCatalog.Pestilence, state.Date) with { SettlementQuarantineActive = true });

        var streams = new RandomStreamSet();
        streams.Add(StreamName, 1, 1);
        var system = new EpidemicContagionSystem(StreamName);
        system.Tick(state, new MonthlyTickContext(state.Date, streams));

        state.PopGroups.TryGet(popKey, out var updated);
        Assert.That(updated.Contentment, Is.EqualTo(Fixed64.FromInt(1) + QuarantineEffectCalculator.ContentmentImpact));
    }

    [Test]
    public void NoQuarantineLeavesContentmentUntouched()
    {
        var state = new WorldState(new GameDate(10));
        var settlementId = state.SettlementIds.Issue();
        var popKey = new PopGroupKey(settlementId, PopGroupType.Operarii);
        state.PopGroups.Add(popKey, PopGroup.Create(settlementId, PopGroupType.Operarii, 10, contentment: Fixed64.FromInt(1)));

        var system = new EpidemicContagionSystem(StreamName);
        system.Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        state.PopGroups.TryGet(popKey, out var updated);
        Assert.That(updated.Contentment, Is.EqualTo(Fixed64.FromInt(1)));
    }

    [Test]
    public void AntoninePlagueOnsetAndWaningEventsFireOnceOnTheirOwnRealMonths()
    {
        var onsetState = new WorldState(AntoninePlagueEra.Start);
        var onsetSystem = new EpidemicContagionSystem(StreamName);
        var onsetEvents = onsetSystem.Tick(onsetState, new MonthlyTickContext(onsetState.Date, new RandomStreamSet()));
        Assert.That(onsetEvents.OfType<AntoninePlagueOnsetEvent>().Count(), Is.EqualTo(1));
        Assert.That(onsetEvents.OfType<AntoninePlagueWaningEvent>(), Is.Empty);

        var earlierState = new WorldState(AntoninePlagueEra.Start.NextMonth());
        var earlierEvents = new EpidemicContagionSystem(StreamName).Tick(earlierState, new MonthlyTickContext(earlierState.Date, new RandomStreamSet()));
        Assert.That(earlierEvents.OfType<AntoninePlagueOnsetEvent>(), Is.Empty);
        Assert.That(earlierEvents.OfType<AntoninePlagueWaningEvent>(), Is.Empty);

        var waningState = new WorldState(AntoninePlagueEra.End.NextMonth());
        var waningEvents = new EpidemicContagionSystem(StreamName).Tick(waningState, new MonthlyTickContext(waningState.Date, new RandomStreamSet()));
        Assert.That(waningEvents.OfType<AntoninePlagueWaningEvent>().Count(), Is.EqualTo(1));
        Assert.That(waningEvents.OfType<AntoninePlagueOnsetEvent>(), Is.Empty);
    }

    [Test]
    public void AntoninePlagueEraStampsANewlyIgnitedPestilenceOutbreakAsImperialScale()
    {
        var date = AntoninePlagueEra.Start.NextMonth();
        var state = new WorldState(date);
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, location: settlementId));

        var ignitionThreshold = Threshold(EpidemicSpreadCalculator.AntoninePlagueIgnitionProbability(1.0));
        var seed = FindSeedForSequentialDraws(v => v < ignitionThreshold);
        var streams = new RandomStreamSet();
        streams.Add(StreamName, seed, 1);

        var system = new EpidemicContagionSystem(StreamName);
        system.Tick(state, new MonthlyTickContext(date, streams));

        var outbreak = state.EpidemicOutbreaks.InAscendingOrder().Single(e => e.Value.ConditionId == DiseaseCatalog.Pestilence).Value;
        Assert.That(outbreak.ImperialScale, Is.True);
    }

    private static uint Threshold(double probability) =>
        (uint)Math.Clamp(probability * 1_000_000, 0, 1_000_000);

    private static ulong FindSeedForSequentialDraws(params Predicate<uint>[] matchesDraw)
    {
        for (ulong seed = 0; seed < 200_000; seed++)
        {
            var probe = new RandomStreamSet();
            probe.Add(StreamName, seed, 1);
            var matched = true;
            foreach (var predicate in matchesDraw)
            {
                if (!predicate(probe.NextUInt(StreamName, 1_000_000)))
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
                return seed;
        }

        throw new InvalidOperationException("No seed found matching the requested draw sequence within the search bound.");
    }
}
