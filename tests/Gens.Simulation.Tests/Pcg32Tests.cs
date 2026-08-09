using FsCheck;
using FsCheck.NUnit;
using Gens.Simulation.Random;
using NUnit.Framework;

namespace Gens.Simulation.Tests;

public sealed class Pcg32Tests
{
    [Test]
    public void GoldenSeedSequenceIsStable()
    {
        var random = new Pcg32(42, 54);
        var actual = Enumerable.Range(0, 6).Select(_ => random.NextUInt()).ToArray();
        Assert.That(actual, Is.EqualTo(new uint[]
        {
            2_707_161_783, 2_068_313_097, 3_122_475_824,
            2_211_639_955, 3_215_226_955, 3_421_331_566,
        }));
    }

    [FsCheck.NUnit.Property]
    public Property CapturedStateContinuesTheSameStream(ulong seed, ulong stream)
    {
        var original = new Pcg32(seed, stream);
        original.NextUInt();
        var restored = Pcg32.Restore(original.CaptureState());
        return (original.NextUInt() == restored.NextUInt()).ToProperty();
    }

    [FsCheck.NUnit.Property]
    public Property BoundedValuesRemainInRange(PositiveInt upperBound, ulong seed)
    {
        var random = new Pcg32(seed, 1);
        var bound = (uint)upperBound.Get;
        return (random.NextUInt(bound) < bound).ToProperty();
    }
}
