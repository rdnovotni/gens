using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
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
/// One autonomous decision a steward or Council made on the player's behalf (Phase 10 items 2/10/11;
/// §10's <c>AutonomousDecisionLog</c> data model). <see cref="CompetenceRollFactor"/> and <see
/// cref="LoyaltyRiskRollFactor"/> are the percent-chance figures <see
/// cref="StewardAutonomousDecisionSystem"/> actually rolled against that month (package 11); an
/// incident and its <see cref="TreasuryImpact"/>, when one occurs, are never revealed to the player as
/// they happen — only <see cref="ReturnReport"/> (built once the assignment ends) surfaces them, per
/// §8's "dramatic reveal on return."
/// </summary>
/// <param name="DecisionType">The chosen <see cref="Actions.ActionDefinition.Id"/>'s <see
/// cref="Identity.DefinitionId{T}.Value"/>, or <c>"none"</c> when the steward held rather than acted
/// this month (no eligible, autonomy-permitted action was found, or a competence-roll fumble).</param>
/// <param name="Outcome">A short human-readable summary — the <see
/// cref="Actions.ActionResultProjection.Summary"/> the chosen action's own projection produced, or a
/// fixed "held" string.</param>
/// <param name="TreasuryImpact">This month's net Treasury effect from the decision itself (positive =
/// a spend like Fund Festival) plus any <see cref="IncidentType"/> loss (always a deduction) — summed
/// by <see cref="ReturnReportGenerator"/> into the assignment's total.</param>
public sealed record AutonomousDecisionLog(
    RuntimeId<AutonomousDecisionLog> LogId,
    RuntimeId<StewardshipAssignment> AssignmentId,
    GameDate Month,
    string DecisionType,
    string Outcome,
    int CompetenceRollFactor,
    int LoyaltyRiskRollFactor,
    StewardIncidentType? IncidentType = null,
    Money TreasuryImpact = default);
