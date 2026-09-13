using Gens.Simulation.Buildings;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;

namespace Gens.Simulation.Tests.Education;

/// <summary>Shared Education-domain test scaffolding for the operational-delivering-building gate
/// (Phase 17 item 2 correctness fix's new <see cref="Gens.Simulation.Education.EducationBuildingResolver"/>),
/// used by both <see cref="Gens.Simulation.Education.StartEducationalTrackCommand"/> and <see
/// cref="Gens.Simulation.Education.RenownAttractsRenownSystem"/> test coverage.</summary>
internal static class EducationTestFixtures
{
    /// <summary>Builds one operational (Pristine, no staffing requirement so it is immediately
    /// operational) building of <paramref name="buildingId"/> on a fresh Plot inside a fresh Holding
    /// occupied by <paramref name="householdId"/> — the minimum <see
    /// cref="Gens.Simulation.Education.EducationBuildingResolver.HasOperationalBuilding"/> needs to see,
    /// mirroring <c>EstateSettlementQueryTests</c>'s own Holding/Plot/Building scaffolding.</summary>
    public static void AddOperationalBuilding(WorldState state, RuntimeId<Household> householdId, DefinitionId<Building> buildingId)
    {
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, state.RegionIds.Issue()));

        var holdingId = state.HoldingIds.Issue();
        state.Holdings.Add(holdingId, Holding.Create(
            holdingId, settlementId, occupantId: householdId.ToTaggedString(), residentCapacity: 6));

        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(
            plotId, settlementId, terrain: TerrainType.Hills, occupyingHoldingId: holdingId));

        var definition = new BuildingDefinition(buildingId, BuildingTier.Tier1, constructionMonths: 1, plotCapacity: 1);
        var runtimeBuildingId = state.BuildingIds.Issue();
        state.Buildings.Add(runtimeBuildingId, new BuildingInstance(runtimeBuildingId, plotId, definition));
    }
}
