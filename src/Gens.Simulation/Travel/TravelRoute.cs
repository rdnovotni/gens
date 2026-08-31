using Gens.Simulation.Identity;
using Gens.Simulation.Regions;

namespace Gens.Simulation.Travel;

/// <summary>The resolved path between two <see cref="TravelLocation"/>s (§3's "the game computes real
/// Travel Time scaling with actual distance"): the Distance Tier it resolves to, the Travel Time that
/// tier implies, and the route-danger classification §4's real-stakes events would weight against.</summary>
public sealed record TravelRoute
{
    private TravelRoute(
        TravelLocation origin, TravelLocation destination, DistanceTier distanceTier,
        RouteRiskLevel riskExposure, int travelTimeMonths)
    {
        Origin = origin;
        Destination = destination;
        DistanceTier = distanceTier;
        RiskExposure = riskExposure;
        TravelTimeMonths = travelTimeMonths;
    }

    public TravelLocation Origin { get; }
    public TravelLocation Destination { get; }
    public DistanceTier DistanceTier { get; }
    public RouteRiskLevel RiskExposure { get; }
    public int TravelTimeMonths { get; }

    /// <summary>Resolves a real route from <paramref name="homeRegionId"/> (the traveler's own home
    /// region — see <see cref="TravelLocation"/>'s own doc comment for why this is a required input
    /// rather than derived) to <paramref name="destination"/>. Travel to <see
    /// cref="LocationKind.Home"/> is always <see cref="DistanceTier.Near"/> by the Home Anchor rule
    /// (<c>gens-starting-regions-design.md</c> §8.1) — this item deliberately does not model
    /// per-Gazetteer-entry in-region distance beyond that, treating every same-region destination
    /// uniformly (a simplification against §8.1's own finer-grained "ordinary in-region Travel cost"
    /// language, left for whichever future item actually builds gazetteer-entry-level travel).</summary>
    public static TravelRoute Resolve(
        TravelLocation origin,
        TravelLocation destination,
        DefinitionId<RegionProfileDefinition> homeRegionId,
        RegionProfileCatalog regions,
        DistanceTierCatalog distanceTiers)
    {
        if (regions is null)
            throw new ArgumentNullException(nameof(regions));
        if (distanceTiers is null)
            throw new ArgumentNullException(nameof(distanceTiers));

        var tier = destination.Kind == LocationKind.Home
            ? DistanceTier.Near
            : distanceTiers.Resolve(homeRegionId, ResolveDestinationRegion(destination, regions));

        var risk = ResolveRisk(destination.Kind, tier);
        var travelTimeMonths = ResolveTravelTimeMonths(tier);

        return new TravelRoute(origin, destination, tier, risk, travelTimeMonths);
    }

    private static DefinitionId<RegionProfileDefinition> ResolveDestinationRegion(TravelLocation destination, RegionProfileCatalog regions)
    {
        if (destination.Kind == LocationKind.Rome)
            return ResolveCapitalRegion(regions);

        return destination.RegionId ?? throw new ArgumentException(
            $"A '{destination.Kind}' destination must carry a region ID to resolve a route against.", nameof(destination));
    }

    /// <summary>Rome carries no region of its own (§10) — its Distance Tier instead reads off whichever
    /// region's Gazetteer seats it as <see cref="GazetteerRole.Capital"/> (§8.3's "exactly one
    /// gazetteer entry... may ever carry this role").</summary>
    private static DefinitionId<RegionProfileDefinition> ResolveCapitalRegion(RegionProfileCatalog regions)
    {
        foreach (var region in regions.All())
        {
            if (region.Gazetteer.Any(entry => entry.Roles.Contains(GazetteerRole.Capital)))
                return region.Id;
        }

        throw new InvalidOperationException("No region in the catalog seats a Capital-role gazetteer entry for Rome.");
    }

    /// <summary>§4: real-stakes routes weight up sharply on Frontier/Campaign regardless of Distance
    /// Tier; everywhere else, risk simply tracks Distance Tier.</summary>
    private static RouteRiskLevel ResolveRisk(LocationKind destinationKind, DistanceTier tier)
    {
        if (destinationKind is LocationKind.FrontierRegion or LocationKind.Campaign)
            return RouteRiskLevel.Dangerous;

        return tier switch
        {
            DistanceTier.Near => RouteRiskLevel.Secure,
            DistanceTier.Moderate => RouteRiskLevel.Guarded,
            DistanceTier.Far => RouteRiskLevel.Dangerous,
            _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown distance tier."),
        };
    }

    /// <summary>Invented baseline, undisclosed by the design corpus (§11: "travel time's actual
    /// distance formula... unsized") — matching <see cref="Characters.DutySlotCatalog"/>'s own
    /// disclaimer for its own invented numbers.</summary>
    private static int ResolveTravelTimeMonths(DistanceTier tier) => tier switch
    {
        DistanceTier.Near => 1,
        DistanceTier.Moderate => 3,
        DistanceTier.Far => 6,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown distance tier."),
    };
}
