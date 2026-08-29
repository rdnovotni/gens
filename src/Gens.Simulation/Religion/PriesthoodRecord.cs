using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Religion;

/// <summary>
/// One Character's tenure in one <see cref="PriesthoodOffice"/> (Phase 12 item 3; §10's own <c>
/// PriesthoodOffice</c> sketch, mirroring <see cref="Magistracies.MagistracyRecord"/>'s own shape by
/// direct instruction). Unlike a Magistracy's annual term (§5.7), a Roman priesthood was historically
/// held for life or until death/disgrace, not re-contested on a fixed cycle — this record therefore
/// carries no term-length or renewal concept at all, only <see cref="EndDate"/> as the "still active"
/// flag, ended solely by the holder's death (<see cref="PriesthoodTrickleSystem"/>). Kept forever once
/// created, active or not, matching <see cref="Magistracies.MagistracyRecord"/>'s identical "kept for
/// the campaign's lifetime" convention.
/// </summary>
/// <param name="FlamenDeity">Set only for <see cref="PriesthoodOffice.Flamen"/> — must match the
/// holder's own household's chosen <see cref="Religion.PatronDeity"/> at appointment time (§6.2: "a
/// priest dedicated specifically to the household's own Patron Deity"). Always null for every other
/// office.</param>
public sealed record PriesthoodRecord(
    RuntimeId<PriesthoodRecord> RecordId,
    RuntimeId<Character> HolderId,
    PriesthoodOffice Office,
    RuntimeId<Settlement> SettlementId,
    GameDate AppointedDate,
    PatronDeity? FlamenDeity = null,
    GameDate? EndDate = null);

/// <summary>Read-side helpers over <see cref="WorldState.PriesthoodRecords"/>, matching <see
/// cref="Magistracies.MagistracyResolver"/>'s identical "a small, hand-curated collection doesn't need a
/// maintained secondary index" linear-scan convention.</summary>
public static class PriesthoodResolver
{
    public static bool IsActive(PriesthoodRecord record) => record.EndDate is null;

    public static PriesthoodRecord? ActiveRecord(
        WorldState state, RuntimeId<Settlement> settlementId, PriesthoodOffice office, RuntimeId<Character> holderId)
    {
        foreach (var entry in state.PriesthoodRecords.InAscendingOrder())
        {
            var record = entry.Value;
            if (IsActive(record) && record.SettlementId == settlementId && record.Office == office && record.HolderId == holderId)
                return record;
        }

        return null;
    }

    /// <summary>The first active record of any office a Character currently holds, if any — used by
    /// <see cref="AppointPriesthoodCommand"/>'s Pontifex capstone gate.</summary>
    public static PriesthoodRecord? AnyActiveRecordFor(WorldState state, RuntimeId<Character> holderId)
    {
        foreach (var entry in state.PriesthoodRecords.InAscendingOrder())
            if (IsActive(entry.Value) && entry.Value.HolderId == holderId)
                return entry.Value;

        return null;
    }
}
