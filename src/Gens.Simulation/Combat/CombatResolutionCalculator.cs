#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Numerics;

namespace Gens.Simulation.Combat;

/// <summary>Pure, RNG-free math for the shared Combat Resolution Engine (design doc §4.4 steps 2-4):
/// effective strength, terrain fit, outcome banding, and loss magnitude. Every figure here is this
/// implementation's own invented number — the design doc's own §11 "All numeric sizing... deliberately
/// unsized" disclosure, the same citation <see cref="Hazards.DisasterDamageCalculator"/> already uses
/// for its own untuned first pass. <see cref="CombatResolutionEngine"/> is the only caller that draws
/// RNG (a single variance roll, fed into <see cref="ResolveOutcomeTier"/> as <c>varianceRoll</c>) —
/// nothing in this file touches a random stream, so it stays trivially unit-testable and reusable by
/// any consumer that wants to score a hypothetical matchup without actually resolving one.</summary>
public static class CombatResolutionCalculator
{
    private static readonly Fixed64 FlatUnfavorableCommanderMultiplier = Fixed64.FromRaw(800_000); // 0.8 — §4.2's "flat, unfavorable default."
    private static readonly Fixed64 FortifiedMultiplier = Fixed64.FromRaw(1_500_000); // 1.5.
    private static readonly Fixed64 AmbushMultiplier = Fixed64.FromRaw(1_250_000); // 1.25.

    private static readonly Fixed64 Half = Fixed64.FromRaw(500_000); // 0.5.
    private static readonly Fixed64 VarianceSpan = Fixed64.FromRaw(300_000); // 0.3 — a ±0.15 band around the raw strength ratio.
    private static readonly Fixed64 DecisiveThreshold = Fixed64.FromRaw(750_000); // 0.75.
    private static readonly Fixed64 CostlyThreshold = Fixed64.FromRaw(550_000); // 0.55.
    private static readonly Fixed64 StalemateThreshold = Fixed64.FromRaw(450_000); // 0.45.
    private static readonly Fixed64 DefeatThreshold = Fixed64.FromRaw(250_000); // 0.25.

    /// <summary>§4.4 step 2: manpower weighted by equipment, current condition (Readiness/Morale),
    /// terrain fit, and the commander modifier, then situational modifiers (Fortified/Ambush) applied
    /// on top of the whole side (§4.3).</summary>
    public static Fixed64 EffectiveStrength(CombatSide side)
    {
        if (side is null)
            throw new ArgumentNullException(nameof(side));

        var total = Fixed64.Zero;
        foreach (var group in side.Groups)
        {
            var manpower = Fixed64.FromInt(Math.Max(0, group.Manpower));
            var equipment = EquipmentTierMultiplier(group.EquipmentTier);
            var condition = ConditionMultiplier(group.Readiness, group.Morale);
            var terrain = TerrainFitMultiplier(group.Type, side.Situation.Terrain);
            total += Fixed64.Multiply(Fixed64.Multiply(manpower, equipment), Fixed64.Multiply(condition, terrain));
        }

        total = Fixed64.Multiply(total, side.Commander?.EffectivenessMultiplier ?? FlatUnfavorableCommanderMultiplier);
        if (side.Situation.Fortified)
            total = Fixed64.Multiply(total, FortifiedMultiplier);
        if (side.Situation.Ambush)
            total = Fixed64.Multiply(total, AmbushMultiplier);

        return total;
    }

    /// <summary>§4.3's Cavalry-favors-Open / Infantry-favors-Hills-Forest / Siege-is-only-relevant-
    /// against-a-fortified-target table. <see cref="CombatantType.Militia"/> and <see
    /// cref="CombatantType.Irregular"/> carry no fixed terrain preference, matching the design doc's own
    /// framing of Irregular as "a looser type" built for callers with no terrain concept of their own yet
    /// (Piracy &amp; Banditry's raids).</summary>
    public static Fixed64 TerrainFitMultiplier(CombatantType type, CombatTerrain terrain) => type switch
    {
        CombatantType.Cavalry => terrain switch
        {
            CombatTerrain.Open => Fixed64.FromRaw(1_300_000),
            CombatTerrain.Hills => Fixed64.FromRaw(600_000),
            CombatTerrain.Forest => Fixed64.FromRaw(500_000),
            CombatTerrain.Coast => Fixed64.FromRaw(900_000),
            CombatTerrain.River => Fixed64.FromRaw(800_000),
            _ => throw new ArgumentOutOfRangeException(nameof(terrain), terrain, "Unhandled terrain."),
        },
        CombatantType.Legionary or CombatantType.Auxiliary => terrain switch
        {
            CombatTerrain.Open => Fixed64.FromRaw(1_000_000),
            CombatTerrain.Hills => Fixed64.FromRaw(1_200_000),
            CombatTerrain.Forest => Fixed64.FromRaw(1_200_000),
            CombatTerrain.Coast => Fixed64.FromRaw(900_000),
            CombatTerrain.River => Fixed64.FromRaw(900_000),
            _ => throw new ArgumentOutOfRangeException(nameof(terrain), terrain, "Unhandled terrain."),
        },
        CombatantType.Siege => Fixed64.FromRaw(500_000), // Only relevant against a fortified target at all (§4.3) — CombatSituation.Fortified carries that bonus separately.
        CombatantType.Militia or CombatantType.Irregular => Fixed64.One,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unhandled combatant type."),
    };

