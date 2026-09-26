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

namespace Gens.Simulation.Activities;

/// <summary>
/// §8's living world that gathers without the player (Phase 17 item 4): "any Living World Actor with
/// real standing... can independently convene their own Activity, entirely outside the player's own
/// initiative." Monthly, every <see cref="LivingWorldActorTier.Noteworthy"/> <see
/// cref="LivingWorldActorType.Gens"/> whose Character-backed head is alive and not already hosting
/// rolls <see cref="ActivityCatalog.NpcHostingMonthlyChancePercent"/> on its own stream. On success it
/// plans a generic Quick gathering at its head's own residence in its home settlement, one month out,
/// through the same <see cref="PlanActivityCommand"/> any player uses — "the player is never a
/// protected special case". The Guest List is the head's own warmest ties (outgoing opinion at or
/// above <see cref="ActivityCatalog.NpcInviteOpinionThreshold"/>, warmest first, capped at <see
/// cref="ActivityCatalog.NpcMaxGuests"/>): so the player receives an Invitation exactly when the
/// rival's head actually likes them (§8.1), and — through the same §4.2 exclusion check every
/// Activity runs when it begins — is pointedly left off when tied to that head but not liked enough
/// to be asked (§8.2).
///
/// §12's "NPC Phase depth" question is answered as "the same engine": an NPC-hosted Activity runs
/// the same Phases and resolves the same Outcome through <see cref="ActivityProgressSystem"/>; only
/// its Quality inputs are summarized from the house's wealth band rather than chosen.
/// </summary>
public sealed class NpcActivityHostingSystem : IMonthlySystem<WorldState>
{
    private const string StreamName = CampaignBootstrapper.ActivityNpcHostingStreamName;

    /// <summary>The Villa room an NPC host's untracked residence is recorded as.</summary>
    public const string NpcResidenceVenueKey = "triclinium";

    public string Id => "activities.npcHosting";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "actors", "characters", "relationships", "activities" };
    public IReadOnlyCollection<string> Writes { get; } =
        new[] { "activities", "activityInvitations", "activityIds", "eventIds", "commandIds", "commandSequence" };

    /// <summary>Runs after <see cref="ActivityProgressSystem"/> so a house whose gathering concluded
    /// this month may plan its next one.</summary>
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "activities.progress" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var hosts = state.Actors.InAscendingOrder()
            .Select(entry => entry.Value)
            .Where(actor => actor.ActorType == LivingWorldActorType.Gens && actor.Tier == LivingWorldActorTier.Noteworthy)
            .ToArray();

        foreach (var actor in hosts)
        {
            if (actor.HeadCharacterId is not { } headId ||
                !state.Characters.TryGet(headId, out var head) || !head.IsAlive ||
                head.GetLifecycleStage(context.Date) < LifecycleStage.Adult ||
                ActivityResolver.OpenActivityHostedBy(state, headId) is not null)
                continue;

            // Every eligible house rolls every month it stays eligible, so one house's outcome never
            // shifts another's draw (ADR 0004 / rule 8).
            if (context.RandomStreams.NextUInt(StreamName, 100) >= (uint)ActivityCatalog.NpcHostingMonthlyChancePercent)
                continue;

            var guests = GuestListFor(state, headId);
            if (guests.Count == 0)
                continue;

            var score = ActivityCatalog.NpcQualityInputScore(actor.NetWorth.Band);
            var inputs = ActivityTypeCatalog.Gathering.QualityInputs
                .Select(definition => new ActivityQualityInput(definition.Key, score))
                .ToArray();

            events.AddRange(PlanActivityCommands.Pipeline.Execute(
                state,
                new PlanActivityCommand(
                    state.CommandIds.Issue(), "system", context.Date, null, headId, null, actor.ActorId,
                    ActivityTypeCatalog.Gathering.Key, ActivityVenueKind.VillaRoom, NpcResidenceVenueKey,
                    actor.HomeSettlementId, null,
                    new GameDate(context.Date.TotalMonths + ActivityCatalog.NpcPlanningLeadMonths), 1,
                    guests, inputs, Money.Zero)).Events);
        }

        return events;
    }

    /// <summary>The head's warmest living ties, warmest first then ascending ID.</summary>
    public static IReadOnlyList<RuntimeId<Character>> GuestListFor(WorldState state, RuntimeId<Character> headId) =>
        state.Relationships.InAscendingOrder()
            .Where(entry => entry.Key.From == headId && entry.Key.To != headId)
            .Where(entry => entry.Value.Opinion >= ActivityCatalog.NpcInviteOpinionThreshold)
            .Where(entry => state.Characters.TryGet(entry.Key.To, out var guest) && guest.IsAlive)
            .OrderByDescending(entry => entry.Value.Opinion)
            .ThenBy(entry => entry.Key.To)
            .Take(ActivityCatalog.NpcMaxGuests)
            .Select(entry => entry.Key.To)
            .ToArray();
}
