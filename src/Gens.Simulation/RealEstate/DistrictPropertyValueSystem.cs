using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Hazards;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;
using Gens.Simulation.Regions;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.RealEstate;

/// <summary>
/// §4's monthly District Property Value tick (Phase 15 item 1). Moves every District's <see
/// cref="District.PropertyValue"/> toward a freshly computed target, smoothed by <see
/// cref="RealEstateCatalog.PropertyValueSmoothing"/> so one month's disaster or population blip does
/// not instantly re-price everything in the District — the same "nudge toward a target rather than
/// snap to it" shape <see cref="Doctrine.DoctrineResolutionSystem"/> already uses for Affinity. The
/// target folds in exactly §4's own named real inputs this codebase already tracks:
///
/// <list type="bullet">
/// <item><description>Settlement Demographics' population trend (month-over-month total population
/// change, read against <see cref="District.PreviousSettlementPopulation"/>) and size-weighted average
/// Contentment.</description></item>
/// <item><description>Natural Disaster damage: <see cref="DisasterEvent.BuildingsDamaged"/> for every
/// Event fired against this District's settlement within <see
/// cref="RealEstateCatalog.DisasterDamageLookbackMonths"/>.</description></item>
/// <item><description>A region's own Gazetteer Prominence Tier, only when the District's own <see
/// cref="District.LinkedGazetteerLocationId"/> resolves against the caller-supplied <see
/// cref="RegionProfileCatalog"/>.</description></item>
/// </list>
///
/// §4's fourth named input, a built Monument, is <b>not</b> wired: Monuments &amp; Legacy Building is
/// Phase 17, confirmed unbuilt by direct search — the same honestly-narrowed gap <see
/// cref="District.PropertyValue"/>'s own doc comment names. A District with no PopGroups at all (a
/// freshly founded Vicus) simply carries no population/Contentment term this month rather than a
/// divide-by-zero.
/// </summary>
public sealed class DistrictPropertyValueSystem : IMonthlySystem<WorldState>
{
    private readonly RegionProfileCatalog? _regions;

    public DistrictPropertyValueSystem(RegionProfileCatalog? regions = null) => _regions = regions;

    public string Id => "realEstate.districtPropertyValue";
    public TickPhase Phase => TickPhase.MarketsLedger;
    public IReadOnlyCollection<string> Reads { get; } =
        new[] { "districts", "popGroups", "disasterEvents", "plotPropertyExtensions", "propertyRecords" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "districts", "plotPropertyExtensions", "propertyRecords" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "characters.contentment", "characters.migration" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.Districts.InAscendingOrder().ToArray())
        {
            var district = entry.Value;
            var (population, contentment) = SettlementDemographics(state, district.SettlementId);
            var populationGrowth = ComputeGrowthRate(district.PreviousSettlementPopulation, population);
            var buildingsDamaged = RecentDisasterDamage(state, district.SettlementId, context.Date);

            var target = RealEstateCatalog.BaselinePropertyValue
                + Fixed64.Multiply(RealEstateCatalog.PopulationGrowthWeight, Clamp(populationGrowth, GrowthRateFloor, GrowthRateCeiling))
                + Fixed64.Multiply(RealEstateCatalog.ContentmentWeight, contentment - Fixed64.FromRaw(500_000))
                - Fixed64.Multiply(RealEstateCatalog.DisasterDamagePerBuildingWeight, Fixed64.FromInt(buildingsDamaged));

            if (district.LinkedGazetteerLocationId is { } gazetteerId &&
                _regions is not null && TryFindGazetteerEntry(_regions, gazetteerId, out var location))
                target += RealEstateCatalog.ProminenceTierBonus(location.ProminenceTier);

            if (target < RealEstateCatalog.MinimumPropertyValue)
                target = RealEstateCatalog.MinimumPropertyValue;

            var moved = district.PropertyValue + Fixed64.Multiply(target - district.PropertyValue, RealEstateCatalog.PropertyValueSmoothing);
            if (moved < RealEstateCatalog.MinimumPropertyValue)
                moved = RealEstateCatalog.MinimumPropertyValue;

            state.Districts.Remove(entry.Key);
            state.Districts.Add(entry.Key, district with { PropertyValue = moved, PreviousSettlementPopulation = population });

            RepriceLinkedProperties(state, entry.Key, district.PropertyValue, moved);
        }

