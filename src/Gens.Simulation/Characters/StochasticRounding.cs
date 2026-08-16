using Gens.Simulation.Numerics;
using Gens.Simulation.Random;

namespace Gens.Simulation.Characters;

/// <summary>
/// Shared rounding helper for <see cref="GrowthMortalitySystem"/> and <see cref="MigrationSystem"/>:
/// both apply a small <see cref="Fixed64"/> rate to a <see cref="PopGroup.Size"/> and need an integer
/// delta. Truncating toward zero every month would systematically under-count slow drift for small
/// groups (a rate too small to move a 40-person group by even one whole unit would never do anything,
/// forever) — instead, the whole part of <c>size * rate</c> always applies, and the fractional
/// remainder is resolved by a single deterministic RNG draw per group per tick (rule 8: every draw
/// goes through <see cref="RandomStreamSet"/>, never <see cref="System.Random"/> or a raw seed), so the
/// remainder applies on average exactly as often as its magnitude implies, with no directional bias.
/// </summary>
public static class StochasticRounding
{
    /// <summary>Rounds <c>size * rate</c> to an integer delta, resolving the fractional remainder via
    /// one draw from <paramref name="streamName"/>. Skips the draw entirely when <paramref
    /// name="size"/> is zero — a delta of zero needs no resolution — so an empty group never perturbs
    /// the stream's draw sequence (matching <see cref="Characters.CharacterLifecycleSystem"/>'s
    /// identical "no roll for characters this system has nothing to do for" convention).</summary>
    public static int RoundedDelta(int size, Fixed64 rate, RandomStreamSet streams, string streamName)
    {
        if (size == 0 || rate == Fixed64.Zero)
            return 0;

        var raw = Fixed64.Multiply(Fixed64.FromInt(size), rate).RawValue;
        var whole = raw / Fixed64.ScaleFactor;
        var remainderRaw = raw - whole * Fixed64.ScaleFactor;
        if (remainderRaw == 0)
            return (int)whole;

        var remainderMagnitude = (uint)Math.Abs(remainderRaw);
        var roll = streams.NextUInt(streamName, (uint)Fixed64.ScaleFactor);
        var extra = roll < remainderMagnitude ? Math.Sign(remainderRaw) : 0;
        return (int)whole + extra;
    }
}
