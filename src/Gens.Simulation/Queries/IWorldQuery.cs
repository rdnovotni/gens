using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Queries;

/// <summary>
/// The sole read path for UI code (ADR 0013): a read-only projection, never a mutable domain
/// object. Phase 2 has no real screens yet (those land in Phase 9), so this interface plus one
/// worked example (<see cref="CampaignClockQuery"/>) only proves the pattern compiles and is
/// testable. A query whose result depends on what a specific observer knows — filtered through
/// <see cref="KnowledgeState"/> — is out of scope until real Character records and knowledge
/// propagation exist; do not assume this interface alone solves observer-filtered queries.
/// </summary>
public interface IWorldQuery<out TProjection>
{
    TProjection Execute(WorldState state, string observerId);
}

/// <summary>An immutable, presentation-ready snapshot of the campaign clock.</summary>
public readonly record struct CampaignClockProjection(int DisplayYear, string Era, int MonthOfYear);

/// <summary>Worked example: the campaign clock has no visibility restriction, so it needs no
/// <see cref="KnowledgeState"/> filtering — every observer sees the same date.</summary>
public sealed class CampaignClockQuery : IWorldQuery<CampaignClockProjection>
{
    public CampaignClockProjection Execute(WorldState state, string observerId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var (astronomicalYear, monthOfYear) = state.Date.ToCalendar();
        var era = astronomicalYear <= 0 ? "BCE" : "CE";
        return new CampaignClockProjection(GameDate.ToDisplayYear(astronomicalYear), era, monthOfYear);
    }
}
