namespace Gens.Simulation.Random;

/// <summary>
/// Owns the named random streams used by a campaign. Stream names are persisted so
/// adding random draws to one simulation system cannot perturb another system.
/// </summary>
public sealed class RandomStreamSet
{
    private readonly Dictionary<string, Pcg32> _streams;

    public RandomStreamSet() => _streams = new(StringComparer.Ordinal);

    private RandomStreamSet(Dictionary<string, Pcg32> streams) => _streams = streams;

    public IEnumerable<string> Names => _streams.Keys.OrderBy(static name => name, StringComparer.Ordinal);

    public void Add(string name, ulong seed, ulong stream)
    {
        ValidateName(name);
        if (!_streams.TryAdd(name, new Pcg32(seed, stream)))
            throw new ArgumentException($"A random stream named '{name}' already exists.", nameof(name));
    }

    /// <summary>Registers a stream whose (seed, stream) pair is deterministically derived from a
    /// campaign root seed and this stream's name (<see cref="SeedDerivation"/>), so adding a new,
    /// unrelated stream never perturbs any existing stream's draws.</summary>
    public void AddDerived(string name, ulong rootSeed)
    {
        var (seed, stream) = SeedDerivation.Derive(rootSeed, name);
        Add(name, seed, stream);
    }

    public uint NextUInt(string name)
    {
        var random = Get(name);
        var value = random.NextUInt();
        _streams[name] = random;
        return value;
    }

    public uint NextUInt(string name, uint exclusiveUpperBound)
    {
        var random = Get(name);
        var value = random.NextUInt(exclusiveUpperBound);
        _streams[name] = random;
        return value;
    }

    public IReadOnlyDictionary<string, Pcg32State> CaptureStates() => _streams
        .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
        .ToDictionary(static pair => pair.Key, static pair => pair.Value.CaptureState(), StringComparer.Ordinal);

    public static RandomStreamSet Restore(IReadOnlyDictionary<string, Pcg32State> states)
    {
        if (states is null)
            throw new ArgumentNullException(nameof(states));
        var streams = new Dictionary<string, Pcg32>(states.Count, StringComparer.Ordinal);
        foreach (var pair in states)
        {
            ValidateName(pair.Key);
            streams.Add(pair.Key, Pcg32.Restore(pair.Value));
        }

        return new RandomStreamSet(streams);
    }

    private Pcg32 Get(string name)
    {
        ValidateName(name);
        if (!_streams.TryGetValue(name, out var random))
            throw new KeyNotFoundException($"Random stream '{name}' is not registered.");
        return random;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A random stream name is required.", nameof(name));
    }
}
