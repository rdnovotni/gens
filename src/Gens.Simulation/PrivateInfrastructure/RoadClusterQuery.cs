using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;

namespace Gens.Simulation.PrivateInfrastructure;

/// <summary>§4/§10's <c>RoadCluster</c> — a connected component of <see cref="PavedRoadConnection"/>
/// edges belonging to one household, matching <see cref="MerchantFamilies.EquestrianStatusQuery"/>'s
/// own "computed, not stored" precedent rather than a redundant, incrementally-maintained record: since
/// every edge is already real, persisted state, a cluster is a pure derivation of it, not a second copy
/// that could drift out of sync. <see cref="ConnectedEstateBonusActive"/> and <see cref="IsUnifiedEstate"/>
/// read directly off §4/§4.1's own thresholds.</summary>
public readonly record struct RoadClusterView(
    IReadOnlyList<RuntimeId<Plot>> PlotIds, bool ConnectedEstateBonusActive, bool IsUnifiedEstate)
{
    /// <summary>§10's <c>isPavedThroughout</c> — trivially true here: a cluster is defined as exactly
    /// the set of Plots a Paved Road already connects, so there is no "un-paved member" state this
    /// derivation could ever produce.</summary>
    public static bool IsPavedThroughout => true;
}

/// <summary>Pure, read-only union-find over one household's own <see cref="PavedRoadConnection"/>
/// edges (Phase 15 item 7).</summary>
public static class RoadClusterQuery
{
    /// <summary>§4's Road Clusters for one household — every connected component with two or more
    /// Plots (a lone, unconnected Plot never forms a "cluster" of one). §4.1's Unified Estate flag is
    /// true only for the one cluster (if any) whose own Plot set exactly equals every Plot this
    /// household currently owns.</summary>
    public static IReadOnlyList<RoadClusterView> ComputeClusters(WorldState state, RuntimeId<Household> householdId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var edges = state.PavedRoadConnections.InAscendingOrder()
            .Where(entry => entry.Value.HouseholdId == householdId)
            .Select(entry => entry.Value)
            .ToArray();
        if (edges.Length == 0)
            return Array.Empty<RoadClusterView>();

        var parent = new Dictionary<RuntimeId<Plot>, RuntimeId<Plot>>();
        RuntimeId<Plot> Find(RuntimeId<Plot> id)
        {
            if (!parent.ContainsKey(id))
                parent[id] = id;
            while (parent[id] != id)
            {
                parent[id] = parent[parent[id]];
                id = parent[id];
            }
            return id;
        }
        void Union(RuntimeId<Plot> a, RuntimeId<Plot> b)
        {
            var rootA = Find(a);
            var rootB = Find(b);
            if (rootA != rootB)
                parent[rootA] = rootB;
        }

        foreach (var edge in edges)
            Union(edge.PlotAId, edge.PlotBId);

        var householdOwnedPlotIds = OwnedPlotIds(state, householdId);

        var groups = parent.Keys.GroupBy(Find)
            .Select(group => group.OrderBy(id => id.Value).ToArray())
            .OrderBy(group => group[0].Value)
            .ToArray();

        var clusters = new List<RoadClusterView>();
        foreach (var group in groups)
        {
            var bonusActive = group.Length >= PrivateInfrastructureCatalog.RoadClusterThreshold;
            var isUnified = householdOwnedPlotIds.Count > 0 && group.Length == householdOwnedPlotIds.Count &&
                group.All(householdOwnedPlotIds.Contains);
            clusters.Add(new RoadClusterView(group, bonusActive, isUnified));
        }

        return clusters;
    }

    /// <summary>Every distinct household this item's own benefits system needs to evaluate a month for
    /// — every <see cref="PropertyOwnerKind.PlayerHousehold"/> owner among <c>state.Plots</c>, plus any
    /// household that has built a <see cref="PavedRoadConnection"/> (redundant in practice, since a
    /// connection always requires ownership of both endpoints at build time, but kept as a defensive
    /// union in case a Plot's ownership later changes hands out from under an existing connection).</summary>
    public static IReadOnlyList<RuntimeId<Household>> OwnedHouseholdIds(WorldState state)
    {
        var householdIds = new HashSet<RuntimeId<Household>>();
        foreach (var entry in state.Plots.InAscendingOrder())
        {
            if (entry.Value.OwnerId is null)
                continue;
            try
            {
                var owner = PropertyOwnerRef.Parse(entry.Value.OwnerId);
                if (owner.Kind == PropertyOwnerKind.PlayerHousehold && owner.OwnerId is { } ownerId)
                    householdIds.Add(RuntimeId<Household>.Parse(ownerId));
            }
            catch (FormatException)
            {
                // Not a recognized/legacy household tag — excluded, matching OwnedPlotIds' own handling.
            }
        }
        foreach (var entry in state.PavedRoadConnections.InAscendingOrder())
            householdIds.Add(entry.Value.HouseholdId);

        return householdIds.OrderBy(id => id.Value).ToArray();
    }

    /// <summary>Every Plot §2's ownership roster resolves as owned by <paramref name="householdId"/> —
    /// the "every Plot the household holds" half of §4.1's Unified Estate reading.</summary>
    public static HashSet<RuntimeId<Plot>> OwnedPlotIds(WorldState state, RuntimeId<Household> householdId)
    {
        var owned = new HashSet<RuntimeId<Plot>>();
        foreach (var entry in state.Plots.InAscendingOrder())
        {
            if (entry.Value.OwnerId is null)
                continue;
            try
            {
                var owner = PropertyOwnerRef.Parse(entry.Value.OwnerId);
                if (owner.Kind == PropertyOwnerKind.PlayerHousehold && owner.OwnerId == householdId.ToTaggedString())
                    owned.Add(entry.Key);
            }
            catch (FormatException)
            {
                // An un-parseable legacy owner tag is never this household's — matching
                // PropertyOwnerRef.Parse's own documented legacy-prefix handling elsewhere; a genuinely
                // malformed tag (neither a recognized kind nor the legacy household prefix) is simply
                // not counted rather than thrown from a read-only query.
            }
        }
        return owned;
    }
}
