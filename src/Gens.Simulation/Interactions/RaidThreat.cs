using System.Linq;
#nullable enable
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Time;

namespace Gens.Simulation.Interactions;

/// <summary>What a raid actually went after (<c>gens-piracy-banditry-design.md</c> §3's "targets goods
/// (a Trade Route)... livestock... or a settlement directly"). Familia-member and background-population
/// kidnapping (§8) are deliberately out of this slice — both open into Ransom negotiation (Characters
/// §9.5), a distinct piece of machinery this item does not build; see <see cref="RaidThreatSystem"/>'s
/// own doc comment for the full deferred list.</summary>
public enum RaidTargetType
{
    TradeGoods,
    Livestock,
    Settlement,
}

/// <summary>How a <see cref="RaidThreat"/> resolved (§3: "'Interceptable,' as the core doc names it
/// directly"). Three real outcomes, not a single catch/no-catch flag, mirroring <see
/// cref="SpyPlacementStatus"/>'s identical reasoning for a different two-roll sequence.</summary>
public enum RaidOutcome
{
    /// <summary>The defender's own security intercepted and drove the raiders off before any loss
    /// occurred (§3: "a well-defended target can repel... the raiders outright").</summary>
    InterceptedRepelled,

    /// <summary>The defender's own security intercepted and captured the raiders (§3: "a well-defended
    /// target can... capture the raiders outright"). §3 goes on to note a captured raider is a real
    /// Character available for a Legal &amp; Court matter or sale into slavery — Phase 16 item 6 closes
    /// this: <see cref="RaidCaptiveGenerator"/> generates that Character and a Crime <see
    /// cref="Crime.DetentionRecord"/> is opened for them, exactly like Military's own captured-Character
    /// path, so the existing <see cref="Crime.OpenRansomNegotiationCommand"/> flow can consume it without
    /// a parallel captive minigame.</summary>
    RaidersCaptured,

    /// <summary>Nothing intercepted the raid; the target household loses <see
    /// cref="RaidThreat.SpoilsLost"/>.</summary>
    RaidSucceeded,
}

/// <summary>
/// One resolved bandit/pirate raid against a household (Phase 16 item 2;
/// <c>gens-piracy-banditry-design.md</c> §2, §3, §9's data model). Unlike <see
/// cref="SpyPlacement"/>, a <see cref="RaidThreat"/> always resolves the same month it is
/// generated (<see cref="RaidThreatSystem"/>'s single roll sequence, mirroring <see
/// cref="SpyPlacementType.QuickOp"/>'s identical "one roll, not an accumulating one" shape) — there is
/// no in-progress state to track, so this record is written once, fully resolved, as a permanent
/// historical entry rather than mutated in place. Immutable like every other <c>WorldState</c> record.
/// </summary>
public sealed record RaidThreat(
    RuntimeId<RaidThreat> RaidId,
    RuntimeId<Actor> ConfederationActorId,
    RuntimeId<Household> TargetHouseholdId,
    RaidTargetType TargetType,
    int DefenderSecurityLevel,
    RaidOutcome Outcome,
    Money SpoilsLost,
    GameDate RaidDate)
{
    /// <summary>The only supported way to construct a <see cref="RaidThreat"/> — always fully resolved
    /// at creation (see this type's own doc comment), so there is no separate in-progress factory the
    /// way <see cref="SpyPlacement.Create"/> needs.</summary>
    public static RaidThreat Create(
        RuntimeId<RaidThreat> raidId,
        RuntimeId<Actor> confederationActorId,
        RuntimeId<Household> targetHouseholdId,
        RaidTargetType targetType,
        int defenderSecurityLevel,
        RaidOutcome outcome,
        Money spoilsLost,
        GameDate raidDate) =>
        new(raidId, confederationActorId, targetHouseholdId, targetType, defenderSecurityLevel, outcome, spoilsLost, raidDate);
}
