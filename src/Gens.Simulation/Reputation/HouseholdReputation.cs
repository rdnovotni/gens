using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Reputation;

/// <summary>
/// One household's running Dignitas total (Phase 12 item 1; <c>gens-politics-patronage-design.md</c>
/// §2's "Dignitas has been the project's standing reputation stat since the core doc... a single
/// tracked number per household"). This is the field every earlier phase's own doc comments named as
/// missing — <see cref="Succession.DeclareHeirCommand"/>, <see cref="Epithets.Agnomen"/>'s
/// <c>DignitasEffect</c>, <see cref="Funerary.FuneraryCatalog"/>'s Grand-tier trade, and <see
/// cref="Queries.InkBarQuery"/>'s reserved ink-bar slot all say some version of "no personal or
/// household Dignitas stat exists yet, only <see cref="Actors.LivingWorldActor.Dignitas"/> for rival
/// houses". This record is that stat, finally given to the player's own <see cref="Household"/> (which,
/// unlike a rival house, is never itself a <see cref="Actors.LivingWorldActor"/> and so never had
/// anywhere to keep one).
///
/// Structurally parallel to <see cref="Funerary.MemoriaState"/>: a sparse, per-household partition
/// (<see cref="Gens.Simulation.State.WorldState.HouseholdReputations"/>), present only once something
/// has actually touched it — see <see cref="DignitasResolver.Current"/> for the "no entry means zero"
/// default. Deliberately unclamped and never decayed on its own: the design doc describes Dignitas as
/// "moved by nearly everything" (Villa Grandeur, Monuments, Funded Actions, marriage alliances,
/// military victories, a won magistracy, a scandal, a defaulted debt) but never as something that
/// erodes merely by the passage of time the way Influence or Fame do — matching <see
/// cref="Funerary.MemoriaState"/>'s identical "no floor, only gravity" convention rather than inventing
/// a decay curve the design doc never asks for.
///
/// <b>Scope note (Phase 12 item 1):</b> this item builds the shared Dignitas primitive — the state slot,
/// the command that moves it, and the audience-scoped visibility of the fact that it moved (see <see
/// cref="AdjustDignitasCommand"/>) — but does not itself retrofit every already-shipped Phase 11/9
/// forward reference to actually start writing through it (the Salutatio trickle and election stakes
/// named in the Politics &amp; Patronage doc are item 2's own job; Agnomen's <c>DignitasEffect</c> and
/// the Funerary Grand-tier trade are Phase 11 items already closed and tested against a null/absent
/// value, and re-opening them is out of this item's scope). <see cref="Queries.InkBarQuery"/> is the one
/// exception: a pure, non-mutating read of a now-real value, safe to wire in immediately.
/// </summary>
/// <param name="HouseholdId">The household this Dignitas total belongs to.</param>
/// <param name="Dignitas">The running total. Positive is respectable standing, negative is a real,
/// recoverable disgrace — never clamped.</param>
public sealed record HouseholdReputation(RuntimeId<Household> HouseholdId, int Dignitas);

/// <summary>Resolves a household's current Dignitas, defaulting a household with no <see
/// cref="HouseholdReputation"/> entry yet to zero — matching <see
/// cref="Funerary.MemoriaResolver"/>'s identical "no entry means the default" convention.</summary>
public static class DignitasResolver
{
    public static int Current(WorldState state, RuntimeId<Household> householdId) =>
        state.HouseholdReputations.TryGet(householdId, out var entry) ? entry!.Dignitas : 0;

    /// <summary>Applies a signed Dignitas delta, creating the household's first <see
    /// cref="HouseholdReputation"/> entry if none exists yet. Replaces the entry (remove then re-add)
    /// rather than mutating in place, matching every other immutable-record partition in <see
    /// cref="WorldState"/> (e.g. <see cref="Funerary.MemoriaResolver.Apply"/>).</summary>
    public static void Apply(WorldState state, RuntimeId<Household> householdId, int delta)
    {
        var current = Current(state, householdId);
        if (state.HouseholdReputations.TryGet(householdId, out _))
            state.HouseholdReputations.Remove(householdId);
        state.HouseholdReputations.Add(householdId, new HouseholdReputation(householdId, current + delta));
    }
}
