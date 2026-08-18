using Gens.Simulation.Buildings;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Villas;

namespace Gens.Simulation.Queries;

/// <summary>One completed building's row within its parent plot (Phase 9 item 6's estate/settlement
/// screen).</summary>
public readonly record struct EstateBuildingRow(
    string BuildingId,
    string DefinitionKey,
    BuildingTier Tier,
    BuildingCondition Condition,
    bool IsOperational);

/// <summary>One plot occupied by the household's holding, with the buildings standing on it.</summary>
public readonly record struct EstatePlotRow(
    string PlotId,
    TerrainType Terrain,
    LandCondition Condition,
    bool IsContested,
    IReadOnlyList<EstateBuildingRow> Buildings);

/// <summary>One holding the household owns or occupies, with its villa stage (if any) and the plots
/// it holds.</summary>
public readonly record struct EstateHoldingRow(
    string HoldingId,
    VillaStage? VillaStage,
    int ResidentCapacity,
    IReadOnlyList<EstatePlotRow> Plots);

/// <summary>The estate/settlement screen's projection (Phase 9 item 6): the settlement's current
/// growth stage plus every holding the caller-specified household occupies within it, each with its
/// plots and buildings. <c>gens-core-design.md</c> §7.4 assigns this screen the diptych's tile-map
/// left leaf and grouped-building-chains right leaf; this projection carries the right leaf's data —
/// a tile-coordinate map layer is presentation-only and belongs to the Unity adapter, not this
/// projection.</summary>
public readonly record struct EstateSettlementProjection(
    string SettlementId,
    SettlementStage SettlementStage,
    IReadOnlyList<EstateHoldingRow> Holdings);

/// <summary>Projects the caller-specified settlement's holdings scoped to one household — mirroring
/// <see cref="HouseholdRosterQuery"/>'s per-household scoping, since a settlement can host holdings
/// belonging to other households (rivals, Phase 10) that this screen does not show. No <see
/// cref="KnowledgeState"/> filtering yet, matching <see cref="HouseholdRosterQuery"/>'s own
/// precedent.</summary>
public sealed class EstateSettlementQuery : IWorldQuery<EstateSettlementProjection>
{
    private readonly RuntimeId<Settlement> _settlementId;
    private readonly RuntimeId<Household> _householdId;

    public EstateSettlementQuery(RuntimeId<Settlement> settlementId, RuntimeId<Household> householdId)
    {
        _settlementId = settlementId;
        _householdId = householdId;
    }

    public EstateSettlementProjection Execute(WorldState state, string observerId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (!state.Settlements.TryGet(_settlementId, out var settlement))
            throw new KeyNotFoundException($"No Settlement with ID '{_settlementId}' exists.");

        var householdTag = _householdId.ToTaggedString();
        var holdingRows = new List<EstateHoldingRow>();

        foreach (var holdingEntry in state.Holdings.InAscendingOrder())
        {
            var holding = holdingEntry.Value;
            if (holding.SettlementId != _settlementId)
                continue;
            if (holding.OccupantId != householdTag && holding.OwnerId != householdTag)
                continue;

            var plotRows = new List<EstatePlotRow>();
            foreach (var plotEntry in state.Plots.InAscendingOrder())
            {
                var plot = plotEntry.Value;
                if (plot.OccupyingHoldingId != holding.Id)
                    continue;

                var buildingRows = new List<EstateBuildingRow>();
                foreach (var buildingEntry in state.Buildings.InAscendingOrder())
                {
                    var building = buildingEntry.Value;
                    if (building.PlotId != plot.Id)
                        continue;

                    buildingRows.Add(new EstateBuildingRow(
                        BuildingId: building.Id.ToTaggedString(),
                        DefinitionKey: building.Definition.Id.Value,
                        Tier: building.Definition.Tier,
                        Condition: building.Condition,
                        IsOperational: building.IsOperational));
                }

                plotRows.Add(new EstatePlotRow(
                    PlotId: plot.Id.ToTaggedString(),
                    Terrain: plot.Terrain,
                    Condition: plot.Condition,
                    IsContested: plot.IsContested,
                    Buildings: buildingRows));
            }

            holdingRows.Add(new EstateHoldingRow(
                HoldingId: holding.Id.ToTaggedString(),
                VillaStage: holding.Villa?.Stage,
                ResidentCapacity: holding.ResidentCapacity,
                Plots: plotRows));
        }

        return new EstateSettlementProjection(
            SettlementId: _settlementId.ToTaggedString(),
            SettlementStage: settlement.Stage,
            Holdings: holdingRows);
    }
}
