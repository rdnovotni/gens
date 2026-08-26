using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Actors;

/// <summary>Emitted when a <see cref="LivingWorldActor"/> goes extinct (Phase 10 item 4;
/// <c>gens-rival-houses-design.md</c> §5.3: "a house goes extinct when its line runs out entirely").
/// Public visibility — the same "Notable Families of the Region" ambient legibility §7 already treats
/// house-level facts as generally knowable applies here. Deliberately does not resolve the extinct
/// actor's Holdings: §5.3 hands that off case-by-case to whichever future system fits (Legal &amp;
/// Court, a Politics &amp; Patronage land grant, Military &amp; Combat conquest) — none of those exist
/// yet, so this event only records that the trigger fired, for whichever of them picks it up later.</summary>
public sealed record LivingWorldActorExtinguishedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Actor> ActorId,
    string ActorName,
    LivingWorldActorTier Tier) : IDomainEvent
{
    public string Type => "actors.extinguished";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ActorId.ToTaggedString() };
    public string? CausationId => null;
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// The monthly check that retires a <see cref="LivingWorldActor"/> whose line has run out (Phase 10
/// item 4's remaining "retirement/extinction" piece; <c>gens-rival-houses-design.md</c> §5.3). Two
/// distinct checks, one per tier, since only a <see cref="LivingWorldActorTier.Noteworthy"/> actor has
/// an actual head Character to check genealogy against:
///
/// <list type="bullet">
/// <item>A Noteworthy actor's head, once dead (<see cref="Character.IsAlive"/> false), is checked for
/// any living descendant by walking <see cref="Character.MotherId"/>/<see cref="Character.FatherId"/>
/// breadth-first — this reuses the genealogy Familia already tracks rather than a parallel heir concept;
/// full heir eligibility/designation rules are Phase 11's job
/// (<c>gens-comprehensive-build-roadmap.md</c> Phase 11 item 1), so "any living descendant at all" is
/// this phase's deliberately coarse stand-in for "no viable heir".</item>
/// <item>A Background actor has no head Character to check at all, so it instead takes the same kind of
/// abstract roll <see cref="BackgroundHouseDriftSystem"/> already uses for fortune drift — see <see
/// cref="LivingWorldActorDriftCatalog.BackgroundExtinctionChancePercent"/>.</item>
/// </list>
///
/// An extinguished actor is removed from <see cref="WorldState.Actors"/> outright (there is nothing to
/// freeze into a lighter tier — extinction is terminal, unlike ordinary demotion) along with every <see
/// cref="HouseStanding"/> entry naming it, so no later lookup can resolve a standing against an actor
/// that no longer exists. <see cref="RivalHouseCreationService"/>'s <i>novus homo</i> and cadet-branch
/// paths (§2.2) are the roster's existing replenishment side of this same "the world needs to replenish,
/// not just thin out" balance — no new replenishment work is needed here.
/// </summary>
public sealed class LivingWorldActorExtinctionSystem : IMonthlySystem<WorldState>
{
    public string Id => "actors.extinction";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "actors", "characters", "houseStandings" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "actors", "houseStandings" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: extinguishing an actor mutates state.Actors and state.HouseStandings
        // mid-iteration, matching BackgroundHouseDriftSystem's identical "snapshot before mutating" guard.
        var actors = state.Actors.InAscendingOrder().ToArray();

        // Background actors are budgeted the same way BackgroundHouseDriftSystem's own roll is (Phase
        // 10 item 7) — a rotating start offset so the per-tick cap doesn't let the same low-ID prefix
        // monopolize every roll forever.
        var backgroundActors = actors.Where(entry => entry.Value.Tier == LivingWorldActorTier.Background).ToArray();
        var budget = Math.Min(LivingWorldActorDriftCatalog.MaxBackgroundActorsProcessedPerTick, backgroundActors.Length);
        var startIndex = backgroundActors.Length == 0
            ? 0
            : (int)(((long)context.Date.TotalMonths % backgroundActors.Length + backgroundActors.Length) % backgroundActors.Length);

        for (var offset = 0; offset < budget; offset++)
        {
            var (actorId, actor) = backgroundActors[(startIndex + offset) % backgroundActors.Length];
            if (actor.StandingTrend != LivingWorldActorStandingTrend.Declining)
                continue;
            if (context.RandomStreams.NextUInt(StreamName, 100) >= LivingWorldActorDriftCatalog.BackgroundExtinctionChancePercent)
                continue;

            Extinguish(state, actorId, actor, context.Date, events);
        }

        // Noteworthy actors are never numerous enough (they exist only once real player contact
        // occurs, §2.3) to need a tick budget — every one with a dead head is checked every tick.
        foreach (var (actorId, actor) in actors)
        {
            if (actor.Tier != LivingWorldActorTier.Noteworthy)
                continue;
            if (actor.HeadCharacterId is not { } headId || !state.Characters.TryGet(headId, out var head))
                continue;
            if (head!.IsAlive || HasLivingDescendant(state, headId))
                continue;

            Extinguish(state, actorId, actor, context.Date, events);
        }

        return events;
    }

    /// <summary>Breadth-first walk of <see cref="Character.MotherId"/>/<see cref="Character.FatherId"/>
    /// looking for any living descendant of <paramref name="ancestorId"/> — this codebase's only
    /// genealogy record is "who are my parents", so finding children/grandchildren means scanning for
    /// characters that name this ancestor (or an already-found descendant) as a parent, rather than
    /// reading a children list directly off the ancestor.</summary>
    private static bool HasLivingDescendant(WorldState state, RuntimeId<Character> ancestorId)
    {
        var frontier = new Queue<RuntimeId<Character>>();
        frontier.Enqueue(ancestorId);
        var visited = new HashSet<RuntimeId<Character>> { ancestorId };

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            foreach (var (childId, child) in state.Characters.InAscendingOrder())
            {
                if (child.MotherId != current && child.FatherId != current)
                    continue;
                if (child.IsAlive)
                    return true;
                if (visited.Add(childId))
                    frontier.Enqueue(childId);
            }
        }

        return false;
    }

    private static void Extinguish(
        WorldState state, RuntimeId<Actor> actorId, LivingWorldActor actor, GameDate occurredDate, List<IDomainEvent> events)
    {
        state.Actors.Remove(actorId);

        foreach (var entry in state.HouseStandings.InAscendingOrder().ToArray())
        {
            if (entry.Key.ActorAId == actorId || entry.Key.ActorBId == actorId)
                state.HouseStandings.Remove(entry.Key);
        }

        events.Add(new LivingWorldActorExtinguishedEvent(state.EventIds.Issue(), occurredDate, actorId, actor.Name, actor.Tier));
    }

    /// <summary>The named random stream this system draws from for its Background-tier extinction roll
    /// (Phase 10 item 4), kept distinct from every other stream for rule 8's "adding a draw in one
    /// system must not perturb another".</summary>
    private const string StreamName = CampaignBootstrapper.ActorExtinctionStreamName;
}
