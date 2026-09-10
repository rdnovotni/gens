#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Military;
using Gens.Simulation.State;

namespace Gens.Simulation.Queries;

public readonly record struct SquadProjection(RuntimeId<Squad> SquadId, string Name, SquadType Type,
    int Manpower, int EquipmentTier, int Readiness, int Morale, SquadStatus Status, string LocationKind);

public readonly record struct MilitaryForceProjection(RuntimeId<Settlement> SettlementId, RuntimeId<Household> HouseholdId,
    ForceInfrastructureTier InfrastructureTier, int SquadCap, IReadOnlyList<SquadProjection> Squads,
    IReadOnlyList<RuntimeId<MilitaryDeployment>> ActiveDeploymentIds);

/// <summary>Presentation-ready snapshot of a household's own persistent force.</summary>
public sealed class MilitaryForceQuery : IWorldQuery<MilitaryForceProjection?>
{
    public MilitaryForceProjection? Execute(WorldState state, string observerId)
    {
        if (state is null) throw new ArgumentNullException(nameof(state));
        if (observerId is null) throw new ArgumentNullException(nameof(observerId));
        var householdId = RuntimeId<Household>.Parse(observerId);
        var forceEntry = state.EstateForces.InAscendingOrder().FirstOrDefault(entry => entry.Value.HouseholdId == householdId);
        if (forceEntry.Value is null) return null;
        var force = forceEntry.Value;
        var squads = state.Squads.InAscendingOrder().Where(entry => entry.Value.ForceSettlementId == force.SettlementId)
            .Select(entry => new SquadProjection(entry.Key, entry.Value.Name, entry.Value.Type, entry.Value.Manpower,
                entry.Value.EquipmentTier, entry.Value.Readiness, entry.Value.Morale, entry.Value.Status,
                entry.Value.Location.Kind.ToString())).ToArray();
        var deployments = state.MilitaryDeployments.InAscendingOrder()
            .Where(entry => entry.Value.ForceSettlementId == force.SettlementId && entry.Value.Status == MilitaryDeploymentStatus.Active)
            .Select(entry => entry.Key).ToArray();
        return new MilitaryForceProjection(force.SettlementId, force.HouseholdId, force.InfrastructureTier,
            force.SquadCap, squads, deployments);
    }
}
