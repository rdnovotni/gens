using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Interactions;

/// <summary>Which flavor of Multi-stage interaction a <see cref="Scheme"/> is running
/// (<c>gens-characters-design.md</c> §9.4's "Scheme (generic wrapper)"). One value for now — Phase 10
/// item 6 only needs to prove the shared engine works end to end; every future consumer named in §10's
/// own closing paragraph (Politics &amp; Patronage's Scheming, Romance &amp; Seduction's courtship,
/// Espionage) adds its own scheme *type* here without changing <see cref="Scheme"/>'s own shape, per
/// that section's explicit design.</summary>
public enum SchemeType
{
    /// <summary>The generic Coercive/Intrigue scheme (§9.4) — the worked example this phase implements.</summary>
    Coercive,
}

/// <summary>A Scheme's terminal (or in-progress) state (<c>gens-characters-design.md</c> §10.5): four
/// real outcomes, not two — "discovered" is meaningfully worse than a clean failure, so it is tracked
/// as its own pair of outcomes rather than a single "failed" bucket with a side flag.</summary>
public enum SchemeStatus
{
    InProgress,
    Succeeded,
    FailedQuietly,
    DiscoveredAndFoiled,
    DiscoveredAndEscalated,
}

/// <summary>
/// One initiator-vs-target Multi-stage Scheme (Phase 10 item 6; <c>gens-characters-design.md</c> §10:
/// "any interaction marked Multi-stage... runs through one shared engine rather than each system
/// inventing its own"). <see cref="Progress"/> and <see cref="DiscoveryRisk"/> are both 0-100 and
/// advanced monthly by <see cref="SchemeProgressSystem"/> until one crosses its resolution threshold —
/// see that system's own doc comment for §10.2-§10.5's progress/discovery/counter-play/resolution
/// formulas. Immutable like every other <c>WorldState</c> record — a tick replaces the entry in <see
/// cref="State.WorldState.Schemes"/> rather than mutating one in place. Deliberately carries no
/// assisting-agent or spend-modifier fields yet (§10.2's "modified by Influence or denarii spent that
/// month", §10.3's "an assisting client is a leak risk") — those are additive follow-up work once a
/// concrete scheme type actually needs them; the bare initiator/target progress-vs-discovery race is
/// what this phase's worked example needs to prove the engine end to end.
/// </summary>
public sealed record Scheme(
    RuntimeId<Scheme> SchemeId,
    RuntimeId<Character> InitiatorCharacterId,
    RuntimeId<Character> TargetCharacterId,
    SchemeType Type,
    SchemeStatus Status,
    int Progress,
    int DiscoveryRisk,
    GameDate InitiatedDate,
    GameDate LastProgressedDate)
{
    public const int MinValue = 0;
    public const int MaxValue = 100;

    public bool IsResolved => Status != SchemeStatus.InProgress;

    /// <summary>The only supported way to construct a <see cref="Scheme"/>. Always starts <see
    /// cref="SchemeStatus.InProgress"/> at zero Progress and zero DiscoveryRisk (§10.1's Initiation
    /// step: a Scheme has run for no time yet, so nothing has advanced and nothing is suspected).</summary>
    public static Scheme Create(
        RuntimeId<Scheme> schemeId,
        RuntimeId<Character> initiatorCharacterId,
        RuntimeId<Character> targetCharacterId,
        SchemeType type,
        GameDate startDate)
    {
        if (initiatorCharacterId == targetCharacterId)
            throw new ArgumentException("A Scheme cannot target its own initiator.", nameof(targetCharacterId));

        return new Scheme(
            schemeId, initiatorCharacterId, targetCharacterId, type, SchemeStatus.InProgress,
            Progress: MinValue, DiscoveryRisk: MinValue, startDate, startDate);
    }
}
