using Gens.Simulation.Random;

namespace Gens.Simulation.Characters;

/// <summary>
/// Deterministically rolls a newborn's baseline <see cref="Condition"/> from a named <see
/// cref="RandomStreamSet"/> stream (Phase 5 item 3). No numeric baseline-Condition formula exists in
/// the design corpus — <c>gens-familia-design.md</c> §2.3 only fixes the five stats' 0-100 shape — so
/// the bands below are this implementation's own choice: a newborn starts near-full Health (birth
/// itself already carries a mortality risk, tracked separately by <see cref="MortalityCalculator"/>,
/// rather than being double-counted as a depressed starting Health), zero Fatigue (nothing to recover
/// from yet), and a mid-range band for the three stats that describe disposition rather than physical
/// condition. Draw order is fixed — Health, Loyalty, Ambition, Fertility, in that order (Fatigue is a
/// constant, not drawn) — matching <see cref="CharacterIdentityGenerator"/>'s "fixed draw order"
/// discipline, so a later addition here never perturbs an existing draw.
/// </summary>
public static class CharacterConditionGenerator
{
    private const int HealthMin = 85;
    private const int HealthMax = 100;
    private const int MidBandMin = 30;
    private const int MidBandMax = 70;

    public static Condition Generate(RandomStreamSet streams, string streamName)
    {
        if (streams is null)
            throw new ArgumentNullException(nameof(streams));

        var health = RollInclusive(streams, streamName, HealthMin, HealthMax);
        const int fatigue = 0;
        var loyalty = RollInclusive(streams, streamName, MidBandMin, MidBandMax);
        var ambition = RollInclusive(streams, streamName, MidBandMin, MidBandMax);
        var fertility = RollInclusive(streams, streamName, MidBandMin, MidBandMax);

        return new Condition(health, fatigue, loyalty, ambition, fertility);
    }

    private static int RollInclusive(RandomStreamSet streams, string streamName, int min, int max) =>
        min + (int)streams.NextUInt(streamName, (uint)(max - min + 1));
}
