using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.MerchantFamilies;

/// <summary>§6's three named Dignitas-investment moves a merchant family actually makes to close the
/// Senate's own Dignitas gap (Phase 15 item 3; <c>gens-merchant-families-design.md</c> §6): "funding a
/// Games &amp; Spectacle event or a Public Works Funded Action... for Dignitas rather than direct
/// profit," "pursuing a strategic marriage into an old, prestige-rich but cash-poor house," and "holding
/// a local magistracy... as a visible, respectability-building stepping stone."</summary>
public enum DignitasInvestmentActionType
{
    FundedGamesOrPublicWorks,
    StrategicMarriage,
    LocalMagistracy,
}

/// <summary>One recorded §6 Dignitas-investment move — the real Dignitas award it actually produced
/// (<see cref="MerchantFamiliesCatalog.DignitasEffectFor"/>) and when it happened, matching §9's own
/// <c>dignitasInvestmentActions: [ { actionType, effect } ]</c> array field.</summary>
public sealed record DignitasInvestmentAction(DignitasInvestmentActionType ActionType, int DignitasEffect, GameDate Date);

/// <summary>The real, persisted half of §9's <c>SenateEntryProgress</c> data model — a player household's
/// own append-only log of §6 investment actions (Phase 15 item 3). Sparse, keyed by <see
/// cref="RuntimeId{Household}"/>, present only once a household has actually recorded its first action,
/// matching <see cref="Reputation.HouseholdReputation"/>'s identical "present only once touched"
/// convention. Restricted to the player's own household, the same narrowing <see
/// cref="Reputation.HouseholdReputation"/>'s own doc comment already made for Dignitas itself: a Rival
/// House's Dignitas lives on <see cref="Actors.LivingWorldActor.Dignitas"/> instead, and no command in
/// this codebase adjusts that field directly (only <see cref="Actors.RivalHouseCreationService"/>'s own
/// creation-time seed and <see cref="Actors.BackgroundHouseDriftSystem"/>'s own periodic drift ever
/// write it) — building a second, parallel Dignitas-adjustment path for a Rival House's own novus homo
/// story is a real scope cut this item makes deliberately rather than invent that mechanism here.</summary>
public sealed record SenateEntryInvestmentLog(RuntimeId<Household> HouseholdId, IReadOnlyList<DignitasInvestmentAction> Actions);

/// <summary>§9's <c>SenateEntryProgress</c> read view (Phase 15 item 3): §6's Net Worth gate and
/// Dignitas gate are computed live off <see cref="Economy.NetWorth"/> and <see
/// cref="Reputation.DignitasResolver"/> respectively — the same "computed, not stored" reasoning <see
/// cref="EquestrianStatus"/>'s own doc comment already gives for §2, extended here to §6's two gates so
/// neither can ever go stale relative to the figures it actually gates on. Only <see
/// cref="DignitasInvestmentActions"/> is real, persisted history (<see
/// cref="SenateEntryInvestmentLog"/>) — the deliberate record of what a household actually did, which no
/// live computation could reconstruct after the fact.</summary>
public readonly record struct SenateEntryProgress(
    bool NetWorthGateCleared, bool DignitasGateCleared, IReadOnlyList<DignitasInvestmentAction> DignitasInvestmentActions);

public static class SenateEntryProgressQuery
{
    public static SenateEntryProgress Current(WorldState state, RuntimeId<Household> householdId)
    {
        var netWorthGateCleared = state.NetWorthAssessments.TryGet(householdId, out var assessment)
            && assessment!.Total >= MerchantFamiliesCatalog.SenateNetWorthThreshold;
        var dignitasGateCleared = DignitasResolver.Current(state, householdId) >= MerchantFamiliesCatalog.SenateDignitasThreshold;
        var actions = state.SenateEntryInvestmentLogs.TryGet(householdId, out var log)
            ? log!.Actions
            : Array.Empty<DignitasInvestmentAction>();

        return new SenateEntryProgress(netWorthGateCleared, dignitasGateCleared, actions);
    }
}
