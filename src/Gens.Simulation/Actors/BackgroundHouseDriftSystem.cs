using Gens.Simulation.Campaign;
using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Actors;

/// <summary>
/// The Background-tier abstract monthly tick (Phase 10 item 3/7; <c>gens-rival-houses-design.md</c>
/// §2.1: "Background Houses... evolve via periodic abstract rolls... no full parallel economy/politics
/// tick"). Deliberately shallow — the only two things that ever move are a Background actor's <see
/// cref="LivingWorldActorNetWorth.Band"/> and its <see cref="LivingWorldActorStandingTrend"/> itself;
/// individual births/deaths/marriages within an unnamed house are never simulated (§2.1's own "head
/// not generated as full Character until needed" already rules that out for the head alone, let alone
/// the rest of the household). Silent like <see cref="Characters.RelationshipDecaySystem"/>'s own
/// monthly drift — no domain events, since this is background flavor, not a report-worthy occurrence;
/// package 8's noteworthy-tier work is what actually surfaces rival activity to the player.
/// <see cref="LivingWorldActorTier.Noteworthy"/> actors are skipped entirely — package 7 gives those
/// their own real decision loop through the ordinary action/command path instead.
/// </summary>
public sealed class BackgroundHouseDriftSystem : IMonthlySystem<WorldState>
{
    public string Id => "actors.backgroundHouseDrift";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "actors" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "actors" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        // Materialize first: the loop body replaces entries in state.Actors mid-iteration, matching
        // RelationshipDecaySystem's identical "snapshot before mutating" guard. Restricted to Gens
        // actors: Phase 12 item 6 starts creating LivingWorldActorType.Collegium entries, which this
        // Rival-Houses-specific fortune drift (and the StandingTrend swings that feed the extinction
        // system's own background-tier roll) was never written to model — a Collegium's own legal
        // standing (Collegia.CollegiumLegalStatus) is a distinct, deliberately separate axis.
        var backgroundActors = state.Actors.InAscendingOrder()
            .Where(entry => entry.Value.Tier == LivingWorldActorTier.Background && entry.Value.ActorType == LivingWorldActorType.Gens)
            .ToArray();

        if (backgroundActors.Length == 0)
            return Array.Empty<IDomainEvent>();

        // A rotating start offset (Phase 10 item 7's tick budget) so that when the population exceeds
        // the per-tick cap, every house still gets processed roughly as often as every other one over
        // time, rather than the same low-ID prefix monopolizing every roll forever.
        var budget = Math.Min(LivingWorldActorDriftCatalog.MaxBackgroundActorsProcessedPerTick, backgroundActors.Length);
        var startIndex = (int)(((long)context.Date.TotalMonths % backgroundActors.Length + backgroundActors.Length) % backgroundActors.Length);

        for (var offset = 0; offset < budget; offset++)
        {
            var index = (startIndex + offset) % backgroundActors.Length;
            var (actorId, actor) = backgroundActors[index];
            var drifted = DriftOne(actor, context);
            if (!ReferenceEquals(drifted, actor) && drifted != actor)
            {
                state.Actors.Remove(actorId);
                state.Actors.Add(actorId, drifted);
            }
        }

        return Array.Empty<IDomainEvent>();
    }

    private static LivingWorldActor DriftOne(LivingWorldActor actor, MonthlyTickContext context)
    {
        var trend = actor.StandingTrend;
        if (context.RandomStreams.NextUInt(StreamName, 100) < LivingWorldActorDriftCatalog.StandingTrendDriftChancePercent)
            trend = DriftTrend(trend, context);

        var netWorth = actor.NetWorth;
        var direction = trend switch
        {
            LivingWorldActorStandingTrend.Rising => 1,
            LivingWorldActorStandingTrend.Declining => -1,
            _ => 0,
        };
        if (direction != 0 && context.RandomStreams.NextUInt(StreamName, 100) < LivingWorldActorDriftCatalog.NetWorthDriftChancePercent)
            netWorth = netWorth with { Band = StepBand(netWorth.Band, direction) };

        return actor with { StandingTrend = trend, NetWorth = netWorth };
    }

    private static LivingWorldActorStandingTrend DriftTrend(LivingWorldActorStandingTrend current, MonthlyTickContext context) =>
        current switch
        {
            LivingWorldActorStandingTrend.Rising => LivingWorldActorStandingTrend.Established,
            LivingWorldActorStandingTrend.Declining => LivingWorldActorStandingTrend.Established,
            LivingWorldActorStandingTrend.Established => context.RandomStreams.NextUInt(StreamName, 2) == 0
                ? LivingWorldActorStandingTrend.Rising
                : LivingWorldActorStandingTrend.Declining,
            _ => current,
        };

    private static HouseholdWealthBand StepBand(HouseholdWealthBand band, int direction)
    {
        var next = Math.Clamp((int)band + direction, (int)HouseholdWealthBand.Ruined, (int)HouseholdWealthBand.Wealthy);
        return (HouseholdWealthBand)next;
    }

    /// <summary>The named random stream this system draws from (Phase 10 item 3), kept distinct from
    /// every other stream for rule 8's "adding a draw in one system must not perturb another".</summary>
    private const string StreamName = CampaignBootstrapper.BackgroundHouseDriftStreamName;
}
