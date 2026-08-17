using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Events;

/// <summary>One option a fired <see cref="EventInstance"/>'s current stage offers (Phase 9 item 3).
/// Deliberately mirrors <see cref="Gens.Simulation.Actions.ActionDefinition"/>'s
/// eligibility/AI-scoring split: <see cref="Eligibility"/> and <see cref="ScoreForAi"/> are pure
/// reads, and the actual mutation happens only through <see cref="ResolveEventOptionCommand"/>'s own
/// <see cref="CommandPipeline{TState,TCommand}"/> (ADR 0006) — <see cref="Resolve"/> below is that
/// pipeline's <c>mutate</c> step, not a shortcut around it.</summary>
public sealed record EventOptionDefinition
{
    public EventOptionDefinition(
        DefinitionId<EventOptionDefinition> id,
        string nameKey,
        string descriptionKey,
        Func<WorldState, EventInvocation, ValidationErrorCode?> eligibility,
        Func<WorldState, EventInvocation, double> scoreForAi,
        Func<WorldState, EventInstance, GameDate, string?, IReadOnlyList<IDomainEvent>> resolve)
    {
        Id = id;
        NameKey = string.IsNullOrEmpty(nameKey) ? throw new ArgumentException("A name key is required.", nameof(nameKey)) : nameKey;
        DescriptionKey = string.IsNullOrEmpty(descriptionKey)
            ? throw new ArgumentException("A description key is required.", nameof(descriptionKey))
            : descriptionKey;
        Eligibility = eligibility ?? throw new ArgumentNullException(nameof(eligibility));
        ScoreForAi = scoreForAi ?? throw new ArgumentNullException(nameof(scoreForAi));
        Resolve = resolve ?? throw new ArgumentNullException(nameof(resolve));
    }

    public DefinitionId<EventOptionDefinition> Id { get; }
    public string NameKey { get; }
    public string DescriptionKey { get; }

    /// <summary><c>null</c> means eligible, matching <see
    /// cref="Gens.Simulation.Actions.ActionDefinition.Eligibility"/>'s identical convention.</summary>
    public Func<WorldState, EventInvocation, ValidationErrorCode?> Eligibility { get; }

    /// <summary>An AI/NPC resolver's own utility score for picking this option — the same convention
    /// <see cref="Gens.Simulation.Actions.ActionDefinition.ScoreForAi"/> establishes for Actions,
    /// mirrored here per this pass's own instruction to reuse it rather than invent a second one.
    /// Higher is more attractive; <see cref="EventPoolSystem"/>'s AI/NPC auto-resolution picks the
    /// highest-scoring eligible option among the current stage's <see
    /// cref="EventStageDefinition.Options"/>.</summary>
    public Func<WorldState, EventInvocation, double> ScoreForAi { get; }

    /// <summary>Mutates <see cref="WorldState"/> and returns the resulting domain events — the
    /// command pipeline's own <c>mutate</c> step (ADR 0006), invoked only from inside <see
    /// cref="ResolveEventOptionCommand"/>'s pipeline once validation has already passed. Never called
    /// directly by a UI or AI caller.</summary>
    public Func<WorldState, EventInstance, GameDate, string?, IReadOnlyList<IDomainEvent>> Resolve { get; }
}

/// <summary>One stage of a (possibly multi-stage) <see cref="EventDefinition"/> (§8): the options
/// offered while this stage is current, and the window before an unresolved stage expires. <see
/// cref="NextStageDelayMonths"/> only matters when a later stage exists in the owning
/// <see cref="EventDefinition.Stages"/> list — see <see cref="EventPoolSystem"/>'s stage-advancement
/// pass for how it is consumed.</summary>
public sealed record EventStageDefinition
{
    public EventStageDefinition(
        int stageIndex,
        string nameKey,
        string descriptionKey,
        IReadOnlyList<EventOptionDefinition> options,
        int expiryMonths,
        int nextStageDelayMonths = 1)
    {
        if (stageIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(stageIndex), stageIndex, "A stage index cannot be negative.");
        if (expiryMonths <= 0)
            throw new ArgumentOutOfRangeException(nameof(expiryMonths), expiryMonths, "A stage's expiry window must be at least one month.");
        if (nextStageDelayMonths <= 0)
            throw new ArgumentOutOfRangeException(nameof(nextStageDelayMonths), nextStageDelayMonths, "A stage's delay to the next stage must be at least one month.");
        if (options is null || options.Count == 0)
            throw new ArgumentException("A stage must offer at least one option.", nameof(options));

        var duplicate = options.GroupBy(option => option.Id).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
            throw new ArgumentException($"Duplicate option ID '{duplicate.Key}' within one stage.", nameof(options));

        StageIndex = stageIndex;
        NameKey = string.IsNullOrEmpty(nameKey) ? throw new ArgumentException("A name key is required.", nameof(nameKey)) : nameKey;
        DescriptionKey = string.IsNullOrEmpty(descriptionKey)
            ? throw new ArgumentException("A description key is required.", nameof(descriptionKey))
            : descriptionKey;
        Options = options;
        ExpiryMonths = expiryMonths;
        NextStageDelayMonths = nextStageDelayMonths;
    }

