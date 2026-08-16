using Gens.Simulation.Numerics;

namespace Gens.Simulation.Characters;

/// <summary>Pure assimilation math (<c>gens-settlement-demographics-design.md</c> §10), factored out
/// of <see cref="AssimilationSystem"/> the same way <see cref="BackgroundJobCapacityCalculator"/> sits
/// beside <see cref="JobCapacitySystem"/>.</summary>
public static class AssimilationCalculator
{
    /// <summary>Monthly share of each tier that assimilates upward into the next tier
    /// (Peregrine→Freedman→LatinRights→Citizen). No numeric assimilation rate exists anywhere in the
    /// design corpus — §10 only says it happens "over time," sped or slowed by systems that don't
    /// exist yet (Education &amp; Culture, Diplomacy with Non-Roman Peoples, Reputation Duality) — this
    /// is this implementation's own invented flat baseline, documented as invented pending those
    /// systems and playtesting, matching every other Phase 7 system's convention.</summary>
    private static readonly Fixed64 MonthlyAssimilationRate = Fixed64.FromRaw(5_000); // 0.005.

    /// <summary>§10's single fastest Assimilation path: "A Peregrine or Latin-Rights individual who
    /// serves and is discharged... typically emerge[s] with full Roman Citizenship." No Military &amp;
    /// Combat system exists yet to drive an actual discharge event, so this is a documented extension
    /// point rather than a stub with dead code — reserved for whenever that system exists to call a
    /// citizenship-upgrade path directly (mirroring how <see cref="SocialMobilitySystem"/>'s own doc
    /// comment leaves the Coloni/Operarii→Veterans direction as Military &amp; Combat's future
    /// responsibility, not this system's).</summary>
    public static Fixed64 MilitaryServiceAssimilationRate => MonthlyAssimilationRate;

    /// <summary>Applies one month of upward assimilation drift to a distribution: LatinRights→Citizen,
    /// then Freedman→LatinRights, then Peregrine→Freedman, each a plain-integer floor of <c>tierCount *
    /// MonthlyAssimilationRate</c> (the same deterministic, no-RNG "drift" convention <see
    /// cref="SocialMobilityCalculator"/> uses, not <see cref="StochasticRounding"/>'s probabilistic
    /// remainder). Processed top-down in that fixed order so a single tick never lets one person hop
    /// two tiers: each step only ever reads a tier no earlier step in the same call already
    /// wrote.</summary>
    public static LegalStatusDistribution Apply(LegalStatusDistribution distribution)
    {
        var latinToCitizen = Floor(distribution.LatinRights);
        distribution = distribution with
        {
            LatinRights = distribution.LatinRights - latinToCitizen,
            Citizen = distribution.Citizen + latinToCitizen,
        };

        var freedmanToLatin = Floor(distribution.Freedman);
        distribution = distribution with
        {
            Freedman = distribution.Freedman - freedmanToLatin,
            LatinRights = distribution.LatinRights + freedmanToLatin,
        };

        var peregrineToFreedman = Floor(distribution.Peregrine);
        distribution = distribution with
        {
            Peregrine = distribution.Peregrine - peregrineToFreedman,
            Freedman = distribution.Freedman + peregrineToFreedman,
        };

        return distribution;
    }

    private static int Floor(int tierCount)
    {
        if (tierCount <= 0)
            return 0;
        var raw = Fixed64.Multiply(Fixed64.FromInt(tierCount), MonthlyAssimilationRate).RawValue;
        return (int)(raw / Fixed64.ScaleFactor);
    }
}
