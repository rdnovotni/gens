using Gens.Simulation.Commands;
using Gens.Simulation.Buildings;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PrivateInfrastructure;

public sealed record ConnectedEstateBonusAppliedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    int PlotCount,
    Money TotalBonus) : IDomainEvent
{
    public string Type => "privateInfrastructure.connectedEstateBonusApplied";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>§4.1's "genuine, visible achievement... a real, Chronicle-worthy milestone" — fired exactly
/// once per household the first month <see cref="RoadClusterQuery.ComputeClusters"/> finds a cluster
/// covering the household's own entire owned-Plot set. <see cref="Chronicle.ChronicleProjector"/> gains
/// a matching case for this event, per that type's own "every system that already flagged something as
/// Chronicle-worthy... is the actual generation source" convention.</summary>
public sealed record UnifiedEstateAchievedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    int PlotCount) : IDomainEvent
{
    public string Type => "privateInfrastructure.unifiedEstateAchieved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>
/// §2.1/§4's real, monthly benefits (Phase 15 item 7), matching <see
/// cref="RealEstate.AdministrativeBurdenSystem"/>'s and every other Phase 15 static-<c>Tick</c>
/// system's identical "no central <c>IMonthlySystem</c> pipeline registry exists anywhere in this
/// codebase for any Phase 15 system to join" convention (<c>PublicContracts.LustrumSystem</c>'s own
/// progress-note wording) — exercised directly by this item's own tests, not registered anywhere.
///
/// §2.1's Trade-Proximity bonus (a real Ledger income line, per <see
/// cref="PrivateInfrastructureCatalog.TradeProximityMonthlyBonus"/>'s own doc comment on why this item
/// posts real income rather than modifying a non-existent "Trade Route effectiveness" figure) applies to
/// every owned Plot that is itself River-adjacent or Coast-adjacent, or that shares an operational <see
/// cref="RoadClusterQuery"/> cluster with one that is. §4's Connected Estate bonus applies once per
/// qualifying cluster, scaled by the count of <see cref="BuildingInstance"/>s standing on that cluster's
/// own Plots. §4.1's Unified Estate milestone is checked last, against the same month's own clusters.
/// </summary>
public static class PrivateInfrastructureBenefitsSystem
{
    public static IReadOnlyList<IDomainEvent> Tick(WorldState state, GameDate date)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var householdIds = RoadClusterQuery.OwnedHouseholdIds(state);

        foreach (var householdId in householdIds)
        {
            var clusters = RoadClusterQuery.ComputeClusters(state, householdId);
            var ownedPlotIds = RoadClusterQuery.OwnedPlotIds(state, householdId);

            ApplyTradeProximity(state, date, householdId, ownedPlotIds, clusters, events);
            ApplyConnectedEstateBonus(state, date, householdId, clusters, events);
            ApplyUnifiedEstateMilestone(state, date, householdId, clusters, events);
        }

        return events;
    }

    private static void ApplyTradeProximity(
        WorldState state, GameDate date, RuntimeId<Household> householdId, HashSet<RuntimeId<Plot>> ownedPlotIds,
        IReadOnlyList<RoadClusterView> clusters, List<IDomainEvent> events)
    {
        var directlyTradeAdjacent = ownedPlotIds.Where(id => IsTradeAdjacent(state, id)).ToHashSet();

        var qualifyingPlotIds = new HashSet<RuntimeId<Plot>>(directlyTradeAdjacent);
        foreach (var cluster in clusters)
        {
            if (cluster.PlotIds.Any(directlyTradeAdjacent.Contains))
                foreach (var plotId in cluster.PlotIds)
                    if (InfrastructureConditionResolver.IsOperational(state, PavedRoadKeyFor(state, plotId, cluster)))
                        qualifyingPlotIds.Add(plotId);
        }

        if (qualifyingPlotIds.Count == 0)
            return;

        var bonus = PrivateInfrastructureCatalog.TradeProximityMonthlyBonus.Scale(Fixed64.FromInt(qualifyingPlotIds.Count));
        if (bonus == Money.Zero)
            return;

        events.Add(LedgerService.Post(
            state, date, LedgerTransactionCategory.Sales,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.Mint, -bonus),
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), bonus),
            },
            reference: $"privateInfrastructure.tradeProximity:{householdId.ToTaggedString()}"));
    }

    private static bool IsTradeAdjacent(WorldState state, RuntimeId<Plot> plotId) =>
        state.Plots.TryGet(plotId, out var plot) &&
        (plot!.Terrain is TerrainType.River or TerrainType.Coast ||
         plot.Features.HasFlag(TerrainFeature.RiverAdjacent) || plot.Features.HasFlag(TerrainFeature.Coastline));

    /// <summary>A cluster's own Trade-Proximity reach requires at least one still-operational Paved Road
    /// segment actually reaching the qualifying Plot — a wholly neglected connection (§8) no longer
    /// carries the bonus, matching <see cref="InfrastructureConditionResolver.IsOperational"/>'s own
    /// binary lapse reading. Any one operational edge touching the Plot is enough; this reads the
    /// cluster's first such edge rather than requiring every edge in a large cluster to be pristine.</summary>
    private static InfrastructureConditionKey PavedRoadKeyFor(WorldState state, RuntimeId<Plot> plotId, RoadClusterView cluster)
    {
        var edge = state.PavedRoadConnections.InAscendingOrder()
            .Select(entry => entry.Value)
            .FirstOrDefault(e => cluster.PlotIds.Contains(e.PlotAId) && cluster.PlotIds.Contains(e.PlotBId) &&
                (e.PlotAId == plotId || e.PlotBId == plotId));
        return edge is null
            ? new InfrastructureConditionKey(InfrastructureStructureType.PavedRoad, "none")
            : edge.ConditionKey;
    }

    private static void ApplyConnectedEstateBonus(
        WorldState state, GameDate date, RuntimeId<Household> householdId, IReadOnlyList<RoadClusterView> clusters,
        List<IDomainEvent> events)
    {
        foreach (var cluster in clusters)
        {
            if (!cluster.ConnectedEstateBonusActive)
                continue;

            var plotIdSet = cluster.PlotIds.ToHashSet();
            var buildingCount = state.Buildings.InAscendingOrder().Count(entry => plotIdSet.Contains(entry.Value.PlotId));
            if (buildingCount == 0)
                continue;

            var bonus = PrivateInfrastructureCatalog.ConnectedEstateBonusPerBuilding.Scale(Fixed64.FromInt(buildingCount));
            if (bonus == Money.Zero)
                continue;

            events.Add(LedgerService.Post(
                state, date, LedgerTransactionCategory.Sales,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.Mint, -bonus),
                    new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), bonus),
                },
                reference: $"privateInfrastructure.connectedEstate:{householdId.ToTaggedString()}"));
            events.Add(new ConnectedEstateBonusAppliedEvent(state.EventIds.Issue(), date, householdId, cluster.PlotIds.Count, bonus));
        }
    }

    private static void ApplyUnifiedEstateMilestone(
        WorldState state, GameDate date, RuntimeId<Household> householdId, IReadOnlyList<RoadClusterView> clusters,
        List<IDomainEvent> events)
    {
        if (state.UnifiedEstateMilestones.TryGet(householdId, out _))
            return;
        var unified = clusters.FirstOrDefault(c => c.IsUnifiedEstate);
        if (unified.PlotIds is null)
            return;

        state.UnifiedEstateMilestones.Add(householdId, date);
        events.Add(new UnifiedEstateAchievedEvent(state.EventIds.Issue(), date, householdId, unified.PlotIds.Count));
    }
}
