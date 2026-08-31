using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Land;

/// <summary>
/// The monthly system that keeps every <see cref="DistantHolding"/>'s <see
/// cref="DistantHolding.MismanagementRiskActive"/> flag and cached <see
/// cref="DistantHolding.ProcuratorCharacterId"/> current (§7.2; Phase 13 item 7). Two responsibilities,
/// both per holding, per month:
///
/// <list type="number">
/// <item>If a Procurator is on record but their backing <see
/// cref="StewardshipContext.SecondSettlementProcurator"/> <see cref="StewardshipAssignment"/> is no
/// longer active (the household's graver need — a Regency, per <see
/// cref="Succession.RegencySystem"/>'s own supersede precedent — already ended it) the holding reverts
/// to unstaffed rather than keeping a stale pointer to an appointee who no longer actually holds the
/// role. If instead the assignment is still formally active but the Procurator themself has died, this
/// system ends that assignment itself, through <see cref="StewardshipCommands.EndPipeline"/> exactly
/// like <see cref="Succession.RegencySystem"/>'s own identical "supersede via the real command, don't
/// just drop the pointer" pattern — leaving it active would both block
/// <see cref="AppointProcuratorCommand"/>'s "household already has an active assignment" check forever
/// and let <see cref="StewardAutonomousDecisionSystem"/> keep acting for a dead appointee.</item>
/// <item>Recompute <see cref="DistantHolding.MismanagementRiskActive"/> per §7.2/§12: true exactly when
/// the holding is <see cref="DistanceTier.Far"/> and either unstaffed or the current Procurator's <see
/// cref="Characters.Condition.Loyalty"/> has fallen below <see
/// cref="StewardIncidentCatalog.LoyaltyRiskThreshold"/>. A Near or Moderate holding is never at risk
/// regardless of staffing (§7.2's own "a Near second holding wouldn't" contrast).</item>
/// </list>
///
/// What actually happens *while* the risk flag is active — skimming, drift, an eventual disloyal-
/// Procurator incident — is deliberately left unbuilt: §11's own open question names "Disloyal
/// Procurator/Senior Position consequences" and "Procurator autonomy boundary" as unresolved, so this
/// system only surfaces the risk state honestly rather than fabricating an incident mechanic the design
/// corpus hasn't sized (matching this codebase's standing "name the gap, don't invent past it"
/// convention). Draws no random numbers: every check here is a deterministic Loyalty/liveness/
/// assignment-state comparison.
/// </summary>
public sealed class DistantHoldingMismanagementRiskSystem : IMonthlySystem<WorldState>
{
    public string Id => "land.distantHoldingMismanagementRisk";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "distantHoldings", "stewardshipAssignments", "characters" };

    // "stewardshipAssignments", "returnReports", "returnReportIds", "eventIds", "commandIds", and
    // "commandSequence" cover every partition StewardshipCommands.EndPipeline's own mutate handler can
    // touch when this system ends a dead Procurator's backing assignment — mirrors RegencySystem.Writes's
    // own doc comment for why ADR 0005's declared write-set must name these, not just "distantHoldings".
    public IReadOnlyCollection<string> Writes { get; } = new[]
    {
        "distantHoldings", "stewardshipAssignments", "returnReports", "returnReportIds", "eventIds",
        "commandIds", "commandSequence",
    };

    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "succession.regency" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var holdings = state.DistantHoldings.InAscendingOrder().ToArray();

        foreach (var (id, holding) in holdings)
        {
            RuntimeId<Character>? procuratorId = null;
            Character? procurator = null;

            if (holding.ProcuratorCharacterId is { } candidateId)
            {
                var backingAssignment = state.StewardshipAssignments.InAscendingOrder()
                    .Select(entry => entry.Value)
                    .FirstOrDefault(a => a.HouseholdId == holding.HouseholdId && a.IsActive &&
                        a.Context == StewardshipContext.SecondSettlementProcurator && a.AppointeeCharacterId == candidateId);

                var candidateAlive = state.Characters.TryGet(candidateId, out var candidate) && candidate.IsAlive;

                if (backingAssignment is not null && !candidateAlive)
                {
                    var endCommand = new EndStewardshipAssignmentCommand(
                        state.CommandIds.Issue(), "system", context.Date, null, backingAssignment.AssignmentId);
                    var endResult = StewardshipCommands.EndPipeline.Execute(state, endCommand);
                    if (endResult.Accepted)
                        events.AddRange(endResult.Events);
                }
                else if (backingAssignment is not null)
                {
                    procuratorId = candidateId;
                    procurator = candidate;
                }
            }

            var riskActive = EvaluateRisk(holding.DistanceTier, procurator);

            if (procuratorId == holding.ProcuratorCharacterId && riskActive == holding.MismanagementRiskActive)
                continue;

            state.DistantHoldings.Remove(id);
            state.DistantHoldings.Add(id, holding with
            {
                ProcuratorCharacterId = procuratorId,
                MismanagementRiskActive = riskActive,
            });
        }

        return events;
    }

    /// <summary>§7.2/§12's mismanagement-risk rule, shared with <see
    /// cref="AppointProcuratorCommand"/>'s own immediate recompute so the two paths never drift apart.
    /// <paramref name="procurator"/> is <c>null</c> for an unstaffed holding.</summary>
    public static bool EvaluateRisk(DistanceTier distanceTier, Character? procurator)
    {
        if (distanceTier != DistanceTier.Far)
            return false;

        return procurator is null || procurator.Condition.Loyalty < StewardIncidentCatalog.LoyaltyRiskThreshold;
    }
}
