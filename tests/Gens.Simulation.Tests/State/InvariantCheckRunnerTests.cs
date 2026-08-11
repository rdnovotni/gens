using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.State;

public sealed class InvariantCheckRunnerTests
{
    [Test]
    public void ChecksRunInOrdinalIdOrderRegardlessOfRegistrationOrder()
    {
        var order = new List<string>();
        var runner = new InvariantCheckRunner(new IInvariantCheck[]
        {
            new RecordingCheck("zzz", order),
            new RecordingCheck("aaa", order),
            new RecordingCheck("mmm", order),
        });

        runner.RunAll(new WorldState(new GameDate(0)));

        Assert.That(order, Is.EqualTo(new[] { "aaa", "mmm", "zzz" }));
    }

    [Test]
    public void NonFatalViolationsAreCollectedNotThrown()
    {
        var runner = new InvariantCheckRunner(new IInvariantCheck[] { new ViolatingCheck("soft", isFatal: false) });

        var violations = runner.RunAll(new WorldState(new GameDate(0)));

        Assert.That(violations, Has.Count.EqualTo(1));
        Assert.That(violations[0].InvariantId, Is.EqualTo("soft"));
    }

    [Test]
    public void FatalViolationThrows()
    {
        var runner = new InvariantCheckRunner(new IInvariantCheck[] { new ViolatingCheck("hard", isFatal: true) });

        Assert.That(() => runner.RunAll(new WorldState(new GameDate(0))),
            Throws.TypeOf<InvariantViolationException>());
    }

    private sealed class RecordingCheck(string id, List<string> order) : IInvariantCheck
    {
        public string Id => id;
        public bool IsFatal => false;

        public IEnumerable<InvariantViolation> Check(WorldState state)
        {
            order.Add(id);
            return Array.Empty<InvariantViolation>();
        }
    }

    private sealed class ViolatingCheck(string id, bool isFatal) : IInvariantCheck
    {
        public string Id => id;
        public bool IsFatal => isFatal;

        public IEnumerable<InvariantViolation> Check(WorldState state) =>
            new[] { new InvariantViolation(id, $"{id} violated", Array.Empty<string>()) };
    }
}
