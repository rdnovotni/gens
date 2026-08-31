using Gens.Simulation.Correspondence;
using Gens.Simulation.Identity;
using Gens.Simulation.Regions;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Tests.Correspondence;

/// <summary>Shared region/distance-tier/reachability fixtures for the Correspondence test suite —
/// mirrors <c>Gens.Simulation.Tests.Travel.TravelTestFixtures</c>'s identical "small, self-contained,
/// not real content" shape.</summary>
public static class CorrespondenceTestFixtures
{
    public static readonly DefinitionId<RegionProfileDefinition> HomeRegionId = new("test-home-region");
    public static readonly DefinitionId<RegionProfileDefinition> NearRegionId = new("test-near-region");
    public static readonly DefinitionId<RegionProfileDefinition> FarRegionId = new("test-far-region");

    public static readonly DefinitionId<Culture> LiterateCultureId = new("test-literate-culture");
    public static readonly DefinitionId<Culture> OralTraditionPartialCultureId = new("test-oral-partial-culture");
    public static readonly DefinitionId<Culture> OralTraditionBlockedCultureId = new("test-oral-blocked-culture");

    public static DistanceTierCatalog BuildDistanceTierCatalog() =>
        new(new[]
        {
            new RegionDistanceTierEntry(HomeRegionId, NearRegionId, DistanceTier.Near),
            new RegionDistanceTierEntry(HomeRegionId, FarRegionId, DistanceTier.Far),
        });

    public static CorrespondenceReachabilityCatalog BuildReachabilityCatalog() =>
        new(new[]
        {
            new CultureReachabilityEntry(OralTraditionPartialCultureId, CorrespondenceReachability.OralTraditionPartial),
            new CultureReachabilityEntry(OralTraditionBlockedCultureId, CorrespondenceReachability.OralTraditionBlocked),
        });
}
