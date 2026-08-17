using Gens.Simulation.Commands;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Economy;

/// <summary>
/// The monthly rent-and-tax tick (Phase 8 item 6; <c>gens-economy-finance-design.md</c> §3.1's Rents
/// and §5.2's Decuma-style direct tax, "layered on top of the rent"). For every Holding whose owner
/// resolves to a Household (<see cref="HouseholdEconomyCalculator.TryGetOwningHousehold"/>), posts two
/// separate Household→SettlementTreasury transactions each month: rent (<see
/// cref="LedgerTransactionCategory.Sales"/>, matching that category's own doc comment naming
/// <c>rentAgricultural</c>/<c>rentUrban</c>), then a further tax cut of the rent itself (<see
/// cref="LedgerTransactionCategory.Taxes"/>). §3.1's Contentment/overcrowding-dependent rent scaling
/// and §5.2's Curia/magistracy political gate are both skipped — no Contentment-linked rent formula or
/// Politics &amp; Patronage magistracy exists yet; the settlement itself stands in as tax authority
/// without any office-holding gate, a documented simplification per this phase's own scoping note.
/// </summary>
public sealed class RentAndTaxSystem : IMonthlySystem<WorldState>
{
    public string Id => "economy.rentAndTax";
    public TickPhase Phase => TickPhase.MarketsLedger;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "settlements", "holdings" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "ledgerAccounts", "ledgerTransactions", "ledgerTransactionIds", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "markets.clearing" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var holdingEntry in state.Holdings.InAscendingOrder())
        {
            var holding = holdingEntry.Value;
            if (!HouseholdEconomyCalculator.TryGetOwningHousehold(holding, out var householdId))
                continue;
            if (holding.ResidentCapacity <= 0)
                continue;

            var rent = HouseholdEconomyCalculator.MultiplyByQuantity(
                HouseholdEconomyCalculator.RentPerResidentCapacity, holding.ResidentCapacity);
            if (rent == Money.Zero)
                continue;

            var treasury = LedgerAccountKey.ForSettlementTreasury(holding.SettlementId);
            var household = LedgerAccountKey.ForHousehold(householdId);

            var rentEvent = LedgerService.Post(
                state, context.Date, LedgerTransactionCategory.Sales,
                new[] { new LedgerPosting(household, -rent), new LedgerPosting(treasury, rent) },
                reference: $"rent:{holdingEntry.Key.ToTaggedString()}");
            events.Add(rentEvent);

            var tax = rent.Scale(HouseholdEconomyCalculator.DecumaTaxRate);
            if (tax != Money.Zero)
            {
                var taxEvent = LedgerService.Post(
                    state, context.Date, LedgerTransactionCategory.Taxes,
                    new[] { new LedgerPosting(household, -tax), new LedgerPosting(treasury, tax) },
                    reference: $"decumaTax:{holdingEntry.Key.ToTaggedString()}");
                events.Add(taxEvent);
            }
        }

        return events;
    }
}
