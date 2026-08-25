using Gens.Simulation.Actions;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Actors;

/// <summary>
/// The Noteworthy-tier decision loop (Phase 10 item 4/1; roadmap step 1's own words: "reusable AI
/// considerations and action selection against the same action definitions used by the player").
/// Every processed head's Ambition (<see cref="Condition.Ambition"/>) and Boldness (<see
/// cref="PersonalityAxis.Boldness"/>, from held Traits) drive whether it acts at all this month
/// (<c>gens-characters-design.md</c> §8.3); which action it takes, among candidate house-standing
/// targets it already has a tracked relationship with, comes from <see cref="ActionSelector"/> ranking
/// <see cref="RivalHouseActionDefinitions.BuildCatalog"/> exactly as a player-facing UI would. The
/// chosen action is then submitted as a real <see cref="AdjustHouseStandingCommand"/> through the
/// ordinary <see cref="AdjustHouseStandingCommands.Pipeline"/> — satisfying the Phase 10 exit gate's
/// "their actions use legal commands." Background-tier actors are untouched here — <see
/// cref="BackgroundHouseDriftSystem"/> is their own, much shallower, tick.
/// </summary>
public sealed class RivalAmbitionSystem : IMonthlySystem<WorldState>
{
    private readonly ActionCatalog _catalog;
    private readonly TraitCatalog _traitCatalog;
    private readonly string _randomStreamName;

    public RivalAmbitionSystem(ActionCatalog catalog, TraitCatalog traitCatalog, string randomStreamName)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _traitCatalog = traitCatalog ?? throw new ArgumentNullException(nameof(traitCatalog));
        _randomStreamName = string.IsNullOrEmpty(randomStreamName)
            ? throw new ArgumentException("A random stream name is required.", nameof(randomStreamName))
            : randomStreamName;
    }

    public string Id => "actors.rivalAmbition";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "actors", "characters", "houseStandings" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "actors", "houseStandings" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "actors.backgroundHouseDrift" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: a chosen command's Mutate step replaces entries in state.Actors and
        // state.HouseStandings mid-iteration, matching RelationshipDecaySystem's identical guard.
        var noteworthyActors = state.Actors.InAscendingOrder()
            .Where(entry => entry.Value.Tier == LivingWorldActorTier.Noteworthy && entry.Value.HeadCharacterId is not null)
            .ToArray();

        foreach (var (actorId, actor) in noteworthyActors)
        {
            if (!state.Characters.TryGet(actor.HeadCharacterId!.Value, out var head))
                continue;

            if (!RollsToAct(head!, context))
                continue;

            var candidateTargets = FindCandidateTargets(state, actorId);
            if (candidateTargets.Count == 0)
                continue;

            var best = candidateTargets
                .SelectMany(targetId => ActionSelector.Rank(
                    state, _catalog, new ActionInvocation(actorId.ToTaggedString(), targetId.ToTaggedString(), context.Date)))
                .OrderByDescending(candidate => candidate.Score)
                .ThenBy(candidate => candidate.Definition.Id.Value, StringComparer.Ordinal)
                .ThenBy(candidate => candidate.Invocation.TargetId, StringComparer.Ordinal)
                .FirstOrDefault();

            if (best.Definition is null)
                continue;

            var direction = RivalHouseActionDefinitions.ToDirection(best.Definition.Id);
            var command = new AdjustHouseStandingCommand(
                state.CommandIds.Issue(), actorId.ToTaggedString(), context.Date, CausationId: null,
                actorId, RuntimeId<Actor>.Parse(best.Invocation.TargetId!), direction);

            var result = AdjustHouseStandingCommands.Pipeline.Execute(state, command);
            if (result.Accepted)
                events.AddRange(result.Events);
        }

        return events;
    }

    private bool RollsToAct(Character head, MonthlyTickContext context)
    {
        var boldnessAxis = _traitCatalog.GetAxisScore(head.Traits, PersonalityAxis.Boldness);
        var boldnessNormalized = (boldnessAxis + 100) / 2;

        var actChancePercent = Math.Clamp(
            RivalAmbitionCatalog.BaseActChancePercent
                + head.Condition.Ambition * RivalAmbitionCatalog.AmbitionWeightPercent / 100
                + boldnessNormalized * RivalAmbitionCatalog.BoldnessWeightPercent / 100,
            0, 100);

        return context.RandomStreams.NextUInt(_randomStreamName, 100) < (uint)actChancePercent;
    }

    /// <summary>Every other actor <paramref name="actorId"/> already has a tracked <see
    /// cref="HouseStanding"/> with — a natural, already-bounded candidate set (reusing package 2's
    /// storage) rather than a combinatorial scan of every actor in the campaign.</summary>
    private static List<RuntimeId<Actor>> FindCandidateTargets(WorldState state, RuntimeId<Actor> actorId)
    {
        var targets = new List<RuntimeId<Actor>>();
        foreach (var entry in state.HouseStandings.InAscendingOrder())
        {
            if (entry.Key.ActorAId == actorId)
                targets.Add(entry.Key.ActorBId);
            else if (entry.Key.ActorBId == actorId)
                targets.Add(entry.Key.ActorAId);
        }

        return targets;
    }
}
