using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Stewardship;

/// <summary>The three contexts a <see cref="StewardshipAssignment"/> can cover
/// (<c>gens-steward-council-auto-management-design.md</c> §7): Travel is the shortest and most
/// common, Regency for a minor heir the longest and highest-stakes.</summary>
public enum StewardshipContext
{
    Travel,
    SecondSettlementProcurator,
    Regency,
}

/// <summary>Whether one appointee or a filled Council of Senior Positions runs the household in the
/// player's absence (§2/§2.1).</summary>
public enum StewardshipMode
{
    SingleSteward,
    Council,
}

/// <summary>The player-set dial mirroring Events' Manual Mode (§3): how much autonomous authority the
/// steward/council actually exercises, bounded by the Always-Held decisions (§4) no level ever
/// crosses.</summary>
public enum StewardAutonomyLevel
{
    Conservative,
    Standard,
    FullAutonomy,
}

/// <summary>One Senior Position's portfolio (§2.1): Rationalis owns Finance, Praefectus Military,
/// Tabularius Legal, and the second-settlement Procurator that settlement's own affairs.</summary>
public enum CouncilDomain
{
    Finance,
    Military,
    Legal,
    SecondSettlement,
}

/// <summary>One filled Council seat (§2.1).</summary>
public readonly record struct CouncilMember(CouncilDomain Domain, RuntimeId<Character> CharacterId);

/// <summary>
/// One household's delegated-management assignment (Phase 10 item 2; §10's <c>StewardshipAssignment</c>
/// data model). Deliberately carries no separate "playbook" field: <see
/// cref="Policies.HouseholdPolicyState"/> (Phase 9 item 2) is already what a steward "follows" — adding
/// a second, parallel settings blob here would duplicate that machinery rather than reuse it (rule 10).
/// Immutable like every other <c>WorldState</c> record — a command replaces the entry in <see
/// cref="State.WorldState.StewardshipAssignments"/> rather than mutating one in place.
/// </summary>
/// <param name="EndDate">Null while the assignment is active; set once (Travel return, Regency's
/// natural end) and never cleared again — an ended assignment's record is kept, not removed, so <see
/// cref="ReturnReport"/> (a later package) can still be built from it afterward.</param>
public sealed record StewardshipAssignment(
    RuntimeId<StewardshipAssignment> AssignmentId,
    RuntimeId<Household> HouseholdId,
    StewardshipContext Context,
    StewardshipMode Mode,
    RuntimeId<Character>? AppointeeCharacterId,
    IReadOnlyList<CouncilMember> CouncilMembers,
    RuntimeId<Character>? CouncilHeadCharacterId,
    StewardAutonomyLevel AutonomyLevel,
    GameDate StartDate,
    GameDate? EndDate = null)
{
    /// <summary>Standard is the stated default at every context (§3) — including, per §7's explicit
    /// callout, the permanent Second-Settlement Procurator role, which does not default to Full
    /// Autonomy just because it is long-running.</summary>
    public const StewardAutonomyLevel DefaultAutonomyLevel = StewardAutonomyLevel.Standard;

    public bool IsActive => EndDate is null;

    /// <summary>The only supported way to construct a <see cref="StewardshipAssignment"/>. Enforces
    /// what each <see cref="StewardshipMode"/> requires: <see cref="StewardshipMode.SingleSteward"/>
    /// needs exactly one appointee and no Council seats; <see cref="StewardshipMode.Council"/> needs at
    /// least one filled seat and no single appointee (§2.1: "once a household has filled specialized
    /// Senior Positions, autonomous management can run as a Council" — one or the other, not both).</summary>
    public static StewardshipAssignment Create(
        RuntimeId<StewardshipAssignment> assignmentId,
        RuntimeId<Household> householdId,
        StewardshipContext context,
        StewardshipMode mode,
        RuntimeId<Character>? appointeeCharacterId,
        IReadOnlyList<CouncilMember>? councilMembers,
        RuntimeId<Character>? councilHeadCharacterId,
        StewardAutonomyLevel autonomyLevel,
        GameDate startDate)
    {
        var members = councilMembers ?? Array.Empty<CouncilMember>();

        if (mode == StewardshipMode.SingleSteward)
        {
            if (appointeeCharacterId is null)
                throw new ArgumentException("A single-steward assignment requires an appointee.", nameof(appointeeCharacterId));
            if (members.Count > 0)
                throw new ArgumentException("A single-steward assignment cannot also carry Council seats.", nameof(councilMembers));
            if (councilHeadCharacterId is not null)
                throw new ArgumentException("A single-steward assignment has no Council head.", nameof(councilHeadCharacterId));
        }
        else
        {
            if (members.Count == 0)
                throw new ArgumentException("A Council assignment requires at least one filled seat.", nameof(councilMembers));
            if (appointeeCharacterId is not null)
                throw new ArgumentException("A Council assignment has no single appointee.", nameof(appointeeCharacterId));
            if (councilHeadCharacterId is { } headId && members.All(m => m.CharacterId != headId))
                throw new ArgumentException("The Council head must be one of the filled seats.", nameof(councilHeadCharacterId));
        }

        return new StewardshipAssignment(
            assignmentId, householdId, context, mode, appointeeCharacterId, members, councilHeadCharacterId,
            autonomyLevel, startDate);
    }
}
