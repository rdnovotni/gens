using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Clientela;

/// <summary>One household's running Influence total (Phase 12 item 2; <c>gens-politics-patronage-design.md</c>
/// §4.4) — "an aggregate resource, distinct from Dignitas and from denarii," spent on elections (<see
/// cref="Magistracies.HoldContestedElectionCommand"/>) and generated/decayed monthly (<see
/// cref="InfluenceCycleSystem"/>). Structurally identical to <see cref="Reputation.HouseholdReputation"/>
/// — a sparse, per-household partition present only once something has touched it — with one real
/// difference: Influence is deliberately clamped at a zero floor (<see cref="InfluenceResolver.Apply"/>)
/// rather than left unclamped like Dignitas, since §4.4 frames it as a spendable stockpile ("spent, not
/// merely accumulated") rather than a reputation score that can meaningfully go negative.
///
/// <b>Scope note:</b> §9's Scheming spend ("undermine a rival candidate... spend Influence") is not
/// wired here — that needs the Characters system's Scheme engine's own scheme-type catalog to attach an
/// Influence cost to, which is this item's own scheme-integration decision to make per <see
/// cref="Magistracies.HoldContestedElectionCommand"/>'s doc comment, not a gap in this resource itself.
/// The election-spend path is the one §4.4 consumer this item actually wires.</summary>
public sealed record HouseholdInfluence(RuntimeId<Household> HouseholdId, int Influence);

/// <summary>Resolves a household's current Influence, defaulting an untouched household to zero —
/// matching <see cref="Reputation.DignitasResolver"/>'s identical convention.</summary>
public static class InfluenceResolver
{
    public static int Current(WorldState state, RuntimeId<Household> householdId) =>
        state.HouseholdInfluences.TryGet(householdId, out var entry) ? entry!.Influence : 0;

    /// <summary>Applies a signed Influence delta, floored at zero (see this record's own doc comment
    /// for why, unlike <see cref="Reputation.DignitasResolver.Apply"/>'s deliberately unclamped
    /// Dignitas). Creates the household's first entry if none exists yet; replaces rather than mutates
    /// in place, matching every other immutable-record partition in <see cref="WorldState"/>.</summary>
    public static void Apply(WorldState state, RuntimeId<Household> householdId, int delta)
    {
        var current = Current(state, householdId);
        var next = Math.Max(0, current + delta);
        if (state.HouseholdInfluences.TryGet(householdId, out _))
            state.HouseholdInfluences.Remove(householdId);
        state.HouseholdInfluences.Add(householdId, new HouseholdInfluence(householdId, next));
    }
}
