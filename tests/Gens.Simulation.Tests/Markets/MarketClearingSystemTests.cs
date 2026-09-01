using Gens.Simulation.Characters;
using Gens.Simulation.Goods;
using Gens.Simulation.Health;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Markets;
using Gens.Simulation.Numerics;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Markets;

public sealed class MarketClearingSystemTests
{
    private static readonly DefinitionId<Good> GrainId = NeedsConsumptionCalculator.ConsumptionGood;
    private static readonly DefinitionId<Good> ToolsId = new("tools");

    private static MarketPriceBoundConfig Bound => new(Fixed64.FromRaw(150_000), Money.FromMinorUnits(1));

    private static MarketClearingSystem BuildSystem() => new(
        new[]
        {
            new KeyValuePair<DefinitionId<Good>, Money>(GrainId, Money.FromDenarii(10)),
            new KeyValuePair<DefinitionId<Good>, Money>(ToolsId, Money.FromDenarii(50)),
        },
        Bound);

    private static (WorldState State, RuntimeId<Settlement> SettlementId) BuildScenario(long grainSupply, int popSize)
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));

        var holdingId = state.HoldingIds.Issue();
        state.Holdings.Add(holdingId, Holding.Create(holdingId, settlementId));
        var stockpile = new Stockpile(1_000);
        if (grainSupply > 0)
            stockpile.Add(new GoodDefinition(GrainId, Perishability.NonPerishable), grainSupply);
        state.Stockpiles.Add(holdingId, stockpile);

        if (popSize > 0)
        {
            var popGroup = PopGroup.Create(settlementId, PopGroupType.Operarii, popSize, needsProfile: DietTier.Meager);
            state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Operarii), popGroup);
        }

        return (state, settlementId);
    }

    private static MonthlyTickContext Context(WorldState state) =>
        new(state.Date, new RandomStreamSet());

    [Test]
    public void ClearingMatchesSupplyAgainstDemandAndReportsUnsatisfiedDemand()
    {
        // Demand = popSize (1 grain/head/month, DietTier.Meager per NeedsConsumptionCalculator).
        var (state, settlementId) = BuildScenario(grainSupply: 3, popSize: 10);

        var events = BuildSystem().Tick(state, Context(state));

        var cleared = (MarketClearedEvent)events.Single();
        var line = cleared.Lines.Single(l => l.GoodId == GrainId);
        Assert.Multiple(() =>
        {
            Assert.That(line.Supply, Is.EqualTo(3));
            Assert.That(line.Demand, Is.EqualTo(10));
            Assert.That(line.ClearedQuantity, Is.EqualTo(3));
            Assert.That(line.UnsatisfiedDemand, Is.EqualTo(7));
        });

        state.MarketPrices.TryGet(new MarketGoodKey(settlementId, GrainId), out var market);
        Assert.That(market!.UnsatisfiedDemand, Is.EqualTo(7));
    }

    [Test]
    public void ClearingLeavesNoUnsatisfiedDemandWhenSupplyMeetsOrExceedsDemand()
    {
        var (state, _) = BuildScenario(grainSupply: 50, popSize: 5);

        var events = BuildSystem().Tick(state, Context(state));

        var cleared = (MarketClearedEvent)events.Single();
        var line = cleared.Lines.Single(l => l.GoodId == GrainId);
        Assert.That(line.ClearedQuantity, Is.EqualTo(5));
        Assert.That(line.UnsatisfiedDemand, Is.EqualTo(0));
    }

    [Test]
    public void AGoodWithNoModeledDemandClearsWithZeroDemandButNonNegativeSupply()
    {
        var (state, settlementId) = BuildScenario(grainSupply: 1, popSize: 1);
        state.Stockpiles.TryGet(state.Holdings.InAscendingOrder().Single().Key, out var stockpile);
        stockpile.Add(new GoodDefinition(ToolsId, Perishability.NonPerishable), 4);

        var events = BuildSystem().Tick(state, Context(state));

        var cleared = (MarketClearedEvent)events.Single();
        var toolsLine = cleared.Lines.Single(l => l.GoodId == ToolsId);
        Assert.That(toolsLine.Demand, Is.EqualTo(0));
        Assert.That(toolsLine.Supply, Is.EqualTo(4));
        Assert.That(toolsLine.ClearedQuantity, Is.EqualTo(0));

        state.MarketPrices.TryGet(new MarketGoodKey(settlementId, ToolsId), out var market);
        Assert.That(market!.Price.RawValue, Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public void PriceMovementNeverExceedsTheConfiguredBoundAcrossRepeatedTicksOfExtremeScarcity()
    {
        var (state, settlementId) = BuildScenario(grainSupply: 1, popSize: 1_000);
        var system = BuildSystem();

        Money? previous = null;
        for (var month = 0; month < 6; month++)
        {
            system.Tick(state, Context(state));
            state.MarketPrices.TryGet(new MarketGoodKey(settlementId, GrainId), out var market);

            if (previous is { } prior)
            {
                var maxUp = prior.Scale(Fixed64.One + Bound.MaxPriceMoveFraction);
                Assert.That(market!.Price, Is.LessThanOrEqualTo(maxUp));
            }

            previous = market!.Price;
            state.AdvanceMonth();
        }
    }

    [Test]
    public void AnActiveSettlementWideQuarantineReducesClearedSupply()
    {
        // 100 grain supply, negligible demand, so ClearedQuantity tracks Supply directly.
        var (state, settlementId) = BuildScenario(grainSupply: 100, popSize: 0);
        var outbreakId = state.EpidemicOutbreakIds.Issue();
        state.EpidemicOutbreaks.Add(
            outbreakId,
            EpidemicOutbreak.Create(outbreakId, settlementId, DiseaseCatalog.Pestilence, state.Date) with { SettlementQuarantineActive = true });

        var events = BuildSystem().Tick(state, Context(state));

        var cleared = (MarketClearedEvent)events.Single();
        var line = cleared.Lines.Single(l => l.GoodId == GrainId);
        var expectedSupply = (long)Math.Round(100 * QuarantineEffectCalculator.CommerceSupplyMultiplier(true), MidpointRounding.AwayFromZero);
        Assert.That(line.Supply, Is.EqualTo(expectedSupply));
        Assert.That(line.Supply, Is.LessThan(100));
    }

    [Test]
    public void RepeatedTicksWithIdenticalInputProduceIdenticalClearingResultsAcrossSeparateRuns()
    {
        static IReadOnlyList<MarketClearedEvent> RunScenario()
        {
            var (state, _) = BuildScenario(grainSupply: 7, popSize: 12);
            var system = BuildSystem();
            var results = new List<MarketClearedEvent>();
            for (var month = 0; month < 3; month++)
            {
                var events = system.Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));
                results.Add((MarketClearedEvent)events.Single());
                state.AdvanceMonth();
            }

            return results;
        }

        var first = RunScenario();
        var second = RunScenario();

        Assert.That(first.Count, Is.EqualTo(second.Count));
        for (var i = 0; i < first.Count; i++)
        {
            Assert.That(first[i].Lines.Count, Is.EqualTo(second[i].Lines.Count));
            for (var lineIndex = 0; lineIndex < first[i].Lines.Count; lineIndex++)
            {
                Assert.That(first[i].Lines[lineIndex], Is.EqualTo(second[i].Lines[lineIndex]));
            }
        }
    }
}
