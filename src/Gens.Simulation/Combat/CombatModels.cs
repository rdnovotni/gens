#nullable enable
using System.Collections.Generic;
using Gens.Simulation.Numerics;

namespace Gens.Simulation.Combat;

/// <summary>Squad/Combatant types the shared kernel reads identically regardless of caller (design doc
/// §4.1). <see cref="Irregular"/> is the deliberately loose type built to cover pirates, bandits, and
/// gladiators for callers that have not yet plugged in.</summary>
public enum CombatantType { Militia, Auxiliary, Legionary, Cavalry, Siege, Irregular }

/// <summary>Terrain the kernel scores a <see cref="CombatantType"/>'s fit against (design doc §4.3).
/// Fortification is tracked separately, as <see cref="CombatSituation.Fortified"/>, since it is a
/// property of a side's position rather than of the terrain itself.</summary>
public enum CombatTerrain { Open, Hills, Forest, Coast, River }

/// <summary>The five outcome tiers every engagement resolves to, attacker- or defender-relative
/// depending on which side's <see cref="CombatResolution"/> field it appears in (design doc §4.5).</summary>
public enum CombatOutcome { DecisiveVictory, CostlyVictory, RepulsedStalemate, Defeat, CatastrophicDefeat }

/// <summary>One caller-defined unit of manpower — a military Squad, a raid party, a single duelist, or
/// a gladiator pairing all project into this same shape (design doc §4.1: "a lighter engagement... can
/// resolve against a single Combatant or a handful, using the same engine at a smaller scale rather
/// than a different one"). The kernel never references a caller's own domain type (e.g. <c>Squad</c>)
/// directly, which is what lets military, guards, raids, duels, and spectacle all share it without a
/// dependency on any one of them.</summary>
public readonly record struct CombatantGroup(
    CombatantType Type,
    int Manpower,
    int EquipmentTier,
    int Readiness,
    int Morale);

/// <summary>The commander weighting the kernel applies to a side's effective strength (design doc
/// §4.2). A caller derives <see cref="EffectivenessMultiplier"/> from whatever inputs it has available
/// — today, a military commander's Martial attribute; a future caller could fold in Traits or
/// Personality Axes without any change here, since the kernel only ever sees the resulting number.
/// <c>null</c> on <see cref="CombatSide"/> means no commander was assigned, which <see
/// cref="CombatResolutionCalculator"/> reads as a flat unfavorable default rather than a neutral one
/// (§4.2: "a Force without one resolves against a flat, unfavorable default... which is deliberate").</summary>
public readonly record struct CombatantCommander(Fixed64 EffectivenessMultiplier);

/// <summary>Terrain and situational modifiers for one side of an engagement (design doc §4.3).
/// <see cref="Ambush"/> and <see cref="Fortified"/> are each a property of the side they favor — an
/// ambushing attacker sets its own <see cref="Ambush"/>, a defender behind walls sets its own
/// <see cref="Fortified"/> — not a single engagement-wide flag.</summary>
public readonly record struct CombatSituation(CombatTerrain Terrain, bool Ambush, bool Fortified);

/// <summary>One side's full input to the kernel (design doc §4.1's Force/Fleet/ad-hoc-Combatant-set,
/// unified): its <see cref="Groups"/>, an optional <see cref="Commander"/>, and its own <see
/// cref="Situation"/>.</summary>
public sealed record CombatSide(
    IReadOnlyList<CombatantGroup> Groups,
    CombatantCommander? Commander,
    CombatSituation Situation);

/// <summary>Losses for one <see cref="CombatSide"/>'s group, keyed back to the caller's own ordering
/// (<see cref="GroupIndex"/> is the index into the <see cref="CombatSide.Groups"/> list the caller
/// supplied) so the kernel never needs to know the caller's own id type (e.g. <c>RuntimeId&lt;Squad&gt;</c>).</summary>
public readonly record struct CombatantGroupLoss(
    int GroupIndex,
    int Casualties,
    int ReadinessLoss,
    int MoraleLoss);

/// <summary>The kernel's full output for one resolved engagement (design doc §4.4's five resolution
/// steps, steps 3-5). <see cref="AttackerOutcome"/> and <see cref="DefenderOutcome"/> are always
/// mirror images of the same underlying result — never independently rolled — matching §4.5's "no
/// resolution" framing for <see cref="CombatOutcome.RepulsedStalemate"/>.</summary>
public sealed record CombatResolution(
    CombatOutcome AttackerOutcome,
    CombatOutcome DefenderOutcome,
    IReadOnlyList<CombatantGroupLoss> AttackerLosses,
    IReadOnlyList<CombatantGroupLoss> DefenderLosses);
