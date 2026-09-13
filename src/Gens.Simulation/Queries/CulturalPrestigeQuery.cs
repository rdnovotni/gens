using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Characters;
using Gens.Simulation.Education;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Queries;

/// <summary>One household's Cultural Prestige standing, presentation-ready (Phase 17 item 2). <see
/// cref="ClearsMarriageMarketThreshold"/> is §7's marriage-market gate (<see
/// cref="CulturalPrestigeResolver.ClearsMarriageMarketThreshold"/>); the magistracy-candidacy gate lands
/// later in this same ticket as <c>Education.EducationGateResolver.CanContestMagistracyAboveLowestRung</c>
/// (it reads a completed Rhetoric Educational Track or an Institution credential, neither of which exists
/// in this domain's own Cultural Prestige partition yet), so it is deliberately not folded into this
/// projection — that would make this query a second, competing read of state it does not itself own.</summary>
public readonly record struct CulturalPrestigeProjection(string HouseholdId, int Prestige, bool ClearsMarriageMarketThreshold);

/// <summary>The one new <see cref="IWorldQuery{TProjection}"/> this ticket adds — closing the gap <see
/// cref="Religion.HouseholdReligion"/>'s own domain left open (Religion added no projection of its own;
/// see this ticket's own plan for that observation). No <see cref="KnowledgeState"/> filtering, matching
/// <see cref="HouseholdRosterQuery"/>'s identical "a player always knows their own household's own
/// standing" precedent.</summary>
public sealed class CulturalPrestigeQuery : IWorldQuery<CulturalPrestigeProjection>
{
    private readonly RuntimeId<Household> _householdId;

    public CulturalPrestigeQuery(RuntimeId<Household> householdId) => _householdId = householdId;

    public CulturalPrestigeProjection Execute(WorldState state, string observerId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        return new CulturalPrestigeProjection(
            _householdId.ToTaggedString(),
            CulturalPrestigeResolver.Current(state, _householdId),
            CulturalPrestigeResolver.ClearsMarriageMarketThreshold(state, _householdId));
    }
}
