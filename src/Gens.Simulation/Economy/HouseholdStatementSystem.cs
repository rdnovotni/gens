using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Economy;

/// <summary>
/// The monthly household-statement tick (Phase 8 item 5's "monthly statements"). Runs last among the
/// Phase 8 items 5-7 economy systems (its <see cref="Prerequisites"/> name every system that can post a
/// Household-account transaction this month), then scans <see
/// cref="WorldState.LedgerTransactions"/> for this month's postings against every <see
/// cref="LedgerAccountKind.Household"/> account, sums them into income/expenses/net, and overwrites
/// that household's <see cref="WorldState.HouseholdStatements"/> entry. A full-log scan rather than an
/// incrementally-accumulated running total, matching <see
/// cref="Ledger.LedgerAccountBalanceConsistencyInvariantCheck"/>'s identical "recompute from the
/// append-only log" preference over a separately-maintained figure that could drift from it.
/// </summary>
public sealed class HouseholdStatementSystem : IMonthlySystem<WorldState>
{
    public string Id => "economy.householdStatement";
    public TickPhase Phase => TickPhase.MarketsLedger;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "ledgerTransactions" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[]
    {
        "economy.householdPurchasing",
        "economy.wages",
        "economy.rentAndTax",
        "economy.debt",
        "economy.insolvency",
        "economy.standingContracts",
    };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        // A plain Dictionary accumulator is safe here for the same reason
        // LedgerAccountBalanceConsistencyInvariantCheck's identical accumulator is: Money addition is
        // commutative, and the deterministic emission order below iterates a sorted key list, not this
        // dictionary directly (ADR 0004).
        var income = new Dictionary<RuntimeId<Household>, Money>();
        var expenses = new Dictionary<RuntimeId<Household>, Money>();

        foreach (var entry in state.LedgerTransactions.InAscendingOrder())
        {
            var transaction = entry.Value;
            if (transaction.OccurredDate != context.Date)
                continue;

            foreach (var posting in transaction.Postings)
            {
                if (posting.Account.Kind != LedgerAccountKind.Household)
                    continue;

                var householdId = RuntimeId<Household>.Parse(posting.Account.OwnerId);
                if (posting.Amount.IsNegative)
                {
                    expenses.TryGetValue(householdId, out var running);
                    expenses[householdId] = running + posting.Amount.Abs();
                }
                else if (posting.Amount != Money.Zero)
                {
                    income.TryGetValue(householdId, out var running);
                    income[householdId] = running + posting.Amount;
                }
            }
        }

        var householdIds = new SortedSet<RuntimeId<Household>>();
        foreach (var id in income.Keys)
            householdIds.Add(id);
        foreach (var id in expenses.Keys)
            householdIds.Add(id);

        var events = new List<IDomainEvent>();
        foreach (var householdId in householdIds)
        {
            var totalIncome = income.TryGetValue(householdId, out var incomeValue) ? incomeValue : Money.Zero;
            var totalExpenses = expenses.TryGetValue(householdId, out var expenseValue) ? expenseValue : Money.Zero;
            var net = totalIncome - totalExpenses;

            var statement = new HouseholdMonthlyStatement(householdId, context.Date, totalIncome, totalExpenses, net);
            state.HouseholdStatements.Remove(householdId);
            state.HouseholdStatements.Add(householdId, statement);

            events.Add(new HouseholdStatementPreparedEvent(
                state.EventIds.Issue(), context.Date, householdId, totalIncome, totalExpenses, net));
        }

        return events;
    }
}
