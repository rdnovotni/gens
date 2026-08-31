using Gens.Simulation.Identity;

namespace Gens.Simulation.Languages;

/// <summary>§10's <c>LanguageFamily</c> shape — supports §5's family-relationship acquisition
/// discount by grouping languages with genuine, real partial mutual intelligibility (Gaulish before
/// Brythonic; Sarmatian/Scythian alongside Parthian). <see cref="IsIsolate"/> is <c>true</c> only for
/// Basque/Aquitanian's own single-member family (§10: "true for Basque/Aquitanian specifically — no
/// discount ever applies") — every other single-member family in <see cref="KnownWorldLanguages"/>
/// (Armenian's own distinct branch, §2.7; every Trade-Contact-Only or thinly-attested isolate in §2.8-
/// §2.10) is a real, separate family with nobody to discount against, but is not itself flagged <see
/// cref="IsIsolate"/> — §10's own wording reserves that flag for Basque specifically, not "any
/// single-member family" generically.</summary>
public sealed record LanguageFamily
{
    public LanguageFamily(
        DefinitionId<LanguageFamily> id, string name, IReadOnlyList<DefinitionId<LanguageDefinition>> memberLanguages,
        bool isIsolate = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A language family requires a non-empty name.", nameof(name));
        if (memberLanguages is null || memberLanguages.Count == 0)
            throw new ArgumentException("A language family requires at least one member language.", nameof(memberLanguages));
        if (isIsolate && memberLanguages.Count != 1)
            throw new ArgumentException("An isolate family must have exactly one member language.", nameof(memberLanguages));

        Id = id;
        Name = name;
        MemberLanguages = memberLanguages;
        IsIsolate = isIsolate;
    }

    public DefinitionId<LanguageFamily> Id { get; }
    public string Name { get; }
    public IReadOnlyList<DefinitionId<LanguageDefinition>> MemberLanguages { get; }
    public bool IsIsolate { get; }
}
