using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Languages;

/// <summary>§7, §10's <c>InterpresAppointment</c> — the small, clearly-scoped slice of §7's Interpres
/// Companion role this item can build without Companions &amp; Court Positions (Phase 16 item 1, not
/// yet built): a household's own standing designation of one already-Characters-tracked individual as
/// its formal Interpres, giving §6's hard gate a reliable answer without needing an actual title/slot
/// system, matching §7's own framing ("gets a standing, reliable answer... without needing to hope the
/// right Character happens to already be fluent"). What this deliberately does not build is a full
/// Court Position — no salary, no household-role slot conflict with any other office, no recruitment
/// flow; those genuinely belong to Companions &amp; Court Positions' own future full design, exactly the
/// same "seam, not a fabricated system" restraint <see cref="Travel.TravelParty"/>'s own retinue-ID-only
/// shape applied to the same not-yet-built system. §11's own open question ("whether a single appointed
/// Interpres can cover multiple languages... or would require several appointments") is answered here
/// permissively — <see cref="LanguagesCovered"/> is a list, one appointment per household — since
/// nothing in §7 forces the more restrictive reading and this item's own scope favors the simpler
/// mechanism over an unauthored capacity limit. Keyed by <see cref="HouseholdId"/> alone (one active
/// Interpres per household at a time), mirroring <see cref="Epithets.DynasticEpithet"/>'s identical
/// "the owning entity is already a unique key" shape.</summary>
public sealed record InterpresAppointment(
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> CharacterId,
    IReadOnlyList<DefinitionId<LanguageDefinition>> LanguagesCovered);

/// <summary>Read-side helper over <see cref="WorldState.InterpresAppointments"/>.</summary>
public static class InterpresQueries
{
    public static bool TryGet(WorldState state, RuntimeId<Household> householdId, out InterpresAppointment appointment) =>
        state.InterpresAppointments.TryGet(householdId, out appointment);

    /// <summary>Whether <paramref name="householdId"/> currently has a standing Interpres covering
    /// <paramref name="languageId"/> specifically.</summary>
    public static bool CoversLanguage(WorldState state, RuntimeId<Household> householdId, DefinitionId<LanguageDefinition> languageId) =>
        TryGet(state, householdId, out var appointment) && appointment.LanguagesCovered.Contains(languageId);
}
