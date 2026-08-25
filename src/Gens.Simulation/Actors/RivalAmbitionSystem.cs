using Gens.Simulation.Actions;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Schemes;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Actors;

/// <summary>Which underlying command a chosen candidate ultimately submits — <see
/// cref="RivalAmbitionSystem"/> merges two differently-shaped catalogs (house-standing actions target
/// another <see cref="LivingWorldActor"/>; scheme actions target another <see
/// cref="Characters.Character"/>) into one ranking, so this tag is what tells the caller which command
/// constructor to use for the winning candidate.</summary>
internal enum RivalCandidateKind
{
    HouseStanding,
    Scheme,
}

internal readonly record struct RivalCandidate(RivalCandidateKind Kind, ScoredActionCandidate Scored);

/// <summary>
/// The Noteworthy-tier decision loop (Phase 10 items 1/4/13; roadmap step 1's own words: "reusable AI
/// considerations and action selection against the same action definitions used by the player").
/// Every processed head's Ambition (<see cref="Condition.Ambition"/>) and Boldness (<see
/// cref="PersonalityAxis.Boldness"/>, from held Traits) drive whether it acts at all this month
/// (<c>gens-characters-design.md</c> §8.3); which action it takes comes from <see
/// cref="ActionSelector"/> ranking two catalogs exactly as a player-facing UI would: <see
/// cref="RivalHouseActionDefinitions.BuildCatalog"/> against every other actor it already has a
/// tracked <see cref="HouseStanding"/> with, and <see cref="Schemes.SchemeActionDefinitions.BuildCatalog"/>
/// against those same actors' own head Characters, when they have one (Phase 10 item 13 — "wire the
/// scheme engine into rival ambition"). The chosen action is then submitted as a real command through
/// its own ordinary pipeline — satisfying the Phase 10 exit gate's "their actions use legal commands."
/// Background-tier actors are untouched here — <see cref="BackgroundHouseDriftSystem"/> is their own,
/// much shallower, tick.
/// </summary>
public sealed class RivalAmbitionSystem : IMonthlySystem<WorldState>
{
    private readonly ActionCatalog _standingCatalog;
    private readonly ActionCatalog _schemeCatalog;
    private readonly TraitCatalog _traitCatalog;
    private readonly string _randomStreamName;

    public RivalAmbitionSystem(ActionCatalog standingCatalog, ActionCatalog schemeCatalog, TraitCatalog traitCatalog, string randomStreamName)
    {
        _standingCatalog = standingCatalog ?? throw new ArgumentNullException(nameof(standingCatalog));
        _schemeCatalog = schemeCatalog ?? throw new ArgumentNullException(nameof(schemeCatalog));
        _traitCatalog = traitCatalog ?? throw new ArgumentNullException(nameof(traitCatalog));
        _randomStreamName = string.IsNullOrEmpty(randomStreamName)
            ? throw new ArgumentException("A random stream name is required.", nameof(randomStreamName))
            : randomStreamName;
    }

    public string Id => "actors.rivalAmbition";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "actors", "characters", "houseStandings", "schemes" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "actors", "houseStandings", "schemes" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "actors.backgroundHouseDrift" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: a chosen command's Mutate step replaces entries in state.Actors,
        // state.HouseStandings, and state.Schemes mid-iteration, matching RelationshipDecaySystem's
        // identical guard.
        var noteworthyActors = state.Actors.InAscendingOrder()
            .Where(entry => entry.Value.Tier == LivingWorldActorTier.Noteworthy && entry.Value.HeadCharacterId is not null)
            .ToArray();

        foreach (var (actorId, actor) in noteworthyActors)
        {
            if (!state.Characters.TryGet(actor.HeadCharacterId!.Value, out var head))
                continue;

            if (!RollsToAct(head!, context))
                continue;

            var candidateActorTargets = FindCandidateActorTargets(state, actorId);
            if (candidateActorTargets.Count == 0)
                continue;

            var standingCandidates = candidateActorTargets
                .SelectMany(targetId => ActionSelector.Rank(
                    state, _standingCatalog, new ActionInvocation(actorId.ToTaggedString(), targetId.ToTaggedString(), context.Date)))
                .Select(scored => new RivalCandidate(RivalCandidateKind.HouseStanding, scored));

            var candidateCharacterTargets = candidateActorTargets
                .Select(targetActorId => state.Actors.TryGet(targetActorId, out var targetActor) ? targetActor!.HeadCharacterId : null)
                .Where(headId => headId is not null)
                .Select(headId => headId!.Value)
                .Distinct();

            var schemeCandidates = candidateCharacterTargets
                .SelectMany(targetCharacterId => ActionSelector.Rank(
                    state, _schemeCatalog, new ActionInvocation(head!.Id.ToTaggedString(), targetCharacterId.ToTaggedString(), context.Date)))
                .Select(scored => new RivalCandidate(RivalCandidateKind.Scheme, scored));

            var best = standingCandidates.Concat(schemeCandidates)
                .OrderByDescending(candidate => candidate.Scored.Score)
                .ThenBy(candidate => candidate.Scored.Definition.Id.Value, StringComparer.Ordinal)
                .ThenBy(candidate => candidate.Scored.Invocation.TargetId, StringComparer.Ordinal)
                .FirstOrDefault();

            if (best.Scored.Definition is null)
                continue;

            var result = best.Kind == RivalCandidateKind.HouseStanding
                ? ExecuteHouseStanding(state, actorId, best.Scored, context.Date)
                : ExecuteScheme(state, head!.Id, best.Scored, context.Date);

            if (result.Accepted)
                events.AddRange(result.Events);
        }

        return events;
    }

    private static CommandResult ExecuteHouseStanding(WorldState state, RuntimeId<Actor> actorId, ScoredActionCandidate best, GameDate date)
    {
        var direction = RivalHouseActionDefinitions.ToDirection(best.Definition.Id);
        var command = new AdjustHouseStandingCommand(
            state.CommandIds.Issue(), actorId.ToTaggedString(), date, CausationId: null,
            actorId, RuntimeId<Actor>.Parse(best.Invocation.TargetId!), direction);
        return AdjustHouseStandingCommands.Pipeline.Execute(state, command);
    }

    private static CommandResult ExecuteScheme(WorldState state, RuntimeId<Character> headId, ScoredActionCandidate best, GameDate date)
    {
        var schemeType = SchemeActionDefinitions.ToSchemeType(best.Definition.Id);
        var command = new InitiateSchemeCommand(
            state.CommandIds.Issue(), headId.ToTaggedString(), date, CausationId: null,
            schemeType, headId, RuntimeId<Character>.Parse(best.Invocation.TargetId!), AssistingAgentCharacterId: null);
        return SchemeCommands.InitiatePipeline.Execute(state, command);
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
    private static List<RuntimeId<Actor>> FindCandidateActorTargets(WorldState state, RuntimeId<Actor> actorId)
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
