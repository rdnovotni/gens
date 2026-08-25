using Gens.Simulation.State;

namespace Gens.Simulation.Actions;

/// <summary>One <see cref="ActionDefinition"/> found eligible for <see cref="Invocation"/>, together
/// with its <see cref="ActionDefinition.ScoreForAi"/> result.</summary>
public readonly record struct ScoredActionCandidate(ActionDefinition Definition, ActionInvocation Invocation, double Score);

/// <summary>
/// Reusable AI action selection against the same <see cref="ActionCatalog"/> and <see
/// cref="ActionDefinition"/>s a player uses (Phase 10 item 1; the roadmap's own words: "reusable AI
/// considerations and action selection against the same action definitions used by the player").
/// Deliberately actor-agnostic — it knows nothing about stewards, rival heads, or any other caller;
/// package 7 (rival ambition) and packages 9-11 (steward autonomy) both build on this directly rather
/// than each inventing their own scoring/filtering pass. Every method is a pure read over <see
/// cref="WorldState"/>: nothing here ever mutates state or submits a command — a caller that decides
/// to act still goes on to submit the concrete <see cref="Commands.ICommand"/> the chosen <see
/// cref="ActionDefinition"/> wraps, through the ordinary <see cref="Commands.CommandPipeline{TState,TCommand}"/>
/// (ADR 0006, rule 2's "one command path").
/// </summary>
public static class ActionSelector
{
    /// <summary>Every <see cref="ActionCatalog"/> entry eligible for <paramref name="invocation"/>
    /// (<see cref="ActionDefinition.Eligibility"/> returns <c>null</c>), ranked by <see
    /// cref="ActionDefinition.ScoreForAi"/> descending. Ties, and <see cref="ActionCatalog.All"/>'s own
    /// unspecified backing-dictionary enumeration order (rule 3), are both broken by <see
    /// cref="ActionDefinition.Id"/> in ascending ordinal order, so the ranked list is fully
    /// deterministic regardless of catalog construction order.</summary>
    public static IReadOnlyList<ScoredActionCandidate> Rank(WorldState state, ActionCatalog catalog, ActionInvocation invocation)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (catalog is null)
            throw new ArgumentNullException(nameof(catalog));

        return catalog.All()
            .Where(definition => definition.Eligibility(state, invocation) is null)
            .Select(definition => new ScoredActionCandidate(definition, invocation, definition.ScoreForAi(state, invocation)))
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Definition.Id.Value, StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>The single top-ranked eligible candidate for <paramref name="invocation"/>, or
    /// <c>null</c> if nothing in <paramref name="catalog"/> is eligible.</summary>
    public static ScoredActionCandidate? SelectBest(WorldState state, ActionCatalog catalog, ActionInvocation invocation)
    {
        var ranked = Rank(state, catalog, invocation);
        return ranked.Count == 0 ? null : ranked[0];
    }
}
