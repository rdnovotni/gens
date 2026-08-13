namespace Gens.Simulation.Characters;

/// <summary>
/// Content-authored name pools (<c>gens-familia-design.md</c> §2.8) a single culture's <see
/// cref="CharacterNameGenerator"/> call draws from. Deliberately plain and content-agnostic — turning
/// compiled <c>names</c> content (<c>content/schemas/names.schema.json</c>) into this shape is later
/// integration work; this item only needs something the generator can draw against deterministically.
/// </summary>
public sealed record NamePool
{
    /// <summary>Male citizen praenomina (§2.8) — e.g. "Marcus", "Gaius", "Lucius".</summary>
    public required IReadOnlyList<string> Praenomina { get; init; }

    /// <summary>Gens names shared by a whole family; feminized for a female citizen (§2.8, <see
    /// cref="CharacterNameGenerator.Feminize"/>).</summary>
    public required IReadOnlyList<string> Nomina { get; init; }

    /// <summary>Branch/personal identifiers, sometimes inherited, sometimes descriptive (§2.8). May be
    /// empty — a cognomen is never required for a generated name.</summary>
    public required IReadOnlyList<string> Cognomina { get; init; }

    /// <summary>Single, origin-flavored given names for the enslaved and peregrini (§2.8) — used until
    /// and unless manumission or a citizenship grant changes that (a later lifecycle-transition
    /// concern, not this generator's).</summary>
    public required IReadOnlyList<string> GivenNames { get; init; }
}
