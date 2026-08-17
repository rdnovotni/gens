using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Actions;

/// <summary>What kind of thing an Action targets (Phase 9 item 1). <see cref="None"/> covers a
/// household-scoped action with no separate target beyond the actor's own household (e.g. a Standing
/// Policy change) — most Funded Actions and Edicts fall here too, since their "target" is really just
/// the acting household or settlement, already implied by <see cref="ActionInvocation.ActorId"/>.
/// </summary>
public enum ActionTargetKind
{
    None,
    Character,
    Household,
    Holding,
    Settlement,
}

/// <summary>Confirmation weight an Action's own consequences call for before the player commits to it
/// — the roadmap's own Phase 9 item 7 vocabulary ("wax-seal confirmation for consequential decisions
/// and ordinary confirmations for reversible actions"), named here rather than invented, since the
/// action-definition layer is exactly where that later UI item will read this from.</summary>
public enum ActionConfirmationSeverity
{
    /// <summary>A reversible, low-stakes action — an ordinary confirmation, or none at all.</summary>
    Ordinary,

    /// <summary>A consequential or hard-to-reverse action — the wax-seal prompt.</summary>
    WaxSeal,
}

/// <summary>What committing an Action costs, up front. Every field defaults to "none paid" so a
/// no-cost action (most Standing Policy changes) doesn't need to spell out three zeroes.</summary>
public readonly record struct ActionCost(Money Treasury, int Influence = 0, int Dignitas = 0)
{
    public static readonly ActionCost None = new(Money.Zero);
}

public enum ActionDurationMode
{
    /// <summary>Resolves within the same tick it was accepted — most commands so far.</summary>
    Instant,

    /// <summary>Occupies a fixed number of months before its outcome resolves — the shape a future
    /// Extended Activity (<c>gens-activities-activity-engine-design.md</c> §3) or a multi-month Edict
    /// effect will need; nothing in this pass yet produces one.</summary>
    FixedMonths,
}

public readonly record struct ActionDuration(ActionDurationMode Mode, int Months = 0)
{
    public static readonly ActionDuration Instant = new(ActionDurationMode.Instant);

    public static ActionDuration Fixed(int months)
    {
        if (months <= 0)
            throw new ArgumentOutOfRangeException(nameof(months), months, "A fixed duration must be at least one month.");
        return new ActionDuration(ActionDurationMode.FixedMonths, months);
    }
}

/// <summary>One resource an accepted Action holds against being double-spent before it resolves — a
/// generic tag-and-amount pair at the action-definition layer's own level of abstraction, mirroring
/// <see cref="Gens.Simulation.Goods.StockReservation"/>'s concrete shape without committing this layer to any one
/// resource kind (treasury, a Labor duty slot, a Venue booking are all plausible future tags).
/// Declarative only: this layer does not itself hold reservations against <see cref="WorldState"/> —
/// a concrete command's own <c>Mutate</c> step is still what actually reserves and releases.</summary>
public readonly record struct ActionReservation(string ResourceTag, long Amount);

/// <summary>The generic envelope an <see cref="ActionDefinition"/>'s own hooks are evaluated against:
/// who's invoking, against what target (if any), and when — independent of any one <see
/// cref="ICommand"/>'s concrete field shape, so the same definition can describe a player-submitted
/// command, an AI candidate under consideration, or a steward's automated choice alike (Phase 10 item
/// 1: "reusable AI considerations and action selection against the same action definitions used by
/// the player").</summary>
public readonly record struct ActionInvocation(string ActorId, string? TargetId, GameDate Date);

/// <summary>A lightweight, non-mutating preview of what committing an Action would do — the wax-seal
/// confirmation prompt's own data source, and equally useful to an AI utility scorer deciding between
/// candidates without actually running any of them. Never produced by mutating <see
/// cref="WorldState"/>.</summary>
public readonly record struct ActionResultProjection(string Summary, IReadOnlyDictionary<string, string> ProjectedChanges)
{
    private static readonly IReadOnlyDictionary<string, string> NoChanges = new Dictionary<string, string>(0);

    public static ActionResultProjection Of(string summary) => new(summary, NoChanges);
}

