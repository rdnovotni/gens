using Gens.Simulation.Random;

namespace Gens.Simulation.Characters;

/// <summary>
/// Deterministically draws a <see cref="CharacterName"/> from a named <see cref="RandomStreamSet"/>
/// stream, following <c>gens-familia-design.md</c> §2.8's naming convention (roadmap Phase 5 item 2).
/// This generates a fresh name for a newly-instantiated Character; it does not implement the
/// manumission renaming transition (§2.8's "the one moment a person's full name changes in play") —
/// that is lifecycle-transition work, later in Phase 5.
/// </summary>
public static class CharacterNameGenerator
{
    /// <summary>Roll chance a male citizen-shaped name also gets a cognomen — "sometimes inherited,
    /// sometimes descriptive" (§2.8), i.e. not universal.</summary>
    private const uint CognomenChancePercent = 50;

    private static readonly string[] OrdinalWords = new[]
    {
        "Prima", "Secunda", "Tertia", "Quarta", "Quinta",
        "Sexta", "Septima", "Octava", "Nona", "Decima",
    };

    /// <param name="fixedNomen">Overrides the pool draw for the gens name — a family member's name
    /// must be the player's own gens, not a random one (§2.8); leave <c>null</c> to draw an unrelated
    /// person's gens name from <paramref name="pool"/> instead.</param>
    /// <param name="femaleOrdinal">A 1-based sister ordinal ("Aurelia Prima", "Aurelia Secunda", §2.8)
    /// added as a disambiguating cognomen for a female citizen-shaped name; <c>null</c> when no
    /// disambiguation is needed. Ignored for every other shape.</param>
    public static CharacterName Generate(
        RandomStreamSet streams,
        string streamName,
        Sex sex,
        LegalStatus status,
        NamePool pool,
        string? fixedNomen = null,
        int? femaleOrdinal = null)
    {
        if (streams is null)
            throw new ArgumentNullException(nameof(streams));
        if (pool is null)
            throw new ArgumentNullException(nameof(pool));

        // Enslaved individuals and peregrini carry a single, origin-flavored name (§2.8) regardless of
        // sex, until a lifecycle event (manumission, a citizenship grant) changes that.
        if (status is LegalStatus.Enslaved or LegalStatus.Peregrine)
            return GenerateSingleName(streams, streamName, pool);

        return sex == Sex.Female
            ? GenerateFemaleCitizenName(streams, streamName, pool, fixedNomen, femaleOrdinal)
            : GenerateMaleCitizenName(streams, streamName, pool, fixedNomen);
    }

    /// <summary>A blunt, deterministic Latin feminization: a "-us" ending becomes "-a" (Aurelius to
    /// Aurelia); anything else gets a plain "-a" appended. Real Latin morphology has more exceptions
    /// than this covers, but every pool entry this project authors is expected to be a standard
    /// second-declension "-us" gens name (§2.8's own examples), so this rule is exact for the content
    /// it will actually see.</summary>
    public static string Feminize(string nomen)
    {
        if (string.IsNullOrEmpty(nomen))
            throw new ArgumentException("A nomen is required.", nameof(nomen));

        return nomen.EndsWith("us", StringComparison.Ordinal)
            ? nomen.Substring(0, nomen.Length - 2) + "a"
            : nomen + "a";
    }

    private static CharacterName GenerateSingleName(RandomStreamSet streams, string streamName, NamePool pool)
    {
        var name = Draw(streams, streamName, pool.GivenNames, nameof(pool.GivenNames));

        // The single-name convention (§2.8) has no separate nomen; Nomen is duplicated rather than
        // left blank because Character.Create requires both fields non-empty (item 1's own invariant).
        return new CharacterName(name, name, null);
    }

    private static CharacterName GenerateMaleCitizenName(
        RandomStreamSet streams, string streamName, NamePool pool, string? fixedNomen)
    {
        var praenomen = Draw(streams, streamName, pool.Praenomina, nameof(pool.Praenomina));
        var nomen = fixedNomen ?? Draw(streams, streamName, pool.Nomina, nameof(pool.Nomina));

        string? cognomen = null;
        if (pool.Cognomina.Count > 0 && streams.NextUInt(streamName, 100) < CognomenChancePercent)
            cognomen = Draw(streams, streamName, pool.Cognomina, nameof(pool.Cognomina));

        return new CharacterName(praenomen, nomen, cognomen);
    }

    private static CharacterName GenerateFemaleCitizenName(
        RandomStreamSet streams, string streamName, NamePool pool, string? fixedNomen, int? femaleOrdinal)
    {
        var nomen = fixedNomen ?? Draw(streams, streamName, pool.Nomina, nameof(pool.Nomina));
        var feminized = Feminize(nomen);
        var cognomen = femaleOrdinal is int ordinal ? OrdinalWord(ordinal) : null;

        // "The feminized nomen alone" (§2.8) has no separate praenomen either; Character.Create's
        // required-non-empty invariant is satisfied by duplication, same as the single-name case above.
        return new CharacterName(feminized, feminized, cognomen);
    }

    private static string OrdinalWord(int ordinal)
    {
        if (ordinal < 1 || ordinal > OrdinalWords.Count)
            throw new ArgumentOutOfRangeException(
                nameof(ordinal), ordinal, $"Only ordinals 1-{OrdinalWords.Count} have a named descriptor.");

        return OrdinalWords[ordinal - 1];
    }

    private static string Draw(RandomStreamSet streams, string streamName, IReadOnlyList<string> pool, string poolName)
    {
        if (pool.Count == 0)
            throw new ArgumentException($"'{poolName}' must contain at least one name.", poolName);

        return pool[(int)streams.NextUInt(streamName, (uint)pool.Count)];
    }
}
