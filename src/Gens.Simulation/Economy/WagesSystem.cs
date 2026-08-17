using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Economy;

/// <summary>
/// The monthly wages tick (Phase 8 item 6; <c>gens-economy-finance-design.md</c> §4.1). Pays a flat
/// <see cref="HouseholdEconomyCalculator.WagePerWorkerPerMonth"/> from the settlement Treasury to every
/// currently-staffed <see cref="Buildings.BuildingInstance"/> slot worker — the only labor-assignment
/// state that already exists and is unambiguously "actually assigned" (per this phase's own scoping
/// note not to invent new staffing state). A staffed worker's ID string (however it was assigned — a
/// tagged Character ID, or a synthetic placeholder in test/soak fixtures) becomes that worker's <see
/// cref="LedgerAccountKind.Actor"/> account owner ID directly: <see cref="LedgerAccountKey"/>'s owner
/// is already "a tagged-string owner reference" by design, so no separate <c>RuntimeId&lt;Actor&gt;</c>
/// needs to be issued just to receive a wage. The settlement Treasury is the payer rather than each
/// worker's own employing household — this codebase has no per-building "owning household" link yet
/// (a <see cref="Buildings.BuildingInstance"/> is tied to a Plot, not a Holding/household), so the
/// settlement absorbing wage cost is a documented simplification, not the design doc's literal model.
/// A poorly-paid free worker's Loyalty eroding (§4.1's other half) is not implemented here — Loyalty
/// dynamics are Labor &amp; Slavery's own system, unmodified by this pass.
/// </summary>
public sealed class WagesSystem : IMonthlySystem<WorldState>
{
    public string Id => "economy.wages";
    public TickPhase Phase => TickPhase.MarketsLedger;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "settlements", "plots", "buildings" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "ledgerAccounts", "ledgerTransactions", "ledgerTransactionIds", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "markets.clearing" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var wage = HouseholdEconomyCalculator.WagePerWorkerPerMonth;

        foreach (var settlementEntry in state.Settlements.InAscendingOrder())
        {
            var settlementId = settlementEntry.Key;
            var workerIds = new List<string>();

            foreach (var buildingEntry in state.Buildings.InAscendingOrder())
            {
                var building = buildingEntry.Value;
                if (!state.Plots.TryGet(building.PlotId, out var plot) || plot.SettlementId != settlementId)
                    continue;

                foreach (var slotWorkers in building.Staff.Values)
                {
                    foreach (var workerId in slotWorkers)
                        workerIds.Add(workerId);
                }
            }

            if (workerIds.Count == 0)
                continue;

            // Deterministic (ADR 0004): worker IDs come from a per-slot SortedSet already, but slots
            // themselves iterate in a dictionary the BuildingInstance.Staff projection doesn't order —
            // re-sort ordinally here rather than trust that projection's enumeration order.
            workerIds.Sort(StringComparer.Ordinal);

            var totalWages = Money.Zero;
            var postings = new List<LedgerPosting>(workerIds.Count + 1);
            foreach (var workerId in workerIds)
            {
                postings.Add(new LedgerPosting(new LedgerAccountKey(LedgerAccountKind.Actor, workerId), wage));
                totalWages += wage;
            }
            postings.Add(new LedgerPosting(LedgerAccountKey.ForSettlementTreasury(settlementId), -totalWages));

            var ledgerEvent = LedgerService.Post(
                state, context.Date, LedgerTransactionCategory.Wages, postings, reference: "wages:staffedSlots");
            events.Add(ledgerEvent);
        }

        return events;
    }
}
