using Gens.Simulation.Identity;
using Gens.Simulation.Land;

namespace Gens.Simulation.Health;

/// <summary>One settlement's standing §6 Sanitation Investment choice — the new <c>WorldState</c>
/// partition (<see cref="Gens.Simulation.State.WorldState.SettlementSanitationInvestments"/>) §11's
/// <c>SanitationInvestment</c> data-model sketch describes. Keyed directly by <see
/// cref="RuntimeId{Settlement}"/> rather than issuing its own <see cref="RuntimeId{T}"/>: exactly one
/// standing choice can exist per settlement at a time (§6 frames it as a policy tier, not a
/// history of entries), the same "keyed by owner, not its own counted entity" shape <see
/// cref="Ledger.LedgerAccountKey"/> already uses for a running balance.</summary>
public sealed record SettlementSanitationInvestment
{
    public required RuntimeId<Settlement> SettlementId { get; init; }
    public required SanitationInvestmentTier Tier { get; init; }

    public static SettlementSanitationInvestment Create(RuntimeId<Settlement> settlementId, SanitationInvestmentTier tier) =>
        new() { SettlementId = settlementId, Tier = tier };
}

/// <summary>Reads <see cref="Gens.Simulation.State.WorldState.SettlementSanitationInvestments"/>, the
/// same "a missing entry means the documented default" shape <see
/// cref="SanitationInvestmentTier.Minimal"/>'s own doc comment establishes.</summary>
public static class SanitationQueries
{
    public static SanitationInvestmentTier EffectiveTier(State.WorldState state, RuntimeId<Settlement> settlementId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        return state.SettlementSanitationInvestments.TryGet(settlementId, out var investment)
            ? investment.Tier
            : SanitationInvestmentTier.Minimal;
    }
}
