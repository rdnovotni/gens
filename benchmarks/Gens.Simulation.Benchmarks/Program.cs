using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Gens.Simulation.Benchmarks;
using Gens.Simulation.Random;

BenchmarkRunner.Run<MonthlyTickBaseline>();

namespace Gens.Simulation.Benchmarks
{
    [MemoryDiagnoser]
    public class MonthlyTickBaseline
    {
        private Pcg32 _random = new(42, 1);

        [Benchmark]
        public uint DeterministicBatch()
        {
            var aggregate = 0U;
            for (var index = 0; index < 10_000; index++)
                aggregate ^= _random.NextUInt();
            return aggregate;
        }
    }
}

