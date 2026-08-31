using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Languages;

/// <summary>§10's <c>LanguageProficiency</c> shape — tracked only for full, named <see
/// cref="Character"/> records (§3's own restraint: ambient population Literacy and Language stay
/// derived, never tracked). A real <see cref="WorldState"/> partition, not content — which languages a
/// specific Character actually knows changes over the course of a campaign (a language learned, a
/// Distant Holding's sustained exposure slowly raising a tier) the same way a <see
/// cref="Travel.TravelTrip"/>'s own progress is genuine campaign state rather than something re-derived
/// fresh from other records. Own <see cref="RuntimeId{T}"/> rather than keyed by <see
/// cref="CharacterId"/> alone, since one Character legitimately holds many of these at once (§8: "no
/// artificial ceiling"), unlike <see cref="Clientela.ClientelaEntry"/>'s one-per-Character shape.</summary>
public sealed record LanguageProficiency(
    RuntimeId<LanguageProficiency> Id,
    RuntimeId<Character> CharacterId,
    DefinitionId<LanguageDefinition> LanguageId,
    FluencyTier FluencyTier,
    LanguageAcquisitionMethod AcquisitionMethod);

/// <summary>Read-side helpers over <see cref="WorldState.LanguageProficiencies"/>, mirroring <see
/// cref="Clientela.ClientelaResolver"/>'s identical "small, hand-curated collection, linear scan is
/// fine" judgment — a Character's own language roster is realistically a handful of entries (§8's own
/// "four or five languages at once" ceiling case), not a population-scale collection.</summary>
public static class LanguageProficiencyQueries
{
    public static IReadOnlyList<LanguageProficiency> ForCharacter(WorldState state, RuntimeId<Character> characterId)
    {
        var results = new List<LanguageProficiency>();
        foreach (var entry in state.LanguageProficiencies.InAscendingOrder())
            if (entry.Value.CharacterId == characterId)
                results.Add(entry.Value);
        return results;
    }

    public static bool TryGet(
        WorldState state, RuntimeId<Character> characterId, DefinitionId<LanguageDefinition> languageId,
        out LanguageProficiency proficiency)
    {
        foreach (var entry in state.LanguageProficiencies.InAscendingOrder())
        {
            if (entry.Value.CharacterId == characterId && entry.Value.LanguageId == languageId)
            {
                proficiency = entry.Value;
                return true;
            }
        }

        proficiency = null!;
        return false;
    }

    /// <summary>§6's own Conversational-or-better floor, read directly off a Character's tracked
    /// proficiency — <c>false</c> for a language this Character has no tracked entry for at all.</summary>
    public static bool HasConversationalOrBetter(WorldState state, RuntimeId<Character> characterId, DefinitionId<LanguageDefinition> languageId) =>
        TryGet(state, characterId, languageId, out var proficiency) &&
        proficiency.FluencyTier is FluencyTier.Conversational or FluencyTier.FluentNative;
}
