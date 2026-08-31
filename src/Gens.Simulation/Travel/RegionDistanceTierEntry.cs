using Gens.Simulation.Identity;
using Gens.Simulation.Regions;

namespace Gens.Simulation.Travel;

/// <summary>One hand-assigned entry of <c>gens-starting-regions-design.md</c> §7.1's Distance Tier
/// lookup table — content-authored, not a formula. Symmetric: <see cref="DistanceTierCatalog"/> reads
/// it in either direction, matching §7.1's own "Italian Heartland to Greek East" framing, where either
/// region is equally validly the traveler's home.</summary>
public sealed record RegionDistanceTierEntry
{
    public RegionDistanceTierEntry(
        DefinitionId<RegionProfileDefinition> regionA,
        DefinitionId<RegionProfileDefinition> regionB,
        DistanceTier tier)
    {
        if (regionA.Equals(regionB))
        {
            throw new ArgumentException(
                "A region's distance to itself is always Near and is never authored explicitly.",
                nameof(regionB));
        }

        RegionA = regionA;
        RegionB = regionB;
        Tier = tier;
    }

    public DefinitionId<RegionProfileDefinition> RegionA { get; }
    public DefinitionId<RegionProfileDefinition> RegionB { get; }
    public DistanceTier Tier { get; }
}
