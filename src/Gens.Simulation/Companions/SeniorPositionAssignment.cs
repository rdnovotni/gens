using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Companions;

/// <summary>
/// Phase 17 item 1's Tier 3 staffing roles (<c>gens-companions-court-positions-design.md</c> §5): full
/// Court standing above a plain <see cref="DutySlot"/> or <see cref="OverseerRole"/>, closing the same
/// long-deferred gap <see cref="Religion.PriesthoodOffice"/> and <see
/// cref="Languages.InterpresAppointment"/>'s own doc comments flag. §5.1's Villa-scope titles come
/// first, §5.2's Estate/settlement-scope titles second, matching that document's own ordering.
/// </summary>
public enum SeniorPositionTitle
{
    // Villa-scope (§5.1).
    Steward,
    Bodyguard,
    HouseholdPriest,
    Secretary,
    Cellarer,
    HeadCook,
    Tutor,
    Nurse,
    WeavingMistress,
    MasterOfHospitality,
    Balneator,
    Chamberlain,
    HouseholdSpymaster,
    GuardCaptain,
    CourtPhysician,
    Treasurer,
    MasterBeekeeper,
    FurnaceMaster,
    MenagerieKeeper,
    HarborSteward,
    DovecoteKeeper,
    BakerInChief,
    GranaryKeeper,
    Curator,
    Ergastularius,
    Symposiarch,

    // Estate/settlement-scope (§5.2).
    Actor,
    InstitorMaximus,
    PraefectusMetallorum,
    PraefectusVigilum,
    NavarchusPrincipes,
    EditorMuneris,
    Tabularius,
    Procurator,
    Rationalis,
}

/// <summary>Whether a <see cref="SeniorPositionTitle"/> belongs to the villa household itself (§5.1,
/// possibly tied to one specific room/building within it) or oversees an estate/settlement-scale
/// concern (§5.2) instead.</summary>
public enum SeniorPositionScope
{
    Villa,
    EstateOrSettlement,
}

/// <summary>
/// One Character's tenure in one <see cref="SeniorPositionTitle"/> (Phase 17 item 1; §5). Kept forever
/// once created, active or not, matching <see cref="OverseerAssignment"/>'s identical convention.
/// </summary>
/// <param name="TiedBuildingId">Set only for a <see cref="SeniorPositionScope.Villa"/> title <see
/// cref="CompanionsCatalog.IsRoomTied"/> flags as tied to one specific room/building (e.g. Cellarer to
/// the cellar) — null for every whole-household or estate/settlement-scope title.</param>
/// <param name="OversightSettlementId">Set only for <see cref="SeniorPositionTitle.Procurator"/> — the
/// second settlement this Procurator administers on the household's behalf (§5.3), linked to a <see
/// cref="Stewardship.StewardshipAssignment"/> created alongside this record by <see
/// cref="AppointSecondSettlementProcuratorCommand"/>.</param>
/// <param name="OnLeaveSince">See <see cref="OverseerAssignment.OnLeaveSince"/>'s identical doc
/// comment.</param>
/// <param name="PromotedFromOverseerRecordId">Set only when this appointment is §6's promotion path
/// out of an existing <see cref="OverseerAssignment"/> (that source record is ended in the same <see
/// cref="AppointSeniorPositionCommand"/> mutate step) — null for a direct, tier-skipping appointment.</param>
public sealed record SeniorPositionAssignment(
    RuntimeId<SeniorPositionAssignment> RecordId,
    RuntimeId<Character> HolderId,
    SeniorPositionTitle Title,
    RuntimeId<Household> HouseholdId,
    SeniorPositionScope Scope,
    RuntimeId<Building>? TiedBuildingId,
    RuntimeId<Settlement>? OversightSettlementId,
    GameDate AssignedDate,
    GameDate? OnLeaveSince = null,
    GameDate? EndDate = null,
    RuntimeId<OverseerAssignment>? PromotedFromOverseerRecordId = null);

/// <summary>Read-side helpers over <see cref="WorldState.SeniorPositionAssignments"/>, matching <see
/// cref="OverseerResolver"/>'s identical linear-scan convention.</summary>
public static class SeniorPositionResolver
{
    public static bool IsActive(SeniorPositionAssignment record) => record.EndDate is null;

    /// <summary>See <see cref="OverseerResolver.IsCurrentlyFilled(OverseerAssignment)"/>'s identical
    /// doc comment.</summary>
    public static bool IsCurrentlyFilled(SeniorPositionAssignment record) => IsActive(record) && record.OnLeaveSince is null;

    public static SeniorPositionAssignment? ActiveRecordForCharacter(WorldState state, RuntimeId<Character> holderId)
    {
        foreach (var entry in state.SeniorPositionAssignments.InAscendingOrder())
            if (IsActive(entry.Value) && entry.Value.HolderId == holderId)
                return entry.Value;

        return null;
    }

    /// <summary>The active holder of <paramref name="title"/> at <paramref name="householdId"/>, if
    /// any — used to enforce §5's "one seat per title per household" gate. Deliberately not
    /// settlement-scoped even for <see cref="SeniorPositionTitle.Procurator"/>: <see
    /// cref="AppointSecondSettlementProcuratorCommand"/> checks its own settlement-scoped uniqueness separately (§5.3
    /// allows multiple concurrent Procurators, one per distant settlement), so callers appointing that
    /// title should not route through this method at all — see <see
    /// cref="AppointSeniorPositionCommands.ValidateShared"/>'s own doc comment.</summary>
    public static SeniorPositionAssignment? ActiveRecordForTitle(WorldState state, RuntimeId<Household> householdId, SeniorPositionTitle title)
    {
        foreach (var entry in state.SeniorPositionAssignments.InAscendingOrder())
        {
            var record = entry.Value;
            if (IsActive(record) && record.HouseholdId == householdId && record.Title == title)
                return record;
        }

        return null;
    }

    /// <summary>Every active Procurator record for <paramref name="householdId"/> at <paramref
    /// name="settlementId"/> specifically — <see cref="AppointSecondSettlementProcuratorCommand"/>'s own
    /// settlement-scoped uniqueness gate.</summary>
    public static SeniorPositionAssignment? ActiveProcuratorRecord(WorldState state, RuntimeId<Household> householdId, RuntimeId<Settlement> settlementId)
    {
        foreach (var entry in state.SeniorPositionAssignments.InAscendingOrder())
        {
            var record = entry.Value;
            if (IsActive(record) && record.HouseholdId == householdId && record.Title == SeniorPositionTitle.Procurator &&
                record.OversightSettlementId == settlementId)
                return record;
        }

        return null;
    }

    /// <summary>Whether <paramref name="householdId"/> currently has a Character present (not
    /// on-leave) and actively holding <paramref name="title"/> — <see
    /// cref="RationalisBonusSystem"/>'s own Treasurer/Institor Maximus/Cellarer cluster check.</summary>
    public static bool IsCurrentlyFilled(WorldState state, RuntimeId<Household> householdId, SeniorPositionTitle title)
    {
        var record = ActiveRecordForTitle(state, householdId, title);
        return record is not null && IsCurrentlyFilled(record);
    }
}
