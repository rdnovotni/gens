using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;

namespace Gens.Simulation.Events;

/// <summary>Weight-input helpers reading real <c>WorldState</c> meters — §3's "weight inputs drawn
/// from whichever hidden meters this project has already built." Nothing here invents a new meter:
/// <see cref="FromCharacterLoyalty"/> reads <see cref="Character.Condition"/>'s already-real Loyalty
/// stat (Phase 5 item 1), <see cref="FromSettlementContentment"/> reads <see cref="PopGroup"/>'s
/// already-real Contentment (Phase 7 item 4). Household and Imperial scope have no concrete meter
/// wired yet (no Dignitas/Prominence record exists, per this pass's explicit Phase 13/other-phase
/// scope boundary) — <see cref="Flat"/> is the generic, pluggable fallback those scopes' definitions
/// use today, swapped for a real meter the moment one exists, with no change needed to <see
/// cref="EventPoolSystem"/> itself.</summary>
public static class EventWeightInputs
{
    /// <summary>A definition with nothing concrete to read yet weights every eligible candidate
    /// equally at this baseline.</summary>
    public const int Flat = 10;

    /// <summary>Higher weight the lower a Character's Loyalty (0-100) is — a discontented Character is
    /// more likely to draw a Personal-scope event exploring that discontent. Returns <see cref="Flat"/>
    /// when <paramref name="characterId"/> does not resolve to a known Character (defensive only; a
    /// caller enumerating <see cref="WorldState.Characters"/> itself never hits this branch).</summary>
    public static int FromCharacterLoyalty(WorldState state, string characterSubjectId)
    {
        var id = RuntimeId<Character>.Parse(characterSubjectId);
        if (!state.Characters.TryGet(id, out var character))
            return Flat;

        return Flat + (100 - character!.Condition.Loyalty);
    }

    /// <summary>Higher weight the lower a Settlement's average background-population Contentment is.
    /// Returns <see cref="Flat"/> for a Settlement with no <see cref="PopGroup"/> entries yet.</summary>
    public static int FromSettlementContentment(WorldState state, RuntimeId<Settlement> settlementId)
    {
        var groups = state.PopGroups.InAscendingOrder()
            .Where(entry => entry.Key.SettlementId == settlementId)
            .Select(entry => entry.Value)
            .ToArray();
        if (groups.Length == 0)
            return Flat;

        var totalContentmentRaw = groups.Sum(group => group.Contentment.RawValue);
        var averageContentment = Fixed64.FromRaw(totalContentmentRaw / groups.Length);
        var discontent = Fixed64.One - averageContentment;
        var discontentPercent = (int)(discontent.RawValue * 100 / Fixed64.ScaleFactor);
        return Flat + Math.Max(0, discontentPercent);
    }
}
