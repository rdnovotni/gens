using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Queries;

/// <summary>The character detail screen's projection (Phase 9 item 6): one Character's full sheet —
/// identity, lifecycle, legal/social standing, attributes, labor skills, condition, current duty,
/// traits, spouse, and visual profile (the placeholder/procedural portrait's own data source, Phase 9
/// item 8). Extends <see cref="CharacterLifecycleQuery"/>'s single-subject shape with
/// every field <c>gens-core-design.md</c> §7.4's "Familia individual record" diptych (identity leaf
/// plus stat-gauge leaf) needs, rather than duplicating that narrower lifecycle-only query.</summary>
public readonly record struct CharacterDetailProjection(
    string CharacterId,
    string FullName,
    Sex Sex,
    int AgeInYears,
    LifecycleStage Stage,
    bool IsAlive,
    LegalStatus LegalStatus,
    SocialClass? SocialClass,
    CoreAttributes Attributes,
    LaborSkills Skills,
    Condition Condition,
    DutySlot? DutySlot,
    IReadOnlyList<string> TraitIds,
    string? CurrentSpouseId,
    string? MotherId,
    string? FatherId,
    CharacterVisualProfile VisualProfile);

/// <summary>Projects a single, caller-specified Character's full detail. No <see
/// cref="KnowledgeState"/> filtering yet, matching <see cref="CharacterLifecycleQuery"/>'s own
/// precedent — every observer currently sees the same projection.</summary>
public sealed class CharacterDetailQuery : IWorldQuery<CharacterDetailProjection>
{
    private readonly RuntimeId<Character> _characterId;

    public CharacterDetailQuery(RuntimeId<Character> characterId) => _characterId = characterId;

    public CharacterDetailProjection Execute(WorldState state, string observerId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (!state.Characters.TryGet(_characterId, out var character))
            throw new KeyNotFoundException($"No Character with ID '{_characterId}' exists.");

        return new CharacterDetailProjection(
            CharacterId: character.Id.ToTaggedString(),
            FullName: FormatFullName(character),
            Sex: character.Sex,
            AgeInYears: character.AgeInYears(state.Date),
            Stage: character.GetLifecycleStage(state.Date),
            IsAlive: character.IsAlive,
            LegalStatus: character.LegalStatus,
            SocialClass: character.SocialClass,
            Attributes: character.Attributes,
            Skills: character.Skills,
            Condition: character.Condition,
            DutySlot: character.Duty?.Slot,
            TraitIds: character.Traits.Select(trait => trait.Value).ToArray(),
            CurrentSpouseId: character.CurrentSpouseId?.ToTaggedString(),
            MotherId: character.MotherId?.ToTaggedString(),
            FatherId: character.FatherId?.ToTaggedString(),
            VisualProfile: character.VisualProfile);
    }

    private static string FormatFullName(Character character) =>
        character.Cognomen is { Length: > 0 } cognomen
            ? $"{character.Praenomen} {character.Nomen} {cognomen}"
            : $"{character.Praenomen} {character.Nomen}";
}
