using Gens.Simulation.State;

namespace Gens.Simulation.Characters;

/// <summary>Resolves the <see cref="RegimenSettings"/> actually in effect for a Character
/// (<c>gens-labor-slavery-design.md</c> §5: "an individual override always takes precedence over its
/// group default"). Resolution order: the individual override, then the household's per-duty-slot
/// default, then the household's whole-household default, then <see cref="RegimenCatalog.Default"/>.</summary>
public static class RegimenResolver
{
    public static RegimenSettings GetEffective(WorldState state, Character character)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        if (character.Regimen is { } individual)
            return individual;

        if (character.Household is { } householdId)
        {
            if (character.Duty is { } duty &&
                state.HouseholdRegimenDefaults.TryGet(new HouseholdRegimenKey(householdId, duty.Slot), out var slotDefault))
                return slotDefault;

            if (state.HouseholdRegimenDefaults.TryGet(new HouseholdRegimenKey(householdId, null), out var householdDefault))
                return householdDefault;
        }

        return RegimenCatalog.Default;
    }
}
