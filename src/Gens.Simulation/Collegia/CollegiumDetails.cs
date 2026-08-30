using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Religion;
using Gens.Simulation.State;

namespace Gens.Simulation.Collegia;

/// <summary>
/// The Collegia-specific data layered on top of one <see cref="LivingWorldActor"/> of <see
/// cref="LivingWorldActorType.Collegium"/> (Phase 12 item 6; <c>gens-collegia-guilds-design.md</c>
/// §11's own data-model sketch) — a Collegium is a real, structured organization the existing Rival
/// Houses framework already reserves a slot for (<see cref="LivingWorldActorType"/>'s own doc comment:
/// "the rest exist here so the framework itself does not need to change shape"), not a parallel entity
/// kind with its own ID counter. Name, Dignitas, and its head Character (the <b>Magister</b>, §3) are
/// all read live off the underlying <see cref="LivingWorldActor"/> record rather than duplicated here,
/// matching <see cref="RivalDossier"/>'s own "read live, hold only what genuinely differs" convention —
/// this record only ever holds facts a <see cref="LivingWorldActor"/> has no field for at all. The
/// <b>Arca</b> (§3's shared treasury) is likewise not a field here: it is a real <see
/// cref="LedgerAccount"/> at <see cref="LedgerAccountKey.ForActor(RuntimeId{Actor})"/> keyed by this
/// collegium's own <see cref="ActorId"/>, reusing the ledger's existing per-Actor account kind directly
/// rather than inventing a second money-tracking mechanism.
///
/// <b>Sparse</b>: an <see cref="Actor"/> with no entry here is not a Collegium at all (a Rival Houses
/// <see cref="LivingWorldActorType.Gens"/> actor, say). Kept forever once a Collegium is founded and
/// removed only alongside its own <see cref="LivingWorldActor"/> entry on <see
/// cref="DissolveCollegiumCommand"/>'s real, terminal dissolution — matching <see
/// cref="LivingWorldActorExtinctionSystem"/>'s identical "removed outright, not frozen" precedent for a
/// genuinely terminal transition.
/// </summary>
/// <param name="LinkedPopGroupType">Set only for <see cref="CollegiumType.Opificum"/> (§2: "drawn
/// directly from... Opifices and Negotiatores"); <c>null</c> for every other type.</param>
/// <param name="LinkedPatronDeity">Set only for <see cref="CollegiumType.CultSpecific"/>; <c>null</c>
/// for every other type — see that enum value's own doc comment for the foreign-cult half this does
/// not cover.</param>
/// <param name="ScholaPropertyId">§3's meeting hall, a Land Ownership &amp; Real Estate Property Record
/// — always <c>null</c> in this implementation. No <c>PropertyRecord</c> type, or any other code from
/// that document, exists anywhere in this codebase yet (Land Ownership &amp; Real Estate is unbuilt), so
/// this field exists only as the documented hook a future pass wires, matching <see
/// cref="Actors.LivingWorldActorMilitaryStrength.ResolvedForceId"/>'s identical "reference an entity
/// kind that does not exist yet" precedent.</param>
/// <param name="PatronHouseholdId">§4's sponsoring patron, set only once <see
/// cref="SponsorCollegiumCommand"/> is accepted — <c>null</c> for an unsponsored collegium.</param>
/// <param name="QuinquennalisCharacterId">§3's census-cycle officer. §12's own open question notes the
/// real census-cycle trigger isn't specified against this game's monthly tick — this field is set only
/// by direct appointment (<see cref="AppointQuinquennalisCommand"/>); no monthly system ever appoints or
/// replaces one automatically.</param>
/// <param name="MemberHouseholdIds">§2's membership roster. A short, hand-curated list of real,
/// already-tracked households (the player's own, or a <see cref="LivingWorldActor"/> whose head is
/// already resolved) rather than a derived read off Settlement Demographics' own pop-group aggregates —
/// <see cref="Characters.PopGroup"/> is keyed by (settlement, group type), not by any individual
/// household, so a trade collegium's linked pop group names the trade it organizes without literally
/// enumerating that pop group's members as roster entries, matching <see
/// cref="Clientela.ClientPoachingSystem"/>'s own "only ever targets an Actor whose head is already
/// resolved" precedent for the same kind of gap.</param>
public sealed record CollegiumDetails(
    RuntimeId<Actor> ActorId,
    CollegiumType CollegiumType,
    CollegiumLegalStatus LegalStatus,
    PopGroupType? LinkedPopGroupType,
    PatronDeity? LinkedPatronDeity,
    string? ScholaPropertyId,
    RuntimeId<Household>? PatronHouseholdId,
    RuntimeId<Character>? QuinquennalisCharacterId,
    IReadOnlyList<RuntimeId<Household>> MemberHouseholdIds);

/// <summary>Read-side helpers over <see cref="WorldState.Collegia"/>, matching <see
/// cref="Magistracies.MagistracyResolver"/>'s identical "a small, hand-curated collection doesn't need
/// a maintained secondary index yet" linear-scan convention.</summary>
public static class CollegiumResolver
{
    /// <summary>The Collegium's own head Character (§3's Magister) — read live off the underlying <see
    /// cref="LivingWorldActor.HeadCharacterId"/>, per this record's own doc comment.</summary>
    public static RuntimeId<Character>? MagisterCharacterId(WorldState state, RuntimeId<Actor> collegiumId) =>
        state.Actors.TryGet(collegiumId, out var actor) ? actor!.HeadCharacterId : null;

    /// <summary>The Collegium's own running Arca balance — read live off its per-Actor <see
    /// cref="LedgerAccount"/>, per this record's own doc comment.</summary>
    public static Money ArcaBalance(WorldState state, RuntimeId<Actor> collegiumId) =>
        state.LedgerAccounts.TryGet(LedgerAccountKey.ForActor(collegiumId), out var account) ? account!.Balance : Money.Zero;

    public static bool IsSponsored(WorldState state, RuntimeId<Actor> collegiumId) =>
        state.Collegia.TryGet(collegiumId, out var details) && details!.PatronHouseholdId is not null;

    public static bool IsMember(WorldState state, RuntimeId<Actor> collegiumId, RuntimeId<Household> householdId) =>
        state.Collegia.TryGet(collegiumId, out var details) && details!.MemberHouseholdIds.Contains(householdId);
}
