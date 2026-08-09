using Gens.Simulation.Random;
using NUnit.Framework;

namespace Gens.Simulation.Tests;

public sealed class RandomStreamSetTests
{
    [Test]
    public void CapturedStreamsResumeIndependently()
    {
        var streams = new RandomStreamSet();
        streams.Add("economy", 42, 1);
        streams.Add("politics", 84, 2);
        streams.NextUInt("economy");

        var restored = RandomStreamSet.Restore(streams.CaptureStates());

        Assert.Multiple(() =>
        {
            Assert.That(restored.NextUInt("economy"), Is.EqualTo(streams.NextUInt("economy")));
            Assert.That(restored.NextUInt("politics"), Is.EqualTo(streams.NextUInt("politics")));
        });
    }

    [Test]
    public void DuplicateNamesAreRejected()
    {
        var streams = new RandomStreamSet();
        streams.Add("events", 1, 1);

        Assert.That(() => streams.Add("events", 2, 2), Throws.ArgumentException);
    }
}
