using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Schemes;

/// <summary>Where a <see cref="SchemeInstance"/> sits in the 5-stage engine (<c>gens-characters-
/// design.md</c> §10): <see cref="Progressing"/> covers stages 2-3 (Progress and Discovery risk both
/// accrue every tick); <see cref="AwaitingCounterPlay"/> is stage 4, once the target's suspicion has
/// crossed the discovery threshold; <see cref="Resolved"/> is stage 5, terminal.</summary>
public enum SchemeStage
{
    Progressing,
    AwaitingCounterPlay,
    Resolved,
}

/// <summary>The four resolution outcomes §10 stage 5 names.</summary>
public enum SchemeOutcome
{
    Succeeded,
    FailedQuietly,
    DiscoveredAndFoiled,
    DiscoveredAndEscalated,
}

/// <summary>
/// One in-progress or resolved scheme (Phase 10 item 12; §10's generic Scheme engine). Deliberately
/// keyed by <see cref="RuntimeId{T}"/>s for <see cref="InitiatorCharacterId"/>/<see
/// cref="TargetCharacterId"/> rather than the bare-string <see cref="Actions.ActionInvocation"/>
/// convention: unlike the action-definition layer, §10 is specifically about one Character scheming
/// against another Character (individual interactions), so the stronger typing is available and used.
/// Immutable like every other <c>WorldState</c> record — <see cref="SchemeProgressSystem"/> and the
/// scheme commands replace the entry in <see cref="State.WorldState.Schemes"/> rather than mutating
/// one in place.
/// </summary>
/// <param name="AssistingAgentCharacterId">A client's Specialty favor or a hired specialist (§10 stage
/// 1) — optional, and itself a leak-risk factor discovery-risk rolls account for.</param>
/// <param name="CounterPlayDeadline">Only meaningful while <see cref="Stage"/> is <see
/// cref="SchemeStage.AwaitingCounterPlay"/> — the month by which the target must submit a <see
/// cref="CounterPlaySchemeCommand"/> or the scheme resolves on its own (§10 stage 4's "real back-and-
/// forth, not single roll+delayed reveal" still bounded by a concrete deadline rather than waiting
/// forever).</param>
public sealed record SchemeInstance(
    RuntimeId<SchemeInstance> SchemeId,
    SchemeType Type,
    RuntimeId<Character> InitiatorCharacterId,
    RuntimeId<Character> TargetCharacterId,
    RuntimeId<Character>? AssistingAgentCharacterId,
    GameDate InitiatedDate,
    int Progress,
    int DiscoveryRisk,
    SchemeStage Stage,
    GameDate? CounterPlayDeadline = null,
    SchemeOutcome? Outcome = null,
    GameDate? ResolvedDate = null);