/// <summary>
/// The action-definition layer itself (Phase 9 item 1): one declarative record per distinct
/// player/AI action, aggregating everything a UI, an AI utility scorer, and a confirmation prompt all
/// need without re-deriving it from a concrete <see cref="ICommand"/>'s own field shape. An
/// <see cref="ActionDefinition"/> never mutates <see cref="WorldState"/> itself — <see
/// cref="Eligibility"/>, <see cref="ScoreForAi"/>, and <see cref="ProjectResult"/> are all pure reads;
/// the actual mutation still goes through the concrete command's own <see cref="CommandPipeline{TState,TCommand}"/>
/// (ADR 0006). This matches rule 10's "content is data, rules are code" split applied to command
/// metadata specifically: the shape here is fixed C# describing an action, not a JSON-authored rules
/// DSL — see <see cref="Gens.Simulation.Policies.PolicyActionDefinitions"/> for a worked example wiring
/// a real command pair through it.
/// </summary>
public sealed record ActionDefinition
{
    public ActionDefinition(
        DefinitionId<ActionDefinition> id,
        string nameKey,
        string descriptionKey,
        ActionTargetKind targetKind,
        ActionCost cost,
        ActionDuration duration,
        ActionConfirmationSeverity confirmation,
        Func<WorldState, ActionInvocation, ValidationErrorCode?> eligibility,
        Func<WorldState, ActionInvocation, double> scoreForAi,
        Func<WorldState, ActionInvocation, ActionResultProjection> projectResult,
        IReadOnlyList<ActionReservation>? reservations = null)
    {
        Id = id;
        NameKey = string.IsNullOrEmpty(nameKey) ? throw new ArgumentException("A name key is required.", nameof(nameKey)) : nameKey;
        DescriptionKey = string.IsNullOrEmpty(descriptionKey)
            ? throw new ArgumentException("A description key is required.", nameof(descriptionKey))
            : descriptionKey;
        TargetKind = targetKind;
        Cost = cost;
        Duration = duration;
        Confirmation = confirmation;
        Eligibility = eligibility ?? throw new ArgumentNullException(nameof(eligibility));
        ScoreForAi = scoreForAi ?? throw new ArgumentNullException(nameof(scoreForAi));
        ProjectResult = projectResult ?? throw new ArgumentNullException(nameof(projectResult));
        Reservations = reservations ?? Array.Empty<ActionReservation>();
    }

    public DefinitionId<ActionDefinition> Id { get; }
    public string NameKey { get; }
    public string DescriptionKey { get; }
    public ActionTargetKind TargetKind { get; }
    public ActionCost Cost { get; }
    public ActionDuration Duration { get; }
    public ActionConfirmationSeverity Confirmation { get; }
    public IReadOnlyList<ActionReservation> Reservations { get; }

    /// <summary>Pure eligibility read — <c>null</c> means eligible, matching <see
    /// cref="CommandPipeline{TState,TCommand}"/>'s own <see cref="ValidationErrorCode"/>? convention
    /// so a UI or AI reuses the exact rejection vocabulary a submitted command would itself produce,
    /// rather than a parallel one. Not a substitute for the command's own validation — a UI still
    /// submits the real command and still handles a real rejection; this hook only lets it check first
    /// without submitting.</summary>
    public Func<WorldState, ActionInvocation, ValidationErrorCode?> Eligibility { get; }

    /// <summary>An AI decision-maker's own utility score for this invocation (Phase 10 item 1) — higher
    /// is more attractive; a caller compares scores across candidate <see cref="ActionDefinition"/>s
    /// (or the same one against multiple targets) to pick one. Never consulted for a player-submitted
    /// command.</summary>
    public Func<WorldState, ActionInvocation, double> ScoreForAi { get; }

    /// <summary>Projects what committing this invocation would change, without mutating <see
    /// cref="WorldState"/>.</summary>
    public Func<WorldState, ActionInvocation, ActionResultProjection> ProjectResult { get; }
}
