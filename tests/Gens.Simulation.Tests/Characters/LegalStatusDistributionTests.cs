using Gens.Simulation.Characters;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class LegalStatusDistributionTests
{
    [Test]
    public void WithNewEntrantsAddsToPeregrineOnly()
    {
        var distribution = new LegalStatusDistribution(2, 1, 3, 1);

        var result = distribution.WithNewEntrants(5);

        Assert.That(result, Is.EqualTo(new LegalStatusDistribution(2, 1, 8, 1)));
    }

    [Test]
    public void WithNewEntrantsRejectsANegativeCount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LegalStatusDistribution.Empty.WithNewEntrants(-1));
    }

    [Test]
    public void SplitApportionsProportionallyAndConservesTheTotal()
    {
        // 100 total (10 Citizen, 20 LatinRights, 60 Peregrine, 10 Freedman); splitting 10 out should
        // take 1/2/6/1 (exact proportional shares, no remainder to distribute).
        var distribution = new LegalStatusDistribution(10, 20, 60, 10);

        var (taken, remaining) = distribution.Split(10);

        Assert.That(taken, Is.EqualTo(new LegalStatusDistribution(1, 2, 6, 1)));
        Assert.That(remaining, Is.EqualTo(new LegalStatusDistribution(9, 18, 54, 9)));
        Assert.That(taken.Total + remaining.Total, Is.EqualTo(distribution.Total));
    }

    [Test]
    public void SplitLargestRemainderMethodAccountsForEveryUnit()
    {
        // 3 total (1 each of Citizen/LatinRights/Peregrine); splitting 2 out can't divide evenly
        // (2/3 each), so the largest-remainder method must assign exactly 2 whole units total.
        var distribution = new LegalStatusDistribution(1, 1, 1, 0);

        var (taken, remaining) = distribution.Split(2);

        Assert.That(taken.Total, Is.EqualTo(2));
        Assert.That(remaining.Total, Is.EqualTo(1));
        // Tie-break order is fixed (Citizen, LatinRights, Peregrine, Freedman): with all three
        // remainders tied at 2/3, Citizen and LatinRights win the two available units.
        Assert.That(taken, Is.EqualTo(new LegalStatusDistribution(1, 1, 0, 0)));
    }

    [Test]
    public void SplitClampsCountToTotal()
    {
        var distribution = new LegalStatusDistribution(2, 0, 0, 0);

        var (taken, remaining) = distribution.Split(100);

        Assert.That(taken, Is.EqualTo(distribution));
        Assert.That(remaining, Is.EqualTo(LegalStatusDistribution.Empty));
    }

    [Test]
    public void SplitOfAnEmptyDistributionTakesNothing()
    {
        var (taken, remaining) = LegalStatusDistribution.Empty.Split(5);

        Assert.That(taken, Is.EqualTo(LegalStatusDistribution.Empty));
        Assert.That(remaining, Is.EqualTo(LegalStatusDistribution.Empty));
    }

    [Test]
    public void ShrinkReturnsOnlyTheRemainingShare()
    {
        var distribution = new LegalStatusDistribution(10, 0, 0, 0);

        var remaining = distribution.Shrink(4);

        Assert.That(remaining, Is.EqualTo(new LegalStatusDistribution(6, 0, 0, 0)));
    }

    [Test]
    public void AdditionOperatorMergesFieldByField()
    {
        var a = new LegalStatusDistribution(1, 2, 3, 4);
        var b = new LegalStatusDistribution(4, 3, 2, 1);

        Assert.That(a + b, Is.EqualTo(new LegalStatusDistribution(5, 5, 5, 5)));
    }
}
