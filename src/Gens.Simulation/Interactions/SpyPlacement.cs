using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Interactions;

/// <summary>Which of the two placement shapes <c>gens-espionage-design.md</c> §2 offers a <see
/// cref="SpyPlacement"/> is. <see cref="QuickOp"/> resolves once, the same month it starts, per §2.1's
/// "a single roll ... done the moment it resolves". <see cref="PersistentNetwork"/> is the standing
/// asset of §2.2, whose <see cref="SpyPlacement.DiscoveryRisk"/> climbs monthly for as long as it
/// survives.</summary>
public enum SpyPlacementType
{
    QuickOp,
    PersistentNetwork,
}

/// <summary>A <see cref="SpyPlacement"/>'s terminal (or in-progress) state (§6). Six real outcomes, not
/// a single catch/no-catch flag: a <see cref="QuickOp"/> that never gets discovered resolves <see
/// cref="Succeeded"/> or <see cref="FailedQuietly"/> exactly like a <see cref="Scheme"/>'s clean
/// resolution; a discovered placement then splits on §6's own second roll into <see
/// cref="DiscoveredUntraced"/> ("caught, not traced") versus <see cref="DiscoveredAndTraced"/> ("caught
/// and traced") — meaningfully different consequences, so tracked as their own pair rather than one
/// "discovered" bucket with a side flag, mirroring <see cref="SchemeStatus"/>'s identical reasoning.
/// <see cref="Withdrawn"/> covers a placement ended by its own side (the spy or sponsor died, or the
/// target actor stopped existing) rather than by any roll.</summary>
public enum SpyPlacementStatus
{
    InProgress,
    Succeeded,
    FailedQuietly,
    DiscoveredUntraced,
    DiscoveredAndTraced,
    Withdrawn,
}

/// <summary>
/// One sponsor-vs-target spy placement (Phase 16 item 1; <c>gens-espionage-design.md</c> §2, §9's data
/// model). A deliberate sibling to <see cref="Scheme"/> rather than a new <see cref="SchemeType"/>: a
/// <see cref="SpyPlacementType.PersistentNetwork"/> has no "Progress reaches 100" success condition —
/// it sits at a steady, climbing <see cref="DiscoveryRisk"/> and keeps delivering §4's benefits monthly
/// until caught or withdrawn, which does not fit <see cref="Scheme"/>'s single race-to-100 shape — and
/// a placement's discovery needs a *second* roll (Traceability, §6) that <see cref="Scheme"/>'s single
/// counter-play roll has no slot for. What is reused from <see cref="Scheme"/> is the pattern, not the
/// record: 0-100 <see cref="DiscoveryRisk"/>, a monthly snapshot-then-mutate tick (<see
/// cref="SpyPlacementProgressSystem"/>), a named RNG stream, and the same <see
/// cref="Actors.RivalDossierRefresh"/> integration on genuine contact.
///
/// <see cref="TargetActorId"/> is always a <see cref="LivingWorldActor"/> — a rival house, or the
/// sponsor's own actor for the "counter-espionage against my own household" case §3 names. Non-actor
/// targets ("the local Roman administration") are out of scope for this slice.
///
/// <see cref="ConcealmentQuality"/> is fixed at placement time (the spy's own Intrigue-derived skill),
/// not recomputed monthly — the same "bare, no bonus-stacking yet" simplicity <see cref="Scheme"/>
/// itself uses for its first pass. Immutable like every other <c>WorldState</c> record — a tick
/// replaces the entry in <see cref="State.WorldState.SpyPlacements"/> rather than mutating one in
/// place.
/// </summary>
public sealed record SpyPlacement(
    RuntimeId<SpyPlacement> PlacementId,
    RuntimeId<Character> SpyCharacterId,
    RuntimeId<Character> SponsoringCharacterId,
    RuntimeId<Actor> TargetActorId,
    SpyPlacementType Type,
    SpyPlacementStatus Status,
    int ConcealmentQuality,
    int DiscoveryRisk,
    int MonthsActive,
    GameDate PlacedDate,
    GameDate LastProgressedDate)
{
    public const int MinValue = 0;
    public const int MaxValue = 100;

    public bool IsResolved => Status != SpyPlacementStatus.InProgress;

    /// <summary>The only supported way to construct a <see cref="SpyPlacement"/>. Always starts <see
    /// cref="SpyPlacementStatus.InProgress"/> at zero <see cref="DiscoveryRisk"/> and zero <see
    /// cref="MonthsActive"/> — a placement has run for no time yet, so nothing has advanced and nothing
    /// is suspected, mirroring <see cref="Scheme.Create"/>'s identical initiation-time reasoning. The
    /// spy cannot be the sponsor themselves.</summary>
    public static SpyPlacement Create(
        RuntimeId<SpyPlacement> placementId,
        RuntimeId<Character> spyCharacterId,
        RuntimeId<Character> sponsoringCharacterId,
        RuntimeId<Actor> targetActorId,
        SpyPlacementType type,
        int concealmentQuality,
        GameDate placedDate)
    {
        if (spyCharacterId == sponsoringCharacterId)
            throw new ArgumentException("A spy cannot be their own sponsor.", nameof(spyCharacterId));
        if (concealmentQuality < MinValue || concealmentQuality > MaxValue)
            throw new ArgumentOutOfRangeException(nameof(concealmentQuality), concealmentQuality, "Concealment quality must be 0-100.");

        return new SpyPlacement(
            placementId, spyCharacterId, sponsoringCharacterId, targetActorId, type, SpyPlacementStatus.InProgress,
            concealmentQuality, DiscoveryRisk: MinValue, MonthsActive: 0, placedDate, placedDate);
    }
}
