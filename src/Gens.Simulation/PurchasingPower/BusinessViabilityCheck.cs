using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.NotableBusinesses;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PurchasingPower;

/// <summary>
/// §7's/§9's <c>BusinessViabilityCheck</c> data model — the direct, practical payoff named in §7's own
/// closing line: "a real, honest constraint this document adds directly to Notable Businesses' own
/// Reputation and income mechanics... a real, concrete reason a Notable Business might choose to Move."
/// <see cref="RecommendedAction"/> is exactly the two real strings §9's own sketch names
/// (<c>"specialize"</c> | <c>"move"</c>), <c>null</c> when matched — an honest read-only recommendation,
/// not an autonomous trigger: nothing in this item calls <see
/// cref="NotableBusinesses.MoveNotableBusinessCommand"/> on a mismatched reading, matching <see
/// cref="BusinessCompetition.MarketSaturationSystem"/>'s own identical "a real, computed primitive with
/// no autonomous caller yet" precedent (no autonomous NPC decision loop exists anywhere in this codebase
/// for a Notable Business to plug this into).
/// </summary>
public sealed record BusinessViabilityCheck(
    RuntimeId<NotableBusiness> BusinessId,
    RuntimeId<District> DistrictId,
    WealthBand OutputGoodTier,
    bool LocalDemandMatch,
    string? RecommendedAction);

public static class BusinessViabilityResolver
{
    public static bool TryGetCurrent(WorldState state, RuntimeId<NotableBusiness> businessId, out BusinessViabilityCheck check) =>
        state.BusinessViabilityChecks.TryGet(businessId, out check!);
}

/// <summary>
/// §7's own monthly viability tick, matching <see cref="BusinessCompetition.GrainHoardingResolutionSystem"/>'s
/// established static <c>Tick(state, date)</c> convention. For every <see
/// cref="NotableBusinessStatus.Tracked"/> business with both a real <see
/// cref="NotableBusiness.OutputGoodId"/> and <see cref="NotableBusiness.DistrictId"/>, resolved against
/// that District's own settlement's <see cref="AggregateDemandReading"/> (recorded earlier the same tick
/// by <see cref="AggregatePurchasingPowerSystem"/>): computes and records the real <see
/// cref="BusinessViabilityCheck"/>, and — §7's own "a real, honest constraint... on Reputation and
/// income" realized concretely, since <see cref="NotableBusiness"/> tracks no separate income field —
/// applies a real, small, recurring Reputation drain (<see
/// cref="PurchasingPowerCatalog.LocalDemandMismatchMonthlyReputationLoss"/>) via the new <see
/// cref="BusinessReputationChangeReason.LocalDemandMismatch"/> reason while the mismatch persists,
/// clamped at <see cref="NotableBusinessesCatalog.MinReputation"/> so a genuinely stranded luxury
/// business settles at the floor rather than an unbounded negative. A business whose settlement has no
/// recorded reading yet (or that carries no District/Output at all) is simply skipped, and any stale
/// reading for it is cleared — mirroring <see cref="BusinessCompetition.MarketSaturationSystem"/>'s own
/// identical stale-entry cleanup.
/// </summary>
public static class BusinessViabilitySystem
{
    public static IReadOnlyList<IDomainEvent> Tick(WorldState state, GameDate date)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var touched = new HashSet<RuntimeId<NotableBusiness>>();

        foreach (var entry in state.NotableBusinesses.InAscendingOrder().ToArray())
        {
            var business = entry.Value;
            if (business.Status != NotableBusinessStatus.Tracked)
                continue;
            if (business.OutputGoodId is not { } goodId || business.DistrictId is not { } districtId)
                continue;
            if (!state.Districts.TryGet(districtId, out var district))
                continue;
            if (!AggregateDemandResolver.TryGetCurrent(state, district!.SettlementId, out var reading))
                continue;

            touched.Add(entry.Key);
            var outputTier = PurchasingPowerCalculator.GetGoodTier(goodId);
            var check = PurchasingPowerCalculator.EvaluateBusinessViability(entry.Key, districtId, outputTier, reading);

            if (state.BusinessViabilityChecks.TryGet(entry.Key, out _))
                state.BusinessViabilityChecks.Remove(entry.Key);
            state.BusinessViabilityChecks.Add(entry.Key, check);

            if (!check.LocalDemandMatch)
            {
                events.AddRange(AdjustBusinessReputationCommands.Pipeline.Execute(
                    state, new AdjustBusinessReputationCommand(
                        state.CommandIds.Issue(), "system", date, null, entry.Key,
                        -PurchasingPowerCatalog.LocalDemandMismatchMonthlyReputationLoss,
                        BusinessReputationChangeReason.LocalDemandMismatch)).Events);
            }
        }

        foreach (var staleKey in state.BusinessViabilityChecks.InAscendingOrder().Select(e => e.Key).ToArray())
        {
            if (!touched.Contains(staleKey))
                state.BusinessViabilityChecks.Remove(staleKey);
        }

        return events;
    }
}
