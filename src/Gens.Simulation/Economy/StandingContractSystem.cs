using Gens.Simulation.Commands;
using Gens.Simulation.Goods;
using Gens.Simulation.Ledger;
using Gens.Simulation.Markets;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Economy;

/// <summary>
/// The monthly standing-contract tick (Phase 8 item 7). Only <see
/// cref="StandingContractKind.ProvincialSupply"/> has recurring monthly behavior: each month, it
/// reserves and consumes <see cref="StandingContract.QuantityPerMonth"/> of <see
/// cref="StandingContract.GoodId"/> from <see cref="StandingContract.HoldingId"/>'s Stockpile (the
/// same <see cref="Stockpile.Reserve"/>/<see cref="Stockpile.ConsumeReserved"/> contract <see
/// cref="HouseholdPurchasingSystem"/> uses), at <see cref="SettlementMarket.Price"/> scaled up by <see
/// cref="StandingContract.PriceOverMarketFraction"/>, and posts settlement Treasury → household (§3.2:
/// "a better price"). A month with insufficient stock or no cleared market price simply skips that
/// contract for the month — no arrears/penalty tracking, a documented simplification (§6.5 names
/// contract-advance arrears as a future debt-bondage door; this system does not implement that side).
/// <see cref="StandingContractKind.TradeRouteInvestment"/> has no monthly action at all — see <see
/// cref="StandingContractService.CommitToTradeRoute"/>'s own doc comment for why the whole
/// investment is a one-off commitment rather than a recurring one.
/// </summary>
public sealed class StandingContractSystem : IMonthlySystem<WorldState>
{
    public string Id => "economy.standingContracts";
    public TickPhase Phase => TickPhase.MarketsLedger;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "stockpiles", "marketPrices" };
    public IReadOnlyCollection<string> Writes { get; } =
        new[] { "stockpiles", "ledgerAccounts", "ledgerTransactions", "ledgerTransactionIds", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } =
        new[] { "markets.clearing", "economy.householdPurchasing" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.StandingContracts.InAscendingOrder())
        {
            var contract = entry.Value;
            if (contract.Status != StandingContractStatus.Active || contract.Kind != StandingContractKind.ProvincialSupply)
                continue;

            var holdingId = contract.HoldingId!.Value;
            var goodId = contract.GoodId!.Value;

            if (!state.Stockpiles.TryGet(holdingId, out var stockpile))
                continue;
            if (!state.MarketPrices.TryGet(new MarketGoodKey(contract.SettlementId, goodId), out var market))
                continue;

            var available = stockpile.AvailableQuantityOf(goodId);
            var quantity = Math.Min(available, contract.QuantityPerMonth);
            if (quantity <= 0)
                continue;

            var reservationId = $"contract-{context.Date.TotalMonths}-{contract.Id.ToTaggedString()}";
            stockpile.Reserve(reservationId, goodId, quantity);
            stockpile.ConsumeReserved(reservationId);

            var unitPrice = market.Price.Scale(Fixed64.One + contract.PriceOverMarketFraction);
            var amount = HouseholdEconomyCalculator.MultiplyByQuantity(unitPrice, quantity);

            var ledgerEvent = LedgerService.Post(
                state, context.Date, LedgerTransactionCategory.Contracts,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.ForHousehold(contract.HouseholdId), amount),
                    new LedgerPosting(LedgerAccountKey.ForSettlementTreasury(contract.SettlementId), -amount),
                },
                reference: $"provincialSupply:{contract.Id.ToTaggedString()}");
            events.Add(ledgerEvent);
        }

        return events;
    }
}
