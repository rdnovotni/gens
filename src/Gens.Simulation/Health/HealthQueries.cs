using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;

namespace Gens.Simulation.Health;

/// <summary>Read-only projections over <c>WorldState.CharacterHealthConditions</c>, the same
/// linear-scan-over-a-small-collection shape <c>Languages.LanguageProficiencyQueries</c> already
/// established for an equivalent "a Character legitimately holds several entries at once"
/// collection.</summary>
public static class HealthQueries
{
    /// <summary>True once <paramref name="characterId"/> holds a <see
    /// cref="CharacterHealthConditionStatus.Recovered"/> case of <paramref name="conditionId"/> with
    /// <see cref="CharacterHealthCondition.GrantedImmunity"/> set (§5) — permanent for the rest of the
    /// campaign, since no entry here is ever removed.</summary>
    public static bool IsImmune(
        WorldState state, RuntimeId<Character> characterId, DefinitionId<HealthConditionDefinition> conditionId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.CharacterHealthConditions.InAscendingOrder())
        {
            var condition = entry.Value;
            if (condition.CharacterId == characterId && condition.ConditionId == conditionId &&
                condition.Status == CharacterHealthConditionStatus.Recovered && condition.GrantedImmunity)
                return true;
        }

        return false;
    }

    /// <summary>True while <paramref name="characterId"/> already carries an <see
    /// cref="CharacterHealthConditionStatus.Active"/> case of <paramref name="conditionId"/> —
    /// <see cref="AfflictCharacterCommand"/> rejects a duplicate rather than opening a second concurrent
    /// case of the same condition.</summary>
    public static bool HasActiveCondition(
        WorldState state, RuntimeId<Character> characterId, DefinitionId<HealthConditionDefinition> conditionId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.CharacterHealthConditions.InAscendingOrder())
        {
            var condition = entry.Value;
            if (condition.CharacterId == characterId && condition.ConditionId == conditionId &&
                condition.Status == CharacterHealthConditionStatus.Active)
                return true;
        }

        return false;
    }

    /// <summary>Every currently-<see cref="CharacterHealthConditionStatus.Active"/> case for one
    /// Character, in ascending onset (RuntimeID) order.</summary>
    public static IEnumerable<CharacterHealthCondition> ActiveConditionsFor(WorldState state, RuntimeId<Character> characterId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.CharacterHealthConditions.InAscendingOrder())
            if (entry.Value.CharacterId == characterId && entry.Value.Status == CharacterHealthConditionStatus.Active)
                yield return entry.Value;
    }

    /// <summary>True while <paramref name="settlementId"/> has at least one <see
    /// cref="EpidemicOutbreakStatus.Active"/> outbreak with <see
    /// cref="EpidemicOutbreak.SettlementQuarantineActive"/> set — §4.2's real, cross-system read Phase 14
    /// item 5 wires into <see cref="Markets.MarketClearingSystem"/>'s Commerce cost and <see
    /// cref="EpidemicContagionSystem"/>'s own Contentment cost, closing the gap <see
    /// cref="SetSettlementQuarantineCommand"/>'s own doc comment named.</summary>
    public static bool IsSettlementUnderQuarantine(WorldState state, RuntimeId<Settlement> settlementId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.EpidemicOutbreaks.InAscendingOrder())
        {
            if (entry.Value.Status == EpidemicOutbreakStatus.Active &&
                entry.Value.SettlementId == settlementId && entry.Value.SettlementQuarantineActive)
                return true;
        }

        return false;
    }
}
