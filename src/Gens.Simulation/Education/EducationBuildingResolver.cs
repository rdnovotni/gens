using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Buildings;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;

namespace Gens.Simulation.Education;

/// <summary>
/// Household-to-building lookups for the Education domain (Phase 17 item 2 correctness fix): whether a
/// household actually owns or occupies an operational building that can deliver something — an
/// Educational Track (§3), or (per <see cref="RenownAttractsRenownSystem"/>'s own separate use of this
/// same walk) an Academia/Library for §12's Renown Attracts Renown check. Walks Household → <see
/// cref="Holding"/> → <see cref="Plot"/> → <see cref="BuildingInstance"/> the same way <see
/// cref="Queries.EstateSettlementQuery"/> already does for the estate/settlement screen — no dedicated
/// household-to-building index exists anywhere in this codebase yet, matching that query's own linear-scan
/// precedent for a household's own (typically small) holding count.
/// </summary>
public static class EducationBuildingResolver
{
    /// <summary>True when <paramref name="householdId"/> owns or occupies at least one <see
    /// cref="BuildingInstance.IsOperational"/> building whose <see cref="BuildingDefinition.Id"/> is one
    /// of <paramref name="deliveringBuildingIds"/> — built, not <see cref="BuildingCondition.Ruined"/>,
    /// and staffed per its own <see cref="BuildingDefinition.StaffingSlots"/> (<see
    /// cref="BuildingInstance.IsOperational"/>'s own definition covers all three).</summary>
    public static bool HasOperationalBuilding(
        WorldState state, RuntimeId<Household> householdId, IReadOnlyCollection<DefinitionId<Building>> deliveringBuildingIds)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (deliveringBuildingIds is null)
            throw new ArgumentNullException(nameof(deliveringBuildingIds));

        var householdTag = householdId.ToTaggedString();
        var holdingIds = new HashSet<RuntimeId<Holding>>();
        foreach (var entry in state.Holdings.InAscendingOrder())
        {
            var holding = entry.Value;
            if (holding.OccupantId == householdTag || holding.OwnerId == householdTag)
                holdingIds.Add(holding.Id);
        }

        if (holdingIds.Count == 0)
            return false;

        var plotIds = new HashSet<RuntimeId<Plot>>();
        foreach (var entry in state.Plots.InAscendingOrder())
        {
            if (entry.Value.OccupyingHoldingId is { } occupyingHoldingId && holdingIds.Contains(occupyingHoldingId))
                plotIds.Add(entry.Key);
        }

        if (plotIds.Count == 0)
            return false;

        foreach (var entry in state.Buildings.InAscendingOrder())
        {
            var building = entry.Value;
            if (!plotIds.Contains(building.PlotId))
                continue;
            if (!deliveringBuildingIds.Contains(building.Definition.Id))
                continue;
            if (building.IsOperational)
                return true;
        }

        return false;
    }
}
