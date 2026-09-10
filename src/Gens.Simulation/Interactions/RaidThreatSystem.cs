using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Actors;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Time;

namespace Gens.Simulation.Interactions;

/// <summary>Emitted whenever a <see cref="RaidThreat"/> resolves. Private to the target household —
/// surfacing a raid to anyone beyond the household it struck (a rumor, a Chronicle entry, the wider
/// regional-risk picture) is a future consumer's job, matching <see
/// cref="SpyPlacementResolvedEvent"/>'s identical visibility reasoning.</summary>
public sealed record RaidOccurredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<RaidThreat> RaidId,
    RuntimeId<Actor> ConfederationActorId,
    RuntimeId<Household> TargetHouseholdId,
    RaidTargetType TargetType,
    RaidOutcome Outcome,
    Money SpoilsLost) : IDomainEvent
{
    public string Type => "interactions.raidOccurred";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { TargetHouseholdId.ToTaggedString() };
    public string? CausationId => null;
    public Visibility Visibility => Visibility.Private(TargetHouseholdId.ToTaggedString());
}

/// <summary>
/// The monthly raid-generation-and-resolution tick for every <see
/// cref="LivingWorldActorType.BanditConfederation"/> (Phase 16 item 2; <c>gens-piracy-banditry-design.md</c>
/// §2, §3, §9). A single-roll-sequence shape, mirroring <see cref="SpyPlacementType.QuickOp"/>'s "one
/// roll, not an accumulating one": each Confederation, each month, rolls whether it raids at all (§2's
/// standing-trend- and strength-weighted chance), then — only if it does — which household in its own
/// region it strikes (§1's "regional risk": a Confederation never reaches outside its own <see
/// cref="LivingWorldActor.RegionId"/>) and what it goes after, then resolves interception (§3's
/// "Interceptable... using a well-defended target's own security level", read from <see
/// cref="EstateSecurityInvestment"/>) and, on an uncontested success, applies the loss and posts it
/// against the target's own ledger.
///
/// Every terminal outcome then rolls a chance to nudge the Confederation's own <see
/// cref="LivingWorldActor.StandingTrend"/> (§2: "an ignored Confederation grows bolder over time... a
/// Confederation left unchallenged... skews Rising... one that's been recently... roughed up... skews
/// Declining"). This is a direct, outcome-fed nudge distinct from <see
/// cref="BackgroundHouseDriftSystem"/>'s unrelated ambient roll, which only ever considers <see
/// cref="LivingWorldActorType.Gens"/> actors and never looks at raid history.
///
/// <para><b>Deliberately not modeled here</b> (matching item 1's own "core vertical slice now, defer the
/// rest" precedent): Bribery &amp; Tribute (§4), Retaliation (§5) — both require a Confederation's real
/// base location and the not-yet-built Combat Resolution Engine — Turning Raider (§6), Allying With
/// &amp; Contracting Raiders including Targeted Contracts (§7, §7.1), Familia-member/background-population
/// kidnapping and the Ransom flow it opens (§8's non-goods targets), and the actual Vigil/Praefectus
/// Vigilum/Navarchus/Watchtower/City Walls sources that should someday feed <see
/// cref="EstateSecurityInvestment.SecurityLevel"/> instead of the direct-investment command this item
/// ships with. A captured raider (<see cref="RaidOutcome.RaidersCaptured"/>) is recorded as an outcome
/// only — no Character is generated for them, and no Legal &amp; Court/Labor &amp; Slavery intake is
/// wired, per that outcome's own doc comment. §9's own "a better-defended estate is also less likely to
/// be targeted... in the first place" axis is likewise deferred — <see cref="FindTarget"/> below is a
/// plain regional tie-break with no <see cref="EstateSecurityInvestment.SecurityLevel"/> weighting; see
/// that type's own doc comment.</para>
///
/// Every numeric constant here is this codebase's own untuned first pass — see <see
/// cref="RaidThreatCatalog"/>'s own doc comments.
/// </summary>
public sealed class RaidThreatSystem : IMonthlySystem<WorldState>
{
    public string Id => "hazards.raidThreat";
    public TickPhase Phase => TickPhase.Hazards;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "actors", "householdHeadships", "characters", "settlements", "estateSecurityInvestments" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "raidThreats", "raidThreatIds", "eventIds", "ledgerAccounts", "ledgerTransactions", "actors" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        var confederations = state.Actors.InAscendingOrder()
            .Where(entry => entry.Value.ActorType == LivingWorldActorType.BanditConfederation)
            .ToArray();
        if (confederations.Length == 0)
            return events;

        var households = state.HouseholdHeadships.InAscendingOrder().ToArray();
        if (households.Length == 0)
            return events;

