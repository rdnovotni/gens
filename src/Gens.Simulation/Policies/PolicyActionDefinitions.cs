using Gens.Simulation.Actions;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;

namespace Gens.Simulation.Policies;

/// <summary>Registers this pass's two commands (<see cref="ChangeRitesBudgetCommand"/>, <see
/// cref="FundFestivalCommand"/>) into the action-definition layer (<see cref="ActionDefinition"/>) —
/// the worked example proving Phase 9 item 1's layer and item 2's standing-policy/funded-action work
/// fit together, and the concrete case Phase 10 item 1 means by "AI action selection against the same
/// action definitions used by the player": an AI steward can score and choose between these two
/// <see cref="ActionCatalog"/> entries without knowing anything about <see cref="CommandPipeline{TState,TCommand}"/>
/// or ledger postings.</summary>
public static class PolicyActionDefinitions
{
    public static readonly DefinitionId<ActionDefinition> ChangeRitesBudget = new("change-rites-budget");
    public static readonly DefinitionId<ActionDefinition> FundFestival = new("fund-festival");

    /// <summary>The amount a <see cref="FundFestivalCommand"/> invoked purely through this catalog
    /// entry's own projection/scoring hooks assumes — a fixed, invented baseline (§10's own untuned-
    /// numbers convention) standing in for a player- or AI-chosen amount a real UI would instead supply
    /// directly on the submitted command.</summary>
    public static readonly Money DefaultFestivalAmount = Money.FromDenarii(50);

    public static ActionCatalog BuildCatalog() => new(new[]
    {
        new ActionDefinition(
            id: ChangeRitesBudget,
            nameKey: "actions.change-rites-budget.name",
            descriptionKey: "actions.change-rites-budget.description",
            targetKind: ActionTargetKind.Household,
            cost: ActionCost.None,
            duration: ActionDuration.Instant,
            confirmation: ActionConfirmationSeverity.Ordinary,
            eligibility: RitesBudgetEligibility,
            scoreForAi: (state, invocation) => RitesBudgetEligibility(state, invocation) is null ? 1.0 : 0.0,
            projectResult: RitesBudgetProjection),
        new ActionDefinition(
            id: FundFestival,
            nameKey: "actions.fund-festival.name",
            descriptionKey: "actions.fund-festival.description",
            targetKind: ActionTargetKind.Household,
            cost: new ActionCost(DefaultFestivalAmount),
            duration: ActionDuration.Instant,
            confirmation: ActionConfirmationSeverity.WaxSeal,
            eligibility: FestivalEligibility,
            scoreForAi: (state, invocation) => FestivalEligibility(state, invocation) is null ? 0.5 : 0.0,
            projectResult: FestivalProjection,
            reservations: new[] { new ActionReservation("treasury", DefaultFestivalAmount.RawValue) }),
    });

    /// <summary>The generic-layer eligibility check: only the cooldown gate, since <see
    /// cref="ActionInvocation"/> carries no target-tier field to check <see
    /// cref="ChangeRitesBudgetCommands.TierUnchanged"/> against — a concrete submitted <see
    /// cref="ChangeRitesBudgetCommand"/> still runs the fuller check itself.</summary>
    private static ValidationErrorCode? RitesBudgetEligibility(WorldState state, ActionInvocation invocation)
    {
        var householdId = ToHouseholdId(invocation);
        if (!state.HouseholdPolicies.TryGet(householdId, out var existing))
            return null;

        var monthsSinceLastChange = invocation.Date.TotalMonths - existing!.LastChangedDate.TotalMonths;
        return monthsSinceLastChange < RitesBudgetCatalog.CooldownMonths ? ChangeRitesBudgetCommands.OnCooldown : null;
    }

    private static ActionResultProjection RitesBudgetProjection(WorldState state, ActionInvocation invocation)
    {
        var current = HouseholdPolicyResolver.GetEffectiveRitesBudget(state, ToHouseholdId(invocation));
        return ActionResultProjection.Of($"Current Rites Budget tier: {current}.");
    }

    private static ValidationErrorCode? FestivalEligibility(WorldState state, ActionInvocation invocation)
    {
        var householdId = ToHouseholdId(invocation);
        if (!state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account) ||
            account!.Balance < DefaultFestivalAmount)
            return FundFestivalCommands.InsufficientTreasury;

        return null;
    }

    private static ActionResultProjection FestivalProjection(WorldState state, ActionInvocation invocation)
    {
        var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(ToHouseholdId(invocation)), out var account)
            ? account!.Balance
            : Money.Zero;
        return ActionResultProjection.Of(
            $"Funding a Festival for {DefaultFestivalAmount.ToDisplayString()} denarii (treasury balance: {balance.ToDisplayString()}).");
    }

    private static RuntimeId<Household> ToHouseholdId(ActionInvocation invocation) => RuntimeId<Household>.Parse(invocation.ActorId);
}
