using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Reputation;

/// <summary>A <see cref="FavorObligation"/>'s lifecycle (Phase 12 item 1).</summary>
public enum FavorStatus
{
    /// <summary>Granted and not yet resolved — the beneficiary genuinely owes the grantor.</summary>
    Outstanding,

    /// <summary>Called in and honored (<see cref="SettleFavorCommand"/>).</summary>
    Repaid,

    /// <summary>Written off by the grantor without ever being called in (<see
    /// cref="SettleFavorCommand"/>).</summary>
    Forgiven,

    /// <summary>Timed out unresolved (<see cref="FavorExpirationSystem"/>) — a debt too old to
    /// plausibly still be live, matching <see cref="Actors.HouseStandingResolver"/>'s own "very old
    /// facts stop mattering" shape without inventing a bespoke decay curve for this primitive.</summary>
    Expired,
}

/// <summary>
/// One favor owed by one Character to another (Phase 12 item 1) — the generic, kind-agnostic
/// obligation primitive <c>gens-politics-patronage-design.md</c> §4.2's Clientela favor system (a
/// Legal/Mercantile/Martial/Religious/Administrative specialty performing "what favor they can
/// actually perform when called on") is meant to be built on top of, per that document's own line "a
/// favor drawn on too often without reciprocation costs the relationship-web opinion between patron and
/// client — Clientela is reciprocal, not a free resource tap". This item deliberately does not build
/// Clientela itself (Phase 12 item 2's job, not item 1's — see the roadmap's own internal ordering) nor
/// wire a repayment/call-in into <see cref="Characters.Relationship.Opinion"/> automatically: item 2 is
/// what actually decides how a Clientela-specific favor request should move a patron/client
/// relationship, and forcing that decision here would be inventing item 2's own mechanic early. What
/// this item does provide is the shared ledger shape any future favor source — Clientela, a Legal &amp;
/// Court witness favor, a Collegium member's assistance — can open, resolve, or let lapse through the
/// same three commands (<see cref="GrantFavorCommand"/>, <see cref="SettleFavorCommand"/>, and time
/// itself via <see cref="FavorExpirationSystem"/>) rather than each inventing its own bookkeeping.
///
/// Kept once resolved rather than removed, matching <see cref="Funerary.FuneralRecord"/>'s identical
/// "resolved or not, kept for the campaign's lifetime" convention — a later system reading "who owes
/// whom, and what's the history" needs the full ledger, not just the live balance.
/// </summary>
/// <param name="Kind">A plain, free-form description of what the favor actually was ("vouched for the
/// household at the Curia", "carried a message past a checkpoint") rather than a closed enum: no single
/// favor-kind catalog spans every future source the way Clientela's own five Specialties do for that
/// one system alone (§4.2) — a future Clientela integration is free to constrain this to its own
/// Specialty vocabulary when it actually calls <see cref="GrantFavorCommand"/>, matching <see
/// cref="AdjustDignitasCommand"/>'s identical <c>Reason</c> convention.</param>
public sealed record FavorObligation(
    RuntimeId<FavorObligation> FavorId,
    RuntimeId<Character> GrantorId,
    RuntimeId<Character> BeneficiaryId,
    string Kind,
    GameDate GrantedDate,
    FavorStatus Status,
    GameDate? ResolvedDate = null);