        foreach (var (confederationActorId, confederation) in confederations)
        {
            var raidChance = Math.Clamp(
                RaidThreatCatalog.BaseRaidChancePercent
                    + TrendRaidChanceAdjustment(confederation.StandingTrend)
                    + (int)confederation.MilitaryStrength.Band * RaidThreatCatalog.MilitaryStrengthBandRaidChanceBonusPercent,
                0, 100);
            var raids = context.RandomStreams.NextUInt(StreamName, 100) < (uint)raidChance;
            if (!raids)
                continue;

            var target = FindTarget(state, confederation, households);
            if (target is not { } targetEntry)
                continue;

            var targetType = (RaidTargetType)context.RandomStreams.NextUInt(StreamName, 3);
            var securityLevel = state.EstateSecurityInvestments.TryGet(targetEntry.Key, out var investment)
                ? investment!.SecurityLevel
                : EstateSecurityInvestment.MinValue;

            var interceptionChance = Math.Clamp(
                RaidThreatCatalog.BaseInterceptionChancePercent
                    + securityLevel * RaidThreatCatalog.MaxSecurityInterceptionBonusPercent / 100
                    - (int)confederation.MilitaryStrength.Band * RaidThreatCatalog.MilitaryStrengthBandInterceptionPenaltyPercent,
                0, 100);
            var intercepted = context.RandomStreams.NextUInt(StreamName, 100) < (uint)interceptionChance;

            RaidOutcome outcome;
            var spoilsLost = Money.Zero;
            if (intercepted)
            {
                var captured = context.RandomStreams.NextUInt(StreamName, 100) < (uint)RaidThreatCatalog.CaptureGivenInterceptedChancePercent;
                outcome = captured ? RaidOutcome.RaidersCaptured : RaidOutcome.InterceptedRepelled;
            }
            else
            {
                outcome = RaidOutcome.RaidSucceeded;
                var baseAmount = targetType == RaidTargetType.Settlement
                    ? RaidThreatCatalog.BaseSpoilsLostDenarii * RaidThreatCatalog.SettlementRaidSpoilsMultiplierPercent / 100
                    : RaidThreatCatalog.BaseSpoilsLostDenarii;
                var reductionPercent = securityLevel * RaidThreatCatalog.MaxSecuritySpoilsReductionPercent / 100;
                var finalAmount = Math.Max(0, baseAmount - baseAmount * reductionPercent / 100);
                spoilsLost = Money.FromDenarii(finalAmount);
            }

            var raidId = state.RaidThreatIds.Issue();
            var raid = RaidThreat.Create(
                raidId, confederationActorId, targetEntry.Key, targetType, securityLevel, outcome, spoilsLost, context.Date);
            state.RaidThreats.Add(raidId, raid);

            if (outcome == RaidOutcome.RaidSucceeded && spoilsLost > Money.Zero)
            {
                var account = LedgerAccountKey.ForHousehold(targetEntry.Key);
                events.Add(LedgerService.Post(
                    state, context.Date, LedgerTransactionCategory.Treasury,
                    new[]
                    {
                        new LedgerPosting(account, -spoilsLost),
                        new LedgerPosting(LedgerAccountKey.Mint, spoilsLost),
                    },
                    reference: $"interactions.raid:{raidId.ToTaggedString()}"));
            }

            events.Add(new RaidOccurredEvent(
                state.EventIds.Issue(), context.Date, raidId, confederationActorId, targetEntry.Key, targetType, outcome, spoilsLost));

            DriftStandingTrend(state, confederationActorId, confederation, outcome, context);
        }

        return events;
    }

    /// <summary>Deterministic tie-break (ADR 0004): the lowest-<see cref="RuntimeId{T}"/> household
    /// whose living head currently resides in the Confederation's own <see
    /// cref="LivingWorldActor.RegionId"/> — never an extra RNG draw just to pick which household in
    /// range a raid actually strikes (§1's "regional risk": exposure never reaches outside a
    /// Confederation's own region).</summary>
    private static KeyValuePair<RuntimeId<Household>, HouseholdHeadship>? FindTarget(
        WorldState state, LivingWorldActor confederation, IReadOnlyList<KeyValuePair<RuntimeId<Household>, HouseholdHeadship>> households)
    {
        foreach (var entry in households)
        {
            if (!state.Characters.TryGet(entry.Value.HeadCharacterId, out var head) || !head!.IsAlive)
                continue;
            if (!state.Settlements.TryGet(head.Location, out var settlement))
                continue;
            if (settlement!.RegionId == confederation.RegionId)
                return entry;
        }

        return null;
    }

    private static void DriftStandingTrend(
        WorldState state, RuntimeId<Actor> confederationActorId, LivingWorldActor confederation, RaidOutcome outcome, MonthlyTickContext context)
    {
        if (context.RandomStreams.NextUInt(StreamName, 100) >= RaidThreatCatalog.StandingTrendShiftChancePercent)
            return;

        var newTrend = outcome == RaidOutcome.RaidSucceeded
            ? StepTowardRising(confederation.StandingTrend)
            : StepTowardDeclining(confederation.StandingTrend);
        if (newTrend == confederation.StandingTrend)
            return;

        state.Actors.Remove(confederationActorId);
        state.Actors.Add(confederationActorId, confederation with { StandingTrend = newTrend });
    }

    private static LivingWorldActorStandingTrend StepTowardRising(LivingWorldActorStandingTrend current) => current switch
    {
        LivingWorldActorStandingTrend.Declining => LivingWorldActorStandingTrend.Established,
        LivingWorldActorStandingTrend.Established => LivingWorldActorStandingTrend.Rising,
        _ => current,
    };

    private static LivingWorldActorStandingTrend StepTowardDeclining(LivingWorldActorStandingTrend current) => current switch
    {
        LivingWorldActorStandingTrend.Rising => LivingWorldActorStandingTrend.Established,
        LivingWorldActorStandingTrend.Established => LivingWorldActorStandingTrend.Declining,
        _ => current,
    };

    private static int TrendRaidChanceAdjustment(LivingWorldActorStandingTrend trend) => trend switch
    {
        LivingWorldActorStandingTrend.Rising => RaidThreatCatalog.RisingTrendRaidChanceBonusPercent,
        LivingWorldActorStandingTrend.Declining => -RaidThreatCatalog.DecliningTrendRaidChancePenaltyPercent,
        _ => 0,
    };

    /// <summary>The named random stream this system draws from for its resolution rolls (Phase 16 item
    /// 2), kept distinct from every other stream for rule 8's "adding a draw in one system must not
    /// perturb another".</summary>
    private const string StreamName = CampaignBootstrapper.RaidThreatStreamName;
}
