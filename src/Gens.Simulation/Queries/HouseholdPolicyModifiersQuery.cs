using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Policies;
using Gens.Simulation.State;

namespace Gens.Simulation.Queries;

/// <summary>Phase 9 item 2's "household modifier projection": the concrete numeric effects a
/// household's current Rites Budget tier resolves to, read the same way <see
/// cref="CharacterLifecycleQuery"/> reads a single caller-specified subject. No <see
/// cref="KnowledgeState"/> filtering, matching that query's own precedent — every observer sees
/// the same policy modifiers.</summary>
public readonly record struct HouseholdPolicyModifiersProjection(
    string HouseholdId,
    RitesBudgetTier RitesBudget,
    Money TreasuryDrawPerMonth,
    int DivineFavorStabilityModifier);

public sealed class HouseholdPolicyModifiersQuery : IWorldQuery<HouseholdPolicyModifiersProjection>
{
    private readonly RuntimeId<Household> _householdId;

    public HouseholdPolicyModifiersQuery(RuntimeId<Household> householdId) => _householdId = householdId;

    public HouseholdPolicyModifiersProjection Execute(WorldState state, string observerId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var tier = HouseholdPolicyResolver.GetEffectiveRitesBudget(state, _householdId);

        return new HouseholdPolicyModifiersProjection(
            HouseholdId: _householdId.ToTaggedString(),
            RitesBudget: tier,
            TreasuryDrawPerMonth: RitesBudgetCatalog.TreasuryDrawPerMonth(tier),
            DivineFavorStabilityModifier: RitesBudgetCatalog.DivineFavorStabilityModifier(tier));
    }
}
