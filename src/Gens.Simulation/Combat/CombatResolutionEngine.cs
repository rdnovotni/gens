#nullable enable
using System;
using Gens.Simulation.Random;

namespace Gens.Simulation.Combat;

/// <summary>The shared Combat Resolution Engine (design doc §4) — "usable by military, guards, raids,
/// duels, and spectacle without giving each a separate damage model" (build roadmap, Phase 16 item 4).
/// Orchestration only: assembling both sides (§4.4 step 1) is the caller's own job, projecting whatever
/// domain model it has (a military <c>Squad</c>, a raid party, a duelist) into <see
/// cref="CombatantGroup"/>s. This engine never touches <c>WorldState</c>, issues no
/// <c>Gens.Simulation.Commands.IDomainEvent</c>, and submits no command of its own — it is a plain library call any
/// consumer's own command pipeline can invoke from its <c>mutate</c> step (the same "a command's mutate
/// closing over an injected <see cref="RandomStreamSet"/>" pattern
/// <c>Gens.Simulation.Religion.RespondToOmenCommands.CreatePipeline</c> already established), so adding
/// a new consumer never means teaching this engine about that consumer's own state shape.</summary>
public static class CombatResolutionEngine
{
    /// <summary>§4.4 steps 2-5 in one call: computes both sides' effective strength, draws one variance
    /// roll from <paramref name="streamName"/>, resolves the outcome tier and both sides' losses. Each
    /// consumer reserves its own <paramref name="streamName"/> (rule 8, ADR 0004) so one consumer's
    /// engagements never perturb another's random draws.</summary>
    public static CombatResolution Resolve(CombatSide attacker, CombatSide defender, RandomStreamSet randomStreams, string streamName)
    {
        if (attacker is null)
            throw new ArgumentNullException(nameof(attacker));
        if (defender is null)
            throw new ArgumentNullException(nameof(defender));
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));

        var attackerStrength = CombatResolutionCalculator.EffectiveStrength(attacker);
        var defenderStrength = CombatResolutionCalculator.EffectiveStrength(defender);
        var varianceRoll = randomStreams.NextUInt(streamName, 1000);

        var attackerOutcome = CombatResolutionCalculator.ResolveOutcomeTier(attackerStrength, defenderStrength, varianceRoll);
        var defenderOutcome = CombatResolutionCalculator.Mirror(attackerOutcome);

        return new CombatResolution(
            attackerOutcome,
            defenderOutcome,
            CombatResolutionCalculator.ResolveLosses(attacker.Groups, attackerOutcome),
            CombatResolutionCalculator.ResolveLosses(defender.Groups, defenderOutcome));
    }
}
