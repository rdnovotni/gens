using Gens.Simulation.Characters;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Markets;
using Gens.Simulation.NotableBusinesses;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;

namespace Gens.Simulation.BusinessCompetition;

/// <summary>§6's own qualitative read of how crowded a settlement's own trade actually is.</summary>
public enum MarketSaturationLevel
{
    Undersaturated,
    Balanced,
    Saturated,
}

/// <summary>
/// §6's/§9's <c>MarketCapacityReading</c> data model (Phase 15 item 5) — "a settlement's own trade
/// capacity is real and finite, read directly from Settlement Demographics' own Employment Ratio and
/// pop-group sizing rather than a new standalone market-size figure." Keyed by <see cref="MarketGoodKey"/>
/// (settlement + good), reused directly from <see cref="Markets.SettlementMarket"/>'s own identical key
/// shape rather than this item inventing a second (settlement, trade) key for what is mechanically the
/// same pairing — §6's own "tradeType" is realized here as the traded <see cref="DefinitionId{Good}"/>
/// itself, since no separate "trade" concept exists anywhere in this codebase beyond the good a business
/// actually outputs.
/// </summary>
public sealed record MarketCapacityReading(
    RuntimeId<Settlement> SettlementId,
    DefinitionId<Good> GoodId,
    int CurrentBusinessCount,
    Fixed64 EmploymentRatioDerived,
    MarketSaturationLevel SaturationLevel);

public static class MarketCapacityResolver
{
    public static bool TryGetCurrent(WorldState state, RuntimeId<Settlement> settlementId, DefinitionId<Good> goodId, out MarketCapacityReading reading) =>
        state.MarketCapacityReadings.TryGet(new MarketGoodKey(settlementId, goodId), out reading!);
}

/// <summary>
/// §6's own monthly capacity/saturation read (Phase 15 item 5), matching <see
/// cref="NotableBusinesses.SupplierDisruptionSystem"/>'s established static <c>Tick(state, date)</c>
/// convention. For every (settlement, good) pair with at least one <see
/// cref="NotableBusinessStatus.Tracked"/> business whose own <see cref="NotableBusiness.OutputGoodId"/> is
/// that good: counts those businesses, reads the settlement's own Opifices (Artisans/Craftsmen) <see
/// cref="PopGroup.EmploymentRatio"/> — §6's own named "pop-group sizing," realized as the one pop group
/// type this codebase's own Notable Business worked examples (bakeries, workshops) map onto most directly
/// — and derives a qualitative <see cref="MarketSaturationLevel"/> from <see
/// cref="BusinessCompetitionCatalog"/>'s own invented business-count thresholds, additionally reading
/// Employment Ratio &lt; 1 (jobs already scarce) as an automatic Saturated read regardless of count.
///
/// This item builds only the real, computed reading itself — §6's own further claim that a crowded, flat-
/// population settlement "genuinely dilutes... demand... raising the odds that §2's own escalation ladder
/// actually gets triggered" describes a decision an autonomous NPC business layer would make by reading
/// this reading, not a mechanic this reading forces; no such autonomous "should I escalate" decision loop
/// exists anywhere in this codebase for Notable Businesses to plug into (matching <see
/// cref="Societates.PartnerDisputeRiskQuery"/>'s and <see cref="MerchantFamilies.SenateEntryProgressQuery"/>'s
/// own identical "a real, computed primitive with no autonomous caller yet" precedent) — so this system
/// only ever writes the reading; nothing in this item auto-fires <see cref="EscalateCompetitiveRungCommand"/>
/// from it. §6's own population-growth-trend refinement ("a settlement with real, growing population...
/// can absorb new entrants... flat or declining population turns every new entrant into a genuine, felt
/// threat") is a real, named scope cut: <see cref="RealEstate.District.PreviousSettlementPopulation"/>
/// tracks a month-over-month population reading per District, not per Settlement, and a settlement can
/// carry several Districts, so deriving one settlement-wide growth trend from it would need an allocation
/// rule this item does not invent — this system reads Employment Ratio and business count only, the two
/// inputs §6 names that this codebase can already resolve at settlement granularity without guessing.
/// </summary>
public static class MarketSaturationSystem
{
    public static void Tick(WorldState state)
    {
        var counts = new Dictionary<MarketGoodKey, int>();

        foreach (var entry in state.NotableBusinesses.InAscendingOrder())
        {
            var business = entry.Value;
            if (business.Status != NotableBusinessStatus.Tracked)
                continue;
            if (business.OutputGoodId is not { } goodId)
                continue;
            if (business.DistrictId is not { } districtId || !state.Districts.TryGet(districtId, out var district))
                continue;

            var key = new MarketGoodKey(district!.SettlementId, goodId);
            counts[key] = counts.TryGetValue(key, out var existing) ? existing + 1 : 1;
        }

        foreach (var (key, count) in counts.OrderBy(pair => pair.Key))
        {
            var employmentRatio = state.PopGroups.TryGet(new PopGroupKey(key.SettlementId, PopGroupType.Opifices), out var popGroup)
                ? popGroup!.EmploymentRatio
                : Fixed64.One;

            var saturation = ComputeSaturation(count, employmentRatio);
            var reading = new MarketCapacityReading(key.SettlementId, key.GoodId, count, employmentRatio, saturation);

            if (state.MarketCapacityReadings.TryGet(key, out _))
                state.MarketCapacityReadings.Remove(key);
            state.MarketCapacityReadings.Add(key, reading);
        }
    }

    private static MarketSaturationLevel ComputeSaturation(int businessCount, Fixed64 employmentRatio)
    {
        if (employmentRatio < Fixed64.One)
            return MarketSaturationLevel.Saturated;
        if (businessCount >= BusinessCompetitionCatalog.SaturatedBusinessCountFloor)
            return MarketSaturationLevel.Saturated;
        if (businessCount <= BusinessCompetitionCatalog.UndersaturatedBusinessCountCeiling)
            return MarketSaturationLevel.Undersaturated;

        return MarketSaturationLevel.Balanced;
    }
}
