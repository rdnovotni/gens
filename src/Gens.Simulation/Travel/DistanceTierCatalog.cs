using Gens.Simulation.Identity;
using Gens.Simulation.Regions;

namespace Gens.Simulation.Travel;

/// <summary>The in-memory lookup over every authored <see cref="RegionDistanceTierEntry"/> — mirrors
/// <see cref="RegionProfileCatalog"/>'s identical "content, not <see cref="State.WorldState"/>" shape.
/// <c>gens-starting-regions-design.md</c> §13 leaves the lookup table's actual real-region-pair
/// contents an explicit open question ("doesn't assign actual tiers to actual region pairs... resolve
/// once the Extensible Slate regions are formally scheduled"); this catalog is the general mechanism
/// that table hangs off, not that table itself. A pair with no authored entry defaults to <see
/// cref="DistanceTier.Moderate"/> — this default, like <see cref="Characters.DutySlotCatalog"/>'s own
/// invented numbers, is this implementation's own baseline rather than anything the design corpus
/// specifies, chosen as the least-committal middle tier rather than assuming every unlisted pair is
/// either trivially Near or maximally Far.</summary>
public sealed class DistanceTierCatalog
{
    private readonly Dictionary<(string, string), DistanceTier> _tiers;

    public DistanceTierCatalog(IEnumerable<RegionDistanceTierEntry> entries)
    {
        if (entries is null)
            throw new ArgumentNullException(nameof(entries));

        var tiers = new Dictionary<(string, string), DistanceTier>();
        foreach (var entry in entries)
        {
            var key = NormalizedKey(entry.RegionA, entry.RegionB);
            if (!tiers.TryAdd(key, entry.Tier))
                throw new ArgumentException($"Duplicate distance tier entry for '{entry.RegionA}'/'{entry.RegionB}'.", nameof(entries));
        }

        _tiers = tiers;
    }

    /// <summary>A region's distance to itself is always <see cref="DistanceTier.Near"/> (never authored
    /// explicitly — see <see cref="RegionDistanceTierEntry"/>'s own constructor guard); any other pair
    /// resolves against the authored table, defaulting to <see cref="DistanceTier.Moderate"/> when
    /// unlisted (this catalog's own doc comment explains why).</summary>
    public DistanceTier Resolve(DefinitionId<RegionProfileDefinition> regionA, DefinitionId<RegionProfileDefinition> regionB)
    {
        if (regionA.Equals(regionB))
            return DistanceTier.Near;

        return _tiers.TryGetValue(NormalizedKey(regionA, regionB), out var tier) ? tier : DistanceTier.Moderate;
    }

    private static (string, string) NormalizedKey(DefinitionId<RegionProfileDefinition> a, DefinitionId<RegionProfileDefinition> b) =>
        string.CompareOrdinal(a.Value, b.Value) <= 0 ? (a.Value, b.Value) : (b.Value, a.Value);
}
