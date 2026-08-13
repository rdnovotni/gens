using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>A Character's current Labor duty slot (<c>gens-familia-design.md</c> §4; the <c>role</c>
/// field in §8's data-model sketch) — at most one at a time, held until explicitly released via <see
/// cref="ReleaseDutyCommand"/> or reassigned.</summary>
public readonly record struct DutyAssignment(RuntimeId<Household> HouseholdId, DutySlot Slot, GameDate AssignedDate);
