using Gens.Simulation.Random;

namespace Gens.Simulation.Characters;

/// <summary>
/// Roadmap Phase 5 item 2's single entry point: draws a Character's name (<see
/// cref="CharacterNameGenerator"/>) and visual profile (<see cref="CharacterVisualProfileGenerator"/>)
/// from the same named stream, name first, in this fixed order — so a given campaign seed and stream
/// name always reproduce the same identity for the same draw sequence, and adding a field to one
/// generator never shifts the other's draws out from under it.
/// </summary>
public static class CharacterIdentityGenerator
{
    public static GeneratedCharacterIdentity Generate(
        RandomStreamSet streams,
        string streamName,
        Sex sex,
        LegalStatus status,
        NamePool pool,
        string? fixedNomen = null,
        int? femaleOrdinal = null)
    {
        var name = CharacterNameGenerator.Generate(streams, streamName, sex, status, pool, fixedNomen, femaleOrdinal);
        var visual = CharacterVisualProfileGenerator.Generate(streams, streamName);
        return new GeneratedCharacterIdentity(name, visual);
    }
}

/// <summary>A freshly generated Character's name and visual profile, ready to feed into <see
/// cref="Character.Create"/> alongside the rest of that record's fields.</summary>
public sealed record GeneratedCharacterIdentity(CharacterName Name, CharacterVisualProfile Visual);
