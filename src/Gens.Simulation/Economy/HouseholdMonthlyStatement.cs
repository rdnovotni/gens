using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Economy;

/// <summary>
/// One household's income/expense/net summary for one month (Phase 8 item 5's "monthly statements"),
/// derived — not separately tracked — from that month's posted <see cref="LedgerTransaction"/> log
/// entries touching that household's <see cref="LedgerAccountKind.Household"/> account. Overwritten
/// each month in <see cref="State.WorldState.HouseholdStatements"/>, matching <see
/// cref="Markets.SettlementMarket"/>'s "latest snapshot only, not an accumulating history" convention —
/// the full history remains recoverable from <see cref="State.WorldState.LedgerTransactions"/>, the
/// append-only source of truth this is only ever a read-model summary of.
/// </summary>
public sealed record HouseholdMonthlyStatement(
    RuntimeId<Household> HouseholdId,
    GameDate Month,
    Money Income,
    Money Expenses,
    Money Net);

/// <summary>Emitted for every household with at least one ledger posting this month — ledger-ready
/// groundwork for the Monthly Report (Phase 9), matching <see
/// cref="Ledger.LedgerTransactionPostedEvent"/>'s identical "receipt of a state change" convention.</summary>
public sealed record HouseholdStatementPreparedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    Money Income,
    Money Expenses,
    Money Net) : IDomainEvent
{
    public string Type => "economy.householdStatementPrepared";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