        return Array.Empty<IDomainEvent>();
    }

    /// <summary>§9's "value... moving from... the District's own trend": every Plot or PropertyRecord
    /// linked to this District (<see cref="PlotPropertyExtension.DistrictId"/>/<see
    /// cref="PropertyRecord.DistrictId"/>) has its own tracked <see cref="PropertyView.Value"/> rescaled
    /// by the same fraction the District's own Property Value just moved — so a transfer price or an
    /// Operator's remittance, both read straight off that tracked Value, actually feel population,
    /// Contentment, disaster damage, and Gazetteer Prominence the way §4/§9 require, rather than only
    /// the District's own index number moving in isolation. A property still priced at <see
    /// cref="Money.Zero"/> (never yet priced by a real acquisition/sale) stays zero — there is nothing
    /// yet to rescale.</summary>
    private static void RepriceLinkedProperties(WorldState state, RuntimeId<District> districtId, Fixed64 previousValue, Fixed64 newValue)
    {
        if (previousValue == Fixed64.Zero || previousValue == newValue)
            return;

        var ratio = Fixed64.Divide(newValue, previousValue);

        foreach (var entry in state.PlotPropertyExtensions.InAscendingOrder().ToArray())
        {
            if (entry.Value.DistrictId != districtId || entry.Value.Value == Money.Zero)
                continue;
            PlotPropertyResolver.Set(state, entry.Value with { Value = entry.Value.Value.Scale(ratio) });
        }

        foreach (var entry in state.PropertyRecords.InAscendingOrder().ToArray())
        {
            if (entry.Value.DistrictId != districtId || entry.Value.Value == Money.Zero)
                continue;
            state.PropertyRecords.Remove(entry.Key);
            state.PropertyRecords.Add(entry.Key, entry.Value with { Value = entry.Value.Value.Scale(ratio) });
        }
    }

    private static (int Population, Fixed64 Contentment) SettlementDemographics(WorldState state, RuntimeId<Settlement> settlementId)
    {
        var totalSize = 0;
        var weightedContentment = Fixed64.Zero;

        foreach (var entry in state.PopGroups.InAscendingOrder())
        {
            if (entry.Key.SettlementId != settlementId || entry.Value.Size == 0)
                continue;
            totalSize += entry.Value.Size;
            weightedContentment += Fixed64.Multiply(entry.Value.Contentment, Fixed64.FromInt(entry.Value.Size));
        }

        var contentment = totalSize > 0 ? Fixed64.Divide(weightedContentment, Fixed64.FromInt(totalSize)) : Fixed64.FromRaw(500_000);
        return (totalSize, contentment);
    }

    private static Fixed64 ComputeGrowthRate(int previousPopulation, int currentPopulation)
    {
        if (previousPopulation <= 0)
            return Fixed64.Zero;
        return Fixed64.Divide(Fixed64.FromInt(currentPopulation - previousPopulation), Fixed64.FromInt(previousPopulation));
    }

    private static int RecentDisasterDamage(WorldState state, RuntimeId<Settlement> settlementId, GameDate now)
    {
        var total = 0;
        foreach (var entry in state.DisasterEvents.InAscendingOrder())
        {
            var disaster = entry.Value;
            if (disaster.SettlementId != settlementId)
                continue;
            var monthsSince = now.TotalMonths - disaster.OccurredDate.TotalMonths;
            if (monthsSince < 0 || monthsSince > RealEstateCatalog.DisasterDamageLookbackMonths)
                continue;
            total += disaster.BuildingsDamaged;
        }

        return total;
    }

    private static bool TryFindGazetteerEntry(
        RegionProfileCatalog regions, DefinitionId<GazetteerLocationDefinition> locationId, out GazetteerLocationDefinition entry)
    {
        foreach (var region in regions.All())
        {
            if (region.TryGetGazetteerEntry(locationId, out var found))
            {
                entry = found;
                return true;
            }
        }

        entry = null!;
        return false;
    }

    private static readonly Fixed64 GrowthRateFloor = Fixed64.FromRaw(-100_000); // -0.1.
    private static readonly Fixed64 GrowthRateCeiling = Fixed64.FromRaw(100_000); // 0.1.

    private static Fixed64 Clamp(Fixed64 value, Fixed64 min, Fixed64 max)
    {
        if (value < min)
            return min;
        return value > max ? max : value;
    }
}
