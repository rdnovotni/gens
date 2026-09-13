using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Education;

/// <summary>
/// One household's running Cultural Prestige total (Phase 17 item 2; <c>gens-education-culture-design.md</c>
/// §7) — "sitting alongside Dignitas... its own separate tracked value" per the design doc's own framing,
/// mirroring <see cref="Reputation.HouseholdReputation.Dignitas"/>'s shape (a bare running int, no floor)
/// but kept as its own partition rather than folded into Dignitas: §7 frames Cultural Prestige as legible
/// to a distinct audience (a household's standing among the culturally literate — patrons, sophists,
/// marriage brokers assessing a bride's own accomplishment) that moves independently of, and sometimes in
/// tension with, plain political Dignitas (a Traditionalist household can carry high Dignitas and zero
/// Cultural Prestige, or vice versa). Sparse: a household this item never touches has no entry, matching
/// <see cref="Religion.HouseholdReligion"/>'s and <see cref="Reputation.HouseholdReputation"/>'s identical
/// "no entry means the default" convention — but, like Dignitas and unlike Favor, a household's first
/// Prestige-moving event auto-creates the entry rather than requiring a founding command first (see <see
/// cref="CulturalPrestigeResolver.Apply"/>), since Cultural Prestige has no "chosen a Patron" analog
/// gating its own existence.
/// </summary>
public sealed record HouseholdCulturalPrestige(RuntimeId<Household> HouseholdId, int Prestige);

/// <summary>Read/write helpers over <see cref="WorldState.HouseholdCulturalPrestiges"/>, matching <see
/// cref="Reputation.DignitasResolver"/>'s identical "no entry means zero" and "replace, don't mutate in
/// place" conventions, including its auto-create-on-first-write behavior.</summary>
public static class CulturalPrestigeResolver
{
    public static int Current(WorldState state, RuntimeId<Household> householdId) =>
        state.HouseholdCulturalPrestiges.TryGet(householdId, out var entry) ? entry!.Prestige : 0;

    /// <summary>Applies a signed Prestige delta, creating the household's first <see
    /// cref="HouseholdCulturalPrestige"/> entry if none exists yet — matching <see
    /// cref="Reputation.DignitasResolver.Apply"/>'s identical auto-create shape.</summary>
    public static void Apply(WorldState state, RuntimeId<Household> householdId, int delta)
    {
        var current = Current(state, householdId);
        if (state.HouseholdCulturalPrestiges.TryGet(householdId, out _))
            state.HouseholdCulturalPrestiges.Remove(householdId);
        state.HouseholdCulturalPrestiges.Add(householdId, new HouseholdCulturalPrestige(householdId, current + delta));
    }

    /// <summary>§7's marriage-market threshold gate — "a household with real Cultural Prestige... reads
    /// as a more attractive marriage partner regardless of its raw Dignitas," computed on demand from
    /// <see cref="CulturalPrestigeCatalog.MarriageMarketPrestigeThreshold"/> rather than stored as its own
    /// persisted boolean, matching <see cref="Religion.HouseholdReligionResolver.IsDivinelyDispleased"/>'s
    /// identical "derive it, don't duplicate it" convention.</summary>
    public static bool ClearsMarriageMarketThreshold(WorldState state, RuntimeId<Household> householdId) =>
        Current(state, householdId) >= CulturalPrestigeCatalog.MarriageMarketPrestigeThreshold;
}
