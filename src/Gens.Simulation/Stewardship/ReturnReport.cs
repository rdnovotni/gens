using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;

namespace Gens.Simulation.Stewardship;

/// <summary>
/// The compiled account the player receives when a <see cref="StewardshipAssignment"/> ends — Travel's
/// return, or a Regency's natural end (Phase 10 package 13; §8's <c>ReturnReport</c> data model:
/// "mirroring Events' own Monthly Report pattern... a genuine narrative summary"). Built once, inside
/// <see cref="StewardshipCommands.MutateEnd"/>, by folding every <see cref="AutonomousDecisionLog"/>
/// recorded for the ending assignment — matching this codebase's "reuse the projection's own summary"
/// convention rather than authoring new prose (<see cref="AutonomousDecisionLog.Outcome"/>'s identical
/// precedent).
/// </summary>
/// <param name="SummaryEntries">Each covered month's own <see cref="AutonomousDecisionLog.Outcome"/>
/// string, in chronological order — the "genuine narrative summary" §8 calls for, reusing text each
/// log entry already produced rather than generating new prose.</param>
/// <param name="TotalTreasuryImpact">The signed sum of every ledger posting tied to an incident
/// discovered during this assignment (negative: money the household lost to Skimming/Embezzlement).
/// Active Sabotage carries no Treasury posting of its own (§6: it perturbs a Standing Policy, not
/// money) and so does not contribute here.</param>
/// <param name="IncidentsDiscovered">The <see cref="AutonomousDecisionLog.LogId"/> of every log entry
/// in this assignment whose <see cref="AutonomousDecisionLog.IncidentType"/> is non-null — referenced
/// by id rather than embedded, matching how detail rows elsewhere in this codebase are referenced
/// rather than duplicated inline.</param>
/// <param name="ChronicleWorthy">True when this absence/regency is "a genuinely well-run one, or a
/// genuinely disastrous one" (§8) — any incident discovered, or a single incident's own impact clearing
/// <see cref="StewardIncidentCatalog.ChronicleWorthyTreasuryImpactDenarii"/>.</param>
public sealed record ReturnReport(
    RuntimeId<ReturnReport> ReportId,
    RuntimeId<StewardshipAssignment> AssignmentId,
    IReadOnlyList<string> SummaryEntries,
    Money TotalTreasuryImpact,
    IReadOnlyList<RuntimeId<AutonomousDecisionLog>> IncidentsDiscovered,
    bool ChronicleWorthy);
