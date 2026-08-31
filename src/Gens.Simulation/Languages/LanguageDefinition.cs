using Gens.Simulation.Identity;

namespace Gens.Simulation.Languages;

/// <summary>One real, learnable language from §2's linguistic geography. Deliberately excludes §2.11's
/// two extinct-but-ritual-use languages (Etruscan, Sicel/Sicani) — "neither is a learnable Language
/// Proficiency entry" per that section's own closing line; both stay Religion's flavor content, out of
/// this catalog entirely.</summary>
public sealed record LanguageDefinition
{
    public LanguageDefinition(DefinitionId<LanguageDefinition> id, string name, DefinitionId<LanguageFamily> familyId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A language definition requires a non-empty name.", nameof(name));

        Id = id;
        Name = name;
        FamilyId = familyId;
    }

    public DefinitionId<LanguageDefinition> Id { get; }
    public string Name { get; }
    public DefinitionId<LanguageFamily> FamilyId { get; }
}
