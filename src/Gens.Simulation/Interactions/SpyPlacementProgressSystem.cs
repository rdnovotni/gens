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
using Gens.Simulation.Time;

namespace Gens.Simulation.Interactions;

/// <summary>Emitted whenever a <see cref="SpyPlacement"/> leaves <see
/// cref="SpyPlacementStatus.InProgress"/>. Private to sponsor and spy, like <see
/// cref="SpyPlacedEvent"/> — surfacing a discovered placement to anyone beyond them (a rumor, a
/// Chronicle entry) is a future consumer's job, not this engine's.</summary>
public sealed record SpyPlacementResolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<SpyPlacement> PlacementId,
    RuntimeId<Character> SpyCharacterId,
    RuntimeId<Character> SponsoringCharacterId,
    SpyPlacementStatus Status) : IDomainEvent
{
    public string Type => "interactions.spyPlacementResolved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SponsoringCharacterId.ToTaggedString(), SpyCharacterId.ToTaggedString() };
    public string? CausationId => null;
    public Visibility Visibility => Visibility.Private(SponsoringCharacterId.ToTaggedString(), SpyCharacterId.ToTaggedString());
}

/// <summary>
/// The monthly Discovery-Risk/Discovery/Traceability tick for every <see
/// cref="SpyPlacementStatus.InProgress"/> <see cref="SpyPlacement"/> (Phase 16 item 1;
/// <c>gens-espionage-design.md</c> §2, §6), mirroring <see cref="SchemeProgressSystem"/>'s shape:
///
/// <list type="number">
/// <item>A dead spy, a dead sponsor, or a target actor that no longer exists ends the placement
/// immediately as <see cref="SpyPlacementStatus.Withdrawn"/> rather than continuing to run for
/// nobody, mirroring <see cref="SchemeProgressSystem"/>'s identical liveness guard.</item>
/// <item><see cref="SpyPlacementType.QuickOp"/> resolves this same tick: a single Discovery roll (<see
/// cref="SpyPlacementCatalog.QuickOpBaseDiscoveryChancePercent"/>, weighted by the placement's own <see
/// cref="SpyPlacement.ConcealmentQuality"/>); undiscovered resolves <see
/// cref="SpyPlacementStatus.Succeeded"/> or <see cref="SpyPlacementStatus.FailedQuietly"/>, discovered
/// immediately rolls Traceability and resolves <see cref="SpyPlacementStatus.DiscoveredUntraced"/> or
/// <see cref="SpyPlacementStatus.DiscoveredAndTraced"/>.</item>
/// <item><see cref="SpyPlacementType.PersistentNetwork"/> advances <see
/// cref="SpyPlacement.DiscoveryRisk"/> by <see cref="SpyPlacementCatalog.BaseDiscoveryRiskPerMonthPercent"/>
/// plus a bonus scaled by the target actor's head Character's Intrigue, minus a reduction scaled by the
/// placement's own Concealment. Crossing <see cref="SpyPlacementCatalog.DiscoveryRiskThresholdPercent"/>
/// triggers the same Discovery-then-Traceability sequence as a Quick Op; otherwise <see
/// cref="SpyPlacement.MonthsActive"/> increments and <see
/// cref="SpyPlacementCatalog.PersistentNetworkUpkeepPerMonthDenarii"/> is posted against the sponsor's
/// household ledger.</item>
/// <item>On any terminal resolution, <see cref="Actors.RivalDossierRefresh"/> is invoked for the target
/// actor only on <see cref="SpyPlacementStatus.DiscoveredAndTraced"/> — the one outcome where the
/// target legitimately learns who was behind the placement; a clean success or an untraced discovery
/// leaves the target none the wiser, so its dossier is not touched.</item>
/// </list>
///
/// Every numeric constant here is this codebase's own untuned first pass — see <see
/// cref="SpyPlacementCatalog"/>'s own doc comments.
/// </summary>
public sealed class SpyPlacementProgressSystem : IMonthlySystem<WorldState>
{
    public string Id => "interactions.spyPlacementProgress";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "spyPlacements", "characters", "actors" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "spyPlacements", "eventIds", "rivalDossiers", "ledgerAccounts", "ledgerTransactions" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: resolving a placement replaces entries in state.SpyPlacements mid-iteration,
        // matching SchemeProgressSystem's identical "snapshot before mutating" guard.
        var inProgress = state.SpyPlacements.InAscendingOrder()
            .Where(entry => entry.Value.Status == SpyPlacementStatus.InProgress)
            .ToArray();

