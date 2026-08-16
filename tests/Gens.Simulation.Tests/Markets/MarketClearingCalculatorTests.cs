using Gens.Simulation.Ledger;
using Gens.Simulation.Markets;
using Gens.Simulation.Numerics;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Markets;

public sealed class MarketClearingCalculatorTests
{
    private static readonly MarketPriceBoundConfig Bound = new(
        maxPriceMoveFraction: Fixed64.FromRaw(200_000), // 0.2
        minimumPrice: Money.FromMinorUnits(1));

    [Test]
    public void ComputeClearingMatchesTheSmallerOfSupplyAndDemand()
    {
        var (cleared, unsatisfied) = MarketClearingCalculator.ComputeClearing(supply: 10, demand: 6);
        Assert.That(cleared, Is.EqualTo(6));
        Assert.That(unsatisfied, Is.EqualTo(0));
    }

    [Test]
    public void ComputeClearingReportsUnsatisfiedDemandWhenSupplyIsShort()
    {
        var (cleared, unsatisfied) = MarketClearingCalculator.ComputeClearing(supply: 4, demand: 10);
        Assert.That(cleared, Is.EqualTo(4));
        Assert.That(unsatisfied, Is.EqualTo(6));
    }

    [Test]
    public void ComputeClearingRejectsNegativeInputs()
    {
        Assert.That(() => MarketClearingCalculator.ComputeClearing(-1, 5), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => MarketClearingCalculator.ComputeClearing(5, -1), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void PriceHoldsWhenSupplyEqualsDemand()
    {
        var previous = Money.FromDenarii(10);
        var next = MarketClearingCalculator.ComputeNextPrice(previous, supply: 5, demand: 5, Bound);
        Assert.That(next, Is.EqualTo(previous));
    }

    [Test]
    public void PriceHoldsWithNoSupplyAndNoDemand()
    {
        var previous = Money.FromDenarii(10);
        var next = MarketClearingCalculator.ComputeNextPrice(previous, supply: 0, demand: 0, Bound);
        Assert.That(next, Is.EqualTo(previous));
    }

    [Test]
    public void PriceRisesTowardTheCapWhenDemandExceedsSupply()
    {
        var previous = Money.FromDenarii(10);
        var next = MarketClearingCalculator.ComputeNextPrice(previous, supply: 5, demand: 20, Bound);
        // Scarcity ratio (4x) would exceed the +/-20% bound, so the move clamps to the cap.
        Assert.That(next, Is.EqualTo(Money.FromDenarii(12)));
    }

    [Test]
    public void PriceFallsTowardTheFloorWhenSupplyExceedsDemand()
    {
        var previous = Money.FromDenarii(10);
        var next = MarketClearingCalculator.ComputeNextPrice(previous, supply: 20, demand: 5, Bound);
        // Scarcity ratio (0.25x) would exceed the -20% bound, so the move clamps to the floor of the move.
        Assert.That(next, Is.EqualTo(Money.FromDenarii(8)));
    }

    [Test]
    public void PriceClampsToTheCapWhenThereIsNoSupplyAtAllButDemandExists()
    {
        var previous = Money.FromDenarii(10);
        var next = MarketClearingCalculator.ComputeNextPrice(previous, supply: 0, demand: 3, Bound);
        Assert.That(next, Is.EqualTo(Money.FromDenarii(12)));
    }

    [Test]
    public void PriceNeverFallsBelowTheConfiguredMinimum()
    {
        var tightBound = new MarketPriceBoundConfig(Fixed64.FromRaw(500_000), Money.FromDenarii(9)); // 0.5 fraction
        var previous = Money.FromDenarii(10);
        var next = MarketClearingCalculator.ComputeNextPrice(previous, supply: 100, demand: 1, tightBound);
        Assert.That(next, Is.EqualTo(Money.FromDenarii(9)));
    }

    [Test]
    public void ASmallScarcityRatioMovesTheDesiredPriceWithoutHittingTheBound()
    {
        var previous = Money.FromDenarii(100);
        // Demand is 10% above supply: desired price is 110, well within the +/-20% bound.
        var next = MarketClearingCalculator.ComputeNextPrice(previous, supply: 100, demand: 110, Bound);
        Assert.That(next, Is.EqualTo(Money.FromDenarii(110)));
    }

    [Test]
    public void ComputeNextPriceRejectsANonPositivePreviousPrice()
    {
        Assert.That(
            () => MarketClearingCalculator.ComputeNextPrice(Money.Zero, 1, 1, Bound),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }
}
