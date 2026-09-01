using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;

namespace Gens.Simulation.Societates;

/// <summary>
/// §7/§9's Ambition half of "a partner's own Reactive Traits (Ambition, Greed)... directly determine a
/// partner's own likelihood of triggering §7's own dispute types" (Phase 15 item 2) — a pure,
/// non-mutating read over a partner's own Ambition (Core Condition), matching <see
/// cref="Queries.FameDivergenceQuery"/>'s own identical "a descriptive gap between fields this project
/// already has, not a new number to track" precedent. No autonomous NPC decision loop reads this yet
/// (this item builds no <c>RivalAmbitionSystem</c>-equivalent for Societates — no design-doc call for
/// one, and no such caller exists in this codebase), so this query is exercised directly by tests
/// standing in for a future one, the same "the primitive ships, the caller doesn't exist yet" precedent
/// <see cref="Reputation.AdjustDignitasCommand"/> and <see cref="Fame.AdjustFameCommand"/> both already
/// established for their own first callers.
/// </summary>
public static class PartnerDisputeRiskQuery
{
    /// <summary>Whether the given partner (a household or individual Character this item can resolve a
    /// living Character for — <see cref="PartnerSkimmingRiskSystem"/>'s own identical owner-kind
    /// roster) reads as a real early-exit risk: Ambition clearing <see
    /// cref="SocietatesCatalog.EarlyExitAmbitionThreshold"/>. Returns <c>false</c> for a partner kind
    /// with no resolvable Character, or a deceased one — an honest "no signal" rather than a false
    /// negative dressed as a real reading.</summary>
    public static bool EarlyExitLikely(WorldState state, PropertyOwnerRef partnerOwner) =>
        TryResolveCharacter(state, partnerOwner, out var character) &&
        character!.Condition.Ambition >= SocietatesCatalog.EarlyExitAmbitionThreshold;

    private static bool TryResolveCharacter(WorldState state, PropertyOwnerRef owner, out Character? character)
    {
        switch (owner.Kind)
        {
            case PropertyOwnerKind.IndividualCharacter:
                return state.Characters.TryGet(RuntimeId<Character>.Parse(owner.OwnerId!), out character) && character!.IsAlive;

            case PropertyOwnerKind.PlayerHousehold:
                if (!state.HouseholdHeadships.TryGet(RuntimeId<Household>.Parse(owner.OwnerId!), out var headship))
                {
                    character = null;
                    return false;
                }
                return state.Characters.TryGet(headship!.HeadCharacterId, out character) && character!.IsAlive;

            default:
                character = null;
                return false;
        }
    }
}
