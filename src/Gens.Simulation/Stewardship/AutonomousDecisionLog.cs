using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Stewardship;

/// <summary>Why a steward's unsupervised Treasury access went wrong (§6): the three severities the
/// design doc names, from least to most severe.</summary>
public enum StewardIncidentType
{
    Skimming,
    Embezzlement,
    ActiveSabotage,
}

/// <summary>
/// One autonomous decision a steward or Council made on the player's behalf (Phase 10 items 2/10;
/// §10's <c>AutonomousDecisionLog</c> data model). <see cref="CompetenceRollFactor"/>, <see
/// cref="LoyaltyRiskRollFactor"/>, and <see cref="IncidentType"/> are this package's placeholder shape
/// only — <see cref="StewardAutonomousDecisionSystem"/> (package 10) always writes them as <c>0</c>/
/// <c>null</c>; wiring the steward's actual Stewardship attribute and Loyalty condition into real rolls
/// is package 11's own scope, matching <see cref="State.KnowledgeState"/>'s identical "the storage
/// shape lands before its real producer does" precedent from Phase 2.
/// </summary>
/// <param name="DecisionType">The chosen <see cref="Actions.ActionDefinition.Id"/>'s <see
/// cref="Identity.DefinitionId{T}.Value"/>, or <c>"none"</c> when the steward held rather than acted
/// this month (no eligible, autonomy-permitted action was found, or the act-consideration simply
/// didn't fire).</param>
/// <param name="Outcome">A short human-readable summary — the <see
/// cref="Actions.ActionResultProjection.Summary"/> the chosen action's own projection produced, or a
/// fixed "held" string.</param>
public sealed record AutonomousDecisionLog(
    RuntimeId<AutonomousDecisionLog> LogId,
    RuntimeId<StewardshipAssignment> AssignmentId,
    GameDate Month,
    string DecisionType,
    string Outcome,
    int CompetenceRollFactor,
    int LoyaltyRiskRollFactor,
    StewardIncidentType? IncidentType = null);
