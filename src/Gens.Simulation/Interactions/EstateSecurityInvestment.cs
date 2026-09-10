using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Interactions;

/// <summary>
/// One household's standing defensive investment against raiding (Phase 16 item 2;
/// <c>gens-piracy-banditry-design.md</c> §9 — "the concrete mechanism behind 'scale with the player's
/// security investment'"). §9 names the Vigil, Praefectus Vigilum, Navarchus, Watchtower, and City Walls
/// as the intended sources of this figure, but none of those roles or buildings exist in this codebase
/// yet (no <c>Watchtower</c>/<c>CityWalls</c> building sector, no Vigil/Navarchus court position) — this
/// record is deliberately their common downstream aggregate rather than a recomputed roll-up of pieces
/// that do not exist, matching <see cref="Actors.LivingWorldActorMilitaryStrength"/>'s identical
/// "abstract the target concept now, wire the real sources in later" convention. A future pass that adds
/// those roles/buildings should feed this same <see cref="SecurityLevel"/> rather than inventing a
/// second security concept.
///
/// Read directly by <see cref="RaidThreatSystem"/> (§3's "Interceptable... using a well-defended
/// target's own security level"): a higher <see cref="SecurityLevel"/> raises a targeted household's
/// odds of repelling or capturing the raiders outright and shrinks the spoils lost on an uncontested
/// success. §9's other named axis — a well-defended estate being less likely to be targeted for a raid
/// in the first place, a worse risk-reward proposition for the Confederation — is deliberately not
/// modeled in this slice: <see cref="RaidThreatSystem"/>'s own target selection is a plain regional
/// tie-break (ADR 0004) with no security weighting, so raising this figure never itself reduces how
/// often a household gets picked as a target, only what happens once it is.
/// </summary>
public sealed record EstateSecurityInvestment(
    RuntimeId<Household> HouseholdId,
    int SecurityLevel,
    GameDate LastAdjustedDate)
{
    public const int MinValue = 0;
    public const int MaxValue = 100;

    /// <summary>The only supported way to construct an <see cref="EstateSecurityInvestment"/>. A fresh
    /// household starts unguarded (<see cref="MinValue"/>) — matching §9's framing of security as
    /// something the player actively invests in, never a free default.</summary>
    public static EstateSecurityInvestment CreateUnguarded(RuntimeId<Household> householdId, GameDate date) =>
        new(householdId, MinValue, date);

    /// <summary>Returns this record with <see cref="SecurityLevel"/> clamped to <paramref
    /// name="newLevel"/> and <see cref="LastAdjustedDate"/> updated — the only supported way to change
    /// an existing entry, matching every other <c>WorldState</c> record's immutable
    /// remove-then-re-add convention.</summary>
    public EstateSecurityInvestment WithLevel(int newLevel, GameDate date) =>
        this with { SecurityLevel = Math.Clamp(newLevel, MinValue, MaxValue), LastAdjustedDate = date };
}
