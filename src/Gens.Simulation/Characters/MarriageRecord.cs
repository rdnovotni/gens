using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>One entry in a Character's marital history (<c>gens-familia-design.md</c> §8's sketch:
/// <c>maritalHistory: [{spouseId, startDate, endDate, endReason}]</c>). An open marriage (<see
/// cref="EndDate"/> is <c>null</c>) has no <see cref="EndReason"/> either; a closed one always has
/// both — validated as a single either-both-or-neither invariant, mirroring <see
/// cref="Condition"/>'s validate-in-constructor style.</summary>
public readonly record struct MarriageRecord
{
    public MarriageRecord(RuntimeId<Character> spouseId, GameDate startDate, GameDate? endDate, MarriageEndReason? endReason)
    {
        if (endDate.HasValue != endReason.HasValue)
            throw new ArgumentException(
                $"'{nameof(endReason)}' must be set if and only if '{nameof(endDate)}' is set.", nameof(endReason));

        SpouseId = spouseId;
        StartDate = startDate;
        EndDate = endDate;
        EndReason = endReason;
    }

    public RuntimeId<Character> SpouseId { get; }
    public GameDate StartDate { get; }
    public GameDate? EndDate { get; }
    public MarriageEndReason? EndReason { get; }
}
