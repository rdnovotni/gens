using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Events;

/// <summary>The four-scope Event Taxonomy (Phase 9 item 3; <c>gens-events-design.md</c> §2) — a
/// reading frame over what an <see cref="EventDefinition"/> targets, not a redesign of any existing
/// system's own content. <see cref="Imperial"/> has no per-entity subject at all (§6): every
/// Imperial-scope <see cref="EventInstance"/> is keyed to the fixed <see
/// cref="EventSubjects.ImperialSubjectId"/> sentinel rather than a real <c>RuntimeId</c>.</summary>
public enum EventScope
{
    Personal,
    Household,
    Settlement,
    Imperial,
}

/// <summary>Random draws from the weighted pool (§3) vs. deterministic firing the instant a condition
/// is met (§4). A Scripted event still queues to the very next Monthly Report tick rather than
/// resolving mid-tick, matching §4's own "queued to the very next Monthly Report tick" wording — see
/// <see cref="EventPoolSystem"/>'s scripted-trigger pass.</summary>
public enum EventTriggerKind
{
    Random,
    Scripted,
}

/// <summary>The runtime status of one fired <see cref="EventInstance"/>.</summary>
public enum EventInstanceStatus
{
    /// <summary>Its current stage's options are open; an option can still be picked.</summary>
    Pending,

    /// <summary>Between stages: the current stage already resolved and a later stage (§8) is queued
    /// for a subsequent tick, per <see cref="EventStageDefinition.NextStageDelayMonths"/>.</summary>
    AwaitingNextStage,

    /// <summary>An option was picked for every stage this definition has; terminal.</summary>
    Resolved,

    /// <summary>No option was picked before <see cref="EventInstance.ExpiresDate"/>; terminal.</summary>
    Expired,
}

/// <summary>The generic envelope an <see cref="EventDefinition"/>'s own hooks are evaluated against —
/// deliberately mirroring <see cref="Gens.Simulation.Actions.ActionInvocation"/>'s shape (Phase 9 item
/// 1) at the Events layer's own level of abstraction: who is being evaluated as a candidate subject,
/// every subject the eventual <see cref="EventInstance"/> would name, and when. <see
/// cref="ActorId"/> is the deciding actor for resolution purposes — the same entity <see
/// cref="EventOptionDefinition.ScoreForAi"/> is scored on behalf of — which for Personal scope is the
/// Character themselves, and for every other scope is that scope's own subject (household, settlement,
/// or the <see cref="EventSubjects.ImperialSubjectId"/> sentinel) until Phase 10's player/steward
/// distinction exists to say otherwise.</summary>
public readonly record struct EventInvocation(string ActorId, IReadOnlyList<string> SubjectIds, GameDate Date);

/// <summary>Implemented by every Events-namespace <see cref="Commands.IDomainEvent"/> so a generic
/// reader (the Monthly Report's grouping, Phase 9 item 4) can assign the real §2 scope taxonomy
/// instead of falling back to a namespace-prefix heuristic.</summary>
public interface IScopedEvent
{
    EventScope Scope { get; }
}

/// <summary>Implemented by every Events-namespace <see cref="Commands.IDomainEvent"/> that names the
/// <see cref="EventInstance"/> it concerns — the Monthly Report's drill-down link (Phase 9 item 4)
/// back to <see cref="Gens.Simulation.Queries.EventInstanceDetailQuery"/>.</summary>
public interface IEventInstanceLinkedEvent
{
    RuntimeId<EventInstance> InstanceId { get; }
}

/// <summary>Fixed sentinel IDs and subject-enumeration helpers used where no real <c>RuntimeId</c>
/// registry exists yet for a scope's own targets (§3's "Household once for the estate, Settlement once
/// for the background population" — enumerated here from whichever partitions already carry that
/// information, per this pass's own "wire to whatever already exists" instruction, rather than adding
/// a new Household/Empire entity record this phase does not otherwise need).</summary>
public static class EventSubjects
{
    /// <summary>The fixed subject ID every Imperial-scope <see cref="EventInstance"/> and <see
    /// cref="EventInvocation"/> uses — there is no per-campaign Imperial entity record (that is
    /// Phase 13's Game Calendar/Historical Timeline work, explicitly out of scope here).</summary>
    public const string ImperialSubjectId = "imperial";

    /// <summary>Personal-scope candidates: every named Character (Phase 5 item 1), ascending-<see
    /// cref="RuntimeId{T}"/> order (ADR 0004) — §3's "Personal rolls per eligible Character."</summary>
    public static IEnumerable<RuntimeId<Character>> Characters(WorldState state) =>
        state.Characters.InAscendingOrder().Select(entry => entry.Key);

    /// <summary>Household-scope candidates: every distinct Household referenced anywhere in <see
    /// cref="WorldState"/>, ascending order. No standalone Household registry exists yet (only the
    /// phantom <see cref="Identity.Household"/> ID kind, issued at bootstrap) — this unions every
    /// partition that already carries a Household reference, per this pass's own "wire to whatever
    /// already exists" instruction, rather than adding a new Household record this phase does not
    /// otherwise need.</summary>
    public static IEnumerable<RuntimeId<Household>> Households(WorldState state)
    {
        var fromCharacters = state.Characters.InAscendingOrder()
            .Select(entry => entry.Value.Household)
            .Where(household => household.HasValue)
            .Select(household => household!.Value);
        var fromPolicies = state.HouseholdPolicies.InAscendingOrder().Select(entry => entry.Key);

        return fromCharacters.Concat(fromPolicies).Distinct().OrderBy(id => id.Value);
    }

    /// <summary>Settlement-scope candidates: §3's "Settlement once for the background population,"
    /// read from the real Settlement registry (Phase 6 item 1).</summary>
    public static IEnumerable<RuntimeId<Settlement>> Settlements(WorldState state) =>
        state.Settlements.InAscendingOrder().Select(entry => entry.Key);
}
