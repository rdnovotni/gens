using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;

namespace Gens.Simulation.Queries;

/// <summary>The Monthly Report screen's financial-summary projection (Phase 9 item 6): the
/// caller-specified household's income/expense/net for the current month, read straight from <see
/// cref="WorldState.HouseholdStatements"/> (Phase 8 item 5). The report's event-derived headlines are
/// deliberately not this query's job — <see cref="Gens.Simulation.Campaign.MonthlyReportProjector"/> already builds
/// those entirely from the month's domain events (Phase 9 item 4), and that projector takes the raw
/// event list the Unity application shell's month-advance call returns rather than reading <see
/// cref="WorldState"/>, so it sits outside the <see cref="IWorldQuery{TProjection}"/> shape this file
/// uses; the Unity presentation layer composes both into one screen view model.</summary>
public readonly record struct HouseholdFinancialsProjection(
    string HouseholdId,
    Money Income,
    Money Expenses,
    Money Net);

/// <summary>Projects the caller-specified household's current-month statement, the same
/// "caller-specified subject" shape <see cref="HouseholdRosterQuery"/> establishes. Zeroed out when no
/// statement has been prepared for the household this month (<see
/// cref="Economy.HouseholdStatementSystem"/> only emits one for a household with at least one ledger
/// posting) rather than throwing — an idle household's report should read as "nothing moved," not as
/// an error.</summary>
public sealed class HouseholdFinancialsQuery : IWorldQuery<HouseholdFinancialsProjection>
{
    private readonly RuntimeId<Household> _householdId;

    public HouseholdFinancialsQuery(RuntimeId<Household> householdId) => _householdId = householdId;

    public HouseholdFinancialsProjection Execute(WorldState state, string observerId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var householdTag = _householdId.ToTaggedString();
        if (!state.HouseholdStatements.TryGet(_householdId, out var statement))
            return new HouseholdFinancialsProjection(householdTag, Money.Zero, Money.Zero, Money.Zero);

        return new HouseholdFinancialsProjection(householdTag, statement.Income, statement.Expenses, statement.Net);
    }
}
