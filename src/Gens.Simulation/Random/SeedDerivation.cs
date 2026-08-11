using System.Text;

namespace Gens.Simulation.Random;

/// <summary>
/// Deterministically derives a per-stream (seed, stream) pair from a campaign root seed and a
/// stream name, so registering a new named stream never disturbs any existing stream's seed
/// (roadmap Phase 2 item 8). The hash is a plain byte-level FNV-1a fold — never
/// <see cref="string.GetHashCode()"/>, which is randomized per process in modern .NET and would
/// silently break reproducibility across runs.
/// </summary>
public static class SeedDerivation
{
    private const ulong OffsetBasis = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;
    private const ulong SeedSalt = 0x9E3779B97F4A7C15UL;
    private const ulong StreamSalt = 0xC2B2AE3D27D4EB4FUL;

    public static (ulong Seed, ulong Stream) Derive(ulong rootSeed, string streamName)
    {
        if (string.IsNullOrEmpty(streamName))
            throw new ArgumentException("A stream name is required.", nameof(streamName));

        return (Hash(rootSeed, streamName, SeedSalt), Hash(rootSeed, streamName, StreamSalt));
    }

    private static ulong Hash(ulong rootSeed, string streamName, ulong salt)
    {
        var hash = OffsetBasis ^ salt;
        foreach (var b in BitConverter.GetBytes(rootSeed))
            hash = unchecked((hash ^ b) * Prime);

        foreach (var b in Encoding.UTF8.GetBytes(streamName))
            hash = unchecked((hash ^ b) * Prime);

        return hash;
    }
}