    /// <summary>§4.4 step 3: strength ratio sets the odds, not a guaranteed outcome ("a real, if usually
    /// small, chance for the weaker side to win outright"). <paramref name="varianceRoll"/> is a
    /// caller-drawn value in <c>[0, 1000)</c>; the same roll always produces the same tier for the same
    /// strengths, so this stays fully deterministic given its inputs (ADR 0004).</summary>
    public static CombatOutcome ResolveOutcomeTier(Fixed64 attackerStrength, Fixed64 defenderStrength, uint varianceRoll)
    {
        var total = attackerStrength + defenderStrength;
        var ratio = total.RawValue == 0 ? Half : Fixed64.Divide(attackerStrength, total);

        var rollFraction = Fixed64.Divide(Fixed64.FromInt((int)Math.Min(varianceRoll, 999)), Fixed64.FromInt(1000));
        var variance = Fixed64.Multiply(rollFraction - Half, VarianceSpan);
        var adjusted = ratio + variance;
        if (adjusted < Fixed64.Zero) adjusted = Fixed64.Zero;
        if (adjusted > Fixed64.One) adjusted = Fixed64.One;

        if (adjusted >= DecisiveThreshold) return CombatOutcome.DecisiveVictory;
        if (adjusted >= CostlyThreshold) return CombatOutcome.CostlyVictory;
        if (adjusted >= StalemateThreshold) return CombatOutcome.RepulsedStalemate;
        if (adjusted >= DefeatThreshold) return CombatOutcome.Defeat;
        return CombatOutcome.CatastrophicDefeat;
    }

    /// <summary>The mirror-image tier for the other side of the same result (§4.4 step 3 is one roll,
    /// not two independent ones).</summary>
    public static CombatOutcome Mirror(CombatOutcome outcome) => outcome switch
    {
        CombatOutcome.DecisiveVictory => CombatOutcome.CatastrophicDefeat,
        CombatOutcome.CostlyVictory => CombatOutcome.Defeat,
        CombatOutcome.RepulsedStalemate => CombatOutcome.RepulsedStalemate,
        CombatOutcome.Defeat => CombatOutcome.CostlyVictory,
        CombatOutcome.CatastrophicDefeat => CombatOutcome.DecisiveVictory,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Unhandled combat outcome."),
    };

    /// <summary>§4.4 step 4: casualties/Readiness/Morale loss proportional to the outcome tier reached,
    /// keyed back to the caller's own group ordering. Every loss is capped by construction — a rate
    /// applied to a per-group stat can never exceed that stat.</summary>
    public static IReadOnlyList<CombatantGroupLoss> ResolveLosses(IReadOnlyList<CombatantGroup> groups, CombatOutcome outcome)
    {
        if (groups is null)
            throw new ArgumentNullException(nameof(groups));

        var casualtyRate = CasualtyRatePercent(outcome);
        var readinessRate = ReadinessLossRatePercent(outcome);
        var moraleRate = MoraleLossRatePercent(outcome);

        var losses = new CombatantGroupLoss[groups.Count];
        for (var index = 0; index < groups.Count; index++)
        {
            var group = groups[index];
            losses[index] = new CombatantGroupLoss(
                index,
                Math.Max(0, group.Manpower) * casualtyRate / 100,
                Math.Max(0, group.Readiness) * readinessRate / 100,
                Math.Max(0, group.Morale) * moraleRate / 100);
        }

        return losses;
    }

    private static Fixed64 EquipmentTierMultiplier(int tier) => Math.Clamp(tier, 0, 3) switch
    {
        0 => Fixed64.FromRaw(400_000),
        1 => Fixed64.FromRaw(700_000),
        2 => Fixed64.One,
        _ => Fixed64.FromRaw(1_300_000),
    };

    private static Fixed64 ConditionMultiplier(int readiness, int morale) => Fixed64.Divide(
        Fixed64.FromInt(Math.Clamp(readiness, 0, 100) + Math.Clamp(morale, 0, 100)), Fixed64.FromInt(200));

    private static int CasualtyRatePercent(CombatOutcome outcome) => outcome switch
    {
        CombatOutcome.DecisiveVictory => 3,
        CombatOutcome.CostlyVictory => 12,
        CombatOutcome.RepulsedStalemate => 8,
        CombatOutcome.Defeat => 20,
        CombatOutcome.CatastrophicDefeat => 40,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Unhandled combat outcome."),
    };

    private static int ReadinessLossRatePercent(CombatOutcome outcome) => outcome switch
    {
        CombatOutcome.DecisiveVictory => 5,
        CombatOutcome.CostlyVictory => 20,
        CombatOutcome.RepulsedStalemate => 15,
        CombatOutcome.Defeat => 30,
        CombatOutcome.CatastrophicDefeat => 50,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Unhandled combat outcome."),
    };

    private static int MoraleLossRatePercent(CombatOutcome outcome) => outcome switch
    {
        CombatOutcome.DecisiveVictory => 0,
        CombatOutcome.CostlyVictory => 15,
        CombatOutcome.RepulsedStalemate => 10,
        CombatOutcome.Defeat => 35,
        CombatOutcome.CatastrophicDefeat => 60,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Unhandled combat outcome."),
    };
}
