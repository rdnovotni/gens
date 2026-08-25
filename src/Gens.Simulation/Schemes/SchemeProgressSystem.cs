using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Schemes;

/// <summary>
/// Stages 2-5 of the generic Scheme engine (Phase 10 item 12; <c>gens-characters-design.md</c> §10):
/// every month, each <see cref="SchemeStage.Progressing"/> scheme rolls Progress (initiator's Intrigue
/// + Boldness) and Discovery Risk (target's Intrigue, plus a leak-risk bonus when an assisting agent
/// is involved) forward; crossing <see cref="SchemeProgressCatalog.DiscoveryThreshold"/> moves it to
/// <see cref="SchemeStage.AwaitingCounterPlay"/> (stage 4), where it waits up to <see
/// cref="SchemeProgressCatalog.CounterPlayWindowMonths"/> months for a <see
/// cref="CounterPlaySchemeCommand"/> before resolving on its own. Reaching full Progress first (while
/// still undiscovered) rolls one final success check (stage 5). Usable by both a player-initiated and
/// an NPC-initiated scheme alike — this system reads only <see cref="SchemeInstance"/> and the two
/// Characters involved, never anything actor-specific.
/// </summary>
public sealed class SchemeProgressSystem : IMonthlySystem<WorldState>
{
    private readonly TraitCatalog _traitCatalog;
    private readonly string _randomStreamName;

    public SchemeProgressSystem(TraitCatalog traitCatalog, string randomStreamName)
    {
        _traitCatalog = traitCatalog ?? throw new ArgumentNullException(nameof(traitCatalog));
        _randomStreamName = string.IsNullOrEmpty(randomStreamName)
            ? throw new ArgumentException("A random stream name is required.", nameof(randomStreamName))
            : randomStreamName;
    }

    public string Id => "schemes.progress";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "schemes", "characters" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "schemes" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: the loop body replaces entries in state.Schemes mid-iteration, matching
        // every other system's identical "snapshot before mutating" guard in this codebase.
        var activeSchemes = state.Schemes.InAscendingOrder()
            .Where(entry => entry.Value.Stage != SchemeStage.Resolved)
            .ToArray();

        foreach (var (schemeId, scheme) in activeSchemes)
            events.AddRange(AdvanceOne(state, schemeId, scheme, context));

        return events;
    }

    private IDomainEvent[] AdvanceOne(WorldState state, RuntimeId<SchemeInstance> schemeId, SchemeInstance scheme, MonthlyTickContext context)
    {
        if (scheme.Stage == SchemeStage.AwaitingCounterPlay)
            return ResolveIfCounterPlayWindowElapsed(state, schemeId, scheme, context);

        // Stage.Progressing: roll both tracks forward.
        state.Characters.TryGet(scheme.InitiatorCharacterId, out var initiator);
        state.Characters.TryGet(scheme.TargetCharacterId, out var target);
        if (initiator is null || target is null)
            return Array.Empty<IDomainEvent>();

        var boldness = _traitCatalog.GetAxisScore(initiator.Traits, PersonalityAxis.Boldness);
        var boldnessNormalized = (boldness + 100) / 2;
        var progressGain = SchemeProgressCatalog.BaseProgressPerMonth
            + (initiator.Attributes.Intrigue * SchemeProgressCatalog.IntrigueProgressWeight / 100)
            + (boldnessNormalized * SchemeProgressCatalog.BoldnessProgressWeight / 100);

        var discoveryGain = SchemeProgressCatalog.BaseDiscoveryPerMonth
            + (target.Attributes.Intrigue * SchemeProgressCatalog.TargetIntrigueDiscoveryWeight / 100)
            + (scheme.AssistingAgentCharacterId is null ? 0 : SchemeProgressCatalog.AssistingAgentDiscoveryBonus);

        var newProgress = Math.Min(100, scheme.Progress + progressGain);
        var newDiscoveryRisk = Math.Min(100, scheme.DiscoveryRisk + discoveryGain);

        if (newDiscoveryRisk >= SchemeProgressCatalog.DiscoveryThreshold)
        {
            var awaiting = scheme with
            {
                Progress = newProgress,
                DiscoveryRisk = newDiscoveryRisk,
                Stage = SchemeStage.AwaitingCounterPlay,
                CounterPlayDeadline = new GameDate(context.Date.TotalMonths + SchemeProgressCatalog.CounterPlayWindowMonths),
            };
            Replace(state, schemeId, awaiting);
            return Array.Empty<IDomainEvent>();
        }

        if (newProgress >= 100)
            return RollFinalOutcome(state, schemeId, scheme with { Progress = newProgress, DiscoveryRisk = newDiscoveryRisk }, context, discovered: false);

        Replace(state, schemeId, scheme with { Progress = newProgress, DiscoveryRisk = newDiscoveryRisk });
        return Array.Empty<IDomainEvent>();
    }

    private IDomainEvent[] ResolveIfCounterPlayWindowElapsed(
        WorldState state, RuntimeId<SchemeInstance> schemeId, SchemeInstance scheme, MonthlyTickContext context)
    {
        if (scheme.CounterPlayDeadline is not { } deadline || context.Date.TotalMonths < deadline.TotalMonths)
            return Array.Empty<IDomainEvent>();

        return scheme.Progress >= 100
            ? RollFinalOutcome(state, schemeId, scheme, context, discovered: true)
            : Resolve(state, schemeId, scheme, context.Date, SchemeOutcome.DiscoveredAndFoiled);
    }

    private IDomainEvent[] RollFinalOutcome(
        WorldState state, RuntimeId<SchemeInstance> schemeId, SchemeInstance scheme, MonthlyTickContext context, bool discovered)
    {
        var succeeded = context.RandomStreams.NextUInt(_randomStreamName, 100) < (uint)SchemeProgressCatalog.MaxSuccessChancePercent;
        var outcome = (succeeded, discovered) switch
        {
            (true, true) => SchemeOutcome.DiscoveredAndEscalated,
            (true, false) => SchemeOutcome.Succeeded,
            (false, true) => SchemeOutcome.DiscoveredAndFoiled,
            (false, false) => SchemeOutcome.FailedQuietly,
        };
        return Resolve(state, schemeId, scheme, context.Date, outcome);
    }

    private static IDomainEvent[] Resolve(
        WorldState state, RuntimeId<SchemeInstance> schemeId, SchemeInstance scheme, GameDate date, SchemeOutcome outcome)
    {
        var resolved = scheme with { Stage = SchemeStage.Resolved, Outcome = outcome, ResolvedDate = date };
        Replace(state, schemeId, resolved);

        return new IDomainEvent[]
        {
            new SchemeResolvedEvent(state.EventIds.Issue(), date, schemeId, scheme.InitiatorCharacterId, scheme.TargetCharacterId, outcome, null),
        };
    }

    private static void Replace(WorldState state, RuntimeId<SchemeInstance> schemeId, SchemeInstance updated)
    {
        state.Schemes.Remove(schemeId);
        state.Schemes.Add(schemeId, updated);
    }
}
