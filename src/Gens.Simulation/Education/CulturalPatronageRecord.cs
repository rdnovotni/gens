using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Education;

/// <summary>§7's two ongoing Cultural Patronage commitments — "an ongoing monthly commitment (a
/// Maecenas-style standing sponsorship), not a one-time act," per this ticket's own confirmed scope
/// decision (modeled on <see cref="Religion.HouseholdReligion"/>/<see
/// cref="Religion.FavorCycleSystem"/>'s founding+cycle shape, not <see
/// cref="Religion.FundFestivalCelebrationCommand"/>'s one-time spend).</summary>
public enum CulturalPatronageType
{
    LiteraryPatron,
    Symposium,
}

/// <summary>One household's standing Cultural Patronage commitment (Phase 17 item 2; §7), keyed by its
/// own <see cref="RuntimeId{T}"/> rather than by household directly — unlike <see
/// cref="Religion.HouseholdReligion"/>'s single-slot shape, a household may hold one active record of
/// each <see cref="CulturalPatronageType"/> at once (a Literary Patronage and a Symposium are not
/// mutually exclusive), so this needs its own id the way <see
/// cref="Magistracies.MagistracyRecord"/> does. <see cref="EndedDate"/> null is the "currently active"
/// flag — an ended record is never removed, only replaced (remove then re-add under the same <see
/// cref="RecordId"/>), matching <see cref="Magistracies.MagistracyRecord"/>'s identical "ended records
/// are kept, not deleted" convention: a household's patronage history is a real part of its cultural
/// standing, not scratch state.</summary>
public sealed record CulturalPatronageRecord(
    RuntimeId<CulturalPatronageRecord> RecordId,
    RuntimeId<Household> HouseholdId,
    CulturalPatronageType Type,
    RuntimeId<Character> HostCharacterId,
    GameDate StartedDate,
    GameDate? EndedDate = null);

/// <summary>Read-side helpers over <see cref="WorldState.CulturalPatronageRecords"/>, matching <see
/// cref="Magistracies.MagistracyResolver"/>'s identical "a small, hand-curated collection doesn't need a
/// maintained secondary index yet" linear-scan judgment call.</summary>
public static class CulturalPatronageResolver
{
    public static bool IsActive(CulturalPatronageRecord record) => record.EndedDate is null;

    /// <summary>The active record of a given type for a household, if any — used to reject a duplicate
    /// commitment and by <see cref="EndPatronageCommand"/> to find what it is cancelling.</summary>
    public static CulturalPatronageRecord? ActiveRecord(WorldState state, RuntimeId<Household> householdId, CulturalPatronageType type)
    {
        foreach (var entry in state.CulturalPatronageRecords.InAscendingOrder())
        {
            var record = entry.Value;
            if (IsActive(record) && record.HouseholdId == householdId && record.Type == type)
                return record;
        }

        return null;
    }

    /// <summary>Every currently-active commitment across every household, in ascending-<see
    /// cref="RuntimeId{T}"/> order — the monthly cycle's own iteration set.</summary>
    public static IEnumerable<CulturalPatronageRecord> AllActive(WorldState state)
    {
        foreach (var entry in state.CulturalPatronageRecords.InAscendingOrder())
            if (IsActive(entry.Value))
                yield return entry.Value;
    }
}
