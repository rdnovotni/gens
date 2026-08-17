using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Events;

/// <summary>
/// The monthly Events-phase system (Phase 9 item 3; <c>gens-events-design.md</c> §3-4, §7-8):
/// expires overdue instances, advances multi-stage instances whose next stage has come due, fires
/// every Scripted-trigger definition whose condition is now met (§4), rolls the weighted pool once
/// per eligible subject per scope (§3), and — for whichever fired/advanced instance an injected
/// policy says should not wait on a human — auto-resolves it using the same utility-scoring
/// convention <see cref="Actions.ActionDefinition.ScoreForAi"/> establishes (Phase 9 item 3's own
/// "AI/NPC resolution" requirement). Registered in <see cref="TickPhase.Events"/>, the phase this
/// project's ten fixed tick phases already reserve for exactly this system.
/// </summary>
public sealed class EventPoolSystem : IMonthlySystem<WorldState>
{
    /// <summary>The fixed "nothing happens" weight every scope-appropriate weighted roll (§3) competes
    /// against — an invented, untuned baseline (matching this codebase's own "§10's untuned-numbers"
    /// convention elsewhere) chosen so a handful of <see cref="EventWeightInputs.Flat"/>-weighted
    /// definitions fire only occasionally rather than every tick.</summary>
    public const int NoFireWeight = 970;

    private static readonly EventScope[] ScopesInOrder =
    {
        EventScope.Personal, EventScope.Household, EventScope.Settlement, EventScope.Imperial,
    };

    private readonly EventCatalog _catalog;
    private readonly string _randomStreamName;
    private readonly Func<WorldState, EventInstance, EventDefinition, bool> _shouldAutoResolve;
    private readonly CommandPipeline<WorldState, FireEventCommand> _firePipeline;
    private readonly CommandPipeline<WorldState, ResolveEventOptionCommand> _resolvePipeline;

