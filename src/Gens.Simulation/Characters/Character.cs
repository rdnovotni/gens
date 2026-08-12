using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>
/// The canonical Character record (Phase 5 item 1; <c>gens-characters-design.md</c> §14,
/// <c>gens-familia-design.md</c> §8): every named individual the game tracks — family, slave,
/// freedman, client, companion, rival, or encounter alike — gets this same full shape, immediately,
/// per <c>gens-characters-design.md</c> §2's "no more lightweight tier" decision. This item only
/// establishes the record's shape and validation; name/appearance generation (item 2), lifecycle
/// transitions and aging (item 3), traits (item 4), relationships (item 5), household roles (item 6),
/// and lazy instantiation/promotion (item 7) are later Phase 5 work.
/// </summary>
public sealed record Character
{
    private Character()
    {
    }

    public required RuntimeId<Character> Id { get; init; }

    // Identity (gens-familia-design.md §2.8).
    public required string Praenomen { get; init; }
    public required string Nomen { get; init; }
    public string? Cognomen { get; init; }
    public required Sex Sex { get; init; }
    public required GameDate BirthDate { get; init; }

    // Legal status & social class (gens-familia-design.md §2.5).
    public required LegalStatus LegalStatus { get; init; }
    public SocialClass? SocialClass { get; init; }

    // Culture (content-authored; gens-familia-design.md §8's originCulture).
    public required DefinitionId<Culture> Culture { get; init; }

    // Location & household membership.
    public required RuntimeId<Settlement> Location { get; init; }
    public RuntimeId<Household>? Household { get; init; }

    // Attributes, skills, condition (gens-familia-design.md §2.1-2.3).
    public required CoreAttributes Attributes { get; init; }
    public required LaborSkills Skills { get; init; }
    public required Condition Condition { get; init; }

    // Provenance (gens-characters-design.md §14).
    public required CharacterSource Source { get; init; }
    public required int InstantiatedAtMonth { get; init; }

    /// <summary>The only supported way to construct a <see cref="Character"/> — validates the
    /// cross-field invariant an object initializer can't enforce on its own (<see cref="SocialClass"/>
    /// is citizen-only, <c>gens-familia-design.md</c> §2.5) before returning.</summary>
    public static Character Create(
        RuntimeId<Character> id,
        string praenomen,
        string nomen,
        string? cognomen,
        Sex sex,
        GameDate birthDate,
        LegalStatus status,
        SocialClass? socialClass,
        DefinitionId<Culture> culture,
        RuntimeId<Settlement> location,
        RuntimeId<Household>? household,
        CoreAttributes attributes,
        LaborSkills skills,
        Condition condition,
        CharacterSource source,
        int instantiatedAtMonth)
    {
        if (string.IsNullOrWhiteSpace(praenomen))
            throw new ArgumentException("A Character requires a non-empty praenomen.", nameof(praenomen));
        if (string.IsNullOrWhiteSpace(nomen))
            throw new ArgumentException("A Character requires a non-empty nomen.", nameof(nomen));
        if (socialClass is not null && status != LegalStatus.RomanCitizen)
            throw new ArgumentException(
                $"'{nameof(socialClass)}' can only be set for a {LegalStatus.RomanCitizen} " +
                $"(gens-familia-design.md §2.5); got legal status '{status}'.",
                nameof(socialClass));

        return new Character
        {
            Id = id,
            Praenomen = praenomen,
            Nomen = nomen,
            Cognomen = cognomen,
            Sex = sex,
            BirthDate = birthDate,
            LegalStatus = status,
            SocialClass = socialClass,
            Culture = culture,
            Location = location,
            Household = household,
            Attributes = attributes,
            Skills = skills,
            Condition = condition,
            Source = source,
            InstantiatedAtMonth = instantiatedAtMonth,
        };
    }

    /// <summary>Derives the lifecycle stage as of <paramref name="asOf"/> from <see cref="BirthDate"/>
    /// (<c>gens-familia-design.md</c> §3's age bands) — never stored, matching <see
    /// cref="GameDate"/>'s own "derived, never stored redundantly" convention.</summary>
    public LifecycleStage GetLifecycleStage(GameDate asOf)
    {
        // Checked directly in months, not via AgeInYears: integer division truncates toward zero, so
        // a BirthDate 1-11 months ahead of asOf would otherwise floor to an in-range 0 years old
        // instead of being caught as "before birth."
        if (asOf.TotalMonths < BirthDate.TotalMonths)
            throw new ArgumentOutOfRangeException(nameof(asOf), asOf, "A Character cannot have a negative age.");

        return AgeInYears(asOf) switch
        {
            <= 3 => LifecycleStage.Infant,
            <= 12 => LifecycleStage.Child,
            <= 17 => LifecycleStage.Adolescent,
            <= 59 => LifecycleStage.Adult,
            _ => LifecycleStage.Elderly,
        };
    }

    /// <summary>Whole years elapsed between <see cref="BirthDate"/> and <paramref name="asOf"/>,
    /// floor-divided from the project's sole month-granular time representation (ADR 0003).</summary>
    public int AgeInYears(GameDate asOf) => (asOf.TotalMonths - BirthDate.TotalMonths) / 12;
}
