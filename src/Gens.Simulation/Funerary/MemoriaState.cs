using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Funerary;

/// <summary>
/// One household's running Memoria total (Phase 11 item 4; <c>gens-ancestor-veneration-funerary-
/// customs-design.md</c> §6's "third axis" — "Dignitas is what the living think of the household;
/// Favor is what the gods think; Memoria is what the household's own dead think"). Structurally
/// parallel to <see cref="Policies.HouseholdPolicyState"/>: a sparse, per-household partition keyed by
/// household, present only once something has actually touched it (a held funeral, an observed or
/// skipped <c>Parentalia</c>) — a household this item never touches simply has no entry, matching <see
/// cref="MemoriaResolver.Current"/>'s "no entry means zero" default rather than every household
/// pre-allocating a zero-Memoria row. Deliberately unclamped: Memoria can go negative under sustained
/// neglect (§6.3), matching <see cref="Characters.Relationship.Opinion"/>'s identical "no floor, only
/// gravity" convention — §6.3's own "nothing here is a hard, unrecoverable failure state" means a
/// negative value is still fully recoverable, not a special state.
/// </summary>
/// <param name="LastParentaliaObservedDate">The last month <see cref="ManesObservanceSystem"/>
/// successfully credited this household's annual <c>Parentalia</c> offering — <c>null</c> if never
/// observed. Read-only bookkeeping; no consumer currently branches on "how long ago", only on whether
/// this year's February credit already landed.</param>
public sealed record MemoriaState(
    RuntimeId<Household> HouseholdId,
    int Memoria,
    GameDate? LastParentaliaObservedDate);

/// <summary>Resolves a household's current Memoria total, defaulting a household with no <see
/// cref="MemoriaState"/> entry yet to zero — matching <see cref="Policies.HouseholdPolicyResolver"/>'s
/// identical "no entry means the default" convention.</summary>
public static class MemoriaResolver
{
    public static int Current(WorldState state, RuntimeId<Household> householdId) =>
        state.MemoriaStates.TryGet(householdId, out var entry) ? entry!.Memoria : 0;

    /// <summary>Applies a signed Memoria delta, creating the household's first <see
    /// cref="MemoriaState"/> entry if none exists yet. Replaces the entry (remove then re-add) rather
    /// than mutating in place, matching every other immutable-record partition in <see
    /// cref="WorldState"/> (e.g. <see cref="Succession.HouseholdHeadship"/>).</summary>
    public static void Apply(WorldState state, RuntimeId<Household> householdId, int delta, GameDate? parentaliaObservedDate = null)
    {
        var existing = state.MemoriaStates.TryGet(householdId, out var found) ? found : null;
        var newTotal = (existing?.Memoria ?? 0) + delta;
        var observedDate = parentaliaObservedDate ?? existing?.LastParentaliaObservedDate;

        if (existing is not null)
            state.MemoriaStates.Remove(householdId);
        state.MemoriaStates.Add(householdId, new MemoriaState(householdId, newTotal, observedDate));
    }
}