    public int StageIndex { get; }
    public string NameKey { get; }
    public string DescriptionKey { get; }
    public IReadOnlyList<EventOptionDefinition> Options { get; }

    /// <summary>Months after this stage becomes current before an unacted-on instance expires (§7's
    /// "unresolved event/option can expire").</summary>
    public int ExpiryMonths { get; }

    /// <summary>Months after this stage resolves before the next stage (if any) becomes current.
    /// Ignored on a definition's final stage.</summary>
    public int NextStageDelayMonths { get; }

    public bool TryGetOption(DefinitionId<EventOptionDefinition> optionId, out EventOptionDefinition option)
    {
        foreach (var candidate in Options)
        {
            if (candidate.Id.Equals(optionId))
            {
                option = candidate;
                return true;
            }
        }

        option = null!;
        return false;
    }
}

/// <summary>
/// The event-definition layer itself (Phase 9 item 3): one declarative record per distinct event,
/// aggregating scope (<c>gens-events-design.md</c> §2), eligibility, weighted-pool inputs (§3),
/// scripted-trigger condition (§4), and its (possibly multi-stage, §8) sequence of <see
/// cref="EventStageDefinition"/>s. Fixed C#, not a JSON-authored rules DSL — the same "content is
/// data, rules are code" split <see cref="Gens.Simulation.Actions.ActionDefinition"/>'s own doc
/// comment describes, applied here to Events; see <see cref="SampleEventDefinitions.BuildCatalog"/>
/// for a worked example.
/// </summary>
public sealed record EventDefinition
{
    public EventDefinition(
        DefinitionId<EventDefinition> id,
        EventScope scope,
        string nameKey,
        string descriptionKey,
        EventTriggerKind triggerKind,
        Func<WorldState, EventInvocation, bool> eligibility,
        IReadOnlyList<EventStageDefinition> stages,
        Func<WorldState, EventInvocation, int>? weight = null,
        Func<WorldState, EventInvocation, bool>? scriptedCondition = null,
        int cadenceMonths = 1)
    {
        if (cadenceMonths <= 0)
            throw new ArgumentOutOfRangeException(nameof(cadenceMonths), cadenceMonths, "Cadence must be at least one month.");
        if (stages is null || stages.Count == 0)
            throw new ArgumentException("A definition must declare at least one stage.", nameof(stages));
        for (var i = 0; i < stages.Count; i++)
        {
            if (stages[i].StageIndex != i)
                throw new ArgumentException("Stage indices must be sequential starting at 0.", nameof(stages));
        }

        if (triggerKind == EventTriggerKind.Random && weight is null)
            throw new ArgumentException("A Random-trigger definition requires a weight function.", nameof(weight));
        if (triggerKind == EventTriggerKind.Scripted && scriptedCondition is null)
            throw new ArgumentException("A Scripted-trigger definition requires a trigger condition.", nameof(scriptedCondition));

        Id = id;
        Scope = scope;
        NameKey = string.IsNullOrEmpty(nameKey) ? throw new ArgumentException("A name key is required.", nameof(nameKey)) : nameKey;
        DescriptionKey = string.IsNullOrEmpty(descriptionKey)
            ? throw new ArgumentException("A description key is required.", nameof(descriptionKey))
            : descriptionKey;
        TriggerKind = triggerKind;
        Eligibility = eligibility ?? throw new ArgumentNullException(nameof(eligibility));
        Stages = stages;
        Weight = weight;
        ScriptedCondition = scriptedCondition;
        CadenceMonths = cadenceMonths;
    }

    public DefinitionId<EventDefinition> Id { get; }
    public EventScope Scope { get; }
    public string NameKey { get; }
    public string DescriptionKey { get; }
    public EventTriggerKind TriggerKind { get; }

    /// <summary>Whether a given candidate subject is even eligible to be considered for this
    /// definition at all — checked before a Random roll and before a Scripted condition, so
    /// ineligible subjects never enter either path.</summary>
    public Func<WorldState, EventInvocation, bool> Eligibility { get; }

    public IReadOnlyList<EventStageDefinition> Stages { get; }
    public EventStageDefinition FirstStage => Stages[0];

    /// <summary>Non-negative weight this definition contributes to the weighted pool (§3) for a given
    /// eligible candidate — read from whichever real <c>WorldState</c> meter this definition wires
    /// to (see <see cref="EventWeightInputs"/>); <c>null</c> for a Scripted-trigger definition, which
    /// never enters the weighted roll at all.</summary>
    public Func<WorldState, EventInvocation, int>? Weight { get; }

    /// <summary>Fires the instant this returns <c>true</c> for an eligible candidate, bypassing the
    /// weighted roll entirely (§4); <c>null</c> for a Random-trigger definition.</summary>
    public Func<WorldState, EventInvocation, bool>? ScriptedCondition { get; }

    /// <summary>How often (in months) this definition is even considered for its scope's roll —
    /// §3's "Imperial on its own slower cadence" generalized to any definition, since nothing in this
    /// pass hardcodes a per-scope cadence.</summary>
    public int CadenceMonths { get; }

    public bool TryGetStage(int stageIndex, out EventStageDefinition stage)
    {
        if (stageIndex >= 0 && stageIndex < Stages.Count)
        {
            stage = Stages[stageIndex];
            return true;
        }

        stage = null!;
        return false;
    }
}
