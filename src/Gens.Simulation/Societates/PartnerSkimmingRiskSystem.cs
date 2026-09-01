using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Time;

namespace Gens.Simulation.Societates;

/// <summary>
/// §7's monthly "suspected skimming or fraud" ground truth (Phase 15 item 2) — "the direct
/// partner-to-partner parallel to Land Ownership &amp; Real Estate's own Operator-skimming risk...
/// detectable the same way." Runs once a month against every active <see cref="Societas"/>, resolving
/// each partner whose <see cref="PropertyOwnerRef.Kind"/> is <see
/// cref="PropertyOwnerKind.PlayerHousehold"/> or <see cref="PropertyOwnerKind.IndividualCharacter"/> —
/// the two owner kinds this item can actually reach a living <see cref="Character"/>'s own
/// Core Condition/Traits through (a household's recorded head via <see
/// cref="HouseholdHeadship"/>, or the individual Character directly). Every other partner kind (Rival
/// Gens, Temple, Collegium, Municipal, Roman State, Societas placeholder, Imperial Patrimonium) has no
/// single Character this item can read Loyalty/Greed off, so it never carries a skim-risk verdict —
/// the same honest "only some owner kinds resolve against a real, checkable entity" narrowing <see
/// cref="PropertyOwnerRef.IsNarrativeOnly"/> already establishes for property ownership itself.
/// </summary>
public sealed class PartnerSkimmingRiskSystem : IMonthlySystem<WorldState>
{
    public string Id => "societates.partnerSkimmingRisk";
    public TickPhase Phase => TickPhase.MarketsLedger;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "societates", "characters", "householdHeadships" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "societates" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.Societates.InAscendingOrder().Where(e => e.Value.IsActive).ToArray())
        {
            var societas = entry.Value;
            var updated = societas;
            var changed = false;

            foreach (var partner in societas.Partners)
            {
                if (!TryResolveCharacter(state, partner.Owner, out var character))
                    continue;

                var isSkimming = character!.Condition.Loyalty < SocietatesCatalog.SkimmingLoyaltyThreshold
                    && character.Traits.Contains(SocietatesCatalog.GreedyTraitId);
                if (isSkimming == partner.IsSuspectedSkimming)
                    continue;

                updated = updated.WithPartner(partner with { IsSuspectedSkimming = isSkimming });
                changed = true;
            }

            if (changed)
            {
                state.Societates.Remove(entry.Key);
                state.Societates.Add(entry.Key, updated);
            }
        }

        return Array.Empty<IDomainEvent>();
    }

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
