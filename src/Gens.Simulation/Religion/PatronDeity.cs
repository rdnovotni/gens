using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Religion;

/// <summary>The twelve major-pantheon picks <c>gens-religion-design.md</c> §2.1 names as "real, viable
/// picks, each with a distinct domain": Jupiter, Juno, Mars, Venus, Minerva, Ceres, Neptune, Mercury,
/// Vesta, Apollo, Diana, and Bacchus. Fixed and code-defined, matching <see
/// cref="Clientela.ClientSpecialty"/>'s identical "§4.2's own table is closed, not an open content
/// catalog" convention — §2.1's list is exhaustive by its own framing ("are all real, viable picks"),
/// not a representative sample. The domain-flavored High-Favor bonus and Low-Favor Ill Omen theme §2.1
/// describes per deity (Mars → Military & Combat, Ceres → harvest stability, and so on) are not
/// resolved anywhere in this codebase: every one of those target systems (Military & Combat, Estate
/// harvest variance, Familia's marriage/romance resolution, Commerce trade-route reliability) is itself
/// unbuilt or, where partly built, has no read hook this item's own narrow scope reaches into — matching
/// <see cref="Reputation.HouseholdReputation"/>'s own "Fame... neither built, so this item does not
/// invent a stand-in" precedent. <see cref="PatronDeity"/> is real, stored, and read by every command
/// and system in this domain; its per-domain payoff is Favor moving on the single shared axis (§2's own
/// "the single Favor score" framing), not a dozen bespoke cross-system hooks this item does not have
/// real consumers for yet.</summary>
public enum PatronDeity
{
    Jupiter,
    Juno,
    Mars,
    Venus,
    Minerva,
    Ceres,
    Neptune,
    Mercury,
    Vesta,
    Apollo,
    Diana,
    Bacchus,
}

/// <summary>
/// One household's Religion state (Phase 12 item 3; <c>gens-religion-design.md</c> §2's <c>
/// HouseholdReligion</c> data model, §10) — the household's chosen <see cref="PatronDeity"/> and its
/// running <see cref="Favor"/> total, "sitting alongside Dignitas as a second, distinct axis of
/// standing... the two usually move together... but can diverge sharply" (§2). Structurally the same
/// sparse, per-household shape as <see cref="Reputation.HouseholdReputation"/> — present only once a
/// household has actually chosen a Patron Deity (<see cref="SetPatronDeityCommand"/>) — but, unlike
/// Dignitas, <see cref="Favor"/> cannot exist independent of a chosen deity: §2's own "the Patron Deity
/// doesn't create a second meter — it determines what the single Favor score actually does" makes the
/// pairing structural, not incidental, so this is one record rather than two independently-sparse
/// partitions the way Clientela split <c>HouseholdInfluence</c> from <c>CharacterFactionAlignment</c>.
///
/// <see cref="Favor"/> is deliberately unclamped, matching <see
/// cref="Reputation.HouseholdReputation.Dignitas"/>'s own "no floor, only gravity" convention exactly —
/// the task's own instruction that Favor is "explicitly analogous to Dignitas but a second distinct
/// axis" is read literally here, down to the clamping policy, in deliberate contrast to Clientela's
/// zero-floored Influence (a spendable stockpile, not a standing score).
///
/// <see cref="ConsecratedUnderHeadCharacterId"/> is this item's own addition, not named directly in
/// §10's data model (which sketches a separate <c>reconsecrationHistory</c> list this item does not
/// build — see <see cref="ReconsecrateCommand"/>'s own doc comment for why a full history log is out of
/// scope): it is the concrete state <see cref="ReconsecrateCommand"/> needs to detect "a new
/// paterfamilias or materfamilias assumes headship" (§2.1) at all, since nothing else in this record
/// remembers who last consecrated the household's patron.
/// </summary>
/// <param name="LastObservedFeastDay">The most recent <see cref="ObserveFeastDayCommand.FeastDay"/> the
/// household passively observed, or <see langword="null"/> if it never has — paired with <see
/// cref="LastObservedFeastDate"/> so <see cref="ObserveFeastDayCommands.Validate"/> can reject a repeat
/// observance of the same named feast inside the same real-world year it already collected §5's "small
/// automatic Favor tick" for, closing the otherwise-unlimited free Favor source a caller could open by
/// resubmitting the same command.</param>
public sealed record HouseholdReligion(
    RuntimeId<Household> HouseholdId,
    PatronDeity PatronDeity,
    int Favor,
    RuntimeId<Character> ConsecratedUnderHeadCharacterId,
    string? LastObservedFeastDay = null,
    GameDate? LastObservedFeastDate = null);

/// <summary>Read/write helpers over <see cref="WorldState.HouseholdReligions"/>, matching <see
/// cref="Reputation.DignitasResolver"/>'s identical "no entry means the default" and "replace, don't
/// mutate in place" conventions — with one structural difference <see cref="HouseholdReligion"/>'s own
/// doc comment already explains: a household with no chosen Patron Deity has no meaningful Favor to
/// default to zero, so <see cref="ApplyFavorDelta"/> requires an existing entry rather than
/// auto-creating one the way <see cref="Reputation.DignitasResolver.Apply"/> does.</summary>
public static class HouseholdReligionResolver
{
    public static bool HasChosenPatron(WorldState state, RuntimeId<Household> householdId) =>
        state.HouseholdReligions.TryGet(householdId, out _);

    public static int CurrentFavor(WorldState state, RuntimeId<Household> householdId) =>
        state.HouseholdReligions.TryGet(householdId, out var entry) ? entry!.Favor : 0;

    /// <summary>§2.3's Divine Displeasure — "at sufficiently low Favor, the household enters Divine
    /// Displeasure" — computed on demand from <see cref="ReligionCatalog.DivineDispleasureThreshold"/>
    /// rather than stored as its own persisted boolean the way §10's data model sketches it: storing a
    /// derived fact risks it drifting out of sync with the Favor it's derived from, matching <see
    /// cref="Magistracies.MagistracyResolver.IsActive"/>'s own "derive it, don't duplicate it" precedent
    /// and <see cref="Policies.HouseholdPolicyState"/>'s "derive from stored dates, never store a
    /// redundant countdown" idiom applied to a boolean instead of a countdown.</summary>
    public static bool IsDivinelyDispleased(WorldState state, RuntimeId<Household> householdId) =>
        HasChosenPatron(state, householdId) && CurrentFavor(state, householdId) <= ReligionCatalog.DivineDispleasureThreshold;

    /// <summary>Applies a signed Favor delta to a household that has already chosen a Patron Deity.
    /// Every caller in this domain (<see cref="AdjustFavorCommand"/>, the Omen/Auspices/Festival
    /// commands, and this domain's own monthly systems) only ever reaches this after confirming a <see
    /// cref="HouseholdReligion"/> entry exists — via each command's own <c>Validate</c> step or, for a
    /// monthly system, by iterating <see cref="WorldState.HouseholdReligions"/> itself — so the
    /// <see cref="InvalidOperationException"/> below is a defensive invariant, not a real, reachable
    /// path in this item's own code.</summary>
    public static void ApplyFavorDelta(WorldState state, RuntimeId<Household> householdId, int delta)
    {
        if (!state.HouseholdReligions.TryGet(householdId, out var existing))
            throw new InvalidOperationException(
                $"Household '{householdId}' has no chosen Patron Deity yet — Favor cannot be adjusted before {nameof(SetPatronDeityCommand)}.");

        state.HouseholdReligions.Remove(householdId);
        state.HouseholdReligions.Add(householdId, existing! with { Favor = existing.Favor + delta });
    }
}
