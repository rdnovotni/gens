using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;

namespace Gens.Simulation.RealEstate;

/// <summary>
/// §10's Displacement integration (Phase 15 item 1; <c>gens-land-ownership-real-estate-design.md</c>
/// §10): "this document adds no new tracked displacement mechanic. Instead, a District's own sharply
/// rising Property Value feeds directly into Settlement Demographics' existing Contentment and
/// Emigration formula as a new input." <see cref="Characters.ContentmentSystem"/> is the one caller —
/// it reads this calculator once per <see cref="PopGroup"/>, for exactly the lower-tier resident
/// groups §10 names (<see cref="PopGroupType.Operarii"/>, <see cref="PopGroupType.Coloni"/>), and
/// folds the result into <see cref="ContentmentCalculator.ComputeContentment(Fixed64,Fixed64,Fixed64,Fixed64)"/>'s
/// own new <c>rentBurden</c> parameter — Emigration itself needs no separate change at all, since <see
/// cref="MigrationCalculator.EmigrationRate"/> already reads straight off <see
/// cref="PopGroup.Contentment"/>, which this rent burden already depresses upstream.
/// </summary>
public static class DistrictRentBurdenCalculator
{
    /// <summary>§10's "lower-tier resident pop groups (Operarii, urban Coloni-adjacent poor)" — the
    /// only <see cref="PopGroupType"/>s a District's rising Property Value ever touches. Every other
    /// group (Elite, Negotiatores, and so on) feels a gentrifying District only through whatever
    /// narrative or future system reads it directly, not through this Contentment channel.</summary>
    public static bool IsRentExposed(PopGroupType groupType) =>
        groupType is PopGroupType.Operarii or PopGroupType.Coloni;

    /// <summary>§10's "higher rent burden depressing Contentment... exactly the way overcrowding or low
    /// Contentment already does" — zero below <see
    /// cref="RealEstateCatalog.RentBurdenPropertyValueThreshold"/> (an ordinary, un-gentrified District
    /// exerts no rent pressure at all), then scales linearly with how far the District's own Property
    /// Value has climbed past that threshold.</summary>
    public static Fixed64 ComputeRentBurden(Fixed64 districtPropertyValue)
    {
        if (districtPropertyValue <= RealEstateCatalog.RentBurdenPropertyValueThreshold)
            return Fixed64.Zero;

        var excess = districtPropertyValue - RealEstateCatalog.RentBurdenPropertyValueThreshold;
        return Fixed64.Multiply(excess, RealEstateCatalog.RentBurdenWeight);
    }

    /// <summary>The highest Property Value among a settlement's own Districts — a rent-exposed pop
    /// group is read against the most gentrified District in its settlement rather than a
    /// settlement-wide average, matching §10's own "a District's own sharply rising Property Value"
    /// framing (the pressure is local to wherever the gentrifying District actually is, not diluted
    /// across the whole settlement). Zero (§10's own quiet no-op) for a settlement with no Districts
    /// established yet (Villa/early Vicus, per §4).</summary>
    public static Fixed64 HighestDistrictPropertyValue(WorldState state, RuntimeId<Settlement> settlementId)
    {
        var highest = Fixed64.Zero;
        var found = false;
        foreach (var entry in state.Districts.InAscendingOrder())
        {
            if (entry.Value.SettlementId != settlementId)
                continue;
            if (!found || entry.Value.PropertyValue > highest)
            {
                highest = entry.Value.PropertyValue;
                found = true;
            }
        }

        return found ? highest : Fixed64.Zero;
    }
}
