using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Economy;

/// <summary>Which of §3.2's contract types / §7's trade-route levers this record represents (Phase 8
/// item 7: "one market contract and one trade-route stub").</summary>
public enum StandingContractKind
{
    /// <summary>§3.2's "Provincial supply contracts" — the simplest of the three named contract types,
    /// chosen per this phase's own instruction to pick exactly one: "recurring bulk sale to the
    /// provincial administration itself... functioning like a Market sale with a better price," with no
    /// further mechanic (no Dignitas-with-Rome tie, since Reputation Duality does not exist yet).</summary>
    ProvincialSupply,

    /// <summary>§7's trade-route stub — "committing denarii toward a specific active trade route." No
    /// disruption/piracy/seasonality mechanics exist yet (Piracy &amp; Banditry is unbuilt); this is
    /// only a persisted record of a one-off commitment, exactly as this phase's own instructions
    /// describe: "it does not need real disruption/piracy mechanics... it just needs to move money
    /// through the ledger and be readable state."</summary>
    TradeRouteInvestment,
}

public enum StandingContractStatus
{
    Active,
    Ended,
}

/// <summary>
/// One standing market contract or trade-route commitment (Phase 8 item 7). Shares the same <see
/// cref="LedgerService"/>/<see cref="Stockpile"/> reservation contracts as ordinary household
/// purchasing/sales (<see cref="HouseholdPurchasingSystem"/>) rather than inventing a parallel
/// mechanism — see <see cref="StandingContractSystem"/> for how each <see cref="StandingContractKind"/>
/// actually posts.
/// </summary>
public sealed record StandingContract
{
    public StandingContract(
        RuntimeId<StandingContract> id,
        StandingContractKind kind,
        RuntimeId<Settlement> settlementId,
        RuntimeId<Household> householdId,
        StandingContractStatus status,
        RuntimeId<Holding>? holdingId = null,
        DefinitionId<Good>? goodId = null,
        long quantityPerMonth = 0,
        Fixed64? priceOverMarketFraction = null,
        Money? denariiCommitted = null,
        string? routeName = null)
    {
        if (kind == StandingContractKind.ProvincialSupply)
        {
            if (holdingId is null || goodId is null || quantityPerMonth <= 0)
                throw new ArgumentException("A provincial supply contract needs a holding, good, and positive monthly quantity.");
        }
        else
        {
            if (denariiCommitted is null || denariiCommitted.Value.RawValue <= 0)
                throw new ArgumentException("A trade-route investment needs a positive committed amount.", nameof(denariiCommitted));
        }

        Id = id;
        Kind = kind;
        SettlementId = settlementId;
        HouseholdId = householdId;
        Status = status;
        HoldingId = holdingId;
        GoodId = goodId;
        QuantityPerMonth = quantityPerMonth;
        PriceOverMarketFraction = priceOverMarketFraction ?? Fixed64.Zero;
        DenariiCommitted = denariiCommitted ?? Money.Zero;
        RouteName = routeName;
    }

    public RuntimeId<StandingContract> Id { get; }
    public StandingContractKind Kind { get; }
    public RuntimeId<Settlement> SettlementId { get; }
    public RuntimeId<Household> HouseholdId { get; }
    public StandingContractStatus Status { get; init; }

    /// <summary><see cref="StandingContractKind.ProvincialSupply"/> only — the supplying Holding.</summary>
    public RuntimeId<Holding>? HoldingId { get; }

    /// <summary><see cref="StandingContractKind.ProvincialSupply"/> only.</summary>
    public DefinitionId<Good>? GoodId { get; }

    /// <summary><see cref="StandingContractKind.ProvincialSupply"/> only.</summary>
    public long QuantityPerMonth { get; }

    /// <summary><see cref="StandingContractKind.ProvincialSupply"/> only — the premium over the
    /// settlement's cleared market price, e.g. <c>0.20</c> for 20% above Market.</summary>
    public Fixed64 PriceOverMarketFraction { get; }

    /// <summary><see cref="StandingContractKind.TradeRouteInvestment"/> only — committed once, at
    /// creation (<see cref="StandingContractService.CommitToTradeRoute"/>), not recurring.</summary>
    public Money DenariiCommitted { get; }

    /// <summary><see cref="StandingContractKind.TradeRouteInvestment"/> only.</summary>
    public string? RouteName { get; }
}

/// <summary>Opens a new <see cref="StandingContract"/> — a plain service method rather than a monthly
/// system trigger, matching <see cref="DebtService.IssueLoan"/>'s identical "Phase 9's not-yet-built
/// actions layer would call this" framing.</summary>
public static class StandingContractService
{
    public static StandingContract OpenProvincialSupplyContract(
        WorldState state,
        RuntimeId<Settlement> settlementId,
        RuntimeId<Household> householdId,
        RuntimeId<Holding> holdingId,
        DefinitionId<Good> goodId,
        long quantityPerMonth,
        Fixed64 priceOverMarketFraction)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var id = state.StandingContractIds.Issue();
        var contract = new StandingContract(
            id, StandingContractKind.ProvincialSupply, settlementId, householdId, StandingContractStatus.Active,
            holdingId: holdingId, goodId: goodId, quantityPerMonth: quantityPerMonth,
            priceOverMarketFraction: priceOverMarketFraction);
        state.StandingContracts.Add(id, contract);
        return contract;
    }

    /// <summary>§7's trade-route stub: commits <paramref name="amount"/> once, immediately, via a
    /// Household → a per-route <see cref="LedgerAccountKind.System"/> account (owner ID
    /// <c>"traderoute:&lt;contract tag&gt;"</c> — the same "one named external account, not an
    /// untracked leak" discipline <see cref="LedgerAccountKey.MintOwnerId"/> already establishes).</summary>
    public static StandingContract CommitToTradeRoute(
        WorldState state,
        GameDate date,
        RuntimeId<Settlement> settlementId,
        RuntimeId<Household> householdId,
        string routeName,
        Money amount)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var id = state.StandingContractIds.Issue();
        var routeAccount = new LedgerAccountKey(LedgerAccountKind.System, $"traderoute:{id.ToTaggedString()}");

        LedgerService.Post(
            state, date, LedgerTransactionCategory.Contracts,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), -amount),
                new LedgerPosting(routeAccount, amount),
            },
            reference: $"tradeRouteCommitment:{id.ToTaggedString()}:{routeName}");

        var contract = new StandingContract(
            id, StandingContractKind.TradeRouteInvestment, settlementId, householdId, StandingContractStatus.Active,
            denariiCommitted: amount, routeName: routeName);
        state.StandingContracts.Add(id, contract);
        return contract;
    }
}
