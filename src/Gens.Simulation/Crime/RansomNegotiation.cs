using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Crime;

/// <summary>§10's real resolution outcomes for a <see cref="RansomNegotiation"/>. <c>mercyReleaseNoRansom</c>
/// — §11's own data-model sketch lists it as a fourth resolution value here — is deliberately not a
/// fourth member of this enum: §10 itself frames mercy as "always available too," not as something
/// that has to flow through an open negotiation first, and this item already builds that exact real
/// primitive as <see cref="ReleaseFromDetentionCommand"/>, usable whether or not a <see
/// cref="RansomNegotiation"/> was ever opened at all. Duplicating that same release path a second time
/// as a resolution of this command specifically would only fragment one real mechanic into two.</summary>
public enum RansomResolution
{
    Paid,
    Refused,
    BargainedDown,
}

/// <summary>
/// One ransom negotiation over a Detained captive (Phase 12 item 5; §10; §11's own data-model sketch).
/// Kept forever once opened, matching <see cref="Legal.LegalCase"/>'s identical "kept for the
/// campaign's lifetime" convention — §10's own "every ransom resolution is real material" for the
/// Dynasty Chronicle needs the full record, not just the live negotiation.
/// </summary>
public sealed record RansomNegotiation(
    RuntimeId<RansomNegotiation> NegotiationId,
    RuntimeId<Character> CaptiveCharacterId,
    RuntimeId<Household> CapturingHouseholdId,
    RuntimeId<Household> TargetHouseholdId,
    Money AmountOffered,
    GameDate OpenedDate,
    Money? AmountCountered = null,
    RansomResolution? Resolution = null,
    GameDate? ResolvedDate = null);

/// <summary>Read-side helpers over <see cref="WorldState.RansomNegotiations"/>, matching <see
/// cref="Legal.LegalCaseResolver"/>'s identical linear-scan convention, plus the one Household-to-Actor
/// bridge <see cref="ResolveRansomNegotiationCommand"/> needs for §10/§11's own Rival Houses Standing
/// integration.</summary>
public static class RansomNegotiationResolver
{
    public static bool IsOpen(RansomNegotiation negotiation) => negotiation.Resolution is null;

    public static RansomNegotiation? ActiveFor(WorldState state, RuntimeId<Character> captiveCharacterId)
    {
        foreach (var entry in state.RansomNegotiations.InAscendingOrder())
            if (IsOpen(entry.Value) && entry.Value.CaptiveCharacterId == captiveCharacterId)
                return entry.Value;

        return null;
    }

    /// <summary>Resolves a Household back to the <see cref="LivingWorldActor"/> it heads, if any — the
    /// same "resolve back to the tracked Actor, if one exists" technique <see
    /// cref="Actors.RivalDossierRefresh.RefreshForCharacter"/> already uses for a bare Character id,
    /// applied here to a Household via its own recorded head. A player-controlled household's own head
    /// never heads a <see cref="LivingWorldActor"/> at all (<see cref="Reputation.HouseholdReputation"/>'s
    /// own doc comment: "unlike a rival house, [a player Household] is never itself a
    /// LivingWorldActor") — so this returns null for exactly that case, which is the real, honest
    /// reason <see cref="ResolveRansomNegotiationCommand"/>'s own Rival Houses Standing event only ever
    /// fires when *both* sides of a ransom happen to be tracked rival houses.</summary>
    public static RuntimeId<Actor>? TryFindActorForHousehold(WorldState state, RuntimeId<Household> householdId)
    {
        if (!state.HouseholdHeadships.TryGet(householdId, out var headship))
            return null;

        foreach (var entry in state.Actors.InAscendingOrder())
            if (entry.Value.HeadCharacterId == headship!.HeadCharacterId)
                return entry.Key;

        return null;
    }
}
