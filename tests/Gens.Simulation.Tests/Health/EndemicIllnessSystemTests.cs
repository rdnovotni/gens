using Gens.Simulation.Characters;
using Gens.Simulation.Health;
using Gens.Simulation.Land;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Tests.Characters;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Health;

public sealed class EndemicIllnessSystemTests
{
    private const string StreamName = "test-endemic-illness";

    [Test]
    public void MarshTerrainCanAfflictAResidentWithRomanFever()
    {
        var state = new WorldState(new GameDate(10));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId, TerrainType.Marsh, capacity: 4));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, location: settlementId));

        var romanFeverThreshold = Threshold(EndemicExposureCalculator.RomanFeverMonthlyProbability(1.0));
        var streams = StreamsWithDraws(v => v < romanFeverThreshold);

        var system = new EndemicIllnessSystem(StreamName);
        var events = system.Tick(state, new MonthlyTickContext(new GameDate(10), streams));

        var romanFeverEvents = events.OfType<CharacterAfflictedEvent>().Where(e => e.ConditionId == DiseaseCatalog.RomanFever).ToArray();
        Assert.That(romanFeverEvents, Has.Length.EqualTo(1));
        Assert.That(romanFeverEvents[0].CharacterId, Is.EqualTo(characterId));
        Assert.That(romanFeverEvents[0].Category, Is.EqualTo(HealthConditionCategory.Chronic));
    }

    [Test]
    public void NoResidentsMeansNoRolls()
    {
        var state = new WorldState(new GameDate(10));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));

        var system = new EndemicIllnessSystem(StreamName);
        var events = system.Tick(state, new MonthlyTickContext(new GameDate(10), new RandomStreamSet()));

        Assert.That(events, Is.Empty);
    }

    [Test]
    public void ComprehensiveSanitationCanPreventAnExposureThatWouldOtherwiseSucceedAtMinimalTier()
    {
        var minimalThreshold = Threshold(EndemicExposureCalculator.TheFluxMonthlyProbability());
        var comprehensiveThreshold = Threshold(
            EndemicExposureCalculator.TheFluxMonthlyProbability() * SanitationInvestmentCalculator.ExposureMultiplier(SanitationInvestmentTier.Comprehensive));
        Assume.That(comprehensiveThreshold, Is.LessThan(minimalThreshold));

        var seed = FindSeedForDraw(v => v >= comprehensiveThreshold && v < minimalThreshold);

        Assert.That(RunAndCheckFluxAfflicted(seed, SanitationInvestmentTier.Minimal), Is.True);
        Assert.That(RunAndCheckFluxAfflicted(seed, SanitationInvestmentTier.Comprehensive), Is.False);
    }

    private static bool RunAndCheckFluxAfflicted(ulong seed, SanitationInvestmentTier tier)
    {
        var state = new WorldState(new GameDate(10));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        // FertilePlain (no Marsh/Hills) keeps Roman Fever and Saturnism's mining branch at zero
        // probability so they draw no RNG at all, making The Flux this settlement's first draw.
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId, TerrainType.FertilePlain, capacity: 4));
        if (tier != SanitationInvestmentTier.Minimal)
        {
            state.SettlementSanitationInvestments.Add(settlementId, SettlementSanitationInvestment.Create(settlementId, tier));
        }
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, location: settlementId));

        var streams = new RandomStreamSet();
        streams.Add(StreamName, seed, 1);

        var system = new EndemicIllnessSystem(StreamName);
        var events = system.Tick(state, new MonthlyTickContext(new GameDate(10), streams));

        return events.OfType<CharacterAfflictedEvent>().Any(e => e.ConditionId == DiseaseCatalog.TheFlux);
    }

    private static uint Threshold(double probability) =>
        (uint)Math.Clamp(probability * 1_000_000, 0, 1_000_000);

    private static ulong FindSeedForDraw(Predicate<uint> matchesDraw)
    {
        for (ulong seed = 0; seed < 200_000; seed++)
        {
            var probe = new RandomStreamSet();
            probe.Add(StreamName, seed, 1);
            if (matchesDraw(probe.NextUInt(StreamName, 1_000_000)))
                return seed;
        }

        throw new InvalidOperationException("No seed found matching the requested draw within the search bound.");
    }

    private static RandomStreamSet StreamsWithDraws(params Predicate<uint>[] matchesDraw)
    {
        var seed = FindSeedForDraw(matchesDraw[0]);
        var streams = new RandomStreamSet();
        streams.Add(StreamName, seed, 1);
        return streams;
    }
}
