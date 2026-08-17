using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Economy;

public sealed class HouseholdEconomyCalculatorTests
{
    [Test]
    public void MultiplyByQuantitySaturatesRatherThanOverflowingAtExtremeValues()
    {
        var result = HouseholdEconomyCalculator.MultiplyByQuantity(Money.MaxValue, 2);
        Assert.That(result, Is.EqualTo(Money.MaxValue));
    }

    [Test]
    public void MultiplyByQuantityComputesTheOrdinaryProduct()
    {
        var result = HouseholdEconomyCalculator.MultiplyByQuantity(Money.FromDenarii(3), 4);
        Assert.That(result, Is.EqualTo(Money.FromDenarii(12)));
    }

    [Test]
    public void MultiplyByQuantityRejectsNegativeQuantity() =>
        Assert.That(() => HouseholdEconomyCalculator.MultiplyByQuantity(Money.FromDenarii(1), -1), Throws.TypeOf<ArgumentOutOfRangeException>());

    [Test]
    public void ApplyFractionSaturatesAtExtremeQuantity()
    {
        var result = HouseholdEconomyCalculator.ApplyFraction(long.MaxValue, Fixed64.One);
        Assert.That(result, Is.EqualTo(long.MaxValue));
    }

    [Test]
    public void ApplyFractionComputesAPartialQuantity()
    {
        var result = HouseholdEconomyCalculator.ApplyFraction(100, Fixed64.FromRaw(500_000)); // 0.5
        Assert.That(result, Is.EqualTo(50));
    }

    [Test]
    public void BandForClassifiesNetWorthIntoTheExpectedBands()
    {
        Assert.Multiple(() =>
        {
            Assert.That(HouseholdEconomyCalculator.BandFor(Money.FromDenarii(-1)), Is.EqualTo(HouseholdWealthBand.Ruined));
            Assert.That(HouseholdEconomyCalculator.BandFor(Money.FromDenarii(0)), Is.EqualTo(HouseholdWealthBand.Modest));
            Assert.That(HouseholdEconomyCalculator.BandFor(Money.FromDenarii(500)), Is.EqualTo(HouseholdWealthBand.Comfortable));
            Assert.That(HouseholdEconomyCalculator.BandFor(Money.FromDenarii(50_000)), Is.EqualTo(HouseholdWealthBand.Wealthy));
        });
    }

    [Test]
    public void TryGetOwningHouseholdResolvesAHouseholdTaggedOwner()
    {
        var householdId = RuntimeId<Household>.Parse("household_0000001");
        var holding = Holding.Create(RuntimeId<Holding>.Parse("holding_0000001"), RuntimeId<Settlement>.Parse("settlement_0000001"), ownerId: householdId.ToTaggedString());

        var resolved = HouseholdEconomyCalculator.TryGetOwningHousehold(holding, out var result);

        Assert.That(resolved, Is.True);
        Assert.That(result, Is.EqualTo(householdId));
    }

    [Test]
    public void TryGetOwningHouseholdReturnsFalseForAnUnownedOrNonHouseholdOwnedHolding()
    {
        var unowned = Holding.Create(RuntimeId<Holding>.Parse("holding_0000001"), RuntimeId<Settlement>.Parse("settlement_0000001"));
        var nonHousehold = Holding.Create(
            RuntimeId<Holding>.Parse("holding_0000002"), RuntimeId<Settlement>.Parse("settlement_0000001"), ownerId: "char_0000001");

        Assert.Multiple(() =>
        {
            Assert.That(HouseholdEconomyCalculator.TryGetOwningHousehold(unowned, out _), Is.False);
            Assert.That(HouseholdEconomyCalculator.TryGetOwningHousehold(nonHousehold, out _), Is.False);
        });
    }
}
