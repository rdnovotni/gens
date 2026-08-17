using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Policies;

/// <summary>Resolves the Rites Budget tier actually in effect for a household, matching <see
/// cref="Gens.Simulation.Characters.RegimenResolver"/>'s identical "no entry yet means the catalog
/// default, not an error" convention — <see cref="WorldState.HouseholdPolicies"/> is sparse: a
/// household that has never issued a <see cref="ChangeRitesBudgetCommand"/> simply has no entry.
/// </summary>
public static class HouseholdPolicyResolver
{
    public static RitesBudgetTier GetEffectiveRitesBudget(WorldState state, RuntimeId<Household> householdId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        return state.HouseholdPolicies.TryGet(householdId, out var policy) ? policy!.RitesBudget : RitesBudgetCatalog.Default;
    }
}
