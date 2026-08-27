using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Succession;

/// <summary>
/// Computes the eligible-heir pool for a Household head (Phase 11 item 1; §3): legitimate children,
/// acknowledged Illegitimate children, and adopted children stand identically, minus anyone <see
/// cref="HeirDesignation.DisownedCharacterIds"/> names or who is no longer alive. Pure — reads <see
/// cref="WorldState"/> but never mutates it, matching <see cref="Markets.MarketClearingCalculator"/>'s
/// "pure math a system calls into" convention.
/// </summary>
public static class HeirEligibilityService
{
    /// <summary>The eligible-heir pool for <paramref name="headId"/>, in §2.4's default agnatic-line
    /// fallback order: sons before daughters, eldest birth date first within each group. A future
    /// game-start toggle relaxing this to birth-order-only or player-neutral (§2.4) is an Open Question
    /// this implementation does not build.</summary>
    public static IReadOnlyList<RuntimeId<Character>> EligibleHeirs(
        WorldState state, RuntimeId<Character> headId, HeirDesignation? designation)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var disowned = designation?.DisownedCharacterIds ?? Array.Empty<RuntimeId<Character>>();
        var adopted = designation?.AdoptedChildIds ?? Array.Empty<RuntimeId<Character>>();
        var acknowledgedIllegitimate = designation?.AcknowledgedIllegitimateChildIds ?? Array.Empty<RuntimeId<Character>>();

        var pool = new List<(RuntimeId<Character> Id, Character Character)>();
        foreach (var (id, character) in state.Characters.InAscendingOrder())
        {
            if (id == headId || !character.IsAlive || disowned.Contains(id))
                continue;

            var isBirthChild = character.MotherId == headId || character.FatherId == headId;
            var isEligibleBirthChild = isBirthChild &&
                (character.Legitimacy == Legitimacy.Legitimate || acknowledgedIllegitimate.Contains(id));
            var isAdoptedChild = adopted.Contains(id);

            if (isEligibleBirthChild || isAdoptedChild)
                pool.Add((id, character));
        }

        return pool
            .OrderByDescending(entry => entry.Character.Sex == Sex.Male)
            .ThenBy(entry => entry.Character.BirthDate.TotalMonths)
            .ThenBy(entry => entry.Id.Value)
            .Select(entry => entry.Id)
            .ToArray();
    }

    /// <summary>§3's "surviving spouse... can hold the estate in trust when no adult heir exists" —
    /// the still-living spouse from the marriage <see cref="CharacterLifecycleSystem"/> closed when
    /// <paramref name="deadHead"/> died this same tick, or <c>null</c> if there is none. Reads the
    /// closed <see cref="MarriageRecord"/> rather than <see cref="Character.CurrentSpouseId"/>, since
    /// that record has already been closed by the time this runs (Lifecycle's phase precedes
    /// RelationshipsActors — ADR 0005).</summary>
    public static RuntimeId<Character>? SurvivingSpouse(WorldState state, Character deadHead)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (deadHead.DeathRecord is not { } death)
            return null;

        foreach (var record in deadHead.MaritalHistory)
        {
            if (record.EndDate != death.Date || record.EndReason != MarriageEndReason.Death)
                continue;
            if (state.Characters.TryGet(record.SpouseId, out var spouse) && spouse.IsAlive)
                return record.SpouseId;
        }

        return null;
    }

    /// <summary>Whether <paramref name="characterId"/> is still below §6.2 Regency's Adult floor as of
    /// <paramref name="asOf"/> (<see cref="SuccessionCatalog.MinimumAdultAgeYears"/>).</summary>
    public static bool IsMinor(WorldState state, RuntimeId<Character> characterId, GameDate asOf)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        return state.Characters.TryGet(characterId, out var character) &&
            character.AgeInYears(asOf) < SuccessionCatalog.MinimumAdultAgeYears;
    }
}
