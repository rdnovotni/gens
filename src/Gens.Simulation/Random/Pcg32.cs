namespace Gens.Simulation.Random;

/// <summary>A small, versioned PCG-XSH-RR generator with serializable state.</summary>
public struct Pcg32
{
    public const int AlgorithmVersion = 1;
    private const ulong Multiplier = 6364136223846793005UL;
    private ulong _state;
    private readonly ulong _increment;

    public Pcg32(ulong seed, ulong stream)
    {
        _state = 0;
        _increment = (stream << 1) | 1;
        NextUInt();
        _state += seed;
        NextUInt();
    }

    public readonly Pcg32State CaptureState() => new(AlgorithmVersion, _state, _increment);

    public static Pcg32 Restore(Pcg32State state)
    {
        if (state.AlgorithmVersion != AlgorithmVersion)
            throw new ArgumentOutOfRangeException(nameof(state), "Unsupported PCG32 version.");
        if ((state.Increment & 1) == 0)
            throw new ArgumentException("A PCG stream increment must be odd.", nameof(state));
        return new Pcg32(state.State, state.Increment, true);
    }

    private Pcg32(ulong state, ulong increment, bool _) => (_state, _increment) = (state, increment);

    public uint NextUInt()
    {
        var oldState = _state;
        _state = unchecked(oldState * Multiplier + _increment);
        var xorShifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
        var rotation = (int)(oldState >> 59);
        return (xorShifted >> rotation) | (xorShifted << ((-rotation) & 31));
    }

    public uint NextUInt(uint exclusiveUpperBound)
    {
        if (exclusiveUpperBound == 0)
            throw new ArgumentOutOfRangeException(nameof(exclusiveUpperBound));
        var threshold = unchecked((uint)(0 - exclusiveUpperBound)) % exclusiveUpperBound;
        while (true)
        {
            var value = NextUInt();
            if (value >= threshold)
                return value % exclusiveUpperBound;
        }
    }
}

public readonly record struct Pcg32State(int AlgorithmVersion, ulong State, ulong Increment);

