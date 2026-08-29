using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Religion;

/// <summary>Whether an Omen was heeded or ignored (Phase 12 item 3; §4.1's own <c>playerChoice</c>
/// field, "heeded" | "ignored").</summary>
public enum OmenChoice
{
    Heeded,
    Ignored,
}

/// <summary>How an Omen ultimately resolved (§4.1's own <c>outcome</c> field). <see cref="Pending"/> is
/// this item's own addition to that vocabulary — §4.1's data model comments the field itself as "not
/// yet resolved" via a nullable value; a real enum member reads more plainly than a null in every
/// switch this domain writes, matching <see cref="Magistracies.MagistracyLossReason"/>'s own "a real,
/// checkable member beats an implied null" precedent.</summary>
public enum OmenOutcome
{
    Pending,
    Averted,
    ConsequenceLanded,
    NoConsequence,
}

/// <summary>
/// One Omen Event (Phase 12 item 3; §4.1, §10's own <c>OmenEvent</c> sketch) — "a flight of birds read a
/// certain way, a strange dream, a sudden storm on an inauspicious day," themed to the household's own
/// Patron Deity at the moment it was raised. Kept forever once raised, resolved or not, matching <see
/// cref="Reputation.FavorObligation"/>'s identical "resolved or not, kept for the campaign's lifetime"
/// convention — a later system reading "how has this household's relationship with the divine gone"
/// needs the full history, not just the live count.
///
/// <b>Scope note:</b> §4.1 describes Omens as surfacing "periodically... entirely independent of
/// anything the player commissions," with frequency/severity scaling off Divine Displeasure and
/// individual Characters' Zealotry axis. No periodic generator exists anywhere in this codebase that
/// this item can hook into for that: Phase 9's weighted Event pool (<see cref="Events.EventPoolSystem"/>)
/// is content-authored and would need a Religion-specific pool entry, which is content work, not code
/// this item's own scope reaches (matching this codebase's own "content is data, rules are code"
/// rule 10) — so <see cref="RaiseOmenCommand"/> below is the commissionable/generatable primitive a
/// future content Event-pool entry (or any other caller) submits, exactly the same "future caller"
/// shape <see cref="Reputation.AdjustDignitasCommand"/>'s own doc comment used for item 1's own
/// forward-referenced triggers.
/// </summary>
public sealed record OmenEvent(
    RuntimeId<OmenEvent> OmenId,
    RuntimeId<Household> HouseholdId,
    GameDate RaisedDate,
    PatronDeity ThemedDeity,
    int Severity,
    OmenChoice? PlayerChoice = null,
    OmenOutcome Outcome = OmenOutcome.Pending);

/// <summary>Read-side helpers over <see cref="WorldState.OmenEvents"/>.</summary>
public static class OmenResolver
{
    public static bool TryGet(WorldState state, RuntimeId<OmenEvent> omenId, out OmenEvent omen) =>
        state.OmenEvents.TryGet(omenId, out omen);
}
