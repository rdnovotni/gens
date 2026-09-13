using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Companions;

/// <summary>
/// Phase 17 item 1's Tier 2 staffing roles (<c>gens-companions-court-positions-design.md</c> §4): the
/// mid-tier, building/plot-scoped working staff above a plain <see cref="DutySlot"/> but below a
/// <see cref="SeniorPositionTitle"/>'s full Court standing — §6.2/§6.20's own long-deferred "not yet
/// built" gap, closed here. Every value below is drawn from §4's own named-role catalogue; none are
/// invented for this item.
/// </summary>
public enum OverseerRole
{
    Vilicus,
    MagisterOfficinae,
    Metallarius,
    Institor,
    Portitor,
    Aquarius,
    Horrearius,
    SacerdosPublicus,
    Rhetor,
    Lanista,
    Editor,
    Valetudinarius,
    Venalicius,
    LenoLena,
    Argentarius,
    Vigil,
    Alimentarius,
    Libitinarius,
    Navarchus,
}

/// <summary>
/// One Character's tenure in one <see cref="OverseerRole"/> at one <see cref="Building"/> (Phase 17
/// item 1; §4). Kept forever once created, active or not, matching <see
/// cref="Magistracies.MagistracyRecord"/>'s identical "resolved or not, kept for the campaign's
/// lifetime" convention. <see cref="EndDate"/> null is the "still active" flag; an ended record is
/// never removed, only replaced (remove then re-add under the same <see cref="RecordId"/>) to set it.
/// </summary>
/// <param name="OnLeaveSince">Non-null while the holder is away as part of a <see
/// cref="Travel.TravelParty"/>'s retinue (§7) — the position is temporarily unfilled without ending
/// the record, so the holder's link to the building survives the trip. Set by <see
/// cref="RetinueVacancyCommands"/> at <see cref="Travel.BeginTravelCommand"/> time and cleared again
/// once the trip's return leg completes (<see cref="Travel.TravelProgressSystem"/>).</param>
public sealed record OverseerAssignment(
    RuntimeId<OverseerAssignment> RecordId,
    RuntimeId<Character> HolderId,
    OverseerRole Role,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Building> BuildingId,
    GameDate AssignedDate,
    GameDate? OnLeaveSince = null,
    GameDate? EndDate = null);

/// <summary>Read-side helpers over <see cref="WorldState.OverseerAssignments"/>, matching <see
/// cref="Magistracies.MagistracyResolver"/>'s identical "a small, hand-curated collection doesn't
/// need a maintained secondary index yet" linear-scan convention.</summary>
public static class OverseerResolver
{
    public static bool IsActive(OverseerAssignment record) => record.EndDate is null;

    /// <summary>Active and present (not currently traveling as part of a retinue) — the "actually
    /// doing the job right now" reading every capacity/cluster check should use in place of plain
    /// <see cref="IsActive"/>.</summary>
    public static bool IsCurrentlyFilled(OverseerAssignment record) => IsActive(record) && record.OnLeaveSince is null;

    public static OverseerAssignment? ActiveRecordForCharacter(WorldState state, RuntimeId<Character> holderId)
    {
        foreach (var entry in state.OverseerAssignments.InAscendingOrder())
            if (IsActive(entry.Value) && entry.Value.HolderId == holderId)
                return entry.Value;

        return null;
    }

    public static OverseerAssignment? ActiveRecordForBuilding(WorldState state, RuntimeId<Building> buildingId)
    {
        foreach (var entry in state.OverseerAssignments.InAscendingOrder())
            if (IsActive(entry.Value) && entry.Value.BuildingId == buildingId)
                return entry.Value;

        return null;
    }

    public static bool HasActiveOverseer(WorldState state, RuntimeId<Building> buildingId) =>
        ActiveRecordForBuilding(state, buildingId) is not null;

    /// <summary>Whether <paramref name="householdId"/> currently has a Character present (not
    /// on-leave) and actively holding <paramref name="role"/> — <see
    /// cref="RationalisBonusSystem"/>'s own Argentarius cluster check.</summary>
    public static bool IsCurrentlyFilled(WorldState state, RuntimeId<Household> householdId, OverseerRole role)
    {
        foreach (var entry in state.OverseerAssignments.InAscendingOrder())
        {
            var record = entry.Value;
            if (record.HouseholdId == householdId && record.Role == role && IsCurrentlyFilled(record))
                return true;
        }

        return false;
    }
}
