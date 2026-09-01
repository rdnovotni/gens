using Gens.Simulation.Commands;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Health;

/// <summary>The monthly Sanitation Investment tick: every settlement carrying a non-<see
/// cref="SanitationInvestmentTier.Minimal"/> tier pays <see
/// cref="SanitationInvestmentCalculator.MonthlyTreasuryCost"/> out of its own Settlement Treasury (<see
/// cref="LedgerAccountKey.ForSettlementTreasury"/>) into this domain's own ledger sink — the same
/// "real cost that competes with every other line on the Ledger" shape <see
/// cref="Religion.FavorCycleSystem"/> already established for Rites Budget, scoped to a settlement
/// account instead of a household one since §6 is explicitly a settlement-wide policy. A <see
/// cref="SanitationInvestmentTier.Minimal"/> settlement (including one with no <see
/// cref="SettlementSanitationInvestment"/> entry at all) pays nothing and is skipped — matching that
/// tier's own "no ongoing cost" doc comment.</summary>
public sealed class SanitationInvestmentSystem : IMonthlySystem<WorldState>
{
    private static readonly LedgerAccountKey SanitationSink = new(LedgerAccountKind.System, "health:sanitationInvestment");

    public string Id => "health.sanitationInvestment";
    public TickPhase Phase => TickPhase.Hazards;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "settlementSanitationInvestments", "settlements", "ledgerAccounts" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "ledgerAccounts", "ledgerTransactions", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.SettlementSanitationInvestments.InAscendingOrder())
        {
            var investment = entry.Value;
            var cost = SanitationInvestmentCalculator.MonthlyTreasuryCost(investment.Tier);
            if (cost == Money.Zero)
                continue;

            var posted = LedgerService.Post(
                state, context.Date, LedgerTransactionCategory.Upkeep,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.ForSettlementTreasury(investment.SettlementId), -cost),
                    new LedgerPosting(SanitationSink, cost),
                },
                reference: $"health:sanitationInvestment:{investment.SettlementId.ToTaggedString()}:{context.Date.TotalMonths}");
            events.Add(posted);
        }

        return events;
    }
}
