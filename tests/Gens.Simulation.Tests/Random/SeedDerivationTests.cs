using Gens.Simulation.Random;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Random;

public sealed class SeedDerivationTests
{
    [Test]
    public void SameRootSeedAndStreamNameAlwaysDeriveTheSamePair()
    {
        var first = SeedDerivation.Derive(42, "economy");
        var second = SeedDerivation.Derive(42, "economy");

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public void DifferentStreamNamesDeriveDifferentPairs()
    {
        var economy = SeedDerivation.Derive(42, "economy");
        var politics = SeedDerivation.Derive(42, "politics");

        Assert.That(economy, Is.Not.EqualTo(politics));
    }

    [Test]
    public void DifferentRootSeedsDeriveDifferentPairs()
    {
        var first = SeedDerivation.Derive(1, "economy");
        var second = SeedDerivation.Derive(2, "economy");

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [Test]
    public void EmptyStreamNameIsRejected()
    {
        Assert.That(() => SeedDerivation.Derive(1, string.Empty), Throws.ArgumentException);
    }

    [Test]
    public void AddingAnUnrelatedStreamDoesNotPerturbExistingDraws()
    {
        const ulong rootSeed = 12345;

        var baseline = new RandomStreamSet();
        baseline.AddDerived("economy", rootSeed);
        baseline.AddDerived("politics", rootSeed);
        var baselineDraws = new[] { baseline.NextUInt("economy"), baseline.NextUInt("politics"), baseline.NextUInt("economy") };

        var withExtraStream = new RandomStreamSet();
        withExtraStream.AddDerived("economy", rootSeed);
        withExtraStream.AddDerived("politics", rootSeed);
        withExtraStream.AddDerived("events", rootSeed);
        var extraDraws = new[] { withExtraStream.NextUInt("economy"), withExtraStream.NextUInt("politics"), withExtraStream.NextUInt("economy") };

        Assert.That(extraDraws, Is.EqualTo(baselineDraws));
    }
}