    /// <param name="catalog">Every registered <see cref="EventDefinition"/>.</param>
    /// <param name="randomStreamName">The named <see cref="Random.RandomStreamSet"/> stream this
    /// system's weighted rolls draw from exclusively (rule 8) — must already be registered on any
    /// <see cref="Random.RandomStreamSet"/> this system ticks against.</param>
    /// <param name="shouldAutoResolve">Decides whether a given newly-current stage should be resolved
    /// immediately by <see cref="EventOptionDefinition.ScoreForAi"/> rather than left <see
    /// cref="EventInstanceStatus.Pending"/> for a human to act on. Defaults to "always" — no
    /// player/steward distinction exists in <see cref="WorldState"/> yet (that is Phase 10's own
    /// item 1/2 work), so until a caller supplies a real policy, every fired instance auto-resolves,
    /// matching how <see cref="Policies.PolicyActionDefinitions"/> stands in a fixed baseline for a
    /// concept ahead of the phase that gives it real content.</param>
    public EventPoolSystem(
        EventCatalog catalog,
        string randomStreamName,
        Func<WorldState, EventInstance, EventDefinition, bool>? shouldAutoResolve = null)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        if (string.IsNullOrWhiteSpace(randomStreamName))
            throw new ArgumentException("A random stream name is required.", nameof(randomStreamName));
        _randomStreamName = randomStreamName;
        _shouldAutoResolve = shouldAutoResolve ?? (static (_, _, _) => true);
        _firePipeline = FireEventCommands.BuildPipeline(catalog);
        _resolvePipeline = ResolveEventOptionCommands.BuildPipeline(catalog);
    }

    public string Id => "events.pool";
    public TickPhase Phase => TickPhase.Events;

    public IReadOnlyCollection<string> Reads { get; } =
        new[] { "characters", "householdPolicies", "settlements", "popGroups", "eventInstances" };

    public IReadOnlyCollection<string> Writes { get; } =
        new[] { "eventInstances", "eventInstanceIds", "commandIds", "eventIds", "commandSequence" };

    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        ExpireOverdueInstances(state, context, events);
        AdvanceDueStages(state, context, events);
        FireScriptedTriggers(state, context, events);
        FireWeightedRolls(state, context, events);

        return events;
    }

    private static void ExpireOverdueInstances(WorldState state, MonthlyTickContext context, List<IDomainEvent> events)
    {
        var overdue = state.EventInstances.InAscendingOrder()
            .Where(entry => entry.Value.Status == EventInstanceStatus.Pending &&
                             entry.Value.ExpiresDate.TotalMonths < context.Date.TotalMonths)
            .Select(entry => entry.Value)
            .ToArray();

        foreach (var instance in overdue)
        {
            state.EventInstances.Remove(instance.InstanceId);
            state.EventInstances.Add(instance.InstanceId, instance with { Status = EventInstanceStatus.Expired });

            events.Add(new EventExpiredEvent(
                state.EventIds.Issue(),
                context.Date,
                instance.InstanceId,
                instance.DefinitionId,
                instance.Scope,
                instance.SubjectIds,
                CausationId: null));
        }
    }

    private void AdvanceDueStages(WorldState state, MonthlyTickContext context, List<IDomainEvent> events)
    {
        var due = state.EventInstances.InAscendingOrder()
            .Where(entry => entry.Value.Status == EventInstanceStatus.AwaitingNextStage &&
                             entry.Value.ExpiresDate.TotalMonths <= context.Date.TotalMonths)
            .Select(entry => entry.Value)
            .ToArray();

        foreach (var instance in due)
        {
            if (!_catalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            var nextStageIndex = instance.CurrentStageIndex + 1;
            if (!definition.TryGetStage(nextStageIndex, out var nextStage))
                continue;

            var newExpiry = new GameDate(checked(context.Date.TotalMonths + nextStage.ExpiryMonths));
            state.EventInstances.Remove(instance.InstanceId);
            var advanced = instance with
            {
                CurrentStageIndex = nextStageIndex,
                Status = EventInstanceStatus.Pending,
                ExpiresDate = newExpiry,
            };
            state.EventInstances.Add(instance.InstanceId, advanced);

            events.Add(new EventStageAdvancedEvent(
                state.EventIds.Issue(),
                context.Date,
                instance.InstanceId,
                instance.DefinitionId,
                instance.Scope,
                nextStageIndex,
                instance.SubjectIds,
                CausationId: null));

            TryAutoResolve(state, context, advanced, definition, events);
        }
    }

    private void FireScriptedTriggers(WorldState state, MonthlyTickContext context, List<IDomainEvent> events)
    {
        var scripted = _catalog.All()
            .Where(definition => definition.TriggerKind == EventTriggerKind.Scripted &&
                                  context.Date.TotalMonths % definition.CadenceMonths == 0)
            .OrderBy(definition => definition.Id.Value, StringComparer.Ordinal);

        foreach (var definition in scripted)
        {
            foreach (var (subjectId, actorId) in SubjectsFor(state, definition.Scope))
            {
                var subjectIds = new[] { subjectId };
                if (HasActiveInstance(state, definition.Id, subjectIds))
                    continue;

                var invocation = new EventInvocation(actorId, subjectIds, context.Date);
                if (!definition.Eligibility(state, invocation))
                    continue;
                if (!definition.ScriptedCondition!(state, invocation))
                    continue;

                Fire(state, context, definition, invocation, events);
            }
        }
    }

    private void FireWeightedRolls(WorldState state, MonthlyTickContext context, List<IDomainEvent> events)
    {
        foreach (var scope in ScopesInOrder)
        {
            var candidates = _catalog.ForScope(scope)
                .Where(definition => definition.TriggerKind == EventTriggerKind.Random &&
                                      context.Date.TotalMonths % definition.CadenceMonths == 0)
                .OrderBy(definition => definition.Id.Value, StringComparer.Ordinal)
                .ToArray();
            if (candidates.Length == 0)
                continue;

            foreach (var (subjectId, actorId) in SubjectsFor(state, scope))
            {
                var subjectIds = new[] { subjectId };
                var invocation = new EventInvocation(actorId, subjectIds, context.Date);

                var eligible = candidates
                    .Where(definition => !HasActiveInstance(state, definition.Id, subjectIds))
                    .Where(definition => definition.Eligibility(state, invocation))
                    .Select(definition => (Definition: definition, Weight: Math.Max(0, definition.Weight!(state, invocation))))
                    .Where(pair => pair.Weight > 0)
                    .ToArray();
                if (eligible.Length == 0)
                    continue;

                var totalWeight = (uint)NoFireWeight + (uint)eligible.Sum(pair => (long)pair.Weight);
                var draw = context.RandomStreams.NextUInt(_randomStreamName, totalWeight);
                if (draw < NoFireWeight)
                    continue;

                var remaining = draw - (uint)NoFireWeight;
                EventDefinition? picked = null;
                foreach (var pair in eligible)
                {
                    if (remaining < (uint)pair.Weight)
                    {
                        picked = pair.Definition;
                        break;
                    }

                    remaining -= (uint)pair.Weight;
                }

                if (picked is null)
                    continue;

                Fire(state, context, picked, invocation, events);
            }
        }
    }

    private void Fire(WorldState state, MonthlyTickContext context, EventDefinition definition, EventInvocation invocation, List<IDomainEvent> events)
    {
        var command = new FireEventCommand(
            state.CommandIds.Issue(), "system", context.Date, CausationId: null, definition.Id, invocation.SubjectIds, invocation.ActorId);
        var result = _firePipeline.Execute(state, command);
        if (!result.Accepted)
            return;

        events.AddRange(result.Events);

        var firedEvent = (EventFiredEvent)result.Events[0];
        if (state.EventInstances.TryGet(firedEvent.InstanceId, out var instance))
            TryAutoResolve(state, context, instance, definition, events);
    }

    private void TryAutoResolve(WorldState state, MonthlyTickContext context, EventInstance instance, EventDefinition definition, List<IDomainEvent> events)
    {
        if (!_shouldAutoResolve(state, instance, definition))
            return;
        if (!definition.TryGetStage(instance.CurrentStageIndex, out var stage))
            return;

        var invocation = new EventInvocation(instance.ActorId, instance.SubjectIds, context.Date);
        var best = stage.Options
            .Where(option => option.Eligibility(state, invocation) is null)
            .Select(option => (Option: option, Score: option.ScoreForAi(state, invocation)))
            .OrderByDescending(pair => pair.Score)
            .ThenBy(pair => pair.Option.Id.Value, StringComparer.Ordinal)
            .Select(pair => pair.Option)
            .FirstOrDefault();
        if (best is null)
            return;

        var command = new ResolveEventOptionCommand(
            state.CommandIds.Issue(), instance.ActorId, context.Date, CausationId: null, instance.InstanceId, best.Id);
        var result = _resolvePipeline.Execute(state, command);
        if (result.Accepted)
            events.AddRange(result.Events);
    }

    private static bool HasActiveInstance(WorldState state, DefinitionId<EventDefinition> definitionId, IReadOnlyList<string> subjectIds) =>
        state.EventInstances.InAscendingOrder().Any(entry =>
            entry.Value.DefinitionId.Equals(definitionId) &&
            entry.Value.Status is EventInstanceStatus.Pending or EventInstanceStatus.AwaitingNextStage &&
            entry.Value.SubjectIds.SequenceEqual(subjectIds));

    private static IEnumerable<(string SubjectId, string ActorId)> SubjectsFor(WorldState state, EventScope scope) => scope switch
    {
        EventScope.Personal => EventSubjects.Characters(state).Select(id => (id.ToTaggedString(), id.ToTaggedString())),
        EventScope.Household => EventSubjects.Households(state).Select(id => (id.ToTaggedString(), id.ToTaggedString())),
        EventScope.Settlement => EventSubjects.Settlements(state).Select(id => (id.ToTaggedString(), id.ToTaggedString())),
        EventScope.Imperial => new[] { (EventSubjects.ImperialSubjectId, EventSubjects.ImperialSubjectId) },
        _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unknown event scope."),
    };
}
