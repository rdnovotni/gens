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
/// longer active (they died, or the household's graver need — a Regency, per <see
/// cref="Succession.RegencySystem"/>'s own supersede precedent — ended it) the holding reverts to
/// unstaffed rather than keeping a stale pointer to an appointee who no longer actually holds the
/// role.</item>
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
    public IReadOnlyCollection<string> Writes { get; } = new[] { "distantHoldings" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "succession.regency" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var holdings = state.DistantHoldings.InAscendingOrder().ToArray();

        foreach (var (id, holding) in holdings)
        {
            RuntimeId<Character>? procuratorId = null;
            Character? procurator = null;

            if (holding.ProcuratorCharacterId is { } candidateId &&
                state.Characters.TryGet(candidateId, out var candidate) &&
                candidate.IsAlive &&
                state.StewardshipAssignments.InAscendingOrder().Any(entry =>
                    entry.Value.HouseholdId == holding.HouseholdId && entry.Value.IsActive &&
                    entry.Value.Context == StewardshipContext.SecondSettlementProcurator &&
                    entry.Value.AppointeeCharacterId == candidateId))
            {
                procuratorId = candidateId;
                procurator = candidate;
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

        return Array.Empty<IDomainEvent>();
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