        foreach (var (placementId, placement) in inProgress)
        {
            if (!state.Characters.TryGet(placement.SpyCharacterId, out var spy) || !spy.IsAlive ||
                !state.Characters.TryGet(placement.SponsoringCharacterId, out var sponsor) || !sponsor.IsAlive ||
                !state.Actors.TryGet(placement.TargetActorId, out var targetActor))
            {
                Resolve(state, placementId, placement, SpyPlacementStatus.Withdrawn, context.Date, events);
                continue;
            }

            var targetInvestigativeCapability = targetActor!.HeadCharacterId is { } headId && state.Characters.TryGet(headId, out var head)
                ? head!.Attributes.Intrigue
                : 0;

            if (placement.Type == SpyPlacementType.QuickOp)
            {
                var discoveryChance = Math.Clamp(
                    SpyPlacementCatalog.QuickOpBaseDiscoveryChancePercent
                        - placement.ConcealmentQuality * SpyPlacementCatalog.QuickOpDiscoveryConcealmentWeightPercent / 100,
                    0, 100);
                var discovered = context.RandomStreams.NextUInt(StreamName, 100) < (uint)discoveryChance;
                if (discovered)
                    ResolveDiscovered(state, placementId, placement, targetInvestigativeCapability, context, events);
                else
                {
                    var successChance = Math.Clamp(
                        SpyPlacementCatalog.QuickOpBaseSuccessChancePercent
                            + placement.ConcealmentQuality * SpyPlacementCatalog.QuickOpSuccessConcealmentWeightPercent / 100,
                        0, 100);
                    var succeeded = context.RandomStreams.NextUInt(StreamName, 100) < (uint)successChance;
                    Resolve(state, placementId, placement, succeeded ? SpyPlacementStatus.Succeeded : SpyPlacementStatus.FailedQuietly, context.Date, events);
                }

                continue;
            }

            var riskDelta = SpyPlacementCatalog.BaseDiscoveryRiskPerMonthPercent
                + targetInvestigativeCapability * SpyPlacementCatalog.MaxTargetInvestigativeRiskBonusPercent / 100
                - placement.ConcealmentQuality * SpyPlacementCatalog.ConcealmentRiskReductionWeightPercent / 100;
            var newRisk = Math.Clamp(placement.DiscoveryRisk + riskDelta, SpyPlacement.MinValue, SpyPlacement.MaxValue);

            if (newRisk >= SpyPlacementCatalog.DiscoveryRiskThresholdPercent)
            {
                var advanced = placement with { DiscoveryRisk = newRisk };
                ResolveDiscovered(state, placementId, advanced, targetInvestigativeCapability, context, events);
                continue;
            }

            var advancedPlacement = placement with
            {
                DiscoveryRisk = newRisk,
                MonthsActive = placement.MonthsActive + 1,
                LastProgressedDate = context.Date,
            };
            state.SpyPlacements.Remove(placementId);
            state.SpyPlacements.Add(placementId, advancedPlacement);

            if (sponsor.Household is { } householdId)
            {
                var upkeep = Money.FromDenarii(SpyPlacementCatalog.PersistentNetworkUpkeepPerMonthDenarii);
                var account = LedgerAccountKey.ForHousehold(householdId);
                events.Add(LedgerService.Post(
                    state, context.Date, LedgerTransactionCategory.Wages,
                    new[]
                    {
                        new LedgerPosting(account, -upkeep),
                        new LedgerPosting(LedgerAccountKey.Mint, upkeep),
                    },
                    reference: $"interactions.spyPlacementUpkeep:{placementId.ToTaggedString()}"));
            }
        }

        return events;
    }

    /// <summary>Rolls Traceability (§6) for a placement that has just been discovered — by <see
    /// cref="SpyPlacementType.QuickOp"/>'s single roll or a <see cref="SpyPlacementType.PersistentNetwork"/>
    /// crossing <see cref="SpyPlacementCatalog.DiscoveryRiskThresholdPercent"/> — and resolves it.</summary>
    private static void ResolveDiscovered(
        WorldState state, RuntimeId<SpyPlacement> placementId, SpyPlacement placement, int targetInvestigativeCapability,
        MonthlyTickContext context, List<IDomainEvent> events)
    {
        var traceabilityChance = Math.Clamp(
            SpyPlacementCatalog.BaseTraceabilityChancePercent
                + (targetInvestigativeCapability - placement.ConcealmentQuality)
                    * SpyPlacementCatalog.TraceabilityConcealmentVsInvestigativeWeightPercent / 100,
            0, 100);
        var traced = context.RandomStreams.NextUInt(StreamName, 100) < (uint)traceabilityChance;
        Resolve(state, placementId, placement, traced ? SpyPlacementStatus.DiscoveredAndTraced : SpyPlacementStatus.DiscoveredUntraced, context.Date, events);
    }

    private static void Resolve(
        WorldState state, RuntimeId<SpyPlacement> placementId, SpyPlacement placement, SpyPlacementStatus status, GameDate date, List<IDomainEvent> events)
    {
        state.SpyPlacements.Remove(placementId);
        state.SpyPlacements.Add(placementId, placement with { Status = status, LastProgressedDate = date });
        events.Add(new SpyPlacementResolvedEvent(state.EventIds.Issue(), date, placementId, placement.SpyCharacterId, placement.SponsoringCharacterId, status));

        if (status == SpyPlacementStatus.DiscoveredAndTraced)
        {
            RivalDossierRefresh.Refresh(
                state, placement.TargetActorId, date,
                "An embedded spy targeting this house was discovered and traced back to its sponsor.");
        }
    }

    /// <summary>The named random stream this system draws from for its resolution rolls (Phase 16 item
    /// 1), kept distinct from every other stream for rule 8's "adding a draw in one system must not
    /// perturb another".</summary>
    private const string StreamName = CampaignBootstrapper.SpyPlacementProgressStreamName;
}
