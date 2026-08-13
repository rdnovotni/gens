using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Queries;

/// <summary>An immutable, presentation-ready snapshot of one Character's lifecycle state (Phase 5
/// item 3): current stage, age, alive/dead status, and current spouse. No <see
/// cref="State.KnowledgeState"/> filtering yet — Character-level knowledge propagation into <see
/// cref="Characters.CharacterTopics"/> is future work, per that type's own doc comment; every observer
/// currently sees the same projection, matching <see cref="CampaignClockQuery"/>'s worked example.</summary>
public readonly record struct CharacterLifecycleProjection(
    string CharacterId,
    LifecycleStage Stage,
    int AgeInYears,
    bool IsAlive,
    string? CurrentSpouseId);

/// <summary>Projects a single, caller-specified Character's lifecycle state — unlike <see
/// cref="CampaignClockQuery"/>'s single implicit subject (the whole campaign clock), a per-Character
/// query needs to know which Character, so it is supplied at construction time.</summary>
public sealed class CharacterLifecycleQuery : IWorldQuery<CharacterLifecycleProjection>
{
    private readonly RuntimeId<Character> _characterId;

    public CharacterLifecycleQuery(RuntimeId<Character> characterId) => _characterId = characterId;

    public CharacterLifecycleProjection Execute(WorldState state, string observerId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (!state.Characters.TryGet(_characterId, out var character))
            throw new KeyNotFoundException($"No Character with ID '{_characterId}' exists.");

        return new CharacterLifecycleProjection(
            CharacterId: character.Id.ToTaggedString(),
            Stage: character.GetLifecycleStage(state.Date),
            AgeInYears: character.AgeInYears(state.Date),
            IsAlive: character.IsAlive,
            CurrentSpouseId: character.CurrentSpouseId?.ToTaggedString());
    }
}
